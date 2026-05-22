using GrishaGuWorkshop;
using UnityEngine;

namespace DiceMiner.Gameplay
{
    public class DropZone : GameObjectBehaviour
    {
        public virtual bool DropMoveable(DraggableSmoothDamp draggable)
        {
            draggable.moveable.targetPosition = transform.position;
            return true;
        }

        public virtual void RemoveMoveable(DraggableSmoothDamp draggable)
        {
            
        }
    }
}