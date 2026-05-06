using Newtonsoft.Json;

namespace DiceMiner
{
    public static class GameSaver
    {
        private const string SaveKey = "saved_game";

        public static SavedGame LoadFromPlayerPrefs()
        {
            if (!UnityEngine.PlayerPrefs.HasKey(SaveKey))
            {
                return SavedGame.New();
            }

            var json = UnityEngine.PlayerPrefs.GetString(SaveKey, string.Empty);
            if (string.IsNullOrWhiteSpace(json))
            {
                return SavedGame.New();
            }

            try
            {
                var loadedGame = JsonConvert.DeserializeObject<SavedGame>(json);
                return loadedGame ?? SavedGame.New();
            }
            catch (JsonException)
            {
                return SavedGame.New();
            }
        }

        public static void SaveToPlayerPrefs(SavedGame savedGame)
        {
            if (savedGame == null)
            {
                UnityEngine.PlayerPrefs.DeleteKey(SaveKey);
                UnityEngine.PlayerPrefs.Save();
                return;
            }

            var json = JsonConvert.SerializeObject(savedGame);
            UnityEngine.PlayerPrefs.SetString(SaveKey, json);
            UnityEngine.PlayerPrefs.Save();
        }
    }
}