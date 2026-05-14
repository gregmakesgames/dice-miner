using DiceMiner.Cheats;
using DiceMiner.Debug;
using DiceMiner.Save;
using DiceMiner.SFX;
using DiceMiner.UI;
using DiceMiner.Vfx;

namespace DiceMiner
{
    public static class G
    {
        public static IUiManager ui;
        public static ISfxManager sfx;
        public static IVfxManager vfx;
        public static IDebugManager debug;
        public static ICheatsManager cheats;
        public static ISaveManager save;

        public static void Init()
        {
            ui = new UiManager();
            debug = new DebugManager();
        }
    }
}