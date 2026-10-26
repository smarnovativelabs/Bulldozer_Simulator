using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Bulldozer.EditorTools
{
    // Replaces the Hierponent callback with a bounded, invalidated cache and an idempotent toggle.
    [InitializeOnLoad]
    internal static class HierarchyComponentIcons
    {
        private sealed class Row
        {
            internal GameObject Target;
            internal Component[] Components;
            internal GUIContent[] Icons;
            internal float LabelWidth;
        }

        private const int MaxCacheRows = 2048;
        private const int MaxIcons = 4;
        private static readonly Dictionary<int, Row> Cache = new Dictionary<int, Row>();
        private static readonly List<Component> Buffer = new List<Component>(16);
        private static bool enabled;
        private static GUIStyle overflowStyle;
        private static readonly GUIContent OverflowContent = new GUIContent("…", "All components");

        static HierarchyComponentIcons()
        {
            enabled = EditorPrefs.GetBool(EditorToolUtility.PreferencePrefix + "HierarchyIcons", true);
            Register();
            EditorApplication.hierarchyChanged += Clear;
            Undo.undoRedoPerformed += Clear;
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
            ObjectChangeEvents.changesPublished += OnObjectsChanged;
        }

        internal static void Register()
        {
            EditorApplication.hierarchyWindowItemOnGUI -= Draw;
            if (enabled) EditorApplication.hierarchyWindowItemOnGUI += Draw;
        }

        [MenuItem(EditorToolUtility.MenuRoot + "Hierarchy/Component Icons", false, 30)]
        private static void Toggle()
        {
            enabled = !enabled;
            EditorPrefs.SetBool(EditorToolUtility.PreferencePrefix + "HierarchyIcons", enabled);
            Register();
            Clear();
        }

        [MenuItem(EditorToolUtility.MenuRoot + "Hierarchy/Component Icons", true)]
        private static bool ValidateToggle()
        {
            Menu.SetChecked(EditorToolUtility.MenuRoot + "Hierarchy/Component Icons", enabled);
            return true;
        }

        private static void OnObjectsChanged(ref ObjectChangeEventStream stream) => Clear();
        private static void OnPlayModeChanged(PlayModeStateChange state) => Clear();
        private static void Clear() { Cache.Clear(); EditorApplication.RepaintHierarchyWindow(); }

        internal static bool IsHidden(Component component) => component is Transform || component is CanvasRenderer || component is ParticleSystemRenderer;

        private static Row GetRow(GameObject go)
        {
            int id = go.GetInstanceID();
            if (Cache.TryGetValue(id, out var row) && row.Target == go) return row;
            if (Cache.Count >= MaxCacheRows) Cache.Clear();
            Buffer.Clear();
            go.GetComponents(Buffer);
            var components = new List<Component>();
            var icons = new List<GUIContent>();
            foreach (var component in Buffer)
            {
                if (component && IsHidden(component)) continue;
                components.Add(component);
                icons.Add(component
                    ? new GUIContent(AssetPreview.GetMiniThumbnail(component), component.GetType().Name + " — click to inspect")
                    : new GUIContent("!", "Missing script — inspect this GameObject"));
            }
            row = new Row { Target = go, Components = components.ToArray(), Icons = icons.ToArray(),
                LabelWidth = EditorStyles.label.CalcSize(new GUIContent(go.name)).x };
            Cache[id] = row;
            return row;
        }

        private static void Draw(int id, Rect rect)
        {
            if (!enabled || Event.current == null) return;
            var type = Event.current.type;
            if (type != EventType.Repaint && type != EventType.MouseDown && type != EventType.MouseUp) return;
            if (!(EditorUtility.InstanceIDToObject(id) is GameObject go)) return;
            var row = GetRow(go);
            if (row.Components.Length == 0) return;
            float leftLimit = rect.x + row.LabelWidth + 28;
            int available = Mathf.FloorToInt((rect.xMax - 22 - leftLimit) / 19f);
            if (available < 1) return;
            int visible = Mathf.Min(MaxIcons, Mathf.Min(row.Components.Length, available));
            bool more = visible < row.Components.Length;
            if (more) visible = Mathf.Max(0, visible - 1);
            float x = rect.xMax - 22;
            var oldColor = GUI.color;
            try
            {
                for (int i = 0; i < visible; i++)
                {
                    x -= 19;
                    var component = row.Components[i];
                    bool inactive = !go.activeInHierarchy || (component is Behaviour behaviour && !behaviour.enabled) ||
                                    (component is Renderer renderer && !renderer.enabled) || (component is Collider collider && !collider.enabled);
                    GUI.color = inactive ? new Color(oldColor.r, oldColor.g, oldColor.b, oldColor.a * .45f) : oldColor;
                    if (GUI.Button(new Rect(x, rect.y + 1, 17, 16), row.Icons[i], GUIStyle.none)) Inspect(component, go);
                }
                GUI.color = oldColor;
                if (more)
                {
                    overflowStyle ??= new GUIStyle(EditorStyles.miniButton) { fontSize = 9, padding = new RectOffset(0, 0, 0, 0) };
                    if (GUI.Button(new Rect(x - 19, rect.y + 1, 18, 16), OverflowContent, overflowStyle))
                    {
                        var menu = new GenericMenu();
                        for (int i = 0; i < row.Components.Length; i++)
                        {
                            var component = row.Components[i];
                            menu.AddItem(new GUIContent((i + 1) + " " + (component ? component.GetType().Name : "Missing script")), false, () => Inspect(component, go));
                        }
                        menu.ShowAsContext();
                    }
                }
            }
            finally { GUI.color = oldColor; }
        }

        private static void Inspect(Component component, GameObject go)
        {
            if (component) EditorUtility.OpenPropertyEditor(component);
            else if (go) { Selection.activeGameObject = go; EditorGUIUtility.PingObject(go); }
        }
    }
}
