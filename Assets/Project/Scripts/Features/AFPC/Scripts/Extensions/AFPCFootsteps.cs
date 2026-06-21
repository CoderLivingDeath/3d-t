using UnityEngine;
using UnityEngine.Events;

namespace AFPC {

/// <summary>
/// Fires a step event at regular walking intervals while the player is grounded and moving.
/// Detects the surface under the player and passes a surface tag string to the event,
/// allowing audio or VFX systems to respond without any audio dependency in this component.
/// </summary>
[AddComponentMenu("AFPC/Footsteps")]
public class AFPCFootsteps : AFPCExtension {

    public bool isDebugLog;

    [Tooltip("Horizontal distance in meters between each step at walking speed.")]
    public float walkStepDistance = 2f;
    [Tooltip("Horizontal distance in meters between each step at running speed.")]
    public float runStepDistance = 1.4f;
    [Tooltip("Layers used to detect the surface under the player.")]
    public LayerMask groundMask = 1;
    [Tooltip("Length of the downward raycast used to detect surface material or layer.")]
    public float rayDistance = 1.5f;

    /// <summary> Fired on each step. Parameter is the PhysicsMaterial name, or layer name if no material is set. Empty string if nothing detected. </summary>
    public UnityEvent<string> onStep;

    private float distanceTraveled;
    private Vector3 lastPosition;

    public override void Initialize () {
        lastPosition = hero.transform.position;
        distanceTraveled = 0f;
    }

    public override void OnUpdate () {
        if (!hero.movement.IsGrounded()) {
            lastPosition = hero.transform.position;
            distanceTraveled = 0f;
            return;
        }

        Vector3 current   = hero.transform.position;
        Vector3 delta     = current - lastPosition;
        delta.y           = 0f;
        distanceTraveled += delta.magnitude;
        lastPosition      = current;

        bool hasInput = hero.movement.movementInputValues.sqrMagnitude > 0.01f;
        if (!hasInput) { distanceTraveled = 0f; return; }

        float threshold = hero.movement.runningInputValue ? runStepDistance : walkStepDistance;
        if (distanceTraveled >= threshold) {
            distanceTraveled -= threshold;
            FireStep();
        }
    }

    private void FireStep () {
        string surface = DetectSurface();
        if (isDebugLog) Debug.Log($"AFPCFootsteps: step on '{surface}'.");
        onStep?.Invoke(surface);
    }

    private string DetectSurface () {
        Vector3 origin = hero.movement.cc.bounds.center;
        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, rayDistance, groundMask)) {
            if (hit.collider.sharedMaterial != null && !string.IsNullOrEmpty(hit.collider.sharedMaterial.name))
                return hit.collider.sharedMaterial.name;
            return LayerMask.LayerToName(hit.collider.gameObject.layer);
        }
        return string.Empty;
    }
}
}
