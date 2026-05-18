using UnityEngine;

namespace GrishaGuWorkshop
{
    public class GameObjectBehaviourBase : MonoBehaviour
    {
        protected virtual void Update() { }
        protected virtual void FixedUpdate() { }
        protected virtual void LateUpdate() { }
        protected virtual void Awake() { }
    }
    
    public class GameObjectBehaviour : GameObjectBehaviourBase
    {
        public static readonly ToggleBlocker PauseAll = new ToggleBlocker();
        
        public virtual bool UpdateWhenPaused { get { return false; } }

        private bool initialized = false;

        public void Initialize()
        {
            if (!initialized)
            {
                initialized = true;
                ManagedInitialize();
            }
        }

        
        protected sealed override void Update()
        {
            if (CanUpdate())
            {
                ManagedUpdate();
            }
        }
        
        protected sealed override void FixedUpdate()
        {
            if (CanUpdate())
            {
                ManagedFixedUpdate();
            }
        }
        
        protected sealed override void LateUpdate()
        {
            if (CanUpdate())
            {
                ManagedLateUpdate();
            }
        }
        
        protected sealed override void Awake()
        {
            Initialize();
        }
        

        private bool CanUpdate()
        {
            return UpdateWhenPaused || !PauseAll.Blocked;
        }
        
        protected virtual void ManagedUpdate() { }
        protected virtual void ManagedFixedUpdate() { }
        protected virtual void ManagedLateUpdate() { }
        protected virtual void ManagedInitialize() { }
    }
}