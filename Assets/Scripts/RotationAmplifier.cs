using UnityEngine;

public class RotationAmplifier : MonoBehaviour
{
    public Transform headTransform; // Arrastra aquí la Main Camera
    public float multiplier = 1.5f; // Ajusta esto (1.1 a 1.5)

    void Update()
    {
        Vector3 headRotation = headTransform.localEulerAngles;
        // Amplifica solo el giro horizontal (Y)
        transform.localEulerAngles = new Vector3(0, headRotation.y * (multiplier - 1), 0);
    }
}

