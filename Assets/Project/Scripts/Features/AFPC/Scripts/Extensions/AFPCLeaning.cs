using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace AFPC {

/// <summary>
/// Smooth left/right lean — tilts the camera and shifts it horizontally while held.
/// The collider does not move; lean is purely visual. Applied in LateUpdate after
/// Follow() sets the base camera position, so it stacks cleanly with all other effects.
/// </summary>
[AddComponentMenu("AFPC/Leaning")]
public class AFPCLeaning : AFPCExtension {

    public bool isDebugLog;

    [Tooltip("Input action held to lean left.")]
    public InputActionReference leanLeftAction;
    [Tooltip("Input action held to lean right.")]
    public InputActionReference leanRightAction;
    [Tooltip("Maximum camera roll in degrees at full lean.")]
    public float leanAngle = 15f;
    [Tooltip("Maximum horizontal camera offset in world units at full lean.")]
    public float leanDistance = 0.35f;
    [Tooltip("Speed at which the lean interpolates to the target (higher = snappier).")]
    public float leanSpeed = 10f;

    /// <summary> Fired when the player starts leaning from a neutral position. </summary>
    public UnityEvent onLeanStart;
    /// <summary> Fired when the player returns to neutral after leaning. </summary>
    public UnityEvent onLeanStop;

    private InputAction leftAction;
    private InputAction rightAction;
    private float currentLean; // –1 = full left, 0 = neutral, 1 = full right
    private bool wasLeaning;

    public override void Initialize () {
        if (leanLeftAction  != null) { leftAction  = leanLeftAction.action;  leftAction.Enable();  }
        if (leanRightAction != null) { rightAction = leanRightAction.action; rightAction.Enable(); }
        currentLean = 0f;
    }

    private void OnDestroy () {
        leftAction?.Disable();
        rightAction?.Disable();
    }

    public override void OnUpdate () {
        float target = 0f;
        if (rightAction != null && rightAction.IsPressed()) target += 1f;
        if (leftAction  != null && leftAction.IsPressed())  target -= 1f;

        currentLean = Mathf.MoveTowards(currentLean, target, leanSpeed * Time.deltaTime);

        bool leaning = Mathf.Abs(currentLean) > 0.01f;
        if (leaning && !wasLeaning) { wasLeaning = true;  onLeanStart?.Invoke(); }
        if (!leaning && wasLeaning) { wasLeaning = false; onLeanStop?.Invoke();  }

        if (isDebugLog) Debug.Log($"AFPCLeaning: lean={currentLean:F2}");
    }

    public override void OnLateUpdate () {
        Camera cam = hero.overview.camera;
        if (!cam) return;
        if (Mathf.Abs(currentLean) < 0.001f) return;

        // Horizontal offset — use hero's right so the shift stays level regardless of pitch
        cam.transform.position += hero.transform.right * (leanDistance * currentLean);

        // Roll — applied after Looking() has set yaw/pitch, so only Z is touched
        Vector3 euler = cam.transform.eulerAngles;
        euler.z = -leanAngle * currentLean;
        cam.transform.eulerAngles = euler;
    }

    /// <summary> Current lean value. –1 = full left, 0 = neutral, 1 = full right. </summary>
    public float GetLeanValue () => currentLean;

    public override bool IsActive () => Mathf.Abs(currentLean) > 0.01f;
}
}
