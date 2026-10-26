using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Bulldozer.EditorTools
{
    // Adapted from I_Am_Janitor's SceneViewWindow; no scene or build-list mutation on refresh.
    public sealed class SceneSwitcherWindow : EditorWindow
    {
        internal sealed class Entry
        {
            internal string Path, Guid, Name;
            internal int BuildIndex = -1;
            internal bool InBuildList;
        }

        [Serializable] private sealed class Favorites { public List<string> guids = new List<string>(); }
        [SerializeField] private string search = "";
        [SerializeField] private int scope = 1;
        [SerializeField] private Vector2 scroll;
        private List<Entry> entries = new List<Entry>();
        private HashSet<string> favorites;
        private GUIStyle buttonStyle;

        [MenuItem(EditorToolUtility.MenuRoot + "Scene Switcher", false, 0)]
        [MenuItem("Tools/Scene Switcher", false, 100)]
        public static void Open()
        {
            var window = GetWindow<SceneSwitcherWindow>("Scene Switcher");
            window.minSize = new Vector2(420, 280);
        }

        private void OnEnable()
        {
            favorites = new HashSet<string>();
            try
            {
                var saved = JsonUtility.FromJson<Favorites>(EditorPrefs.GetString(EditorToolUtility.PreferencePrefix + "SceneFavorites", "{}"));
                if (saved?.guids != null) favorites.UnionWith(saved.guids);
            }
            catch (ArgumentException) { }
            EditorApplication.projectChanged += Refresh;
            EditorBuildSettings.sceneListChanged += Refresh;
            EditorSceneManager.sceneOpened += SceneOpened;
            EditorApplication.playModeStateChanged += PlayModeChanged;
            Refresh();
        }

        private void OnDisable()
        {
            EditorApplication.projectChanged -= Refresh;
            EditorBuildSettings.sceneListChanged -= Refresh;
            EditorSceneManager.sceneOpened -= SceneOpened;
            EditorApplication.playModeStateChanged -= PlayModeChanged;
        }

        private void SceneOpened(Scene scene, OpenSceneMode mode) => Repaint();
        private void PlayModeChanged(PlayModeStateChange state) => Repaint();

        internal static List<Entry> BuildEntries(EditorBuildSettingsScene[] buildScenes, IEnumerable<string> projectPaths)
        {
            var results = new List<Entry>();
            var byPath = new Dictionary<string, Entry>(StringComparer.Ordinal);
            int index = 0;
            foreach (var scene in buildScenes)
            {
                if (string.IsNullOrEmpty(scene.path)) continue;
                if (!byPath.TryGetValue(scene.path, out var entry))
                {
                    entry = new Entry { Path = scene.path, Name = Path.GetFileNameWithoutExtension(scene.path), InBuildList = true };
                    byPath.Add(scene.path, entry);
                    results.Add(entry);
                }
                if (scene.enabled)
                {
                    if (entry.BuildIndex < 0) entry.BuildIndex = index;
                    index++;
                }
            }
            var sortedPaths = new List<string>(projectPaths);
            sortedPaths.Sort(StringComparer.OrdinalIgnoreCase);
            foreach (var path in sortedPaths)
            {
                if (byPath.ContainsKey(path)) continue;
                var entry = new Entry { Path = path, Name = Path.GetFileNameWithoutExtension(path) };
                byPath.Add(path, entry);
                results.Add(entry);
            }
            return results;
        }

        private void Refresh()
        {
            var paths = new List<string>();
            foreach (var guid in AssetDatabase.FindAssets("t:Scene", new[] { "Assets" }))
                paths.Add(AssetDatabase.GUIDToAssetPath(guid));
            entries = BuildEntries(EditorBuildSettings.scenes, paths);
            foreach (var entry in entries) entry.Guid = AssetDatabase.AssetPathToGUID(entry.Path);
            Repaint();
        }

        private void OnGUI()
        {
            buttonStyle ??= new GUIStyle(GUI.skin.button) { alignment = TextAnchor.MiddleLeft };
            EditorGUILayout.HelpBox("Build list = shared scene list. A Unity 6 Build Profile can override it. Switching scenes is disabled during Play Mode/import.", MessageType.Info);
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                search = GUILayout.TextField(search, EditorStyles.toolbarTextField, GUILayout.MinWidth(100), GUILayout.ExpandWidth(true));
                if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(60))) Refresh();
            }
            scope = GUILayout.Toolbar(scope, new[] { "Build list", "All project scenes", "Favorites" });
            scroll = EditorGUILayout.BeginScrollView(scroll);
            int visible = 0;
            foreach (var entry in entries)
            {
                if (scope == 0 && !entry.InBuildList) continue;
                if (scope == 2 && !favorites.Contains(entry.Guid)) continue;
                if (!string.IsNullOrEmpty(search) && entry.Path.IndexOf(search, StringComparison.OrdinalIgnoreCase) < 0) continue;
                visible++;
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        bool oldFavorite = !string.IsNullOrEmpty(entry.Guid) && favorites.Contains(entry.Guid);
                        using (new EditorGUI.DisabledScope(string.IsNullOrEmpty(entry.Guid)))
                        {
                            bool favorite = GUILayout.Toggle(oldFavorite, "★", GUI.skin.button, GUILayout.Width(25));
                            if (favorite != oldFavorite)
                            {
                                if (favorite) favorites.Add(entry.Guid); else favorites.Remove(entry.Guid);
                                EditorPrefs.SetString(EditorToolUtility.PreferencePrefix + "SceneFavorites", JsonUtility.ToJson(new Favorites { guids = new List<string>(favorites) }));
                            }
                        }
                        var loaded = SceneManager.GetSceneByPath(entry.Path);
                        string prefix = entry.BuildIndex >= 0 ? entry.BuildIndex + ": " : entry.InBuildList ? "[disabled] " : "";
                        string suffix = loaded.IsValid() && loaded.isLoaded ? "  • open" : "";
                        using (new EditorGUI.DisabledScope(!EditorToolUtility.CanEdit || string.IsNullOrEmpty(entry.Guid)))
                        {
                            if (GUILayout.Button(prefix + entry.Name + suffix, buttonStyle)) OpenScene(entry.Path, false);
                            if (GUILayout.Button(new GUIContent("+", "Open additively"), GUILayout.Width(24))) OpenScene(entry.Path, true);
                        }
                        if (GUILayout.Button("Ping", GUILayout.Width(40))) EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<SceneAsset>(entry.Path));
                    }
                    EditorGUILayout.LabelField(entry.Path, EditorStyles.miniLabel);
                }
            }
            if (visible == 0) EditorGUILayout.HelpBox("No matching scenes. Try All project scenes or clear the search.", MessageType.None);
            EditorGUILayout.EndScrollView();
        }

        private static void OpenScene(string path, bool additive)
        {
            if (!EditorToolUtility.CanEdit || !AssetDatabase.LoadAssetAtPath<SceneAsset>(path)) return;
            var loaded = SceneManager.GetSceneByPath(path);
            if (additive && loaded.IsValid() && loaded.isLoaded) { SceneManager.SetActiveScene(loaded); return; }
            if (!additive && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            EditorSceneManager.OpenScene(path, additive ? OpenSceneMode.Additive : OpenSceneMode.Single);
            GUIUtility.ExitGUI();
        }
    }
}
