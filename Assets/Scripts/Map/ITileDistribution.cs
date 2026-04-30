using System;
using System.Collections.Generic;

public interface ITileDistribution
{
    void Initialize(IReadOnlyList<ConfigEntity> tileConfigs, int width, int height, Random random);
    ConfigEntity PickTile(int x, int y);
}
