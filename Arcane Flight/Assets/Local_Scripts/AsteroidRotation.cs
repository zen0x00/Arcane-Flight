using UnityEngine;

public class AsteroidRotation : MonoBehaviour
{
    public float rotationSpeed = 20f;
    Vector3 axis;

    void Start()
    {
        axis = Random.onUnitSphere;
    }

    void Update()
    {
        transform.Rotate(axis, rotationSpeed * Time.deltaTime);
    }
}
