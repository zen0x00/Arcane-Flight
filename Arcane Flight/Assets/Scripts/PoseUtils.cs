using UnityEngine;

public static class PoseUtils
{
    public static Vector3 GetLandmark(PosePacket p, int index, float scale = 1f)
    {
        if (p == null || p.landmarks == null || index >= p.landmarks.Count)
            return Vector3.zero;

        Landmark3D lm = p.landmarks[index];

        return new Vector3(
            lm.x * scale,
            -lm.y * scale,
            lm.z * scale
        );
    }
}
