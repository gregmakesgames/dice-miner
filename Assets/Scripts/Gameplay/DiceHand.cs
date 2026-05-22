using System.Collections.Generic;
using GrishaGuWorkshop;
using UnityEngine;

namespace DiceMiner.Gameplay
{
    public class DiceHand : DropZone
    {
        [SerializeField] private float spacing;
        private List<DraggableSmoothDamp> draggables = new();

        public override bool DropMoveable(DraggableSmoothDamp draggable)
        {
            if (draggable.GetComponent<DiceEntity>() == null) return false;
            draggables.Add(draggable);
            UpdatePositions();
            return true;
        }

        public override void RemoveMoveable(DraggableSmoothDamp draggable)
        {
            draggables.Remove(draggable);
            UpdatePositions();
        }

        protected override void ManagedUpdate()
        {
            base.ManagedUpdate();
            UpdatePositions();
        }

        private void UpdatePositions()
        {
            if (draggables.Count == 0) return;

            var center = (Vector2)transform.position;
            var totalSpan = (draggables.Count - 1) * spacing;
            var startX = center.x - totalSpan * 0.5f;

            for (var i = 0; i < draggables.Count; i++)
            {
                var draggable = draggables[i];
                if (draggable == null) continue;

                if (draggable.IsDragging) continue;

                draggable.moveable.targetPosition = new Vector2(startX + i * spacing, center.y);
            }
        }
    }
}