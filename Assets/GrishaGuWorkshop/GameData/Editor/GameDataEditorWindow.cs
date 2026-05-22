using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace GrishaGuWorkshop.Editor
{
    public sealed class GameDataEditorWindow : EditorWindow
    {
        private const float LeftPaneWidth = 280f;

        private List<DataEntity> _entities = new();
        private List<Type> _entityTypes = new();
        private List<Type> _tagTypes = new();

        private DataEntity _selected;
        private bool _isDirty;
        private Vector2 _leftScroll;
        private Vector2 _rightScroll;
        private bool _tagsFoldout = true;

        private int _typeFilterIndex;
        private int _tagFilterIndex;
        private string _search = string.Empty;

        [MenuItem("Dev Tools/Game Data Editor")]
        public static void Open()
        {
            GetWindow<GameDataEditorWindow>("Game Data");
        }

        private void OnEnable()
        {
            DiscoverTypes();
            LoadAll();
        }

        private void OnGUI()
        {
            DrawToolbar();

            EditorGUILayout.BeginHorizontal();
            DrawLeftPane();
            DrawRightPane();
            EditorGUILayout.EndHorizontal();
        }

        private void DiscoverTypes()
        {
            _entityTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(GetAssemblyTypes)
                .Where(t => t is { IsClass: true, IsAbstract: false } && typeof(DataEntity).IsAssignableFrom(t))
                .OrderBy(t => t.Name)
                .ToList();

            _tagTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(GetAssemblyTypes)
                .Where(t => t is { IsClass: true, IsAbstract: false } && typeof(DataEntityTag).IsAssignableFrom(t))
                .OrderBy(t => t.Name)
                .ToList();
        }

        private static IEnumerable<Type> GetAssemblyTypes(Assembly assembly)
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

        private void LoadAll()
        {
            _entities = GameDataIO.LoadEntities();
            EnsureTagsInitialized();
            _selected = _entities.FirstOrDefault();
            _isDirty = false;
            Repaint();
        }

        private void EnsureTagsInitialized()
        {
            foreach (var entity in _entities)
            {
                entity.tags ??= new List<DataEntityTag>();
            }
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            EditorGUI.BeginDisabledGroup(!_isDirty);
            if (GUILayout.Button("Save", EditorStyles.toolbarButton))
            {
                Save();
            }
            EditorGUI.EndDisabledGroup();

            if (GUILayout.Button("Reload", EditorStyles.toolbarButton))
            {
                if (_isDirty && !EditorUtility.DisplayDialog(
                        "Reload",
                        "Unsaved changes will be lost. Continue?",
                        "Reload",
                        "Cancel"))
                {
                    return;
                }

                LoadAll();
            }

            if (GUILayout.Button("+ Add", EditorStyles.toolbarButton, GUILayout.Width(50)))
            {
                ShowAddEntityMenu();
            }

            GUILayout.Space(8);
            GUILayout.Label("Type:", GUILayout.Width(32));
            var typeLabels = BuildTypeFilterLabels();
            var newTypeIndex = EditorGUILayout.Popup(_typeFilterIndex, typeLabels, EditorStyles.toolbarPopup, GUILayout.Width(100));
            if (newTypeIndex != _typeFilterIndex)
            {
                _typeFilterIndex = newTypeIndex;
            }

            GUILayout.Space(4);
            GUILayout.Label("Tag:", GUILayout.Width(28));
            var tagLabels = BuildTagFilterLabels();
            var newTagIndex = EditorGUILayout.Popup(_tagFilterIndex, tagLabels, EditorStyles.toolbarPopup, GUILayout.Width(100));
            if (newTagIndex != _tagFilterIndex)
            {
                _tagFilterIndex = newTagIndex;
            }

            GUILayout.Space(4);
            _search = GUILayout.TextField(_search, EditorStyles.toolbarSearchField);

            EditorGUILayout.EndHorizontal();
        }

        private string[] BuildTypeFilterLabels()
        {
            var labels = new string[_entityTypes.Count + 1];
            labels[0] = "All";
            for (var i = 0; i < _entityTypes.Count; i++)
            {
                labels[i + 1] = _entityTypes[i].Name;
            }

            return labels;
        }

        private string[] BuildTagFilterLabels()
        {
            var labels = new string[_tagTypes.Count + 1];
            labels[0] = "All";
            for (var i = 0; i < _tagTypes.Count; i++)
            {
                labels[i + 1] = _tagTypes[i].Name;
            }

            return labels;
        }

        private Type GetSelectedTypeFilter()
        {
            if (_typeFilterIndex <= 0 || _typeFilterIndex > _entityTypes.Count)
            {
                return null;
            }

            return _entityTypes[_typeFilterIndex - 1];
        }

        private Type GetSelectedTagFilter()
        {
            if (_tagFilterIndex <= 0 || _tagFilterIndex > _tagTypes.Count)
            {
                return null;
            }

            return _tagTypes[_tagFilterIndex - 1];
        }

        private List<DataEntity> GetFilteredEntities()
        {
            IEnumerable<DataEntity> query = _entities;

            var typeFilter = GetSelectedTypeFilter();
            if (typeFilter != null)
            {
                query = query.Where(e => e.GetType() == typeFilter);
            }

            var tagFilter = GetSelectedTagFilter();
            if (tagFilter != null)
            {
                query = query.Where(e => e.tags != null && e.tags.Any(t => tagFilter.IsInstanceOfType(t)));
            }

            if (!string.IsNullOrWhiteSpace(_search))
            {
                var searchLower = _search.Trim().ToLowerInvariant();
                query = query.Where(e => e.Id != null && e.Id.ToLowerInvariant().Contains(searchLower));
            }

            return query
                .OrderBy(e => e.GetType().Name)
                .ThenBy(e => e.Id)
                .ToList();
        }

        private void DrawLeftPane()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(LeftPaneWidth));
            _leftScroll = EditorGUILayout.BeginScrollView(_leftScroll);

            var filtered = GetFilteredEntities();
            var typeFilter = GetSelectedTypeFilter();
            string currentGroup = null;

            foreach (var entity in filtered)
            {
                var typeName = entity.GetType().Name;
                if (typeFilter == null && typeName != currentGroup)
                {
                    currentGroup = typeName;
                    EditorGUILayout.Space(4);
                    EditorGUILayout.LabelField(typeName, EditorStyles.boldLabel);
                }

                DrawEntityRow(entity);
            }

            if (filtered.Count == 0)
            {
                EditorGUILayout.HelpBox("No entities match the current filters.", MessageType.Info);
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawEntityRow(DataEntity entity)
        {
            var isSelected = _selected == entity;
            var style = isSelected ? "SelectionRect" : EditorStyles.label;

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button(entity.Id, style, GUILayout.ExpandWidth(true)))
            {
                _selected = entity;
                GUI.FocusControl(null);
            }

            if (GUILayout.Button("D", EditorStyles.miniButton, GUILayout.Width(20)))
            {
                DuplicateEntity(entity);
            }

            if (GUILayout.Button("x", EditorStyles.miniButton, GUILayout.Width(20)))
            {
                DeleteEntity(entity);
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawRightPane()
        {
            EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
            _rightScroll = EditorGUILayout.BeginScrollView(_rightScroll);

            if (_selected == null)
            {
                EditorGUILayout.HelpBox("Select an entity from the list.", MessageType.Info);
            }
            else
            {
                DrawEntityDetails(_selected);
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawEntityDetails(DataEntity entity)
        {
            EditorGUILayout.LabelField("Type", entity.GetType().Name, EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            var newId = EditorGUILayout.TextField("Id", entity.Id);
            if (EditorGUI.EndChangeCheck())
            {
                entity.Id = newId;
                MarkDirty();
            }

            DrawIdValidation(entity);

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Fields", EditorStyles.boldLabel);
            DrawObjectFields(entity);

            EditorGUILayout.Space(8);
            _tagsFoldout = EditorGUILayout.Foldout(_tagsFoldout, "Tags", true);
            if (_tagsFoldout)
            {
                DrawTagsSection(entity);
            }
        }

        private void DrawIdValidation(DataEntity entity)
        {
            if (string.IsNullOrWhiteSpace(entity.Id))
            {
                EditorGUILayout.HelpBox("Id cannot be empty.", MessageType.Error);
                return;
            }

            var duplicate = _entities.Any(e =>
                e != entity &&
                e.GetType() == entity.GetType() &&
                string.Equals(e.Id, entity.Id, StringComparison.Ordinal));

            if (duplicate)
            {
                EditorGUILayout.HelpBox(
                    $"Another {entity.GetType().Name} already uses id '{entity.Id}'.",
                    MessageType.Error);
            }
        }

        private void DrawTagsSection(DataEntity entity)
        {
            entity.tags ??= new List<DataEntityTag>();

            for (var i = 0; i < entity.tags.Count; i++)
            {
                var tag = entity.tags[i];
                if (tag == null)
                {
                    continue;
                }

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(tag.GetType().Name, EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Remove", GUILayout.Width(60)))
                {
                    entity.tags.RemoveAt(i);
                    MarkDirty();
                    break;
                }
                EditorGUILayout.EndHorizontal();

                DrawObjectFields(tag);
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(4);
            }

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("+ Add tag", GUILayout.Width(80)))
            {
                ShowAddTagMenu(entity);
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawObjectFields(object target)
        {
            if (target == null)
            {
                return;
            }

            var type = target.GetType();

            foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public))
            {
                if (ShouldSkipMember(field.Name))
                {
                    continue;
                }

                if (field.GetCustomAttribute<JsonIgnoreAttribute>() != null)
                {
                    continue;
                }

                EditorGUI.BeginChangeCheck();
                var value = field.GetValue(target);
                var newValue = DrawMemberValue(field.Name, value, field.FieldType);
                if (EditorGUI.EndChangeCheck())
                {
                    field.SetValue(target, newValue);
                    MarkDirty();
                }
            }

            foreach (var prop in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                if (!prop.CanRead || !prop.CanWrite || prop.GetIndexParameters().Length > 0)
                {
                    continue;
                }

                if (ShouldSkipMember(prop.Name))
                {
                    continue;
                }

                if (prop.GetCustomAttribute<JsonIgnoreAttribute>() != null)
                {
                    continue;
                }

                EditorGUI.BeginChangeCheck();
                var value = prop.GetValue(target);
                var newValue = DrawMemberValue(prop.Name, value, prop.PropertyType);
                if (EditorGUI.EndChangeCheck())
                {
                    prop.SetValue(target, newValue);
                    MarkDirty();
                }
            }
        }

        private static bool ShouldSkipMember(string name)
        {
            return name is "Id" or "tags";
        }

        private static object DrawMemberValue(string label, object value, Type memberType)
        {
            if (memberType == typeof(int))
            {
                return EditorGUILayout.IntField(label, value is int i ? i : 0);
            }

            if (memberType == typeof(long))
            {
                return EditorGUILayout.LongField(label, value is long l ? l : 0L);
            }

            if (memberType == typeof(float))
            {
                return EditorGUILayout.FloatField(label, value is float f ? f : 0f);
            }

            if (memberType == typeof(double))
            {
                return EditorGUILayout.DoubleField(label, value is double d ? d : 0d);
            }

            if (memberType == typeof(bool))
            {
                return EditorGUILayout.Toggle(label, value is bool b && b);
            }

            if (memberType == typeof(string))
            {
                return EditorGUILayout.TextField(label, value as string ?? string.Empty);
            }

            if (memberType.IsEnum)
            {
                return EditorGUILayout.EnumPopup(label, value is Enum e ? e : (Enum)Enum.GetValues(memberType).GetValue(0));
            }

            if (memberType == typeof(Vector2))
            {
                return EditorGUILayout.Vector2Field(label, value is Vector2 v2 ? v2 : Vector2.zero);
            }

            if (memberType == typeof(Vector3))
            {
                return EditorGUILayout.Vector3Field(label, value is Vector3 v3 ? v3 : Vector3.zero);
            }

            if (memberType == typeof(Color))
            {
                return EditorGUILayout.ColorField(label, value is Color c ? c : Color.white);
            }

            if (memberType == typeof(List<string>))
            {
                return DrawStringList(label, value as List<string>);
            }

            if (memberType == typeof(List<int>))
            {
                return DrawIntList(label, value as List<int>);
            }

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.TextField(label, "unsupported type — edit JSON manually");
            EditorGUI.EndDisabledGroup();
            return value;
        }

        private static List<string> DrawStringList(string label, List<string> list)
        {
            list ??= new List<string>();
            EditorGUILayout.LabelField(label);
            EditorGUI.indentLevel++;
            for (var i = 0; i < list.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                list[i] = EditorGUILayout.TextField($"[{i}]", list[i]);
                if (GUILayout.Button("-", GUILayout.Width(22)))
                {
                    list.RemoveAt(i);
                    break;
                }
                EditorGUILayout.EndHorizontal();
            }

            if (GUILayout.Button("Add item"))
            {
                list.Add(string.Empty);
            }

            EditorGUI.indentLevel--;
            return list;
        }

        private static List<int> DrawIntList(string label, List<int> list)
        {
            list ??= new List<int>();
            EditorGUILayout.LabelField(label);
            EditorGUI.indentLevel++;
            for (var i = 0; i < list.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                list[i] = EditorGUILayout.IntField($"[{i}]", list[i]);
                if (GUILayout.Button("-", GUILayout.Width(22)))
                {
                    list.RemoveAt(i);
                    break;
                }
                EditorGUILayout.EndHorizontal();
            }

            if (GUILayout.Button("Add item"))
            {
                list.Add(0);
            }

            EditorGUI.indentLevel--;
            return list;
        }

        private void ShowAddEntityMenu()
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent(nameof(DataEntity)), false, () => AddEntity(typeof(DataEntity)));
            foreach (var entityType in _entityTypes)
            {
                menu.AddItem(new GUIContent(entityType.Name), false, () => AddEntity(entityType));
            }

            menu.ShowAsContext();
        }

        private void AddEntity(Type entityType)
        {
            var entity = (DataEntity)Activator.CreateInstance(entityType);
            entity.tags = new List<DataEntityTag>();
            entity.Id = GenerateUniqueId(entityType);
            _entities.Add(entity);
            _selected = entity;
            MarkDirty();
            Repaint();
        }

        private string GenerateUniqueId(Type entityType)
        {
            var baseId = $"new_{entityType.Name}";
            var id = baseId;
            var index = 1;
            while (_entities.Any(e => e.GetType() == entityType && e.Id == id))
            {
                id = $"{baseId}_{index}";
                index++;
            }

            return id;
        }

        private void DuplicateEntity(DataEntity source)
        {
            var serializer = GameDataIO.CreateSerializer();
            var copy = (DataEntity)JObject.FromObject(source, serializer).ToObject(source.GetType(), serializer);
            copy.tags ??= new List<DataEntityTag>();
            copy.Id = GenerateUniqueId(source.GetType());
            _entities.Add(copy);
            _selected = copy;
            MarkDirty();
        }

        private void DeleteEntity(DataEntity entity)
        {
            if (!EditorUtility.DisplayDialog("Delete Entity", $"Delete '{entity.Id}'?", "Delete", "Cancel"))
            {
                return;
            }

            _entities.Remove(entity);
            if (_selected == entity)
            {
                _selected = _entities.FirstOrDefault();
            }

            MarkDirty();
        }

        private void ShowAddTagMenu(DataEntity entity)
        {
            entity.tags ??= new List<DataEntityTag>();
            var menu = new GenericMenu();
            var hasItems = false;

            foreach (var tagType in _tagTypes)
            {
                if (entity.tags.Any(t => tagType.IsInstanceOfType(t)))
                {
                    continue;
                }

                hasItems = true;
                menu.AddItem(new GUIContent(tagType.Name), false, () => AddTag(entity, tagType));
            }

            if (!hasItems)
            {
                menu.AddDisabledItem(new GUIContent("No tags available"));
            }

            menu.ShowAsContext();
        }

        private void AddTag(DataEntity entity, Type tagType)
        {
            entity.tags ??= new List<DataEntityTag>();
            var tag = (DataEntityTag)Activator.CreateInstance(tagType);
            entity.tags.Add(tag);
            MarkDirty();
        }

        private void Save()
        {
            var invalid = _entities
                .GroupBy(e => e.GetType())
                .SelectMany(g => g
                    .Where(e => string.IsNullOrWhiteSpace(e.Id))
                    .Select(e => $"{g.Key.Name}: empty id"))
                .Concat(_entities
                    .GroupBy(e => new { Key = e.GetType(), e.Id })
                    .Where(g => g.Count() > 1)
                    .Select(g => $"{g.Key.GetType().Name}: duplicate id '{g.Key.Id}'"))
                .ToList();

            if (invalid.Count > 0)
            {
                EditorUtility.DisplayDialog(
                    "Cannot Save",
                    string.Join("\n", invalid),
                    "OK");
                return;
            }

            var root = GameDataIO.LoadDataRoot();
            var configs = GameDataIO.GetConfigsObject(root);
            var serializer = GameDataIO.CreateSerializer();

            foreach (var entityType in _entityTypes)
            {
                var array = new JArray();
                foreach (var entity in _entities.Where(e => e.GetType() == entityType))
                {
                    array.Add(JObject.FromObject(entity, serializer));
                }

                configs[entityType.Name] = array;
            }

            GameDataIO.Save(root);
            _isDirty = false;
            Debug.Log("Game data saved.");
        }

        private void MarkDirty()
        {
            _isDirty = true;
        }
    }
}
