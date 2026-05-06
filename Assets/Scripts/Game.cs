using DiceMiner.Meta;

namespace DiceMiner
{
    public class Game
    {
        public static Game Instance { get; private set; }
        
        public UpgradeManager UpgradeManager { get; private set; }
    }
}