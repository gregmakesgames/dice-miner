using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class MapGenerator : MonoBehaviour
{
    [Header("Map Size")]
    [SerializeField] private int width = 30;
    [SerializeField] private int height = 30;
    [SerializeField] private int emptyCenterSize = 5;
    [SerializeField] private float tileSize = 1f;

    [Header("Random")]
    [SerializeField] private bool useRandomSeed = true;
    [SerializeField] private int seed = 12345;

    [Header("Hierarchy")]
    [SerializeField] private bool generateOnStart = true;
    [SerializeField] private Transform tilesParent;

    private ITileDistribution distribution = new RandomTileDistribution();
    private System.Random random;
    private Transform generatedTilesRoot;

    private void Start()
    {
        if (generateOnStart)
        {
            Generate();
        }
    }

    public void SetDistribution(ITileDistribution customDistribution)
    {
        distribution = customDistribution ?? new RandomTileDistribution();
    }

    [ContextMenu("Regenerate")]
    public void Generate()
    {
        int validatedWidth = Mathf.Max(1, width);
        int validatedHeight = Mathf.Max(1, height);
        int validatedCenterSize = Mathf.Clamp(emptyCenterSize, 0, Mathf.Min(validatedWidth, validatedHeight));
        float validatedTileSize = Mathf.Max(0.01f, tileSize);

        IReadOnlyList<ConfigEntity> tileConfigs = GameDataRegistry.GetAll("TileType");
        if (tileConfigs.Count == 0)
        {
            Debug.LogWarning("MapGenerator: no TileType configs found. Add TileType entries to game data.");
            ClearGeneratedTiles();
            return;
        }

        random = useRandomSeed ? new System.Random() : new System.Random(seed);
        distribution.Initialize(tileConfigs, random);

        Transform parent = ResolveTilesParent();
        ClearGeneratedTiles();
        generatedTilesRoot = new GameObject("GeneratedTiles").transform;
        generatedTilesRoot.SetParent(parent, false);

        int emptyMinX = (validatedWidth - validatedCenterSize) / 2;
        int emptyMinY = (validatedHeight - validatedCenterSize) / 2;
        int emptyMaxX = emptyMinX + validatedCenterSize - 1;
        int emptyMaxY = emptyMinY + validatedCenterSize - 1;

        for (int y = 0; y < validatedHeight; y++)
        {
            for (int x = 0; x < validatedWidth; x++)
            {
                if (validatedCenterSize > 0 &&
                    x >= emptyMinX && x <= emptyMaxX &&
                    y >= emptyMinY && y <= emptyMaxY)
                {
                    continue;
                }

                ConfigEntity tileConfig = distribution.PickTile(x, y);
                if (tileConfig == null)
                {
                    continue;
                }

                Sprite sprite = tileConfig.GetSprite("sprite");
                if (sprite == null)
                {
                    continue;
                }

                GameObject tile = new($"Tile_{x}_{y}_{tileConfig.Id}");
                tile.transform.SetParent(generatedTilesRoot, false);
                tile.transform.localPosition = GridToLocalPosition(x, y, validatedWidth, validatedHeight, validatedTileSize);

                SpriteRenderer renderer = tile.AddComponent<SpriteRenderer>();
                renderer.sprite = sprite;
            }
        }
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

    private void ClearGeneratedTiles()
    {
        Transform parent = ResolveTilesParent();
        if (generatedTilesRoot == null)
        {
            generatedTilesRoot = parent.Find("GeneratedTiles");
        }

        if (generatedTilesRoot == null)
        {
            return;
        }

        if (Application.isPlaying)
        {
            Destroy(generatedTilesRoot.gameObject);
        }
        else
        {
            DestroyImmediate(generatedTilesRoot.gameObject);
        }

        generatedTilesRoot = null;
    }
}
