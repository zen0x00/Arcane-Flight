using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Offset")]
    public Vector3 offset = new Vector3(0f, 3f, -8f);

    [Header("Follow Settings")]
    public float positionSmooth = 5f;
    public float rotationSmooth = 6f;

    void LateUpdate()
    {
        if (target == null)
            return;

        // Desired position
        Vector3 desiredPos = target.position + target.TransformDirection(offset);

        // Smooth position
        transform.position = Vector3.Lerp(
            transform.position,
            desiredPos,
            Time.deltaTime * positionSmooth
        );

        // Look at target smoothly
        Quaternion desiredRot = Quaternion.LookRotation(
            target.position - transform.position,
            Vector3.up
        );

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            desiredRot,
            Time.deltaTime * rotationSmooth
        );
    }
}
