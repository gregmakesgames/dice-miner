using System;
using System.Collections.Generic;
using GameData;
using UnityEngine;

public sealed class MapGenerator : MonoBehaviour
{
    public enum DungeonMode
    {
        Rooms,
        Vertical,
    }

    [Header("Generator")]
    [SerializeField] private DungeonMode mode = DungeonMode.Rooms;

    [Header("Map Size")]
    [SerializeField] private int width = 30;
    [SerializeField] private int height = 30;
    [SerializeField] private float tileSize = 1f;

    [Header("Rooms")]
    [SerializeField] private int roomCountMin = 3;
    [SerializeField] private int roomCountMax = 6;
    [SerializeField] private int roomSizeMin = 4;
    [SerializeField] private int roomSizeMax = 8;

    [Header("Random")]
    [SerializeField] private bool useRandomSeed = true;
    [SerializeField] private int seed = 12345;

    [Header("Hierarchy")]
    [SerializeField] private bool generateOnStart = true;
    [SerializeField] private Transform tilesParent;
    [SerializeField] private Tile tilePrefab;
    [SerializeField] private FieldEntity fieldEntityPrefab;

    [Header("Player")]
    [SerializeField] private Transform player;
    [SerializeField] private CameraFollow cameraFollow;

    [Header("Pooling")]
    [SerializeField] private int poolDefaultCapacity = 256;
    [SerializeField] private int poolMaxSize = 4096;
    [SerializeField] private int fieldEntityPoolDefaultCapacity = 64;
    [SerializeField] private int fieldEntityPoolMaxSize = 1024;

    private IDungeonGenerator dungeon;
    private System.Random random;
    private Transform generatedTilesRoot;
    private Transform generatedFieldEntitiesRoot;
    private TilePool tilePool;
    private FieldEntityPool fieldEntityPool;
    private Tile pooledTilePrefab;
    private FieldEntity pooledFieldEntityPrefab;
    private Tile[,] tilesByCell;
    private int generatedWidth;
    private int generatedHeight;
    private float generatedTileSize = 1f;

    public IReadOnlyList<Tile> Tiles => tilePool?.ActiveTiles;
    public IReadOnlyList<FieldEntity> FieldEntities => fieldEntityPool?.ActiveEntities;
    public int GridWidth => generatedWidth;
    public int GridHeight => generatedHeight;
    public float TileSize => generatedTileSize;
    public Transform TilesParent => tilesParent;

    private void OnDestroy()
    {
        DisposeTilePool();
        DisposeFieldEntityPool();
    }

    [ContextMenu("Regenerate")]
    public void Generate(int difficulty = 1)
    {
        int validatedWidth;
        int validatedHeight;

        if (mode == DungeonMode.Vertical)
        {
            // Vertical generator owns its dimensions based on difficulty.
            validatedWidth = VerticalDungeonGenerator.GetWidthForDifficulty(difficulty);
            validatedHeight = VerticalDungeonGenerator.GetHeightForDifficulty(difficulty);
        }
        else
        {
            validatedWidth = Mathf.Max(1, width);
            validatedHeight = Mathf.Max(1, height);
        }

        float validatedTileSize = Mathf.Max(0.01f, tileSize);
        generatedWidth = validatedWidth;
        generatedHeight = validatedHeight;
        generatedTileSize = validatedTileSize;
        tilesByCell = new Tile[generatedWidth, generatedHeight];

        if (tilePrefab == null)
        {
            Debug.LogWarning("MapGenerator: tilePrefab is not assigned.");
            ReleaseActiveTiles();
            ReleaseActiveFieldEntities();
            return;
        }

        IReadOnlyList<TileTypeData> tileConfigs = GameDataRegistry.GetAll<TileTypeData>();
        if (tileConfigs.Count == 0)
        {
            Debug.LogWarning("MapGenerator: no TileType configs found. Add TileType entries to game data.");
            ReleaseActiveTiles();
            ReleaseActiveFieldEntities();
            return;
        }

        IReadOnlyList<FieldEntityData> fieldEntityConfigs = GameDataRegistry.GetAll<FieldEntityData>();

        random = useRandomSeed ? new System.Random() : new System.Random(seed);

        dungeon = mode == DungeonMode.Vertical
            ? new VerticalDungeonGenerator(difficulty)
            : new RoomsDungeonGenerator(roomCountMin, roomCountMax, roomSizeMin, roomSizeMax);

        dungeon.Initialize(tileConfigs, fieldEntityConfigs, validatedWidth, validatedHeight, random);

        EnsureTilePool();
        tilePool.ReleaseAll();

        EnsureFieldEntityPool();
        fieldEntityPool.ReleaseAll();

        for (int y = 0; y < validatedHeight; y++)
        {
            for (int x = 0; x < validatedWidth; x++)
            {
                Vector3 localPos = GridToLocalPosition(x, y, validatedWidth, validatedHeight, validatedTileSize);

                var tileData = dungeon.PickTile(x, y);
                if (tileData != null)
                {
                    Tile tile = tilePool.Get();
                    tile.name = $"Tile_{x}_{y}_{tileData.Id}";
                    tile.transform.localPosition = localPos;
                    tile.Init(tileData, RollHealth(tileData), ReleaseTile);
                    tilesByCell[x, y] = tile;
                }

                var fieldEntityData = dungeon.PickFieldEntity(x, y);
                if (fieldEntityData != null)
                {
                    var entity = fieldEntityPool.Get();
                    entity.name = $"FieldEntity_{x}_{y}_{fieldEntityData.Id}";
                    entity.transform.localPosition = localPos;
                    entity.Init(fieldEntityData, ReleaseFieldEntity);
                }
            }
        }
    }

