using System;
using System.Collections.Generic;
using GameData;

public interface IDungeonGenerator
{
    void Initialize(
        IReadOnlyList<TileTypeData> tileConfigs,
        IReadOnlyList<FieldEntityData> fieldEntityConfigs,
        int width,
        int height,
        Random random);

    TileTypeData PickTile(int x, int y);
    FieldEntityData PickFieldEntity(int x, int y);
}
