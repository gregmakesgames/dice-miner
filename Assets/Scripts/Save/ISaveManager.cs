using System.Collections.Generic;

namespace DiceMiner.Save
{
    public interface ISaveManager
    {
        public SavedGame SaveGame();
        public IReadOnlyList<SavedGame> GetSaves();
        SavedGame CreateNewGame();
        public void LoadGame(SavedGame savedGame);
    }
}