using UnityEngine;

namespace AFPC {

    public class AFPCInteractable : MonoBehaviour, IInteractable {
        
        public void Interact (Hero hero) {

        }

        public string GetPrompt () => gameObject.name;   // shown in UI hint
    }
}
