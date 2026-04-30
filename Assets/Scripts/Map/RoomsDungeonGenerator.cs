using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public sealed class RoomsDungeonGenerator : IDungeonGenerator
{
    private const string WallFamily = "wall";
    private const string FillerFamily = "filler";
    private const string OutlineFamily = "outline";
    private const string AnyFamily = "any";
    private const string PlacementUnderneath = "underneath";
    private const string PlacementReplace = "replace";
    private const string RolePlayerSpawn = "playerSpawn";
    private const string RoleExit = "exit";
    private const int PlayerSpawnClearRadius = 1; // 3x3 area: center +/- 1
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
    private ConfigEntity[,] fieldEntities;
    private bool[,] tileRemoved;
    private List<RectInt> rooms;
    private int gridWidth;
    private int gridHeight;
    private bool ready;

    private ConfigEntity wallTile;
    private ConfigEntity fillerTile;
    private ConfigEntity outlineTile;

    public RoomsDungeonGenerator(int roomCountMin, int roomCountMax, int roomSizeMin, int roomSizeMax)
    {
        this.roomCountMin = Mathf.Max(0, roomCountMin);
        this.roomCountMax = Mathf.Max(this.roomCountMin, roomCountMax);
        this.roomSizeMin = Mathf.Max(1, roomSizeMin);
        this.roomSizeMax = Mathf.Max(this.roomSizeMin, roomSizeMax);
    }

    public void Initialize(
        IReadOnlyList<ConfigEntity> tileConfigs,
        IReadOnlyList<ConfigEntity> fieldEntityConfigs,
        int width,
        int height,
        System.Random random)
    {
        ready = false;
        wallTile = null;
        fillerTile = null;
        outlineTile = null;
        grid = null;
        fieldEntities = null;
        tileRemoved = null;
        rooms = null;
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
                "RoomsDungeonGenerator: missing tile configs. " +
                $"Found walls={walls.Count}, fillers={fillers.Count}, outlines={outlines.Count}. " +
                "Need at least one TileType per family ('wall', 'filler', 'outline').");
            return;
        }

        wallTile = walls[rng.Next(0, walls.Count)];
        fillerTile = fillers[rng.Next(0, fillers.Count)];
        outlineTile = outlines[rng.Next(0, outlines.Count)];

        BuildLayout(rng);
        PlaceFieldEntities(rng, fieldEntityConfigs);
        ready = true;
    }

    public ConfigEntity PickTile(int x, int y)
    {
        if (!ready || grid == null || x < 0 || y < 0 || x >= gridWidth || y >= gridHeight)
        {
            return null;
        }

        if (tileRemoved != null && tileRemoved[y, x])
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

    public ConfigEntity PickFieldEntity(int x, int y)
    {
        if (!ready || fieldEntities == null || x < 0 || y < 0 || x >= gridWidth || y >= gridHeight)
        {
            return null;
        }

        return fieldEntities[x, y];
    }

    private void BuildLayout(System.Random rng)
    {
        grid = new Category[gridHeight, gridWidth];
        fieldEntities = new ConfigEntity[gridWidth, gridHeight];
        tileRemoved = new bool[gridHeight, gridWidth];
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

        rooms = PlaceRooms(rng, carved);
        ConnectRooms(rng, rooms, carved);
        StampWalls(carved);
    }

    private void PlaceFieldEntities(System.Random rng, IReadOnlyList<ConfigEntity> fieldEntityConfigs)
    {
        if (fieldEntityConfigs == null || fieldEntityConfigs.Count == 0 || fieldEntities == null)
        {
            return;
        }

        ConfigEntity playerSpawnConfig = fieldEntityConfigs.First(x => x.Id == "player_spawn");
        ConfigEntity exitConfig = fieldEntityConfigs.First(x => x.Id == "exit");

        int spawnRoomIndex = -1;
        spawnRoomIndex = PlacePlayerSpawn(rng, playerSpawnConfig);

        PlaceExit(exitConfig, spawnRoomIndex);

        List<Vector2Int> candidates = new();

        for (int i = 0; i < fieldEntityConfigs.Count; i++)
        {
            ConfigEntity entity = fieldEntityConfigs[i];
            if (entity == null)
            {
                continue;
            }

            string role = entity.GetString("role");
            if (!string.IsNullOrEmpty(role))
            {
                continue;
            }

            int spawnMin = Mathf.Max(0, entity.GetInt("spawnCountMin"));
            int spawnMax = Mathf.Max(spawnMin, entity.GetInt("spawnCountMax"));
            int count = spawnMin == spawnMax ? spawnMin : rng.Next(spawnMin, spawnMax + 1);
            if (count <= 0)
            {
                continue;
            }

            string allowedFamily = entity.GetString("allowedTileFamily");
            string placement = entity.GetString("placement");
            bool replace = string.Equals(placement, PlacementReplace, StringComparison.Ordinal);

            CollectCandidates(allowedFamily, candidates);
            if (candidates.Count == 0)
            {
                continue;
            }

            int placements = Mathf.Min(count, candidates.Count);
            for (int p = 0; p < placements; p++)
            {
                int pickIndex = rng.Next(p, candidates.Count);
                (candidates[p], candidates[pickIndex]) = (candidates[pickIndex], candidates[p]);

                Vector2Int cell = candidates[p];
                fieldEntities[cell.x, cell.y] = entity;
                if (replace)
                {
                    tileRemoved[cell.y, cell.x] = true;
                }
            }
        }
    }

    private int PlacePlayerSpawn(System.Random rng, ConfigEntity config)
    {
        if (rooms == null || rooms.Count == 0)
        {
            Debug.LogWarning(
                "RoomsDungeonGenerator: cannot place player spawn - no rooms were generated.");
            return -1;
        }

        int roomIndex = rng.Next(0, rooms.Count);
        RectInt room = rooms[roomIndex];
        Vector2Int center = RoomCenter(room);

        // Clamp the 3x3 clearing inside the room so we never punch holes through
        // the surrounding wall ring even if the room is exactly minimum size.
        int x0 = Mathf.Max(center.x - PlayerSpawnClearRadius, room.xMin);
        int y0 = Mathf.Max(center.y - PlayerSpawnClearRadius, room.yMin);
        int x1 = Mathf.Min(center.x + PlayerSpawnClearRadius, room.xMax - 1);
        int y1 = Mathf.Min(center.y + PlayerSpawnClearRadius, room.yMax - 1);

        for (int y = y0; y <= y1; y++)
        {
            for (int x = x0; x <= x1; x++)
            {
                tileRemoved[y, x] = true;
                fieldEntities[x, y] = null;
            }
        }

        fieldEntities[center.x, center.y] = config;
        return roomIndex;
    }

    private void PlaceExit(ConfigEntity config, int spawnRoomIndex)
    {
        if (rooms == null || rooms.Count == 0)
        {
            Debug.LogWarning(
                "RoomsDungeonGenerator: cannot place exit - no rooms were generated.");
            return;
        }

        if (rooms.Count < 2 && spawnRoomIndex >= 0)
        {
            Debug.LogWarning(
                "RoomsDungeonGenerator: cannot place exit at least 1 room away - only one room was generated.");
            return;
        }

        // Pick the room with the greatest chain distance from the spawn room.
        // Rooms are connected sequentially by ConnectRooms, so |i - spawn| is a
        // reasonable proxy for "rooms between us".
        int exitRoomIndex = -1;
        int bestDistance = -1;
        for (int i = 0; i < rooms.Count; i++)
        {
            if (i == spawnRoomIndex)
            {
                continue;
            }

            int distance = spawnRoomIndex >= 0 ? Mathf.Abs(i - spawnRoomIndex) : i;
            if (distance > bestDistance)
            {
                bestDistance = distance;
                exitRoomIndex = i;
            }
        }

        if (exitRoomIndex < 0)
        {
            return;
        }

        RectInt room = rooms[exitRoomIndex];
        Vector2Int center = RoomCenter(room);

        
        fieldEntities[center.x, center.y] = config;
    }

    private void CollectCandidates(string allowedFamily, List<Vector2Int> output)
    {
        output.Clear();

        bool any = string.IsNullOrEmpty(allowedFamily) ||
                   string.Equals(allowedFamily, AnyFamily, StringComparison.Ordinal);
        bool allowFiller = any || string.Equals(allowedFamily, FillerFamily, StringComparison.Ordinal);
        bool allowWall = any || string.Equals(allowedFamily, WallFamily, StringComparison.Ordinal);
        bool allowOutline = any || string.Equals(allowedFamily, OutlineFamily, StringComparison.Ordinal);

        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                if (fieldEntities[x, y] != null || tileRemoved[y, x])
                {
                    continue;
                }

                bool match = grid[y, x] switch
                {
                    Category.Filler => allowFiller,
                    Category.Wall => allowWall,
                    Category.Outline => allowOutline,
                    _ => false,
                };

                if (match)
                {
                    output.Add(new Vector2Int(x, y));
                }
            }
        }
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
