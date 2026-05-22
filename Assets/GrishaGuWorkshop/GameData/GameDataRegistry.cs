using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace GrishaGuWorkshop
{
    public class GameDataRegistry
    {
        private const string DataResourcePath = "GameData/data";

        private List<DataEntity> _entities = new();
        
        private bool isLoaded;

        public void Load(bool forceReload = false)
        {
            if (isLoaded && !forceReload)
            {
                return;
            }

            _entities.Clear();

            LoadData();
            isLoaded = true;
        }

        public T Get<T>(string id) where T : DataEntity
        {
            Load();
            
            return _entities.OfType<T>().FirstOrDefault(x => x.Id == id);
        }

        public List<T> GetAll<T>() where T : DataEntity
        {
            Load();
            return _entities.OfType<T>().ToList();
        }


        public List<DataEntity> GetAllWithTag<TG>() where TG : DataEntityTag
        {
            Load();
            return _entities.Where(x => x.GetTag<TG>() != null).ToList();
        }

        public List<T> GetAllOfTypeWithTag<T, TG>() where TG : DataEntityTag where T : DataEntity
        {
            Load();
            return _entities.OfType<T>().Where(x => x.GetTag<TG>() != null).ToList();
        }
        
        private void LoadData()
        {
            TextAsset dataAsset = Resources.Load<TextAsset>(DataResourcePath);
            if (dataAsset == null)
            {
                Debug.LogWarning($"GameData data file missing at Resources path '{DataResourcePath}'.");
                return;
            }

            JObject root;
            try
            {
                root = JObject.Parse(dataAsset.text);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to parse game data file: {ex.Message}");
                return;
            }

            JObject configs = root.Value<JObject>("configs");
            if (configs == null)
            {
                Debug.LogWarning("GameData data file does not contain 'configs' object.");
                return;
            }

            JsonSerializer serializer = JsonSerializer.CreateDefault();

            var allTypes = typeof(DataEntity).Assembly.GetTypes().Where(x => x.IsAssignableFrom(typeof(DataEntity))).ToList();
            
            foreach ((string typeName, JToken token) in configs)
            {
                var type = allTypes.FirstOrDefault(x => x.Name == typeName);
                if (type == null)
                {
                    Debug.LogWarning(
                        $"GameData type '{typeName}' is not in the generated type map. " +
                        "Run Tools > Game Data > Regenerate Code. Skipped.");
                    continue;
                }

                if (token is not JArray array)
                {
                    Debug.LogWarning($"GameData type '{typeName}' must be an array.");
                    continue;
                }

                for (int i = 0; i < array.Count; i++)
                {
                    if (array[i] is not JObject entry)
                    {
                        Debug.LogWarning(
                            $"GameData '{typeName}' contains a non-object entry at index {i}. Skipped.");
                        continue;
                    }

                    DataEntity entity;
                    try
                    {
                        entity = (DataEntity)entry.ToObject(type, serializer);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError(
                            $"GameData '{typeName}' entry at index {i}: deserialization failed: {ex.Message}");
                        continue;
                    }

                    if (entity == null)
                    {
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(entity.Id))
                    {
                        Debug.LogWarning(
                            $"GameData '{typeName}' entry at index {i} is missing id. Skipped.");
                        continue;
                    }

                    _entities.Add(entity);
                }
            }
        }
    }
}
