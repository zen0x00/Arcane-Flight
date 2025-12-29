import cv2
import time
import math
import socket
import struct
import numpy as np
from dataclasses import dataclass
from mediapipe import Image as MpImage, ImageFormat
from mediapipe.tasks import python
from mediapipe.tasks.python import vision

import os
# ======================================================
# CONFIG
# ======================================================

script_dir = os.path.dirname(os.path.abspath(__file__))
MODEL_PATH = os.path.join(script_dir, "pose_landmarker_heavy.task")


TRACK_W, TRACK_H = 640, 480
SMOOTHING_3D = 0.7

POSE_IP = "127.0.0.1"
POSE_PORT = 5052

ENABLE_DISPLAY = False        # set False for headless
DRAW_SKELETON = False

MAGIC = 0x504F5345           # "POSE"
VERSION = 1

# ======================================================
# DATA MODEL
# ======================================================
@dataclass
class Landmark3D:
    x: float
    y: float
    z: float
    v: float
    p: float

# MediaPipe indices
LS, RS = 11, 12
LH, RH = 23, 24

POSE_CONNECTIONS = [
    # Arms
    (11, 13), (13, 15),
    (12, 14), (14, 16),

    # Legs
    (23, 25), (25, 27),
    (24, 26), (26, 28),

    # Shoulders & hips (horizontal)
    (11, 12),
    (23, 24),

    # Central spine (virtual joints)
    ("hip_center", "shoulder_center"),

    # Shoulder fan (clean, symmetric)
    ("shoulder_center", 11),
    ("shoulder_center", 12),

    # Hip fan (clean, symmetric)
    ("hip_center", 23),
    ("hip_center", 24),
]


# ======================================================
# UDP
# ======================================================
sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

# ======================================================
# MEDIAPIPE
# ======================================================
landmarker = vision.PoseLandmarker.create_from_options(
    vision.PoseLandmarkerOptions(
        base_options=python.BaseOptions(model_asset_path=MODEL_PATH),
        running_mode=vision.RunningMode.VIDEO,
        num_poses=1,
        output_segmentation_masks=False
    )
)

# ======================================================
# HELPERS
# ======================================================
def smooth(prev, curr, f):
    if prev is None:
        return curr
    out = []
    for p, c in zip(prev, curr):
        out.append(Landmark3D(
            p.x * f + c.x * (1 - f),
            p.y * f + c.y * (1 - f),
            p.z * f + c.z * (1 - f),
            c.v, c.p
        ))
    return out

def midpoint(a, b):
    return Landmark3D(
        (a.x + b.x) * 0.5,
        (a.y + b.y) * 0.5,
        (a.z + b.z) * 0.5,
        min(a.v, b.v),
        min(a.p, b.p)
    )

def angle(a, b, c):
    ab = np.array([a.x - b.x, a.y - b.y, a.z - b.z])
    cb = np.array([c.x - b.x, c.y - b.y, c.z - b.z])
    na = np.linalg.norm(ab)
    nb = np.linalg.norm(cb)
    if na * nb == 0:
        return 0.0
    cosv = np.clip(np.dot(ab, cb) / (na * nb), -1.0, 1.0)
    return float(np.degrees(np.arccos(cosv)))

def draw_skeleton(frame, lm2d):
    h, w = frame.shape[:2]

    def resolve(name):
        if name == "shoulder_center":
            a, b = lm2d[11], lm2d[12]
            return int((a.x + b.x) * 0.5 * w), int((a.y + b.y) * 0.5 * h)
        if name == "hip_center":
            a, b = lm2d[23], lm2d[24]
            return int((a.x + b.x) * 0.5 * w), int((a.y + b.y) * 0.5 * h)

        p = lm2d[name]
        return int(p.x * w), int(p.y * h)

    for a, b in POSE_CONNECTIONS:
        A = resolve(a)
        B = resolve(b)
        cv2.line(frame, A, B, (0, 255, 0), 2)

# ======================================================
# MAIN
# ======================================================
cap = cv2.VideoCapture(0)
cap.set(3, TRACK_W)
cap.set(4, TRACK_H)

prev_world = None
print("[OK] Binary posture streamer started")

while True:
    ok, frame = cap.read()
    if not ok:
        break

    rgb = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
    mp_img = MpImage(image_format=ImageFormat.SRGB, data=rgb)
    ts = int(time.time() * 1000)

    result = landmarker.detect_for_video(mp_img, ts)
    if not result.pose_world_landmarks:
        if ENABLE_DISPLAY:
            cv2.imshow("Posture", frame)
            if cv2.waitKey(1) == 27:
                break
        continue

    lm3d = result.pose_world_landmarks[0]
    world = [Landmark3D(p.x, p.y, p.z, p.visibility, p.presence) for p in lm3d]

    # virtual landmarks
    world.append(midpoint(world[LS], world[RS]))  # 33
    world.append(midpoint(world[LH], world[RH]))  # 34

    world = smooth(prev_world, world, SMOOTHING_3D)
    prev_world = world

    # angles
    angles = {
        "left_shoulder": angle(world[23], world[11], world[13]),
        "right_shoulder": angle(world[24], world[12], world[14]),
        # "left_elbow": angle(world[11], world[13], world[15]),
        # "right_elbow": angle(world[12], world[14], world[16]),
        # "left_knee": angle(world[23], world[25], world[27]),
        # "right_knee": angle(world[24], world[26], world[28]),
        # "spine": angle(world[34], world[33], world[0])
    }

    # ========== BINARY PACK ==========
    buf = bytearray()
    buf += struct.pack("<IIQ", MAGIC, VERSION, ts)

    buf += struct.pack("<H", len(world))
    for lm in world:
        buf += struct.pack("<fffff", lm.x, lm.y, lm.z, lm.v, lm.p)

    buf += struct.pack("<H", len(angles))
    for name, val in angles.items():
        nb = name.encode("ascii")
        buf += struct.pack("<B", len(nb))
        buf += nb
        buf += struct.pack("<f", val)

    sock.sendto(buf, (POSE_IP, POSE_PORT))

    # display
    if ENABLE_DISPLAY:
        if DRAW_SKELETON and result.pose_landmarks:
            draw_skeleton(frame, result.pose_landmarks[0])

        cv2.imshow("Posture", frame)
        if cv2.waitKey(1) == 27:
            break

cap.release()
sock.close()
cv2.destroyAllWindows()
print("[STOP] Streamer exited")
