namespace AFPC {

/// <summary>
/// Implement this interface on any MonoBehaviour to make it interactable via AFPCInteraction.
/// </summary>
public interface IInteractable {

    /// <summary> Called when the player presses the interact key while looking at this object. </summary>
    void Interact (Hero hero);

    /// <summary> Short label shown in UI hints (e.g. "Open", "Pick up", "Talk"). Return empty to suppress. </summary>
    string GetPrompt ();
}
}
