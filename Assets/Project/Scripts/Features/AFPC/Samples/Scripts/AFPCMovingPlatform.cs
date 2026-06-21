using UnityEngine;

namespace AFPC {

/// <summary>
/// Test script for a kinematic platform that moves back and forth and optionally rotates.
/// Requires a Rigidbody set to isKinematic = true.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[AddComponentMenu("AFPC/Test/Moving Platform")]
public class AFPCMovingPlatform : MonoBehaviour {

    [Header("Translation")]
    [Tooltip("Direction and distance the platform travels from its start position.")]
    public Vector3 moveOffset = new Vector3(5f, 0f, 0f);
    [Tooltip("How long one full back-and-forth cycle takes in seconds.")]
    public float moveDuration = 4f;
    [Tooltip("Smooths the motion at endpoints. Disable for constant speed.")]
    public bool smoothMotion = true;

    [Header("Rotation")]
    [Tooltip("Enable continuous Y-axis rotation.")]
    public bool rotate;
    [Tooltip("Degrees per second around the Y axis.")]
    public float rotationSpeed = 30f;

    private Rigidbody rb;
    private Vector3 startPosition;
    private float moveTimer;

    private void Start () {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        startPosition = rb.position;
    }

    private void FixedUpdate () {
        moveTimer += Time.fixedDeltaTime;
        float t = Mathf.PingPong(moveTimer / moveDuration, 1f);
        if (smoothMotion) t = t * t * (3f - 2f * t);
        Vector3 target = startPosition + moveOffset * t;
        rb.MovePosition(target);

        if (rotate) {
            Quaternion yaw = Quaternion.Euler(0f, rotationSpeed * Time.fixedDeltaTime, 0f);
            rb.MoveRotation(rb.rotation * yaw);
        }
    }
}
}
