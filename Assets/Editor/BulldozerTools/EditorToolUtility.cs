using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Bulldozer.EditorTools
{
    internal static class EditorToolUtility
    {
        internal const string MenuRoot = "Tools/Bulldozer/";
        internal const string AssetRoot = "Assets/Editor/BulldozerTools/";
        internal static readonly string PreferencePrefix = "Bulldozer.Tools." + Hash128.Compute(Application.dataPath) + ".";
        internal static bool CanEdit => !EditorApplication.isPlayingOrWillChangePlaymode &&
                                        !EditorApplication.isCompiling && !EditorApplication.isUpdating;

        internal static bool RequireEditMode()
        {
            if (CanEdit) return true;
            EditorUtility.DisplayDialog("Editor busy", "Stop Play Mode and wait for compilation/import to finish.", "OK");
            return false;
        }

        internal static void Style(VisualElement root)
        {
            var sheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(AssetRoot + "EditorTools.uss");
            if (sheet != null) root.styleSheets.Add(sheet);
            root.AddToClassList("tool-root");
        }

        internal static Label Text(string text, string className = "description")
        {
            var label = new Label(text);
            label.AddToClassList(className);
            return label;
        }

        internal static Button Button(string text, Action action)
        {
            return new Button(action) { text = text };
        }

        internal static string HierarchyPath(GameObject target)
        {
            if (!target) return "";
            var names = new List<string>();
            for (var t = target.transform; t != null; t = t.parent) names.Add(t.name);
            names.Reverse();
            return target.scene.name + ":/" + string.Join("/", names);
        }

        internal static List<GameObject> OpenSceneRoots()
        {
            var roots = new List<GameObject>();
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (scene.isLoaded && !UnityEditor.SceneManagement.EditorSceneManager.IsPreviewScene(scene))
                    roots.AddRange(scene.GetRootGameObjects());
            }
            return roots;
        }

        internal static void RevealExisting(string path)
        {
            if (Directory.Exists(path) || File.Exists(path)) EditorUtility.RevealInFinder(path);
            else EditorUtility.DisplayDialog("Folder not created yet", path, "OK");
        }

        [MenuItem(MenuRoot + "Selection/Copy Hierarchy Path", false, 100)]
        private static void CopyPath()
        {
            GUIUtility.systemCopyBuffer = HierarchyPath(Selection.activeGameObject);
        }

        [MenuItem(MenuRoot + "Selection/Copy Hierarchy Path", true)]
        private static bool CanCopyPath() => Selection.activeGameObject != null;

        [MenuItem(MenuRoot + "Open Tool Guide", false, 200)]
        private static void OpenGuide()
        {
            var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(AssetRoot + "README.md");
            if (asset) AssetDatabase.OpenAsset(asset);
        }
    }
}
