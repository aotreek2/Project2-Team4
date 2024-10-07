using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateObject : MonoBehaviour
{
    // Speed of rotation (can be adjusted in the Unity editor)
    public float rotationSpeed = 100f;

    // Update is called once per frame
    void Update()
    {
        // Rotate the object around its Z axis (Vector3.forward is shorthand for (0, 0, 1))
        transform.Rotate(Vector3.forward * rotationSpeed * Time.deltaTime);
    }
}
