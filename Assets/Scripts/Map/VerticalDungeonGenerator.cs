using System;
using System.Collections.Generic;
using GameData;
using Map;
using UnityEngine;

public sealed class VerticalDungeonGenerator : IDungeonGenerator
{
    private const string FillerFamily = "filler";
    private const string OutlineFamily = "outline";
    private const string AnyFamily = "any";

    // Difficulty 1 produces a 5x100 vertical strip; each extra level
    // widens the shaft slightly and lengthens it considerably.
    private const int BaseWidth = 7;
    private const int BaseHeight = 100;
    private const int WidthStep = 2;
    private const int HeightStep = 50;

    private const int ExitRowOffset = 1;          // distance from bottom outline

    private readonly int difficulty;

    private TileTypeData[,] tiles;
    private FieldEntityData[,] fieldEntities;
    private bool[,] tileRemoved;
    private int gridWidth;
    private int gridHeight;
    private bool ready;

    public int Difficulty => difficulty;

    public VerticalDungeonGenerator(int difficulty)
    {
        this.difficulty = Mathf.Max(1, difficulty);
    }

    public static int GetWidthForDifficulty(int difficulty)
    {
        return BaseWidth + (Mathf.Max(1, difficulty) - 1) * WidthStep;
    }

    public static int GetHeightForDifficulty(int difficulty)
    {
        return BaseHeight + (Mathf.Max(1, difficulty) - 1) * HeightStep;
    }

    public void Initialize(
        IReadOnlyList<TileTypeData> tileConfigs,
        IReadOnlyList<FieldEntityData> fieldEntityConfigs,
        int width,
        int height,
        System.Random random)
    {
        ready = false;
        tiles = null;
        fieldEntities = null;
        tileRemoved = null;

        // Vertical generator owns its dimensions; the requested width/height
        // are treated as a hint and clamped to what difficulty produces.
        gridWidth = GetWidthForDifficulty(difficulty);
        gridHeight = GetHeightForDifficulty(difficulty);

        if (tileConfigs == null || tileConfigs.Count == 0 || gridWidth <= 0 || gridHeight <= 0)
        {
            return;
        }

        var rng = random ?? new System.Random();

        var fillers = new List<TileTypeData>();
        var outlines = new List<TileTypeData>();

        for (var i = 0; i < tileConfigs.Count; i++)
        {
            var entry = tileConfigs[i];
            if (entry == null)
            {
                continue;
            }

            var family = entry.Family;
            if (string.Equals(family, FillerFamily, StringComparison.Ordinal))
            {
                fillers.Add(entry);
            }
            else if (string.Equals(family, OutlineFamily, StringComparison.Ordinal))
            {
                outlines.Add(entry);
            }
        }

        if (fillers.Count == 0 || outlines.Count == 0)
        {
            Debug.LogWarning(
                "VerticalDungeonGenerator: missing tile configs. " +
                $"Found fillers={fillers.Count}, outlines={outlines.Count}. " +
                "Need at least one TileType per family ('filler', 'outline').");
            return;
        }

        BuildLayout(rng, fillers, outlines);
        PlaceFieldEntities(rng, fieldEntityConfigs);
        ready = true;
    }

    public TileTypeData PickTile(int x, int y)
    {
        if (!ready || tiles == null || x < 0 || y < 0 || x >= gridWidth || y >= gridHeight)
        {
            return null;
        }

        if (tileRemoved != null && tileRemoved[y, x])
        {
            return null;
        }

        return tiles[y, x];
    }

    public FieldEntityData PickFieldEntity(int x, int y)
    {
        if (!ready || fieldEntities == null || x < 0 || y < 0 || x >= gridWidth || y >= gridHeight)
        {
            return null;
        }

        return fieldEntities[x, y];
    }

