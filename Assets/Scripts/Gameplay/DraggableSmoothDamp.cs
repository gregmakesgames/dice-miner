using GrishaGuWorkshop;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DiceMiner.Gameplay
{
    public class DraggableSmoothDamp : GameObjectBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        [SerializeField] private MoveableBase moveable;
        public bool IsDragging { get; private set; }

        private Camera _mainCamera;

        private Vector2 _origin;
        private Vector3 _offset;

        protected override void ManagedInitialize()
        {
            _mainCamera = Camera.main;
            moveable.targetPosition = transform.position;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            IsDragging = true;
            _origin = moveable.targetPosition;
            _offset = transform.position - _mainCamera.ScreenToWorldPoint(new Vector3(eventData.position.x, 
                                                                                         eventData.position.y, 
                                                                                         _mainCamera.WorldToScreenPoint(transform.position).z));
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            IsDragging = false;
            moveable.targetPosition = _origin;
        }

        public void OnDrag(PointerEventData eventData)
        {
            Vector3 cursorPoint = new Vector3(eventData.position.x, eventData.position.y, _mainCamera.WorldToScreenPoint(transform.position).z);
            Vector3 cursorPosition = _mainCamera.ScreenToWorldPoint(cursorPoint) + _offset;
            cursorPosition.z = transform.position.z;
            
            moveable.targetPosition = cursorPosition;
        }
    }
}
