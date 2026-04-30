using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace GameData
{
    public static class GameDataRegistry
    {
        private const string DataResourcePath = "GameData/data";

        private static readonly Dictionary<Type, Dictionary<string, DataEntity>> entitiesByType = new();
        private static readonly Dictionary<Type, IList> orderedEntitiesByType = new();

        private static bool isLoaded;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void EnsureLoadedAtStartup()
        {
            Load();
        }

        public static void Load(bool forceReload = false)
        {
            if (isLoaded && !forceReload)
            {
                return;
            }

            entitiesByType.Clear();
            orderedEntitiesByType.Clear();

            LoadData();
            isLoaded = true;
        }

        public static T Get<T>(string id) where T : DataEntity
        {
            return GetById(typeof(T), id) as T;
        }

        public static IReadOnlyList<T> GetAll<T>() where T : DataEntity
        {
            Load();
            if (orderedEntitiesByType.TryGetValue(typeof(T), out IList list) &&
                list is List<T> typed)
            {
                return typed;
            }

            return Array.Empty<T>();
        }

        public static DataEntity GetById(Type type, string id)
        {
            Load();
            if (type == null || string.IsNullOrWhiteSpace(id))
            {
                return null;
            }

            return entitiesByType.TryGetValue(type, out Dictionary<string, DataEntity> byId) &&
                   byId.TryGetValue(id, out DataEntity entity)
                ? entity
                : null;
        }

        private static void LoadData()
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

            foreach ((string typeName, JToken token) in configs)
            {
                if (!GameDataTypes.Map.TryGetValue(typeName, out Type type))
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

                Dictionary<string, DataEntity> byId = new(StringComparer.Ordinal);
                IList ordered = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(type));
                entitiesByType[type] = byId;
                orderedEntitiesByType[type] = ordered;

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

                    byId[entity.Id] = entity;
                    ordered.Add(entity);
                }
            }
        }
    }
}
