using UnityEngine;

public class PlaneMovementController : MonoBehaviour
{
    [Header("Forward Movement")]
    public float forwardSpeed = 5f;

    [Header("Tilt Control")]
    public float maxTilt = 25f;

    [Tooltip("Degrees around neutral where no tilt happens")]
    public float neutralAngle = 15f;

    [Tooltip("Degrees after neutralAngle to reach full tilt")]
    public float activeRange = 30f;

    [Tooltip("Smoothness of tilt")]
    public float tiltSmoothTime = 0.15f;

    [Header("Pose Input")]
    public ShoulderRehabController shoulder;

    float currentTilt;
    float tiltVelocity;

    float neutralDiff;
    bool isGameOver;

    void Start()
    {
        CalibrateNeutral();
    }

    void Update()
    {
        if (isGameOver || shoulder == null)
            return;

        // ---------------- FORWARD ----------------
        transform.Translate(
            Vector3.forward * forwardSpeed * Time.deltaTime,
            Space.World
        );

        // ---------------- SHOULDER DIFFERENCE ----------------
        float left =
            shoulder.smoothLeft - shoulder.neutralLeft;
        float right =
            shoulder.smoothRight - shoulder.neutralRight;

        float diff = (left - right) - neutralDiff;

        // ---------------- NEUTRAL WINDOW ----------------
        float absDiff = Mathf.Abs(diff);
        float normalized = 0f;

        if (absDiff > neutralAngle)
        {
            float effective = absDiff - neutralAngle;
            normalized = Mathf.Clamp01(effective / activeRange);
            normalized *= Mathf.Sign(diff);
        }

        float targetTilt = normalized * maxTilt;

        // ---------------- SMOOTH ----------------
        currentTilt = Mathf.SmoothDamp(
            currentTilt,
            targetTilt,
            ref tiltVelocity,
            tiltSmoothTime
        );

        // ---------------- APPLY ----------------
        transform.localRotation =
            Quaternion.Euler(0f, 0f, -currentTilt);
    }

    // ---------------- CALIBRATION ----------------
    public void CalibrateNeutral()
    {
        if (shoulder == null)
            return;

        neutralDiff =
            (shoulder.smoothLeft - shoulder.neutralLeft) -
            (shoulder.smoothRight - shoulder.neutralRight);

        Debug.Log($"[CALIBRATED] Neutral diff = {neutralDiff:F2}");
    }

    // 🔥 REQUIRED BY PlayerCollisionHandler
    public void StopGame()
    {
        isGameOver = true;
    }
}
