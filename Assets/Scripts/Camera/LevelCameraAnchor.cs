using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class LevelCameraAnchor : MonoBehaviour
{
    [Tooltip("Extra offset applied on top of the computed level center.")]
    [SerializeField] private Vector2 offset = Vector2.zero;

    [Tooltip("If true, only tiles a player can break (IsHittable) influence the anchor.")]
    [SerializeField] private bool onlyHittableTiles = true;

    private readonly List<Tile> trackedTiles = new();
    private bool dirty;

    public void SetTiles(IReadOnlyList<Tile> tiles)
    {
        UnsubscribeAll();
        trackedTiles.Clear();

        if (tiles != null)
        {
            for (var i = 0; i < tiles.Count; i++)
            {
                var tile = tiles[i];
                if (tile == null)
                {
                    continue;
                }

                if (onlyHittableTiles && !tile.IsHittable)
                {
                    continue;
                }

                trackedTiles.Add(tile);
                tile.Destroyed += OnTileDestroyed;
            }
        }

        dirty = false;
        Recompute();
    }

    public void Clear()
    {
        UnsubscribeAll();
        trackedTiles.Clear();
    }

    private void OnDestroy()
    {
        UnsubscribeAll();
    }

    private void OnTileDestroyed(Tile tile)
    {
        if (tile != null)
        {
            tile.Destroyed -= OnTileDestroyed;
        }

        // The list is compacted lazily during Recompute so we don't pay the
        // cost on every individual destruction within a frame.
        dirty = true;
    }

    private void LateUpdate()
    {
        if (!dirty)
        {
            return;
        }

        dirty = false;
        Recompute();
    }

    private void Recompute()
    {
        var minX = float.PositiveInfinity;
        var maxX = float.NegativeInfinity;
        var topY = float.NegativeInfinity;
        var hasAny = false;

        for (var i = trackedTiles.Count - 1; i >= 0; i--)
        {
            var tile = trackedTiles[i];
            if (tile == null || tile.IsDestroyed)
            {
                trackedTiles.RemoveAt(i);
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

    private void UnsubscribeAll()
    {
        for (var i = 0; i < trackedTiles.Count; i++)
        {
            var tile = trackedTiles[i];
            if (tile != null)
            {
                tile.Destroyed -= OnTileDestroyed;
            }
        }
    }
}