    private void BuildLayout(System.Random rng, List<TileTypeData> fillers, List<TileTypeData> outlines)
    {
        tiles = new TileTypeData[gridHeight, gridWidth];
        fieldEntities = new FieldEntityData[gridWidth, gridHeight];
        tileRemoved = new bool[gridHeight, gridWidth];

        // Pick a single outline tile so the perimeter looks consistent,
        // but vary the filler per cell to give the shaft visual variety.
        var outlineTile = outlines[rng.Next(0, outlines.Count)];

        for (var y = 0; y < gridHeight; y++)
        {
            for (var x = 0; x < gridWidth; x++)
            {
                var isPerimeter = x == 0 || y == 0 || x == gridWidth - 1 || y == gridHeight - 1;
                tiles[y, x] = isPerimeter
                    ? outlineTile
                    : fillers[rng.Next(0, fillers.Count)];
            }
        }
    }

    private void PlaceFieldEntities(System.Random rng, IReadOnlyList<FieldEntityData> fieldEntityConfigs)
    {
        if (fieldEntityConfigs == null || fieldEntityConfigs.Count == 0)
        {
            return;
        }

        FieldEntityData exitConfig = null;

        for (var i = 0; i < fieldEntityConfigs.Count; i++)
        {
            var entry = fieldEntityConfigs[i];
            if (entry == null)
            {
                continue;
            }

            if (entry.Id == MapEntitiesIds.EXIT)
            {
                exitConfig = entry;
            }
        }

        var exitPos = PlaceExit(exitConfig);

        var candidates = new List<Vector2Int>();

        for (var i = 0; i < fieldEntityConfigs.Count; i++)
        {
            var entity = fieldEntityConfigs[i];
            if (entity == null || entity.Id == MapEntitiesIds.PLAYER_SPAWN || entity.Id == MapEntitiesIds.EXIT)
            {
                continue;
            }

            var spawnMin = Mathf.Max(0, entity.SpawnCountMin);
            var spawnMax = Mathf.Max(spawnMin, entity.SpawnCountMax);
            var count = spawnMin == spawnMax ? spawnMin : rng.Next(spawnMin, spawnMax + 1);
            if (count <= 0)
            {
                continue;
            }

            CollectCandidates(entity.AllowedTileFamily, exitPos, candidates);
            if (candidates.Count == 0)
            {
                continue;
            }

            var placements = Mathf.Min(count, candidates.Count);
            for (var p = 0; p < placements; p++)
            {
                var pickIndex = rng.Next(p, candidates.Count);
                (candidates[p], candidates[pickIndex]) = (candidates[pickIndex], candidates[p]);

                var cell = candidates[p];
                fieldEntities[cell.x, cell.y] = entity;
                if (entity.IsReplace)
                {
                    tileRemoved[cell.y, cell.x] = true;
                }
            }
        }
    }

    private Vector2Int PlaceExit(FieldEntityData config)
    {
        // Exit sits near the bottom of the shaft so the player digs down to reach it.
        var centerX = gridWidth / 2;
        var exitY = Mathf.Min(gridHeight - 2, ExitRowOffset);
        var exit = new Vector2Int(centerX, exitY);

        if (config != null)
        {
            fieldEntities[exit.x, exit.y] = config;
        }

        return exit;
    }

    private void CollectCandidates(
        string allowedFamily,
        Vector2Int exitPos,
        List<Vector2Int> output)
    {
        output.Clear();

        var any = string.IsNullOrEmpty(allowedFamily) ||
                  string.Equals(allowedFamily, AnyFamily, StringComparison.Ordinal);

        for (var y = 0; y < gridHeight; y++)
        {
            for (var x = 0; x < gridWidth; x++)
            {
                if (fieldEntities[x, y] != null || tileRemoved[y, x])
                {
                    continue;
                }

                if (x == exitPos.x && y == exitPos.y)
                {
                    continue;
                }

                var tile = tiles[y, x];
                if (tile == null)
                {
                    continue;
                }

                var match = any || string.Equals(tile.Family, allowedFamily, StringComparison.Ordinal);
                if (match)
                {
                    output.Add(new Vector2Int(x, y));
                }
            }
        }
    }
}
