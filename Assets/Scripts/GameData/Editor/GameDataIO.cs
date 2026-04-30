using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

public static class GameDataIO
{
    public const string SchemaAssetPath = "Assets/Resources/GameData/schema.json";
    public const string DataAssetPath = "Assets/Resources/GameData/data.json";

    public static GameDataSchemaRoot LoadSchema()
    {
        if (!File.Exists(SchemaAssetPath))
        {
            return new GameDataSchemaRoot();
        }

        string text = File.ReadAllText(SchemaAssetPath);
        return JsonConvert.DeserializeObject<GameDataSchemaRoot>(text) ?? new GameDataSchemaRoot();
    }

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

    public static void Save(GameDataSchemaRoot schema, JObject dataRoot)
    {
        EnsureDirectories();
        string schemaJson = JsonConvert.SerializeObject(schema, Formatting.Indented);
        File.WriteAllText(SchemaAssetPath, schemaJson + "\n");

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
        string schemaDir = Path.GetDirectoryName(SchemaAssetPath);
        string dataDir = Path.GetDirectoryName(DataAssetPath);

        if (!string.IsNullOrEmpty(schemaDir))
        {
            Directory.CreateDirectory(schemaDir);
        }

        if (!string.IsNullOrEmpty(dataDir))
        {
            Directory.CreateDirectory(dataDir);
        }
    }
}
