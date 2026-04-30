using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

public sealed class GameDataEditorWindow : EditorWindow
{
    private enum MainTab
    {
        Configs,
        Schema
    }

    private GameDataSchemaRoot schema;
    private JObject dataRoot;
    private MainTab mainTab;
    private int selectedTypeIndex;
    private readonly Dictionary<string, int> selectedEntityByType = new(StringComparer.Ordinal);
    private Vector2 schemaTypeScroll;
    private Vector2 schemaFieldScroll;
    private Vector2 entityListScroll;
    private Vector2 entityEditScroll;
    private bool isDirty;

    [MenuItem("Tools/Game Data Editor")]
    public static void Open()
    {
        GetWindow<GameDataEditorWindow>("Game Data Editor");
    }

    private void OnEnable()
    {
        Reload();
    }

    private void OnGUI()
    {
        DrawToolbar();
        DrawMainTabs();

        if (schema == null)
        {
            Reload();
        }

        if (schema == null)
        {
            EditorGUILayout.HelpBox("Failed to load schema.", MessageType.Error);
            return;
        }

        switch (mainTab)
        {
            case MainTab.Configs:
                DrawConfigsTab();
                break;
            case MainTab.Schema:
                DrawSchemaTab();
                break;
        }
    }

    private void DrawToolbar()
    {
        using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
        {
            if (GUILayout.Button("Reload", EditorStyles.toolbarButton))
            {
                if (!isDirty || EditorUtility.DisplayDialog("Discard changes?", "Unsaved changes will be lost.", "Discard", "Cancel"))
                {
                    Reload();
                }
            }

            if (GUILayout.Button("Save", EditorStyles.toolbarButton))
            {
                Save();
            }

            GUILayout.FlexibleSpace();
            GUIStyle statusStyle = new(EditorStyles.label)
            {
                normal = { textColor = isDirty ? new Color(0.85f, 0.45f, 0.1f) : EditorStyles.label.normal.textColor }
            };
            GUILayout.Label(isDirty ? "Unsaved changes" : "Up to date", statusStyle);
        }
    }

    private void DrawMainTabs()
    {
        mainTab = (MainTab)GUILayout.Toolbar((int)mainTab, new[] { "Configs", "Schema" });
        EditorGUILayout.Space(4f);
    }

