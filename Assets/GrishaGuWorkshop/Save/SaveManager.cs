using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;

namespace GrishaGuWorkshop
{
    public class SaveManager
    {
        private const string SAVE_KEY = "game_save_slot";
        
        public SavedGame SaveGame(object saveRoot)
        {
            var save = SavedGame.New();

            if (saveRoot != null)
            {
                foreach (var field in saveRoot.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (field.GetValue(saveRoot) is ISavableService savableService)
                    {
                        savableService.SaveTo(save);
                    }
                }
            }

            SaveToPlayerPrefs(save);
            
            return save;
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

        public void LoadGame(SavedGame savedGame, object loadRoot)
        {
            if (loadRoot != null)
            {
                foreach (var field in loadRoot.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (field.GetValue(loadRoot) is ILoadableService savableService)
                    {
                        savableService.LoadFrom(savedGame);
                    }
                }
            }
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