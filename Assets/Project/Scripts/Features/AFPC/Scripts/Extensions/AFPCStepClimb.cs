using UnityEngine;
using UnityEngine.Events;

namespace AFPC {

/// <summary>
/// Automatically steps up small obstacles. Rigidbody-based controllers don't get free
/// step-climbing unlike CharacterController, so this detects a low ledge directly ahead
/// and applies an upward velocity boost to lift the player over it.
/// </summary>
[AddComponentMenu("AFPC/Step Climb")]
public class AFPCStepClimb : AFPCExtension {

    public bool isDebugLog;

    [Tooltip("Maximum obstacle height in world units that the player can step over.")]
    public float maxStepHeight = 0.4f;
    [Tooltip("Upward velocity applied when a climbable step is detected.")]
    public float stepUpForce = 4f;
    [Tooltip("How far ahead to check for an obstacle. Should roughly match the collider radius.")]
    public float checkDistance = 0.4f;
    [Tooltip("Minimum seconds between step boosts to prevent repeated triggering on the same ledge.")]
    public float cooldown = 0.3f;
    [Tooltip("Layers considered as obstacles for step detection.")]
    public LayerMask stepMask = 1;

    /// <summary> Fired each time the player steps up an obstacle. </summary>
    public UnityEvent onStep;

    private float cooldownTimer;

    public override void OnFixedUpdate () {
        if (cooldownTimer > 0f) { cooldownTimer -= Time.fixedDeltaTime; return; }
        if (!hero.movement.IsGrounded()) return;
        if (hero.movement.movementInputValues.sqrMagnitude < 0.01f) return;

        // Build a movement direction from input projected onto the hero's horizontal plane
        Vector3 inputDir = new Vector3(hero.movement.movementInputValues.x, 0f, hero.movement.movementInputValues.y);
        Vector3 worldDir = hero.transform.TransformDirection(inputDir).normalized;

        Vector3 foot = hero.movement.cc.bounds.min + Vector3.up * 0.05f;

        // Low ray: must hit something at foot level (the step face)
        if (!Physics.Raycast(foot, worldDir, checkDistance, stepMask)) return;

        // High ray: must be clear above the step height (room to climb onto)
        Vector3 top = foot + Vector3.up * (maxStepHeight + 0.05f);
        if (Physics.Raycast(top, worldDir, checkDistance, stepMask)) return;

        // Confirm there is actually a ledge surface to land on
        Vector3 peekOrigin = foot + worldDir * checkDistance + Vector3.up * (maxStepHeight + 0.1f);
        if (!Physics.Raycast(peekOrigin, Vector3.down, maxStepHeight + 0.2f, stepMask)) return;

        // Apply step
        hero.movement.SetVerticalVelocity(stepUpForce);
        cooldownTimer = cooldown;

        if (isDebugLog) Debug.Log("AFPCStepClimb: stepped up.");
        onStep?.Invoke();
    }
}
}
