namespace DiceMiner
{
    public class SavedGame
    {
        public static SavedGame New(int slot)
        {
            return new SavedGame()
            {
                slot = slot,
            };
        }

        public int slot = 0;

    }
}