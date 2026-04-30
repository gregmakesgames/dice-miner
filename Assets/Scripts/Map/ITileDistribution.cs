using System;
using System.Collections.Generic;

public interface ITileDistribution
{
    void Initialize(IReadOnlyList<ConfigEntity> tileConfigs, Random random);
    ConfigEntity PickTile(int x, int y);
}
