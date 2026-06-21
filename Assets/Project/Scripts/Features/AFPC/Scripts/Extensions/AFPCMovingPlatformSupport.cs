using UnityEngine;
using UnityEngine.Events;

namespace AFPC {

/// <summary>
/// Tracks kinematic platforms under the player and applies their movement and rotation.
/// When the player leaves a platform, the platform's velocity is transferred to the player.
/// </summary>
[AddComponentMenu("AFPC/Moving Platform")]
public class AFPCMovingPlatformSupport : AFPCExtension {

    public bool isDebugLog;

    [Tooltip("How much of the platform's velocity transfers to the player when leaving. 0 = none, 1 = full.")]
    [Range(0f, 1f)] public float velocityInheritance = 1f;
    [Tooltip("Maximum speed that can be inherited from a platform.")]
    public float maxInheritedSpeed = 30f;

    /// <summary> Fired when the player lands on a moving platform. </summary>
    public UnityEvent onPlatformEnter;
    /// <summary> Fired when the player leaves a moving platform. </summary>
    public UnityEvent onPlatformExit;

    private Transform currentPlatform;
    private Vector3 lastPlatformPosition;
    private float lastPlatformYaw;
    private Vector3 platformVelocity;
    private bool onPlatform;

    public override bool IsActive () => onPlatform;

    /// <summary>
    /// The transform of the current platform, or null if not on a platform.
    /// </summary>
    public Transform GetCurrentPlatform () => currentPlatform;

    /// <summary>
    /// The current platform's velocity (position delta / fixedDeltaTime). Zero if not on a platform.
    /// </summary>
    public Vector3 GetPlatformVelocity () => platformVelocity;

    public override void OnFixedUpdate () {
        Transform groundTransform = hero.movement.GetGroundTransform();

        bool isPlatform = false;
        if (hero.movement.IsGrounded() && groundTransform != null) {
            Rigidbody platformRb = groundTransform.GetComponentInParent<Rigidbody>();
            isPlatform = platformRb != null && platformRb.isKinematic;
            if (isPlatform) groundTransform = platformRb.transform;
        }

        if (isPlatform) {
            if (currentPlatform != groundTransform) {
                if (onPlatform) ExitPlatform(false);
                EnterPlatform(groundTransform);
            }
            ApplyPlatformDelta();
        }
        else {
            if (onPlatform) ExitPlatform(true);
        }
    }

    private void EnterPlatform (Transform platform) {
        currentPlatform = platform;
        lastPlatformPosition = platform.position;
        lastPlatformYaw = platform.eulerAngles.y;
        platformVelocity = Vector3.zero;
        onPlatform = true;
        onPlatformEnter?.Invoke();
        if (isDebugLog) Debug.Log($"AFPCMovingPlatformSupport: entered '{platform.name}'.");
    }

    private void ExitPlatform (bool inheritVelocity) {
        if (inheritVelocity && velocityInheritance > 0f) {
            Vector3 addVelocity = platformVelocity * velocityInheritance;
            if (addVelocity.magnitude > maxInheritedSpeed)
                addVelocity = addVelocity.normalized * maxInheritedSpeed;
            hero.movement.AddVelocity(addVelocity);
            if (isDebugLog) Debug.Log($"AFPCMovingPlatformSupport: inherited velocity {addVelocity.magnitude:F1} m/s.");
        }
        string platformName = currentPlatform != null ? currentPlatform.name : "destroyed";
        currentPlatform = null;
        platformVelocity = Vector3.zero;
        onPlatform = false;
        onPlatformExit?.Invoke();
        if (isDebugLog) Debug.Log($"AFPCMovingPlatformSupport: exited '{platformName}'.");
    }

    private void ApplyPlatformDelta () {
        if (currentPlatform == null) {
            ExitPlatform(true);
            return;
        }

        Vector3 deltaPosition = currentPlatform.position - lastPlatformPosition;
        float currentYaw = currentPlatform.eulerAngles.y;
        float deltaYaw = Mathf.DeltaAngle(lastPlatformYaw, currentYaw);

        hero.movement.rb.position += deltaPosition;

        if (Mathf.Abs(deltaYaw) > 0.001f) {
            hero.overview.RotateYaw(deltaYaw);
            Vector3 pivotOffset = hero.movement.rb.position - currentPlatform.position;
            Vector3 rotatedOffset = Quaternion.Euler(0, deltaYaw, 0) * pivotOffset;
            Vector3 rotationDisplacement = rotatedOffset - pivotOffset;
            hero.movement.rb.position += rotationDisplacement;
        }

        platformVelocity = deltaPosition / Time.fixedDeltaTime;
        lastPlatformPosition = currentPlatform.position;
        lastPlatformYaw = currentYaw;
    }
}
}