    private void DrawSchemaTab()
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            DrawSchemaTypesPanel();
            EditorGUILayout.Space(6f);
            DrawSchemaFieldsPanel();
        }
    }

    private void DrawSchemaTypesPanel()
    {
        using (new EditorGUILayout.VerticalScope(GUILayout.Width(260f)))
        {
            EditorGUILayout.LabelField("Config Types", EditorStyles.boldLabel);
            schemaTypeScroll = EditorGUILayout.BeginScrollView(schemaTypeScroll);
            for (int i = 0; i < schema.configTypes.Count; i++)
            {
                bool isSelected = i == selectedTypeIndex;
                if (GUILayout.Toggle(isSelected, schema.configTypes[i].name, "Button") && !isSelected)
                {
                    selectedTypeIndex = i;
                }
            }

            EditorGUILayout.EndScrollView();

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("+ Add Type"))
                {
                    string baseName = "NewType";
                    string uniqueName = GenerateUniqueTypeName(baseName);
                    schema.configTypes.Add(new GameDataConfigTypeDef { name = uniqueName });
                    GameDataIO.GetOrCreateTypeArray(dataRoot, uniqueName);
                    selectedTypeIndex = schema.configTypes.Count - 1;
                    MarkDirty();
                }

                if (GUILayout.Button("Delete Type") && TryGetSelectedType(out GameDataConfigTypeDef type))
                {
                    if (EditorUtility.DisplayDialog("Delete type?", $"Delete '{type.name}' and all its data entries?", "Delete", "Cancel"))
                    {
                        DeleteType(type.name);
                        schema.configTypes.RemoveAt(selectedTypeIndex);
                        selectedTypeIndex = Mathf.Clamp(selectedTypeIndex, 0, schema.configTypes.Count - 1);
                        MarkDirty();
                    }
                }
            }
        }
    }

    private void DrawSchemaFieldsPanel()
    {
        using (new EditorGUILayout.VerticalScope())
        {
            if (!TryGetSelectedType(out GameDataConfigTypeDef type))
            {
                EditorGUILayout.HelpBox("No type selected.", MessageType.Info);
                return;
            }

            EditorGUILayout.LabelField("Type Definition", EditorStyles.boldLabel);
            string oldTypeName = type.name;
            string newTypeName = EditorGUILayout.TextField("Type Name", type.name);
            if (newTypeName != oldTypeName && !string.IsNullOrWhiteSpace(newTypeName))
            {
                type.name = newTypeName.Trim();
                OnTypeRenamed(oldTypeName, type.name);
                MarkDirty();
            }

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Fields", EditorStyles.boldLabel);
            schemaFieldScroll = EditorGUILayout.BeginScrollView(schemaFieldScroll);
            for (int i = 0; i < type.fields.Count; i++)
            {
                DrawFieldRow(type, i);
                EditorGUILayout.Space(4f);
            }

            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("+ Add Field"))
            {
                type.fields.Add(new GameDataFieldDef
                {
                    name = GenerateUniqueFieldName(type, "newField"),
                    type = GameDataFieldType.String
                });
                ApplySchemaToTypeData(type);
                MarkDirty();
            }
        }
    }

    private void DrawFieldRow(GameDataConfigTypeDef type, int fieldIndex)
    {
        GameDataFieldDef field = type.fields[fieldIndex];
        using (new EditorGUILayout.VerticalScope("box"))
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                string oldFieldName = field.name;
                string newFieldName = EditorGUILayout.TextField("Name", field.name);
                if (newFieldName != oldFieldName && !string.IsNullOrWhiteSpace(newFieldName))
                {
                    field.name = newFieldName.Trim();
                    RenameField(type, oldFieldName, field.name);
                    MarkDirty();
                }

                if (GUILayout.Button("Delete", GUILayout.Width(72f)))
                {
                    if (EditorUtility.DisplayDialog("Delete field?", $"Delete field '{field.name}' from type '{type.name}'?", "Delete", "Cancel"))
                    {
                        DeleteField(type, field.name);
                        type.fields.RemoveAt(fieldIndex);
                        MarkDirty();
                    }

                    return;
                }
            }

            GameDataFieldType previousType = field.type;
            field.type = (GameDataFieldType)EditorGUILayout.EnumPopup("Type", field.type);
            if (previousType != field.type)
            {
                if (field.type != GameDataFieldType.Ref)
                {
                    field.refType = string.Empty;
                }

                ApplySchemaToTypeData(type);
                MarkDirty();
            }

            if (field.type == GameDataFieldType.Ref)
            {
                string[] typeNames = GetTypeNames();
                int current = Mathf.Max(0, Array.IndexOf(typeNames, field.refType));
                int next = EditorGUILayout.Popup("Ref Type", current, typeNames);
                string nextType = typeNames.Length == 0 ? string.Empty : typeNames[next];
                if (nextType != field.refType)
                {
                    field.refType = nextType;
                    MarkDirty();
                }
            }
        }
    }

    private void DrawConfigsTab()
    {
        if (schema.configTypes.Count == 0)
        {
            EditorGUILayout.HelpBox("Schema has no config types. Add one in the Schema tab.", MessageType.Info);
            return;
        }

        string[] typeNames = GetTypeNames();
        selectedTypeIndex = Mathf.Clamp(selectedTypeIndex, 0, typeNames.Length - 1);
        selectedTypeIndex = GUILayout.Toolbar(selectedTypeIndex, typeNames);
        EditorGUILayout.Space(6f);

        GameDataConfigTypeDef type = schema.configTypes[selectedTypeIndex];

        using (new EditorGUILayout.HorizontalScope())
        {
            DrawEntityList(type);
            EditorGUILayout.Space(6f);
            DrawEntityEditor(type);
        }
    }

    private void DrawEntityList(GameDataConfigTypeDef type)
    {
        using (new EditorGUILayout.VerticalScope(GUILayout.Width(280f)))
        {
            EditorGUILayout.LabelField($"{type.name} Entities", EditorStyles.boldLabel);
            JArray array = GameDataIO.GetOrCreateTypeArray(dataRoot, type.name);
            int selected = GetSelectedEntityIndex(type.name, array.Count);
            entityListScroll = EditorGUILayout.BeginScrollView(entityListScroll);

            for (int i = 0; i < array.Count; i++)
            {
                if (array[i] is not JObject obj)
                {
                    continue;
                }

                string id = obj.Value<string>("id") ?? $"entity_{i}";
                bool isSelected = i == selected;
                if (GUILayout.Toggle(isSelected, id, "Button") && !isSelected)
                {
                    SetSelectedEntityIndex(type.name, i);
                }
            }

            EditorGUILayout.EndScrollView();

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("+ Add"))
                {
                    JObject newEntity = CreateEntityWithDefaults(type, GenerateUniqueEntityId(type.name, "new_entity"));
                    array.Add(newEntity);
                    SetSelectedEntityIndex(type.name, array.Count - 1);
                    MarkDirty();
                }

                if (GUILayout.Button("Duplicate") && selected >= 0 && selected < array.Count && array[selected] is JObject selectedObj)
                {
                    JObject duplicate = (JObject)selectedObj.DeepClone();
                    duplicate["id"] = GenerateUniqueEntityId(type.name, (selectedObj.Value<string>("id") ?? "copy") + "_copy");
                    array.Add(duplicate);
                    SetSelectedEntityIndex(type.name, array.Count - 1);
                    MarkDirty();
                }

                if (GUILayout.Button("Delete") && selected >= 0 && selected < array.Count && array[selected] is JObject deleteObj)
                {
                    string deletedId = deleteObj.Value<string>("id") ?? string.Empty;
                    int refs = CountReferencesTo(type.name, deletedId);
                    string msg = refs > 0
                        ? $"Delete entity '{deletedId}'? {refs} reference(s) point to it."
                        : $"Delete entity '{deletedId}'?";

                    if (EditorUtility.DisplayDialog("Delete entity?", msg, "Delete", "Cancel"))
                    {
                        array.RemoveAt(selected);
                        SetSelectedEntityIndex(type.name, Mathf.Clamp(selected, 0, array.Count - 1));
                        MarkDirty();
                    }
                }
            }
        }
    }

    private void DrawEntityEditor(GameDataConfigTypeDef type)
    {
        using (new EditorGUILayout.VerticalScope())
        {
            JArray array = GameDataIO.GetOrCreateTypeArray(dataRoot, type.name);
            int selected = GetSelectedEntityIndex(type.name, array.Count);
            if (selected < 0 || selected >= array.Count || array[selected] is not JObject entity)
            {
                EditorGUILayout.HelpBox("Select an entity to edit.", MessageType.Info);
                return;
            }

            entityEditScroll = EditorGUILayout.BeginScrollView(entityEditScroll);
            DrawEntityIdEditor(type.name, entity);
            EditorGUILayout.Space(4f);
            DrawEntityFields(type, entity);
            EditorGUILayout.EndScrollView();
        }
    }

    private void DrawEntityIdEditor(string typeName, JObject entity)
    {
        string oldId = entity.Value<string>("id") ?? string.Empty;
        string newId = EditorGUILayout.TextField("id", oldId);
        if (newId == oldId || string.IsNullOrWhiteSpace(newId))
        {
            return;
        }

        newId = newId.Trim();
        if (EntityIdExists(typeName, newId))
        {
            EditorGUILayout.HelpBox($"Id '{newId}' already exists in '{typeName}'.", MessageType.Warning);
            return;
        }

        int option = EditorUtility.DisplayDialogComplex(
            "Update references?",
            $"Entity id changed from '{oldId}' to '{newId}'. Rewrite references to this id?",
            "Rewrite references",
            "Leave dangling",
            "Cancel");

        if (option == 2)
        {
            return;
        }

        entity["id"] = newId;
        if (option == 0)
        {
            RewriteReferences(typeName, oldId, newId);
        }

        MarkDirty();
    }

    private void DrawEntityFields(GameDataConfigTypeDef type, JObject entity)
    {
        for (int i = 0; i < type.fields.Count; i++)
        {
            GameDataFieldDef field = type.fields[i];
            JToken token = entity[field.name];
            JToken newToken = DrawFieldControl(field, token);
            if (!JToken.DeepEquals(token, newToken))
            {
                entity[field.name] = newToken;
                MarkDirty();
            }
        }
    }

    private JToken DrawFieldControl(GameDataFieldDef field, JToken token)
    {
        switch (field.type)
        {
            case GameDataFieldType.Int:
                return new JValue(EditorGUILayout.IntField(field.name, token?.Value<int>() ?? 0));
            case GameDataFieldType.Float:
                return new JValue(EditorGUILayout.FloatField(field.name, token?.Value<float>() ?? 0f));
            case GameDataFieldType.Bool:
                return new JValue(EditorGUILayout.Toggle(field.name, token?.Value<bool>() ?? false));
            case GameDataFieldType.String:
                return new JValue(EditorGUILayout.TextField(field.name, token?.Value<string>() ?? string.Empty));
            case GameDataFieldType.Vector2:
            {
                Vector2 vec = new(
                    token?["x"]?.Value<float>() ?? 0f,
                    token?["y"]?.Value<float>() ?? 0f);
                vec = EditorGUILayout.Vector2Field(field.name, vec);
                return new JObject { ["x"] = vec.x, ["y"] = vec.y };
            }
            case GameDataFieldType.Vector3:
            {
                Vector3 vec = new(
                    token?["x"]?.Value<float>() ?? 0f,
                    token?["y"]?.Value<float>() ?? 0f,
                    token?["z"]?.Value<float>() ?? 0f);
                vec = EditorGUILayout.Vector3Field(field.name, vec);
                return new JObject { ["x"] = vec.x, ["y"] = vec.y, ["z"] = vec.z };
            }
            case GameDataFieldType.Color:
            {
                Color color = new(
                    token?["r"]?.Value<float>() ?? 0f,
                    token?["g"]?.Value<float>() ?? 0f,
                    token?["b"]?.Value<float>() ?? 0f,
                    token?["a"]?.Value<float>() ?? 1f);
                color = EditorGUILayout.ColorField(field.name, color);
                return new JObject { ["r"] = color.r, ["g"] = color.g, ["b"] = color.b, ["a"] = color.a };
            }
            case GameDataFieldType.Ref:
            {
                string[] ids = GetReferenceOptions(field.refType);
                string current = token?.Value<string>() ?? string.Empty;
                int selected = Mathf.Max(0, Array.IndexOf(ids, current));
                int next = EditorGUILayout.Popup(field.name, selected, ids);
                string selectedId = ids.Length == 0 ? string.Empty : ids[next];
                return new JValue(selectedId == "<none>" ? string.Empty : selectedId);
            }
            case GameDataFieldType.Sprite:
                return DrawAssetField<Sprite>(field.name, token);
            case GameDataFieldType.Mesh:
                return DrawAssetField<Mesh>(field.name, token);
            case GameDataFieldType.Prefab:
                return DrawPrefabAssetField(field.name, token);
            default:
                return new JValue(string.Empty);
        }
    }

    private static JToken DrawAssetField<T>(string label, JToken token) where T : UnityEngine.Object
    {
        string currentPath = token?.Value<string>() ?? string.Empty;
        T current = LoadResourceAsset<T>(currentPath);
        T next = (T)EditorGUILayout.ObjectField(label, current, typeof(T), false);
        string nextPath = ToResourcesPath(next);
        DrawAssetWarning(next, nextPath);
        return new JValue(nextPath);
    }

    private static JToken DrawPrefabAssetField(string label, JToken token)
    {
        string currentPath = token?.Value<string>() ?? string.Empty;
        GameObject current = LoadResourceAsset<GameObject>(currentPath);
        GameObject next = (GameObject)EditorGUILayout.ObjectField(label, current, typeof(GameObject), false);
        if (next != null && PrefabUtility.GetPrefabAssetType(next) == PrefabAssetType.NotAPrefab)
        {
            EditorGUILayout.HelpBox($"'{next.name}' is not a prefab asset.", MessageType.Warning);
            return new JValue(currentPath);
        }

        string nextPath = ToResourcesPath(next);
        DrawAssetWarning(next, nextPath);
        return new JValue(nextPath);
    }

    private static void DrawAssetWarning(UnityEngine.Object asset, string resourcesPath)
    {
        if (asset != null && string.IsNullOrEmpty(resourcesPath))
        {
            EditorGUILayout.HelpBox(
                "Asset must live under a 'Resources' folder so it can be loaded at runtime.",
                MessageType.Warning);
        }
    }

    private static T LoadResourceAsset<T>(string resourcesPath) where T : UnityEngine.Object
    {
        return string.IsNullOrEmpty(resourcesPath) ? null : Resources.Load<T>(resourcesPath);
    }

    private static string ToResourcesPath(UnityEngine.Object asset)
    {
        if (asset == null)
        {
            return string.Empty;
        }

        string assetPath = AssetDatabase.GetAssetPath(asset);
        if (string.IsNullOrEmpty(assetPath))
        {
            return string.Empty;
        }

        const string marker = "/Resources/";
        int idx = assetPath.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
        if (idx < 0)
        {
            return string.Empty;
        }

        string relative = assetPath.Substring(idx + marker.Length);
        string withoutExtension = Path.ChangeExtension(relative, null);
        return withoutExtension == null ? string.Empty : withoutExtension.Replace('\\', '/');
    }

    private void DeleteType(string typeName)
    {
        JObject configs = GameDataIO.GetConfigsObject(dataRoot);
        configs.Remove(typeName);
        selectedEntityByType.Remove(typeName);

        for (int i = 0; i < schema.configTypes.Count; i++)
        {
            GameDataConfigTypeDef configType = schema.configTypes[i];
            for (int j = 0; j < configType.fields.Count; j++)
            {
                if (configType.fields[j].type == GameDataFieldType.Ref && configType.fields[j].refType == typeName)
                {
                    configType.fields[j].refType = string.Empty;
                }
            }
        }
    }

    private void OnTypeRenamed(string oldType, string newType)
    {
        if (string.IsNullOrWhiteSpace(oldType) || oldType == newType)
        {
            return;
        }

        JObject configs = GameDataIO.GetConfigsObject(dataRoot);
        if (configs[oldType] is JArray array)
        {
            configs.Remove(oldType);
            configs[newType] = array;
        }
        else
        {
            configs[newType] ??= new JArray();
        }

        if (selectedEntityByType.TryGetValue(oldType, out int selected))
        {
            selectedEntityByType.Remove(oldType);
            selectedEntityByType[newType] = selected;
        }

        for (int i = 0; i < schema.configTypes.Count; i++)
        {
            for (int j = 0; j < schema.configTypes[i].fields.Count; j++)
            {
                GameDataFieldDef field = schema.configTypes[i].fields[j];
                if (field.type == GameDataFieldType.Ref && field.refType == oldType)
                {
                    field.refType = newType;
                }
            }
        }
    }

    private void RenameField(GameDataConfigTypeDef type, string oldFieldName, string newFieldName)
    {
        if (string.IsNullOrWhiteSpace(oldFieldName) || oldFieldName == newFieldName)
        {
            return;
        }

        JArray array = GameDataIO.GetOrCreateTypeArray(dataRoot, type.name);
        for (int i = 0; i < array.Count; i++)
        {
            if (array[i] is JObject entity && entity.TryGetValue(oldFieldName, out JToken value))
            {
                entity.Remove(oldFieldName);
                entity[newFieldName] = value;
            }
        }
    }

    private void DeleteField(GameDataConfigTypeDef type, string fieldName)
    {
        JArray array = GameDataIO.GetOrCreateTypeArray(dataRoot, type.name);
        for (int i = 0; i < array.Count; i++)
        {
            if (array[i] is JObject entity)
            {
                entity.Remove(fieldName);
            }
        }
    }

    private int CountReferencesTo(string targetType, string targetId)
    {
        if (string.IsNullOrWhiteSpace(targetId))
        {
            return 0;
        }

        int count = 0;
        JObject configs = GameDataIO.GetConfigsObject(dataRoot);
        for (int t = 0; t < schema.configTypes.Count; t++)
        {
            GameDataConfigTypeDef type = schema.configTypes[t];
            if (configs[type.name] is not JArray array)
            {
                continue;
            }

            for (int i = 0; i < type.fields.Count; i++)
            {
                GameDataFieldDef field = type.fields[i];
                if (field.type != GameDataFieldType.Ref || field.refType != targetType)
                {
                    continue;
                }

                for (int e = 0; e < array.Count; e++)
                {
                    if (array[e] is JObject entity && entity.Value<string>(field.name) == targetId)
                    {
                        count++;
                    }
                }
            }
        }

        return count;
    }

    private void RewriteReferences(string targetType, string oldId, string newId)
    {
        JObject configs = GameDataIO.GetConfigsObject(dataRoot);
        for (int t = 0; t < schema.configTypes.Count; t++)
        {
            GameDataConfigTypeDef type = schema.configTypes[t];
            if (configs[type.name] is not JArray array)
            {
                continue;
            }

            for (int i = 0; i < type.fields.Count; i++)
            {
                GameDataFieldDef field = type.fields[i];
                if (field.type != GameDataFieldType.Ref || field.refType != targetType)
                {
                    continue;
                }

                for (int e = 0; e < array.Count; e++)
                {
                    if (array[e] is JObject entity && entity.Value<string>(field.name) == oldId)
                    {
                        entity[field.name] = newId;
                    }
                }
            }
        }
    }

    private void ApplySchemaToTypeData(GameDataConfigTypeDef type)
    {
        JArray array = GameDataIO.GetOrCreateTypeArray(dataRoot, type.name);
        HashSet<string> validFields = new(StringComparer.Ordinal);
        for (int i = 0; i < type.fields.Count; i++)
        {
            validFields.Add(type.fields[i].name);
        }

        for (int i = 0; i < array.Count; i++)
        {
            if (array[i] is not JObject entity)
            {
                continue;
            }

            for (int f = 0; f < type.fields.Count; f++)
            {
                GameDataFieldDef field = type.fields[f];
                if (!entity.ContainsKey(field.name))
                {
                    entity[field.name] = CreateDefaultValueToken(field.type);
                }
            }

            List<string> keysToRemove = new();
            foreach ((string key, JToken _) in entity)
            {
                if (key != "id" && !validFields.Contains(key))
                {
                    keysToRemove.Add(key);
                }
            }

            for (int k = 0; k < keysToRemove.Count; k++)
            {
                entity.Remove(keysToRemove[k]);
            }
        }
    }

    private JObject CreateEntityWithDefaults(GameDataConfigTypeDef type, string id)
    {
        JObject entity = new()
        {
            ["id"] = id
        };

        for (int i = 0; i < type.fields.Count; i++)
        {
            GameDataFieldDef field = type.fields[i];
            entity[field.name] = CreateDefaultValueToken(field.type);
        }

        return entity;
    }

    private static JToken CreateDefaultValueToken(GameDataFieldType type)
    {
        return type switch
        {
            GameDataFieldType.Int => new JValue(0),
            GameDataFieldType.Float => new JValue(0f),
            GameDataFieldType.Bool => new JValue(false),
            GameDataFieldType.String => new JValue(string.Empty),
            GameDataFieldType.Vector2 => new JObject { ["x"] = 0f, ["y"] = 0f },
            GameDataFieldType.Vector3 => new JObject { ["x"] = 0f, ["y"] = 0f, ["z"] = 0f },
            GameDataFieldType.Color => new JObject { ["r"] = 1f, ["g"] = 1f, ["b"] = 1f, ["a"] = 1f },
            GameDataFieldType.Ref => new JValue(string.Empty),
            GameDataFieldType.Sprite => new JValue(string.Empty),
            GameDataFieldType.Mesh => new JValue(string.Empty),
            GameDataFieldType.Prefab => new JValue(string.Empty),
            _ => JValue.CreateNull()
        };
    }

    private string[] GetTypeNames()
    {
        string[] names = new string[schema.configTypes.Count];
        for (int i = 0; i < schema.configTypes.Count; i++)
        {
            names[i] = schema.configTypes[i].name;
        }

        return names;
    }

    private string[] GetReferenceOptions(string refType)
    {
        List<string> ids = new() { "<none>" };
        if (!string.IsNullOrWhiteSpace(refType))
        {
            ids.AddRange(GameDataIO.GetEntityIds(dataRoot, refType));
        }

        return ids.ToArray();
    }

    private bool EntityIdExists(string typeName, string id)
    {
        JArray array = GameDataIO.GetOrCreateTypeArray(dataRoot, typeName);
        for (int i = 0; i < array.Count; i++)
        {
            if (array[i] is JObject obj && obj.Value<string>("id") == id)
            {
                return true;
            }
        }

        return false;
    }

    private string GenerateUniqueTypeName(string prefix)
    {
        int index = 1;
        string name = prefix;
        HashSet<string> existing = new(StringComparer.Ordinal);
        for (int i = 0; i < schema.configTypes.Count; i++)
        {
            existing.Add(schema.configTypes[i].name);
        }

        while (existing.Contains(name))
        {
            name = $"{prefix}{index}";
            index++;
        }

        return name;
    }

    private static string GenerateUniqueFieldName(GameDataConfigTypeDef type, string prefix)
    {
        int index = 1;
        string name = prefix;
        HashSet<string> existing = new(StringComparer.Ordinal);
        for (int i = 0; i < type.fields.Count; i++)
        {
            existing.Add(type.fields[i].name);
        }

        while (existing.Contains(name))
        {
            name = $"{prefix}{index}";
            index++;
        }

        return name;
    }

    private string GenerateUniqueEntityId(string typeName, string prefix)
    {
        int index = 1;
        string id = prefix;
        while (EntityIdExists(typeName, id))
        {
            id = $"{prefix}_{index}";
            index++;
        }

        return id;
    }

    private bool TryGetSelectedType(out GameDataConfigTypeDef type)
    {
        if (schema.configTypes.Count == 0)
        {
            type = null;
            return false;
        }

        selectedTypeIndex = Mathf.Clamp(selectedTypeIndex, 0, schema.configTypes.Count - 1);
        type = schema.configTypes[selectedTypeIndex];
        return true;
    }

    private int GetSelectedEntityIndex(string typeName, int count)
    {
        if (!selectedEntityByType.TryGetValue(typeName, out int selected))
        {
            selected = count > 0 ? 0 : -1;
            selectedEntityByType[typeName] = selected;
        }

        if (count == 0)
        {
            selected = -1;
        }
        else
        {
            selected = Mathf.Clamp(selected, 0, count - 1);
        }

        selectedEntityByType[typeName] = selected;
        return selected;
    }

    private void SetSelectedEntityIndex(string typeName, int index)
    {
        selectedEntityByType[typeName] = index;
    }

    private void Reload()
    {
        schema = GameDataIO.LoadSchema();
        dataRoot = GameDataIO.LoadData();
        selectedTypeIndex = Mathf.Clamp(selectedTypeIndex, 0, Mathf.Max(0, schema.configTypes.Count - 1));
        isDirty = false;

        for (int i = 0; i < schema.configTypes.Count; i++)
        {
            ApplySchemaToTypeData(schema.configTypes[i]);
        }
    }

    private void Save()
    {
        for (int i = 0; i < schema.configTypes.Count; i++)
        {
            ApplySchemaToTypeData(schema.configTypes[i]);
        }

        GameDataIO.Save(schema, dataRoot);
        isDirty = false;
    }

    private void MarkDirty()
    {
        isDirty = true;
    }
}
