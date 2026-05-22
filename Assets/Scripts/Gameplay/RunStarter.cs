using GrishaGuWorkshop;

namespace DiceMiner.Gameplay
{
    public class RunStarter
    {
        public void StartRun(SavedGame savedGame)
        {
            Game.run = new Run();

            if (savedGame != null)
            {
                Game.save.LoadGame(savedGame, Game.run);   
            }
        }
    }
}