using System;
using System.Collections.Generic;
using GameData;

public sealed class RandomDungeonGenerator : IDungeonGenerator
{
    private IReadOnlyList<TileTypeData> tileConfigs = Array.Empty<TileTypeData>();
    private Random random = new();

    public void Initialize(
        IReadOnlyList<TileTypeData> tileConfigs,
        IReadOnlyList<FieldEntityData> fieldEntityConfigs,
        int width,
        int height,
        Random random)
    {
        this.tileConfigs = tileConfigs ?? Array.Empty<TileTypeData>();
        this.random = random ?? new Random();
    }

    public TileTypeData PickTile(int x, int y)
    {
        if (tileConfigs.Count == 0)
        {
            return null;
        }

        int index = random.Next(0, tileConfigs.Count);
        return tileConfigs[index];
    }

    public FieldEntityData PickFieldEntity(int x, int y)
    {
        return null;
    }
}
