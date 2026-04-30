using System;
using System.Collections.Generic;

public sealed class RandomDungeonGenerator : IDungeonGenerator
{
    private IReadOnlyList<ConfigEntity> tileConfigs = Array.Empty<ConfigEntity>();
    private Random random = new();

    public void Initialize(
        IReadOnlyList<ConfigEntity> tileConfigs,
        IReadOnlyList<ConfigEntity> fieldEntityConfigs,
        int width,
        int height,
        Random random)
    {
        this.tileConfigs = tileConfigs ?? Array.Empty<ConfigEntity>();
        this.random = random ?? new Random();
    }

    public ConfigEntity PickTile(int x, int y)
    {
        if (tileConfigs.Count == 0)
        {
            return null;
        }

        int index = random.Next(0, tileConfigs.Count);
        return tileConfigs[index];
    }

    public ConfigEntity PickFieldEntity(int x, int y)
    {
        return null;
    }
}
