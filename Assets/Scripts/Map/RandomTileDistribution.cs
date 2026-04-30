using System;
using System.Collections.Generic;

public sealed class RandomTileDistribution : ITileDistribution
{
    private IReadOnlyList<ConfigEntity> tileConfigs = Array.Empty<ConfigEntity>();
    private Random random = new();

    public void Initialize(IReadOnlyList<ConfigEntity> tileConfigs, int width, int height, Random random)
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
}
