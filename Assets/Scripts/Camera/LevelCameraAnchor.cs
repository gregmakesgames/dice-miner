using System;
using DiceMiner.Gameplay;
using DiceMiner.Gameplay.Map;
using UnityEngine;

namespace DiceMiner.Camera
{
    public sealed class LevelCameraAnchor : MonoBehaviour
    {
        [Tooltip("Extra offset applied on top of the computed level center.")]
        [SerializeField] private Vector2 offset = Vector2.zero;

        [Tooltip("If true, only tiles a player can break (IsHittable) influence the anchor.")]
        [SerializeField] private bool onlyHittableTiles = true;

        private bool _dirty;

        private IDisposable _destroysubscribtion;
    
        public void Init()
        {
            Unsubscribe();

            _destroysubscribtion = VisualMessageBroker.Subscribe(VisualMessageType.TileDestroyed, async (x, y, z) => OnTileDestroyed(x as Tile));

            _dirty = false;
            Recompute();
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }

        private void OnTileDestroyed(Tile tile)
        {
            _dirty = true;
        }

        private void LateUpdate()
        {
            if (!_dirty)
            {
                return;
            }

            _dirty = false;
            Recompute();
        }

        private void Recompute()
        {
            var minX = float.PositiveInfinity;
            var maxX = float.NegativeInfinity;
            var topY = float.NegativeInfinity;
            var hasAny = false;

            for (var i = FieldController.Instance.FieldEntities.Count - 1; i >= 0; i--)
            {
                var tile = FieldController.Instance.FieldEntities[i] as Tile;
                if (tile == null || tile.IsDestroyed)
                {
                    continue;
                }

                var p = tile.transform.position;
                if (p.x < minX)
                {
                    minX = p.x;
                }

                if (p.x > maxX)
                {
                    maxX = p.x;
                }

                if (p.y > topY)
                {
                    topY = p.y;
                }

                hasAny = true;
            }

            if (!hasAny)
            {
                return;
            }

            var centerX = (minX + maxX) * 0.5f;
            var pos = transform.position;
            transform.position = new Vector3(centerX + offset.x, topY + offset.y, pos.z);
        }

        private void Unsubscribe()
        {
            _destroysubscribtion?.Dispose();
            _destroysubscribtion = null;
        }
    }
}
