using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace DiceMiner.Save
{
    public class SaveManager : ISaveManager
    {
        private const string SAVE_KEY = "game_save_slot";

        private SavedGame _currentSave = null;
        
        public SavedGame SaveGame()
        {
            return _currentSave;
        }

        public IReadOnlyList<SavedGame> GetSaves()
        {
            var list = new List<SavedGame>();
            var save = LoadFromPlayerPrefs();
            if (save != null)
            {
                list.Add(save);
            }

            return list;
        }

        public SavedGame CreateNewGame()
        {
            var index = GetSaves().Select(x => x.slot).Max() + 1;
            var save = SavedGame.New(index);
            return save;
        }
        
        public void LoadGame(SavedGame savedGame)
        {
            _currentSave = savedGame;
        }
        
        
        private void SaveToPlayerPrefs(SavedGame savedGame)
        {
            if (savedGame == null)
            {
                UnityEngine.PlayerPrefs.DeleteKey(SAVE_KEY);
                UnityEngine.PlayerPrefs.Save();
                return;
            }

            var json = JsonConvert.SerializeObject(savedGame);
            UnityEngine.PlayerPrefs.SetString(SAVE_KEY, json);
            UnityEngine.PlayerPrefs.Save();
        }
        
        private SavedGame LoadFromPlayerPrefs()
        {
            if (!UnityEngine.PlayerPrefs.HasKey(SAVE_KEY))
            {
                return null;
            }

            var json = UnityEngine.PlayerPrefs.GetString(SAVE_KEY, string.Empty);
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            try
            {
                var loadedGame = JsonConvert.DeserializeObject<SavedGame>(json);
                return loadedGame;
            }
            catch (JsonException)
            {
                return null;
            }
        }
    }
}