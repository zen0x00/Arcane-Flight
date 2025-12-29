using UnityEngine;

public class PlaneDirectFromPose : MonoBehaviour
{
    public float maxTilt = 25f;
    public float sensitivity = 0.02f;
    public float smoothTime = 0.15f;

    float currentTilt;
    float tiltVelocity;

    void Update()
    {
        PoseFrame frame = PoseUdpReceiver.latest;
        if (frame == null || frame.angles == null)
            return;

        if (!frame.angles.TryGetValue("left_shoulder", out float left))
            return;

        if (!frame.angles.TryGetValue("right_shoulder", out float right))
            return;

        float diff = left - right;

        float targetTilt =
            Mathf.Clamp(diff * sensitivity, -1f, 1f) * maxTilt;

        currentTilt = Mathf.SmoothDamp(
            currentTilt,
            targetTilt,
            ref tiltVelocity,
            smoothTime
        );

        transform.localRotation =
            Quaternion.Euler(0f, 0f, -currentTilt);

        Debug.Log($"L={left:F2} R={right:F2} Diff={diff:F2}");
    }
}
