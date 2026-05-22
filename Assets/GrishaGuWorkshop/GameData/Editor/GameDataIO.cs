using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;

namespace GrishaGuWorkshop.GameData.Editor
{
    public static class GameDataIO
    {
        public const string DataAssetPath = "Assets/Resources/GameData/data.json";
        
        public static JObject LoadData()
        {
            if (!File.Exists(DataAssetPath))
            {
                return CreateEmptyDataRoot();
            }

            string text = File.ReadAllText(DataAssetPath);
            if (string.IsNullOrWhiteSpace(text))
            {
                return CreateEmptyDataRoot();
            }

            JObject root = JObject.Parse(text);
            if (root["configs"] is not JObject)
            {
                root["configs"] = new JObject();
            }

            return root;
        }

        public static void Save(JObject dataRoot)
        {
            EnsureDirectories();

            string dataJson = dataRoot.ToString(Formatting.Indented);
            File.WriteAllText(DataAssetPath, dataJson + "\n");

            AssetDatabase.Refresh();
        }

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
            JObject configs = GetConfigsObject(dataRoot);
            if (configs[typeName] is not JArray array)
            {
                array = new JArray();
                configs[typeName] = array;
            }

            return array;
        }

        public static List<string> GetEntityIds(JObject dataRoot, string typeName)
        {
            List<string> ids = new();
            JArray array = GetOrCreateTypeArray(dataRoot, typeName);
            for (int i = 0; i < array.Count; i++)
            {
                if (array[i] is JObject obj)
                {
                    string id = obj.Value<string>("id");
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

        private static void EnsureDirectories()
        {
            string dataDir = Path.GetDirectoryName(DataAssetPath);

            if (!string.IsNullOrEmpty(dataDir))
            {
                Directory.CreateDirectory(dataDir);
            }
        }
    }
}
