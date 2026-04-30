using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class MapGenerator : MonoBehaviour
{
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

    [Header("Pooling")]
    [SerializeField] private int poolDefaultCapacity = 256;
    [SerializeField] private int poolMaxSize = 4096;
    [SerializeField] private int fieldEntityPoolDefaultCapacity = 64;
    [SerializeField] private int fieldEntityPoolMaxSize = 1024;

    private IDungeonGenerator dungeon;
    private bool dungeonExplicitlySet;
    private System.Random random;
    private Transform generatedTilesRoot;
    private Transform generatedFieldEntitiesRoot;
    private TilePool tilePool;
    private FieldEntityPool fieldEntityPool;
    private Tile pooledTilePrefab;
    private FieldEntity pooledFieldEntityPrefab;

    private void Start()
    {
        if (generateOnStart)
        {
            Generate();
        }
    }

    private void OnDestroy()
    {
        DisposeTilePool();
        DisposeFieldEntityPool();
    }

    public void SetDungeonGenerator(IDungeonGenerator customGenerator)
    {
        dungeon = customGenerator;
        dungeonExplicitlySet = customGenerator != null;
    }

    [ContextMenu("Regenerate")]
    public void Generate()
    {
        int validatedWidth = Mathf.Max(1, width);
        int validatedHeight = Mathf.Max(1, height);
        float validatedTileSize = Mathf.Max(0.01f, tileSize);

        if (tilePrefab == null)
        {
            Debug.LogWarning("MapGenerator: tilePrefab is not assigned.");
            ReleaseActiveTiles();
            ReleaseActiveFieldEntities();
            return;
        }

        IReadOnlyList<ConfigEntity> tileConfigs = GameDataRegistry.GetAll("TileType");
        if (tileConfigs.Count == 0)
        {
            Debug.LogWarning("MapGenerator: no TileType configs found. Add TileType entries to game data.");
            ReleaseActiveTiles();
            ReleaseActiveFieldEntities();
            return;
        }

        IReadOnlyList<ConfigEntity> fieldEntityConfigs = GameDataRegistry.GetAll("FieldEntity");

        random = useRandomSeed ? new System.Random() : new System.Random(seed);

        if (!dungeonExplicitlySet)
        {
            // Rebuild the default each call so inspector tweaks to room params apply live.
            dungeon = BuildDefaultDungeonGenerator();
        }

        dungeon.Initialize(tileConfigs, fieldEntityConfigs, validatedWidth, validatedHeight, random);

        EnsureTilePool();
        tilePool.ReleaseAll();

        bool hasFieldEntityPrefab = fieldEntityPrefab != null;
        if (hasFieldEntityPrefab)
        {
            EnsureFieldEntityPool();
            fieldEntityPool.ReleaseAll();
        }
        else
        {
            ReleaseActiveFieldEntities();
        }

        for (int y = 0; y < validatedHeight; y++)
        {
            for (int x = 0; x < validatedWidth; x++)
            {
                Vector3 localPos = GridToLocalPosition(x, y, validatedWidth, validatedHeight, validatedTileSize);

                ConfigEntity tileConfig = dungeon.PickTile(x, y);
                if (tileConfig != null)
                {
                    Tile tile = tilePool.Get();
                    tile.name = $"Tile_{x}_{y}_{tileConfig.Id}";
                    tile.transform.localPosition = localPos;
                    tile.Init(tileConfig, RollHealth(tileConfig), ReleaseTile);
                }

                ConfigEntity fieldEntityConfig = dungeon.PickFieldEntity(x, y);
                if (fieldEntityConfig != null && hasFieldEntityPrefab)
                {
                    FieldEntity entity = fieldEntityPool.Get();
                    entity.name = $"FieldEntity_{x}_{y}_{fieldEntityConfig.Id}";
                    entity.transform.localPosition = localPos;
                    entity.Init(fieldEntityConfig, ReleaseFieldEntity);
                }
                else if (fieldEntityConfig != null)
                {
                    Debug.LogWarning(
                        $"MapGenerator: FieldEntity '{fieldEntityConfig.Id}' was scheduled at ({x},{y}) but fieldEntityPrefab is not assigned.");
                }
            }
        }
    }

    private IDungeonGenerator BuildDefaultDungeonGenerator()
    {
        return new RoomsDungeonGenerator(roomCountMin, roomCountMax, roomSizeMin, roomSizeMax);
    }

    private int RollHealth(ConfigEntity config)
    {
        int min = Mathf.Max(0, config.GetInt("healthMin"));
        int max = Mathf.Max(min, config.GetInt("healthMax"));
        if (min == max)
        {
            return min;
        }

        return random.Next(min, max + 1);
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
        Transform parent = ResolveTilesParent();

        if (generatedTilesRoot == null)
        {
            Transform existing = parent.Find("GeneratedTiles");
            generatedTilesRoot = existing != null
                ? existing
                : new GameObject("GeneratedTiles").transform;
            generatedTilesRoot.SetParent(parent, false);
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
        Transform parent = ResolveTilesParent();

        if (generatedFieldEntitiesRoot == null)
        {
            Transform existing = parent.Find("GeneratedFieldEntities");
            generatedFieldEntitiesRoot = existing != null
                ? existing
                : new GameObject("GeneratedFieldEntities").transform;
            generatedFieldEntitiesRoot.SetParent(parent, false);
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

    private Transform ResolveTilesParent()
    {
        return tilesParent != null ? tilesParent : transform;
    }

    private Vector3 GridToLocalPosition(int x, int y, int mapWidth, int mapHeight, float step)
    {
        float centeredX = (x - mapWidth / 2f + 0.5f) * step;
        float centeredY = (y - mapHeight / 2f + 0.5f) * step;
        return new Vector3(centeredX, centeredY, 0f);
    }
}
