using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using Object = UnityEngine.Object;

namespace DiceMiner.Gameplay.Map
{
    public sealed class TilePool : IDisposable
    {
        private static TilePool _instance = null;
        
        private readonly Tile prefab;
        private readonly Transform parent;
        private readonly ObjectPool<Tile> pool;
        private readonly List<Tile> activeTiles = new();

        public TilePool(Tile prefab, Transform parent, int defaultCapacity = 256, int maxSize = 4096)
        {
            _instance = this;
            if (prefab == null)
            {
                throw new ArgumentNullException(nameof(prefab));
            }

            this.prefab = prefab;
            this.parent = parent;

            pool = new ObjectPool<Tile>(
                createFunc: CreateTile,
                actionOnGet: OnGet,
                actionOnRelease: OnRelease,
                actionOnDestroy: OnDestroyTile,
                collectionCheck: true,
                defaultCapacity: Mathf.Max(0, defaultCapacity),
                maxSize: Mathf.Max(1, maxSize));
        }

        public IReadOnlyList<Tile> ActiveTiles => activeTiles;

        public static Tile Get()
        {
            return _instance.GetInner();
        }

        private Tile GetInner()
        {
            Tile tile = pool.Get();
            activeTiles.Add(tile);
            return tile;
        }

        public static void Release(Tile tile)
        {
            _instance.ReleaseInner(tile);
        }

        private void ReleaseInner(Tile tile)
        {
            if (tile == null)
            {
                return;
            }

            if (!activeTiles.Remove(tile))
            {
                Object.Destroy(tile.gameObject);
                return;
            }

            pool.Release(tile);
        }

        public void ReleaseAll()
        {
            for (int i = activeTiles.Count - 1; i >= 0; i--)
            {
                Tile tile = activeTiles[i];
                if (tile != null)
                {
                    pool.Release(tile);
                }
            }

            activeTiles.Clear();
        }

        public void Dispose()
        {
            activeTiles.Clear();
            pool.Dispose();
        }

#region Pool functions
        
        private Tile CreateTile()
        {
            return UnityEngine.Object.Instantiate(prefab, parent);
        }

        private static void OnGet(Tile tile)
        {
            tile.gameObject.SetActive(true);
        }

        private void OnRelease(Tile tile)
        {
            tile.Clear();
            tile.gameObject.SetActive(false);

            if (parent != null && tile.transform.parent != parent)
            {
                tile.transform.SetParent(parent, false);
            }
        }

        private static void OnDestroyTile(Tile tile)
        {
            if (tile == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                UnityEngine.Object.Destroy(tile.gameObject);
            }
            else
            {
                UnityEngine.Object.DestroyImmediate(tile.gameObject);
            }
        }
        
#endregion
    }
}
