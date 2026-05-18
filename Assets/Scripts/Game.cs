using DiceMiner.Cheats;
using DiceMiner.Gameplay;
using DiceMiner.Save;
using DiceMiner.UI;
using DiceMiner.Vfx;
using GrishaGuWorkshop;

namespace DiceMiner
{
    public static class Game
    {
        public static RunStarter runStarter;
        public static Run run;
        public static IUiManager ui;
        public static AudioManager audio => AudioManager.Instance;
        public static IVfxManager vfx;
        public static ICheatsManager cheats;
        public static ISaveManager save;

        public static void Init()
        {
            ui = new UiManager();
            save = new SaveManager();
            runStarter = new RunStarter();
        }
    }
}