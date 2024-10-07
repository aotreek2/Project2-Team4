using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReverseRotation : MonoBehaviour
{
    // Speed of rotation in degrees per second
    public float rotationSpeed = 30f;

    void Update()
    {
        // Calculate the rotation for this frame in the opposite direction
        float rotationAmount = -rotationSpeed * Time.deltaTime;

        // Rotate the object around its Y-axis
        transform.Rotate(0, rotationAmount, 0);
    }
}
