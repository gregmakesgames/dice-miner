using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

public sealed class ConfigEntity
{
    private readonly Dictionary<string, object> values;
    private readonly Dictionary<string, GameDataFieldDef> fieldsByName;

    public ConfigEntity(string configTypeName, string id, Dictionary<string, object> values, List<GameDataFieldDef> fields)
    {
        ConfigTypeName = configTypeName;
        Id = id;
        this.values = values;
        fieldsByName = new Dictionary<string, GameDataFieldDef>(StringComparer.Ordinal);
        for (int i = 0; i < fields.Count; i++)
        {
            fieldsByName[fields[i].name] = fields[i];
        }
    }

    public string ConfigTypeName { get; }
    public string Id { get; private set; }

    public IReadOnlyDictionary<string, object> Values => values;

    public void SetId(string id)
    {
        Id = id;
    }

    public int GetInt(string fieldName)
    {
        if (TryGetValue(fieldName, out object value))
        {
            if (value is int intValue)
            {
                return intValue;
            }

            if (value is long longValue)
            {
                return (int)longValue;
            }

            if (value is float floatValue)
            {
                return Mathf.RoundToInt(floatValue);
            }

            if (value is double doubleValue)
            {
                return Mathf.RoundToInt((float)doubleValue);
            }
        }

        return 0;
    }

    public float GetFloat(string fieldName)
    {
        if (TryGetValue(fieldName, out object value))
        {
            if (value is float floatValue)
            {
                return floatValue;
            }

            if (value is double doubleValue)
            {
                return (float)doubleValue;
            }

            if (value is int intValue)
            {
                return intValue;
            }

            if (value is long longValue)
            {
                return longValue;
            }
        }

        return 0f;
    }

    public bool GetBool(string fieldName)
    {
        if (TryGetValue(fieldName, out object value))
        {
            if (value is bool boolValue)
            {
                return boolValue;
            }
        }

        return false;
    }

    public string GetString(string fieldName)
    {
        if (TryGetValue(fieldName, out object value))
        {
            return value?.ToString() ?? string.Empty;
        }

        return string.Empty;
    }

    public Vector2 GetVector2(string fieldName)
    {
        if (TryGetValue(fieldName, out object value))
        {
            if (value is Vector2 vector2)
            {
                return vector2;
            }

            if (value is JObject obj)
            {
                return new Vector2(
                    obj.Value<float?>("x") ?? 0f,
                    obj.Value<float?>("y") ?? 0f);
            }
        }

        return Vector2.zero;
    }

    public Vector3 GetVector3(string fieldName)
    {
        if (TryGetValue(fieldName, out object value))
        {
            if (value is Vector3 vector3)
            {
                return vector3;
            }

            if (value is JObject obj)
            {
                return new Vector3(
                    obj.Value<float?>("x") ?? 0f,
                    obj.Value<float?>("y") ?? 0f,
                    obj.Value<float?>("z") ?? 0f);
            }
        }

        return Vector3.zero;
    }

    public Color GetColor(string fieldName)
    {
        if (TryGetValue(fieldName, out object value))
        {
            if (value is Color color)
            {
                return color;
            }

            if (value is JObject obj)
            {
                return new Color(
                    obj.Value<float?>("r") ?? 0f,
                    obj.Value<float?>("g") ?? 0f,
                    obj.Value<float?>("b") ?? 0f,
                    obj.Value<float?>("a") ?? 1f);
            }
        }

        return Color.white;
    }

    public ConfigEntity GetRef(string fieldName)
    {
        if (!fieldsByName.TryGetValue(fieldName, out GameDataFieldDef def) ||
            def.type != GameDataFieldType.Ref ||
            string.IsNullOrWhiteSpace(def.refType))
        {
            return null;
        }

        string targetId = GetString(fieldName);
        if (string.IsNullOrWhiteSpace(targetId))
        {
            return null;
        }

        ConfigEntity target = GameDataRegistry.Get(def.refType, targetId);
        if (target == null)
        {
            Debug.LogWarning($"GameData reference '{ConfigTypeName}.{fieldName}' points to missing '{def.refType}:{targetId}'.");
        }

        return target;
    }

    public Sprite GetSprite(string fieldName)
    {
        return LoadAsset<Sprite>(fieldName, GameDataFieldType.Sprite);
    }

    public Mesh GetMesh(string fieldName)
    {
        return LoadAsset<Mesh>(fieldName, GameDataFieldType.Mesh);
    }

    public GameObject GetPrefab(string fieldName)
    {
        return LoadAsset<GameObject>(fieldName, GameDataFieldType.Prefab);
    }

    public string GetAssetPath(string fieldName)
    {
        return GetString(fieldName);
    }

    public bool TryGetValue(string fieldName, out object value)
    {
        return values.TryGetValue(fieldName, out value);
    }

    private T LoadAsset<T>(string fieldName, GameDataFieldType expectedType) where T : UnityEngine.Object
    {
        if (!fieldsByName.TryGetValue(fieldName, out GameDataFieldDef def) || def.type != expectedType)
        {
            return null;
        }

        string path = GetString(fieldName);
        if (string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        T asset = Resources.Load<T>(path);
        if (asset == null)
        {
            Debug.LogWarning($"GameData asset '{ConfigTypeName}.{fieldName}' could not load '{path}' as {typeof(T).Name}.");
        }

        return asset;
    }
}
