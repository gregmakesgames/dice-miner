using System.Collections.Generic;
using DiceMiner.Gameplay.Data;
using DiceMiner.Gameplay.Map;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DiceMiner.Gameplay.UI
{
    public class DiceHolder : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Header("Follow")]
        [SerializeField] private float followSmoothTime = 0.18f;

        [Header("Float (local offset on top of follow)")]
        [SerializeField] private float floatAmplitude = 4f;
        [SerializeField] private float floatSpeed = 1.1f;
        [SerializeField] private float floatPhaseSpread = 0.37f;

        [Header("Spawn fall")]
        [SerializeField] private float spawnFallFromAbove = 720f;
        [SerializeField] private float spawnFallDuration = 0.5f;
        [SerializeField] private float spawnFallStagger = 0.15f;
        [SerializeField] private Ease spawnFallEase = Ease.OutQuad;

        private DiceGameplayData _dice;
        private RectTransform _anchor;
        private RectTransform _rectTransform;
        private RectTransform _parentRect;
        private GameplayDiceController _owner;

        private Vector2 _followLocal;
        private Vector2 _followVelocity;
        private float _floatPhase;

        private Tweener _spawnFallTween;
        private float _spawnFallYOffset;
        private bool _spawnFallActive;

        private CanvasGroup _canvasGroup;
        private bool _dragging;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        public void Init(DiceGameplayData dice, RectTransform anchor, GameplayDiceController owner, int spawnIndex = 0)
        {
            KillSpawnFallTween();

            _dice = dice;
            _anchor = anchor;
            _owner = owner;
            _rectTransform = (RectTransform)transform;
            _parentRect = _rectTransform.parent as RectTransform;
            _floatPhase = Random.Range(0f, Mathf.PI * 2f);

            ClearSpawnedDice();
            if (_dice?.Type != null)
            {
                var created = DiceFactory.CreateDice(_dice.Type);
                if (created != null)
                {
                    var instanceRoot = created.transform;
                    while (instanceRoot.parent != null)
                        instanceRoot = instanceRoot.parent;

                    instanceRoot.SetParent(_rectTransform, false);
                    if (instanceRoot is RectTransform instanceRect)
                    {
                        instanceRect.anchorMin = new Vector2(0.5f, 0.5f);
                        instanceRect.anchorMax = new Vector2(0.5f, 0.5f);
                        instanceRect.anchoredPosition = Vector2.zero;
                        instanceRect.localRotation = Quaternion.identity;
                        instanceRect.localScale = Vector3.one;
                    }
                    else
                    {
                        instanceRoot.localPosition = Vector3.zero;
                        instanceRoot.localRotation = Quaternion.identity;
                        instanceRoot.localScale = Vector3.one;
                    }

                    created.Init(Vector2Int.zero, Random.Range(1, 7));
                }
            }

            if (_anchor != null && _parentRect != null)
                _followLocal = GetAnchorLocalInParent();
            else
                _followLocal = _rectTransform.anchoredPosition;

            _followVelocity = Vector2.zero;

            _spawnFallYOffset = spawnFallFromAbove;
            _spawnFallActive = true;
            _spawnFallTween = DOTween.To(() => _spawnFallYOffset, y => _spawnFallYOffset = y, 0f, spawnFallDuration)
                .SetEase(spawnFallEase)
                .SetDelay(spawnIndex * spawnFallStagger)
                .OnComplete(() =>
                {
                    _spawnFallActive = false;
                    _spawnFallTween = null;
                });

            ApplyAnchoredPosition();
        }

        private void OnDestroy()
        {
            KillSpawnFallTween();
        }

        private void KillSpawnFallTween()
        {
            _spawnFallTween?.Kill();
            _spawnFallTween = null;
            _spawnFallActive = false;
            _spawnFallYOffset = 0f;
        }

        private void ClearSpawnedDice()
        {
            var holder = (RectTransform)transform;
            for (var i = holder.childCount - 1; i >= 0; i--)
            {
                var child = holder.GetChild(i);
                if (child.GetComponentInChildren<Dice>(true) != null)
                    Destroy(child.gameObject);
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            Debug.Log("OnBeginDrag");
            if (_owner == null || _parentRect == null)
                return;

            KillSpawnFallTween();
            _dragging = true;
            _canvasGroup.blocksRaycasts = false;
            _followVelocity = Vector2.zero;
        }

        public void OnDrag(PointerEventData eventData)
        {
            Debug.Log("OnDrag");
            if (!_dragging || _parentRect == null)
                return;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _parentRect, eventData.position, eventData.pressEventCamera, out var localPoint))
            {
                _followLocal = localPoint;
                ApplyAnchoredPosition();
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            Debug.Log("OnEndDrag");
            if (!_dragging)
                return;

            _dragging = false;
            if (_canvasGroup != null)
                _canvasGroup.blocksRaycasts = true;

            if (_owner == null || !TryResolveDropCell(out var cell))
                return;

            if (_owner.TryCommitDropFromHolder(this, cell))
                return;

            _followVelocity = Vector2.zero;
        }
        
        private bool TryResolveDropCell(out Vector2Int cell)
        {
            cell = default;
            if (_parentRect == null)
                return false;

            var worldPoint = _parentRect.TransformPoint(new Vector3(_followLocal.x, _followLocal.y, 0f));
            return MapHelper.TryWorldPointToGridCell(worldPoint, out cell);
        }
        
        private void LateUpdate()
        {
            if (_parentRect == null)
                return;

            if (_dragging)
            {
                ApplyAnchoredPosition();
                return;
            }

            if (_anchor == null)
                return;

            var anchorLocal = GetAnchorLocalInParent();
            _followLocal = Vector2.SmoothDamp(_followLocal, anchorLocal, ref _followVelocity, followSmoothTime);

            _floatPhase += Time.deltaTime * floatSpeed;
            ApplyAnchoredPosition();
        }

        private void ApplyAnchoredPosition()
        {
            var floatOffset = _spawnFallActive || _dragging
                ? Vector2.zero
                : new Vector2(
                    Mathf.Sin(_floatPhase) * floatAmplitude,
                    Mathf.Cos(_floatPhase * (1f + floatPhaseSpread)) * floatAmplitude);

            _rectTransform.anchoredPosition = _followLocal + floatOffset + new Vector2(0f, _spawnFallYOffset);
        }

        private Vector2 GetAnchorLocalInParent()
        {
            var canvas = _rectTransform.GetComponentInParent<Canvas>();
            Camera cam = null;
            if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceCamera)
                cam = canvas.worldCamera != null ? canvas.worldCamera : Camera.main;

            var screenPoint = RectTransformUtility.WorldToScreenPoint(cam, _anchor.position);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_parentRect, screenPoint, cam, out var localPoint);
            return localPoint;
        }
    }
}
