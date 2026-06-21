using UnityEngine;

namespace AFPC {

    /// <summary>
    /// Base class for Hero extensions. Add as a component and assign to Hero.extensions.
    /// </summary>
    public class AFPCExtension : MonoBehaviour {

        [HideInInspector] public Hero hero;
        public int order;
        public AFPCObjectInfo info = new AFPCObjectInfo();

        public virtual void Initialize () { }
        public virtual void OnUpdate () { }
        public virtual void OnFixedUpdate () { }
        public virtual void OnLateUpdate () { }

        /// <summary>
        /// Returns true while the extension is actively in use (e.g. sliding, gliding, hooked).
        /// Used by AFPCUI to highlight the extension row. Override in subclasses.
        /// </summary>
        public virtual bool IsActive () => false;
    }
}
