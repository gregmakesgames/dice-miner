using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

public static class GameDataRegistry
{
    private const string SchemaResourcePath = "GameData/schema";
    private const string DataResourcePath = "GameData/data";

    private static readonly Dictionary<string, Dictionary<string, ConfigEntity>> entitiesByType =
        new(StringComparer.Ordinal);
    private static readonly Dictionary<string, List<ConfigEntity>> orderedEntitiesByType =
        new(StringComparer.Ordinal);
    private static readonly Dictionary<string, GameDataConfigTypeDef> schemaByType =
        new(StringComparer.Ordinal);

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
        schemaByType.Clear();

        LoadSchema();
        LoadData();
        isLoaded = true;
    }

    public static ConfigEntity Get(string configType, string id)
    {
        Load();
        if (string.IsNullOrWhiteSpace(configType) || string.IsNullOrWhiteSpace(id))
        {
            return null;
        }

        return entitiesByType.TryGetValue(configType, out Dictionary<string, ConfigEntity> byId) &&
               byId.TryGetValue(id, out ConfigEntity entity)
            ? entity
            : null;
    }

    public static IReadOnlyList<ConfigEntity> GetAll(string configType)
    {
        Load();
        if (string.IsNullOrWhiteSpace(configType))
        {
            return Array.Empty<ConfigEntity>();
        }

        return orderedEntitiesByType.TryGetValue(configType, out List<ConfigEntity> list)
            ? list
            : Array.Empty<ConfigEntity>();
    }

    private static void LoadSchema()
    {
        TextAsset schemaAsset = Resources.Load<TextAsset>(SchemaResourcePath);
        if (schemaAsset == null)
        {
            Debug.LogWarning($"GameData schema file missing at Resources path '{SchemaResourcePath}'.");
            return;
        }

        GameDataSchemaRoot schema;
        try
        {
            schema = JsonConvert.DeserializeObject<GameDataSchemaRoot>(schemaAsset.text) ?? new GameDataSchemaRoot();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to parse game data schema: {ex.Message}");
            return;
        }

        for (int i = 0; i < schema.configTypes.Count; i++)
        {
            GameDataConfigTypeDef typeDef = schema.configTypes[i];
            if (string.IsNullOrWhiteSpace(typeDef.name))
            {
                Debug.LogWarning("GameData schema contains a config type with an empty name. Skipped.");
                continue;
            }

            schemaByType[typeDef.name] = typeDef;
        }
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

        foreach ((string typeName, JToken token) in configs)
        {
            if (!schemaByType.TryGetValue(typeName, out GameDataConfigTypeDef typeDef))
            {
                Debug.LogWarning($"GameData type '{typeName}' exists in data but not in schema. Skipped.");
                continue;
            }

            if (token is not JArray array)
            {
                Debug.LogWarning($"GameData type '{typeName}' must be an array.");
                continue;
            }

            Dictionary<string, ConfigEntity> byId = new(StringComparer.Ordinal);
            List<ConfigEntity> ordered = new();
            entitiesByType[typeName] = byId;
            orderedEntitiesByType[typeName] = ordered;

            for (int i = 0; i < array.Count; i++)
            {
                if (array[i] is not JObject entry)
                {
                    Debug.LogWarning($"GameData '{typeName}' contains a non-object entry at index {i}. Skipped.");
                    continue;
                }

                string id = entry.Value<string>("id");
                if (string.IsNullOrWhiteSpace(id))
                {
                    Debug.LogWarning($"GameData '{typeName}' entry at index {i} is missing id. Skipped.");
                    continue;
                }

                Dictionary<string, object> values = new(StringComparer.Ordinal);
                for (int f = 0; f < typeDef.fields.Count; f++)
                {
                    GameDataFieldDef field = typeDef.fields[f];
                    if (string.IsNullOrWhiteSpace(field.name))
                    {
                        continue;
                    }

                    JToken rawValue = entry[field.name];
                    values[field.name] = ConvertValue(field.type, rawValue);
                }

                ConfigEntity entity = new(typeName, id, values, typeDef.fields);
                byId[id] = entity;
                ordered.Add(entity);
            }
        }
    }

    private static object ConvertValue(GameDataFieldType type, JToken token)
    {
        if (token == null || token.Type == JTokenType.Null)
        {
            return GetDefaultValue(type);
        }

        try
        {
            return type switch
            {
                GameDataFieldType.Int => token.Value<int>(),
                GameDataFieldType.Float => token.Value<float>(),
                GameDataFieldType.Bool => token.Value<bool>(),
                GameDataFieldType.String => token.Value<string>() ?? string.Empty,
                GameDataFieldType.Vector2 => ParseVector2(token),
                GameDataFieldType.Vector3 => ParseVector3(token),
                GameDataFieldType.Color => ParseColor(token),
                GameDataFieldType.Ref => token.Value<string>() ?? string.Empty,
                GameDataFieldType.Sprite => token.Value<string>() ?? string.Empty,
                GameDataFieldType.Mesh => token.Value<string>() ?? string.Empty,
                GameDataFieldType.Prefab => token.Value<string>() ?? string.Empty,
                _ => GetDefaultValue(type)
            };
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"GameData value conversion failed for type '{type}': {ex.Message}");
            return GetDefaultValue(type);
        }
    }

    private static object GetDefaultValue(GameDataFieldType type)
    {
        return type switch
        {
            GameDataFieldType.Int => 0,
            GameDataFieldType.Float => 0f,
            GameDataFieldType.Bool => false,
            GameDataFieldType.String => string.Empty,
            GameDataFieldType.Vector2 => Vector2.zero,
            GameDataFieldType.Vector3 => Vector3.zero,
            GameDataFieldType.Color => Color.white,
            GameDataFieldType.Ref => string.Empty,
            GameDataFieldType.Sprite => string.Empty,
            GameDataFieldType.Mesh => string.Empty,
            GameDataFieldType.Prefab => string.Empty,
            _ => null
        };
    }

    private static Vector2 ParseVector2(JToken token)
    {
        JObject obj = token as JObject;
        return new Vector2(
            obj?.Value<float?>("x") ?? 0f,
            obj?.Value<float?>("y") ?? 0f);
    }

    private static Vector3 ParseVector3(JToken token)
    {
        JObject obj = token as JObject;
        return new Vector3(
            obj?.Value<float?>("x") ?? 0f,
            obj?.Value<float?>("y") ?? 0f,
            obj?.Value<float?>("z") ?? 0f);
    }

    private static Color ParseColor(JToken token)
    {
        JObject obj = token as JObject;
        return new Color(
            obj?.Value<float?>("r") ?? 0f,
            obj?.Value<float?>("g") ?? 0f,
            obj?.Value<float?>("b") ?? 0f,
            obj?.Value<float?>("a") ?? 1f);
    }
}
