using System.Collections.Generic;
using DiceMiner.Gameplay.Data;

namespace DiceMiner
{
    public class SavedGame
    {
        public static SavedGame New()
        {
            return new SavedGame()
            {
                dices = new List<DiceGameplaySaveData>(),
            };
        }

        public List<DiceGameplaySaveData> dices;

    }
}