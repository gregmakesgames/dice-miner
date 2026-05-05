using System;
using System.Collections.Generic;
using GameData;

namespace Gameplay.Map
{
    public interface IMapGeneratorAlhorythm
    {
        void Initialize(
            IReadOnlyList<TileTypeData> tileConfigs,
            int width,
            int height,
            Random random);

        TileTypeData PickTile(int x, int y);
    }
}
