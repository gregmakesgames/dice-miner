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

    [Header("Pooling")]
    [SerializeField] private int poolDefaultCapacity = 256;
    [SerializeField] private int poolMaxSize = 4096;

    private ITileDistribution distribution;
    private bool distributionExplicitlySet;
    private System.Random random;
    private Transform generatedTilesRoot;
    private TilePool tilePool;
    private Tile pooledPrefab;

    private void Start()
    {
        if (generateOnStart)
        {
            Generate();
        }
    }

    private void OnDestroy()
    {
        DisposePool();
    }

    public void SetDistribution(ITileDistribution customDistribution)
    {
        distribution = customDistribution;
        distributionExplicitlySet = customDistribution != null;
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
            return;
        }

        IReadOnlyList<ConfigEntity> tileConfigs = GameDataRegistry.GetAll("TileType");
        if (tileConfigs.Count == 0)
        {
            Debug.LogWarning("MapGenerator: no TileType configs found. Add TileType entries to game data.");
            ReleaseActiveTiles();
            return;
        }

        random = useRandomSeed ? new System.Random() : new System.Random(seed);

        if (!distributionExplicitlySet)
        {
            // Rebuild the default each call so inspector tweaks to room params apply live.
            distribution = BuildDefaultDistribution();
        }

        distribution.Initialize(tileConfigs, validatedWidth, validatedHeight, random);

        EnsurePool();
        tilePool.ReleaseAll();

        for (int y = 0; y < validatedHeight; y++)
        {
            for (int x = 0; x < validatedWidth; x++)
            {
                ConfigEntity tileConfig = distribution.PickTile(x, y);
                if (tileConfig == null)
                {
                    continue;
                }

                Tile tile = tilePool.Get();
                tile.name = $"Tile_{x}_{y}_{tileConfig.Id}";
                tile.transform.localPosition = GridToLocalPosition(x, y, validatedWidth, validatedHeight, validatedTileSize);
                tile.Init(tileConfig, RollTileHealth(tileConfig), ReleaseTile);
            }
        }
    }

    private ITileDistribution BuildDefaultDistribution()
    {
        return new RoomsTileDistribution(roomCountMin, roomCountMax, roomSizeMin, roomSizeMax);
    }

    private int RollTileHealth(ConfigEntity tileConfig)
    {
        int min = Mathf.Max(0, tileConfig.GetInt("healthMin"));
        int max = Mathf.Max(min, tileConfig.GetInt("healthMax"));
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

    private void EnsurePool()
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

        if (tilePool != null && pooledPrefab == tilePrefab)
        {
            return;
        }

        DisposePool();
        tilePool = new TilePool(tilePrefab, generatedTilesRoot, poolDefaultCapacity, poolMaxSize);
        pooledPrefab = tilePrefab;
    }

    private void ReleaseActiveTiles()
    {
        if (tilePool != null)
        {
            tilePool.ReleaseAll();
        }
    }

    private void DisposePool()
    {
        if (tilePool == null)
        {
            return;
        }

        tilePool.Dispose();
        tilePool = null;
        pooledPrefab = null;
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
