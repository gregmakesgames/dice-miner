using System;
using System.Collections.Generic;

public interface IDungeonGenerator
{
    void Initialize(
        IReadOnlyList<ConfigEntity> tileConfigs,
        IReadOnlyList<ConfigEntity> fieldEntityConfigs,
        int width,
        int height,
        Random random);

    ConfigEntity PickTile(int x, int y);
    ConfigEntity PickFieldEntity(int x, int y);
}
