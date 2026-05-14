using GameData;

namespace DiceMiner
{
    public static class GameAssets
    {
        public static GameDataRegistry configs;

        public static void Init()
        {
            configs = new GameDataRegistry();
        }
    }
}