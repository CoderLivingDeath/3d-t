using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace AFPC {

/// <summary>
/// Raycast-based interaction system. Detects objects that implement IInteractable,
/// fires focus/unfocus events as the player looks at them, and calls Interact() on key press.
/// </summary>
[AddComponentMenu("AFPC/Interaction")]
public class AFPCInteraction : AFPCExtension {

    public bool isDebugLog;

    [Tooltip("Input action that triggers interaction.")]
    public InputActionReference interactAction;
    [Tooltip("Maximum distance at which objects can be interacted with.")]
    public float interactDistance = 2.5f;
    [Tooltip("Layers considered for the interaction raycast.")]
    public LayerMask interactMask = 1;

    /// <summary> Fired when the player starts looking at an interactable. Parameter: the focused GameObject. </summary>
    public UnityEvent<GameObject> onFocus;
    /// <summary> Fired when the player stops looking at the previously focused object. </summary>
    public UnityEvent onUnfocus;
    /// <summary> Fired when the player successfully interacts. Parameter: the interacted GameObject. </summary>
    public UnityEvent<GameObject> onInteract;
    /// <summary> Fired when the interact key is pressed but nothing interactable is in range. </summary>
    public UnityEvent onInteractFailed;

    private InputAction action;
    private GameObject focused;
    private IInteractable focusedInteractable;

    public override void Initialize () {
        if (interactAction != null) {
            action = interactAction.action;
            action.Enable();
        }
    }

    private void OnDestroy () {
        action?.Disable();
    }

    public override void OnUpdate () {
        UpdateFocus();

        if (action != null && action.WasPerformedThisFrame()) {
            if (focusedInteractable != null) {
                if (isDebugLog) Debug.Log($"AFPCInteraction: interacting with '{focused.name}'.");
                focusedInteractable.Interact(hero);
                onInteract?.Invoke(focused);
            } else {
                if (isDebugLog) Debug.Log("AFPCInteraction: interact pressed but nothing in range.");
                onInteractFailed?.Invoke();
            }
        }
    }

    private void UpdateFocus () {
        Camera cam = hero.overview.camera;
        if (!cam) return;

        GameObject hit = null;
        IInteractable interactable = null;

        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit info, interactDistance, interactMask)) {
            IInteractable candidate = info.collider.GetComponentInParent<IInteractable>();
            if (candidate != null) {
                hit = info.collider.gameObject;
                interactable = candidate;
            }
        }

        if (hit != focused) {
            if (focused != null) {
                if (isDebugLog) Debug.Log($"AFPCInteraction: unfocus '{focused.name}'.");
                onUnfocus?.Invoke();
            }
            focused = hit;
            focusedInteractable = interactable;
            if (focused != null) {
                if (isDebugLog) Debug.Log($"AFPCInteraction: focus '{focused.name}' — '{interactable.GetPrompt()}'.");
                onFocus?.Invoke(focused);
            }
        }
    }

    /// <summary> Returns the prompt string of the currently focused interactable, or empty if none. </summary>
    public string GetFocusedPrompt () => focusedInteractable?.GetPrompt() ?? string.Empty;

    /// <summary> Returns the currently focused GameObject, or null. </summary>
    public GameObject GetFocused () => focused;

    public override bool IsActive () => focused != null;
}
}
