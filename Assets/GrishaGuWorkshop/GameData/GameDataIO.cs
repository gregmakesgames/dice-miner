using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GrishaGuWorkshop.Converters;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GrishaGuWorkshop
{
    public static class GameDataIO
    {
        public const string DataAssetPath = "Assets/Resources/GameData/data.json";
        public const string DataResourcePath = "GameData/data";

        public static JObject LoadDataRoot()
        {
#if UNITY_EDITOR
            if (File.Exists(DataAssetPath))
            {
                var text = File.ReadAllText(DataAssetPath);
                if (!string.IsNullOrWhiteSpace(text))
                {
                    return NormalizeRoot(JObject.Parse(text));
                }
            }
#endif

            var dataAsset = Resources.Load<TextAsset>(DataResourcePath);
            if (dataAsset == null || string.IsNullOrWhiteSpace(dataAsset.text))
            {
                return CreateEmptyDataRoot();
            }

            try
            {
                return NormalizeRoot(JObject.Parse(dataAsset.text));
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to parse game data file: {ex.Message}");
                return CreateEmptyDataRoot();
            }
        }

        public static List<DataEntity> LoadEntities()
        {
            var entities = new List<DataEntity>();
            var root = LoadDataRoot();
            var configs = root.Value<JObject>("configs");
            if (configs == null)
            {
                Debug.LogWarning("GameData data file does not contain 'configs' object.");
                return entities;
            }

            var serializer = CreateSerializer();
            var allTypes = GetDataEntityTypes();

            foreach (var (typeName, token) in configs)
            {
                var type = allTypes.FirstOrDefault(x => x.FullName == typeName);
                if (type == null)
                {
                    Debug.LogWarning(
                        $"GameData type '{typeName}' is not in the type map. Skipped.");
                    continue;
                }

                if (token is not JArray array)
                {
                    Debug.LogWarning($"GameData type '{typeName}' must be an array.");
                    continue;
                }

                for (var i = 0; i < array.Count; i++)
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

                    entities.Add(entity);
                }
            }

            return entities;
        }

        public static JsonSerializer CreateSerializer()
        {
            var serializer = JsonSerializer.Create(CreateSettings());
            return serializer;
        }

        public static JsonSerializerSettings CreateSettings()
        {
            return new JsonSerializerSettings
            {
                Converters =
                {
                    new ColorJsonConverter(),
                    new Vector2JsonConverter(),
                    new Vector3JsonConverter(),
                    new DataEntityTagJsonConverter()
                }
            };
        }

#if UNITY_EDITOR
        public static void Save(JObject dataRoot)
        {
            EnsureDirectories();

            var dataJson = dataRoot.ToString(Formatting.Indented);
            File.WriteAllText(DataAssetPath, dataJson + "\n");

            AssetDatabase.Refresh();
        }
#endif

        public static JObject GetConfigsObject(JObject dataRoot)
        {
            if (dataRoot["configs"] is not JObject configs)
            {
                configs = new JObject();
                dataRoot["configs"] = configs;
            }

            return configs;
        }

        public static JArray GetOrCreateTypeArray(JObject dataRoot, string typeName)
        {
            var configs = GetConfigsObject(dataRoot);
            if (configs[typeName] is not JArray array)
            {
                array = new JArray();
                configs[typeName] = array;
            }

            return array;
        }

        public static List<string> GetEntityIds(JObject dataRoot, string typeName)
        {
            var ids = new List<string>();
            var array = GetOrCreateTypeArray(dataRoot, typeName);
            for (var i = 0; i < array.Count; i++)
            {
                if (array[i] is JObject obj)
                {
                    var id = obj.Value<string>("id");
                    if (!string.IsNullOrWhiteSpace(id))
                    {
                        ids.Add(id);
                    }
                }
            }

            return ids;
        }

        public static JObject CreateEmptyDataRoot()
        {
            return new JObject
            {
                ["configs"] = new JObject()
            };
        }

        private static JObject NormalizeRoot(JObject root)
        {
            if (root["configs"] is not JObject)
            {
                root["configs"] = new JObject();
            }

            return root;
        }

        private static List<Type> GetDataEntityTypes()
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(GetAssemblyTypes)
                .Where(t => t is { IsClass: true, IsAbstract: false } && typeof(DataEntity).IsAssignableFrom(t))
                .ToList();
        }

        private static IEnumerable<Type> GetAssemblyTypes(System.Reflection.Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch
            {
                return Type.EmptyTypes;
            }
        }

#if UNITY_EDITOR
        private static void EnsureDirectories()
        {
            var dataDir = Path.GetDirectoryName(DataAssetPath);

            if (!string.IsNullOrEmpty(dataDir))
            {
                Directory.CreateDirectory(dataDir);
            }
        }
#endif
    }
}
