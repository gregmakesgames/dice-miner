using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class RoomsTileDistribution : ITileDistribution
{
    private const string WallFamily = "wall";
    private const string FillerFamily = "filler";
    private const string OutlineFamily = "outline";
    private const int RoomPlacementRetries = 32;

    private enum Category
    {
        Filler,
        Wall,
        Outline
    }

    private readonly int roomCountMin;
    private readonly int roomCountMax;
    private readonly int roomSizeMin;
    private readonly int roomSizeMax;

    private Category[,] grid;
    private int gridWidth;
    private int gridHeight;
    private bool ready;

    private ConfigEntity wallTile;
    private ConfigEntity fillerTile;
    private ConfigEntity outlineTile;

    public RoomsTileDistribution(int roomCountMin, int roomCountMax, int roomSizeMin, int roomSizeMax)
    {
        this.roomCountMin = Mathf.Max(0, roomCountMin);
        this.roomCountMax = Mathf.Max(this.roomCountMin, roomCountMax);
        this.roomSizeMin = Mathf.Max(1, roomSizeMin);
        this.roomSizeMax = Mathf.Max(this.roomSizeMin, roomSizeMax);
    }

    public void Initialize(IReadOnlyList<ConfigEntity> tileConfigs, int width, int height, System.Random random)
    {
        ready = false;
        wallTile = null;
        fillerTile = null;
        outlineTile = null;
        grid = null;
        gridWidth = Mathf.Max(0, width);
        gridHeight = Mathf.Max(0, height);

        if (tileConfigs == null || tileConfigs.Count == 0 || gridWidth == 0 || gridHeight == 0)
        {
            return;
        }

        System.Random rng = random ?? new System.Random();

        List<ConfigEntity> walls = new();
        List<ConfigEntity> fillers = new();
        List<ConfigEntity> outlines = new();

        for (int i = 0; i < tileConfigs.Count; i++)
        {
            ConfigEntity entry = tileConfigs[i];
            if (entry == null)
            {
                continue;
            }

            string family = entry.GetString("family");
            if (string.Equals(family, WallFamily, StringComparison.Ordinal))
            {
                walls.Add(entry);
            }
            else if (string.Equals(family, FillerFamily, StringComparison.Ordinal))
            {
                fillers.Add(entry);
            }
            else if (string.Equals(family, OutlineFamily, StringComparison.Ordinal))
            {
                outlines.Add(entry);
            }
        }

        if (walls.Count == 0 || fillers.Count == 0 || outlines.Count == 0)
        {
            Debug.LogWarning(
                "RoomsTileDistribution: missing tile configs. " +
                $"Found walls={walls.Count}, fillers={fillers.Count}, outlines={outlines.Count}. " +
                "Need at least one TileType per family ('wall', 'filler', 'outline').");
            return;
        }

        wallTile = walls[rng.Next(0, walls.Count)];
        fillerTile = fillers[rng.Next(0, fillers.Count)];
        outlineTile = outlines[rng.Next(0, outlines.Count)];

        BuildLayout(rng);
        ready = true;
    }

    public ConfigEntity PickTile(int x, int y)
    {
        if (!ready || grid == null || x < 0 || y < 0 || x >= gridWidth || y >= gridHeight)
        {
            return null;
        }

        return grid[y, x] switch
        {
            Category.Wall => wallTile,
            Category.Outline => outlineTile,
            _ => fillerTile,
        };
    }

    private void BuildLayout(System.Random rng)
    {
        grid = new Category[gridHeight, gridWidth];
        bool[,] carved = new bool[gridHeight, gridWidth];

        // Outer ring -> outline.
        for (int x = 0; x < gridWidth; x++)
        {
            grid[0, x] = Category.Outline;
            grid[gridHeight - 1, x] = Category.Outline;
        }

        for (int y = 0; y < gridHeight; y++)
        {
            grid[y, 0] = Category.Outline;
            grid[y, gridWidth - 1] = Category.Outline;
        }

        List<RectInt> placedRooms = PlaceRooms(rng, carved);
        ConnectRooms(rng, placedRooms, carved);
        StampWalls(carved);
    }

    private List<RectInt> PlaceRooms(System.Random rng, bool[,] carved)
    {
        List<RectInt> rooms = new();

        int innerMinX = 2;
        int innerMinY = 2;
        int innerMaxX = gridWidth - 3;
        int innerMaxY = gridHeight - 3;

        if (innerMinX > innerMaxX || innerMinY > innerMaxY)
        {
            return rooms;
        }

        int targetCount = roomCountMin == roomCountMax
            ? roomCountMin
            : rng.Next(roomCountMin, roomCountMax + 1);

        int innerWidth = innerMaxX - innerMinX + 1;
        int innerHeight = innerMaxY - innerMinY + 1;
        int maxRoomWidth = Mathf.Min(roomSizeMax, innerWidth);
        int maxRoomHeight = Mathf.Min(roomSizeMax, innerHeight);
        int minRoomWidth = Mathf.Min(roomSizeMin, maxRoomWidth);
        int minRoomHeight = Mathf.Min(roomSizeMin, maxRoomHeight);

        if (maxRoomWidth <= 0 || maxRoomHeight <= 0)
        {
            return rooms;
        }

        for (int i = 0; i < targetCount; i++)
        {
            for (int attempt = 0; attempt < RoomPlacementRetries; attempt++)
            {
                int size = minRoomWidth == maxRoomWidth
                    ? minRoomWidth
                    : rng.Next(minRoomWidth, maxRoomWidth + 1);
                size = Mathf.Min(size, maxRoomHeight);
                if (size <= 0)
                {
                    break;
                }

                int x0 = rng.Next(innerMinX, innerMaxX - size + 2);
                int y0 = rng.Next(innerMinY, innerMaxY - size + 2);
                RectInt candidate = new(x0, y0, size, size);

                if (OverlapsExisting(candidate, rooms))
                {
                    continue;
                }

                CarveRect(candidate, carved);
                rooms.Add(candidate);
                break;
            }
        }

        return rooms;
    }

    private static bool OverlapsExisting(RectInt candidate, List<RectInt> rooms)
    {
        // Inflate by 1 cell so adjacent rooms keep at least a one-cell gap for their wall ring.
        RectInt inflated = new(candidate.xMin - 1, candidate.yMin - 1, candidate.width + 2, candidate.height + 2);
        for (int i = 0; i < rooms.Count; i++)
        {
            RectInt other = rooms[i];
            if (inflated.xMax > other.xMin && other.xMax > inflated.xMin &&
                inflated.yMax > other.yMin && other.yMax > inflated.yMin)
            {
                return true;
            }
        }

        return false;
    }

    private static void CarveRect(RectInt rect, bool[,] carved)
    {
        for (int y = rect.yMin; y < rect.yMax; y++)
        {
            for (int x = rect.xMin; x < rect.xMax; x++)
            {
                carved[y, x] = true;
            }
        }
    }

    private void ConnectRooms(System.Random rng, List<RectInt> rooms, bool[,] carved)
    {
        if (rooms.Count < 2)
        {
            return;
        }

        for (int i = 1; i < rooms.Count; i++)
        {
            Vector2Int a = RoomCenter(rooms[i - 1]);
            Vector2Int b = RoomCenter(rooms[i]);
            CarveCorridor(a, b, rng.Next(0, 2) == 0, carved);
        }
    }

    private static Vector2Int RoomCenter(RectInt rect)
    {
        return new Vector2Int(rect.xMin + rect.width / 2, rect.yMin + rect.height / 2);
    }

    private void CarveCorridor(Vector2Int from, Vector2Int to, bool horizontalFirst, bool[,] carved)
    {
        int x = from.x;
        int y = from.y;

        if (horizontalFirst)
        {
            CarveHorizontal(y, x, to.x, carved);
            CarveVertical(to.x, y, to.y, carved);
        }
        else
        {
            CarveVertical(x, y, to.y, carved);
            CarveHorizontal(to.y, x, to.x, carved);
        }
    }

    private void CarveHorizontal(int y, int xStart, int xEnd, bool[,] carved)
    {
        if (y < 1 || y > gridHeight - 2)
        {
            return;
        }

        int min = Mathf.Min(xStart, xEnd);
        int max = Mathf.Max(xStart, xEnd);
        min = Mathf.Max(min, 1);
        max = Mathf.Min(max, gridWidth - 2);

        for (int x = min; x <= max; x++)
        {
            carved[y, x] = true;
        }
    }

    private void CarveVertical(int x, int yStart, int yEnd, bool[,] carved)
    {
        if (x < 1 || x > gridWidth - 2)
        {
            return;
        }

        int min = Mathf.Min(yStart, yEnd);
        int max = Mathf.Max(yStart, yEnd);
        min = Mathf.Max(min, 1);
        max = Mathf.Min(max, gridHeight - 2);

        for (int y = min; y <= max; y++)
        {
            carved[y, x] = true;
        }
    }

    private void StampWalls(bool[,] carved)
    {
        // Every non-outline, non-carved cell becomes a wall so the area
        // outside rooms and corridors is solid.
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                if (grid[y, x] == Category.Outline || carved[y, x])
                {
                    continue;
                }

                grid[y, x] = Category.Wall;
            }
        }
    }
}