    private int RollHealth(TileTypeData config)
    {
        int min = Mathf.Max(0, config.HealthMin);
        int max = Mathf.Max(min, config.HealthMax);
        if (min == max)
        {
            return min;
        }

        return random.Next(min, max + 1);
    }

    public bool TryWorldXToColumn(float worldX, out int column)
    {
        column = -1;
        if (generatedWidth <= 2 || generatedTileSize <= 0f || tilesParent == null)
        {
            return false;
        }

        var localX = tilesParent.InverseTransformPoint(new Vector3(worldX, 0f, 0f)).x;
        var normalized = localX / generatedTileSize + generatedWidth / 2f - 0.5f;
        var rawColumn = Mathf.RoundToInt(normalized);
        column = rawColumn;
        return true;
    }

    public Vector3 GridToWorldPosition(int x, int y)
    {
        if (generatedWidth <= 0 || generatedHeight <= 0)
        {
            return Vector3.zero;
        }

        var local = GridToLocalPosition(x, y, generatedWidth, generatedHeight, generatedTileSize);
        return tilesParent != null ? tilesParent.TransformPoint(local) : local;
    }

    public int GetTopFreeRow(int column)
    {
        if (generatedHeight <= 0 || tilesByCell == null || column < 0 || column >= generatedWidth)
        {
            return -1;
        }

        for (var y = 0; y < generatedHeight; y++)
        {
            var tile = tilesByCell[column, y];
            if (tile == null || tile.IsDestroyed)
            {
                return y;
            }
        }

        return 0;
    }

    private void ReleaseTile(Tile tile)
    {
        if (tilePool == null || tile == null)
        {
            return;
        }

        tilePool.Release(tile);
    }

    private void ReleaseFieldEntity(FieldEntity entity)
    {
        if (fieldEntityPool == null || entity == null)
        {
            return;
        }

        fieldEntityPool.Release(entity);
    }

    private void EnsureTilePool()
    {
        if (generatedTilesRoot == null)
        {
            Transform existing = tilesParent.Find("GeneratedTiles");
            generatedTilesRoot = existing != null
                ? existing
                : new GameObject("GeneratedTiles").transform;
            generatedTilesRoot.SetParent(tilesParent, false);
        }

        if (tilePool != null && pooledTilePrefab == tilePrefab)
        {
            return;
        }

        DisposeTilePool();
        tilePool = new TilePool(tilePrefab, generatedTilesRoot, poolDefaultCapacity, poolMaxSize);
        pooledTilePrefab = tilePrefab;
    }

    private void EnsureFieldEntityPool()
    {
        if (generatedFieldEntitiesRoot == null)
        {
            Transform existing = tilesParent.Find("GeneratedFieldEntities");
            generatedFieldEntitiesRoot = existing != null
                ? existing
                : new GameObject("GeneratedFieldEntities").transform;
            generatedFieldEntitiesRoot.SetParent(tilesParent, false);
        }

        if (fieldEntityPool != null && pooledFieldEntityPrefab == fieldEntityPrefab)
        {
            return;
        }

        DisposeFieldEntityPool();
        fieldEntityPool = new FieldEntityPool(
            fieldEntityPrefab,
            generatedFieldEntitiesRoot,
            fieldEntityPoolDefaultCapacity,
            fieldEntityPoolMaxSize);
        pooledFieldEntityPrefab = fieldEntityPrefab;
    }

    private void ReleaseActiveTiles()
    {
        if (tilePool != null)
        {
            tilePool.ReleaseAll();
        }
    }

    private void ReleaseActiveFieldEntities()
    {
        if (fieldEntityPool != null)
        {
            fieldEntityPool.ReleaseAll();
        }
    }

    private void DisposeTilePool()
    {
        if (tilePool == null)
        {
            return;
        }

        tilePool.Dispose();
        tilePool = null;
        pooledTilePrefab = null;
    }

    private void DisposeFieldEntityPool()
    {
        if (fieldEntityPool == null)
        {
            return;
        }

        fieldEntityPool.Dispose();
        fieldEntityPool = null;
        pooledFieldEntityPrefab = null;
    }

    private Vector3 GridToLocalPosition(int x, int y, int mapWidth, int mapHeight, float step)
    {
        float centeredX = (x - mapWidth / 2f + 0.5f) * step;
        float centeredY = (-y - mapHeight / 2f + 0.5f) * step;
        return new Vector3(centeredX, centeredY, 0f);
    }
}
