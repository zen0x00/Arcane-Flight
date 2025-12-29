using UnityEngine;

public class ShoulderRehabController : MonoBehaviour
{
    public enum ShoulderMovement
    {
        Neutral,
        Abduction,
        Adduction,
        Flexion
    }

    [Header("Live Angles (from UDP)")]
    public float smoothLeft;
    public float smoothRight;

    [Header("Neutral Calibration")]
    public float neutralLeft;
    public float neutralRight;

    [Header("Thresholds (Degrees)")]
    public float abductionThreshold = 15f;
    public float adductionThreshold = -15f;
    public float flexionThreshold = 30f;

    [Header("Detected Movements")]
    public ShoulderMovement leftMovement;
    public ShoulderMovement rightMovement;
    public ShoulderMovement combinedMovement;

    void Update()
    {
        // ---------------- READ UDP DATA ----------------
        PoseFrame frame = PoseUdpReceiver.latest;
        if (frame == null || frame.angles == null)
            return;

        // 🔥 THESE KEYS MUST MATCH YOUR PYTHON OUTPUT
        if (frame.angles.TryGetValue("left_shoulder", out float left))
        {
            smoothLeft = left;
        }

        if (frame.angles.TryGetValue("right_shoulder", out float right))
        {
            smoothRight = right;
        }

        // ---------------- CLASSIFY ----------------
        float leftDelta = smoothLeft - neutralLeft;
        float rightDelta = smoothRight - neutralRight;

        leftMovement = Classify(leftDelta);
        rightMovement = Classify(rightDelta);

        // FLEXION only when BOTH hands flex
        if (leftMovement == ShoulderMovement.Flexion &&
            rightMovement == ShoulderMovement.Flexion)
        {
            combinedMovement = ShoulderMovement.Flexion;
        }
        else
        {
            combinedMovement = ShoulderMovement.Neutral;
        }
    }

    ShoulderMovement Classify(float delta)
    {
        if (delta > flexionThreshold)
            return ShoulderMovement.Flexion;

        if (delta > abductionThreshold)
            return ShoulderMovement.Abduction;

        if (delta < adductionThreshold)
            return ShoulderMovement.Adduction;

        return ShoulderMovement.Neutral;
    }

    // ---------------- MANUAL CALIBRATION ----------------
    [ContextMenu("Calibrate Neutral")]
    public void CalibrateNeutral()
    {
        neutralLeft = smoothLeft;
        neutralRight = smoothRight;

        Debug.Log(
            $"[CALIBRATION] Neutral set: L={neutralLeft:F1}, R={neutralRight:F1}"
        );
    }
}
