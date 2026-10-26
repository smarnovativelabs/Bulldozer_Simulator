using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Bulldozer.EditorTools
{
    public sealed class MobileAuditWindow : EditorWindow
    {
        private AuditReport report;
        private ListView list;
        private Label summary;
        private VisualElement scanButtons;

        [MenuItem(EditorToolUtility.MenuRoot + "Mobile Audit", false, 20)]
        public static void Open()
        {
            var window = GetWindow<MobileAuditWindow>("Mobile Audit");
            window.minSize = new Vector2(520, 360);
        }

        public void CreateGUI()
        {
            var root = rootVisualElement;
            root.Clear();
            EditorToolUtility.Style(root);
            root.Add(EditorToolUtility.Text("Mobile authoring audit", "title"));
            root.Add(EditorToolUtility.Text("Read-only snapshots for Android and iOS preparation. Review findings are starting-budget hints, not errors or proof of device performance. Re-scan after changes."));
            scanButtons = new VisualElement();
            scanButtons.AddToClassList("toolbar-row");
            scanButtons.Add(EditorToolUtility.Button("Scan open scenes", () => Scan(false)));
            scanButtons.Add(EditorToolUtility.Button("Scan selected objects", () => Scan(true)));
            scanButtons.Add(EditorToolUtility.Button("Audit selected assets' textures", ScanTextures));
            root.Add(scanButtons);
            summary = EditorToolUtility.Text("Choose a scope. Texture audit uses selected Project assets or folders; scenes/FBX/prefabs include dependencies.");
            root.Add(summary);
            list = new ListView { selectionType = SelectionType.None, virtualizationMethod = CollectionVirtualizationMethod.FixedHeight, fixedItemHeight = 84 };
            list.AddToClassList("report-list");
            list.makeItem = () =>
            {
                var row = new VisualElement();
                row.AddToClassList("report-item");
                var text = new Label { name = "finding" };
                text.AddToClassList("report-text");
                row.Add(text);
                var button = new Button { text = "Select", name = "select" };
                button.AddToClassList("report-button");
                button.clicked += () =>
                {
                    if (row.userData is AuditFinding finding && finding.target)
                    { Selection.activeObject = finding.target; EditorGUIUtility.PingObject(finding.target); }
                };
                row.Add(button);
                return row;
            };
            list.bindItem = (row, index) =>
            {
                var finding = report.findings[index];
                row.userData = finding;
                string text = finding.severity + ": " + finding.message + "\n" + finding.location;
                row.Q<Label>("finding").text = text;
                row.tooltip = text;
                row.Q<Button>("select").SetEnabled(finding.target != null);
            };
            root.Add(list);
            var bottom = new VisualElement();
            bottom.AddToClassList("toolbar-row");
            bottom.Add(EditorToolUtility.Button("Export report JSON...", Export));
            bottom.Add(EditorToolUtility.Button("Select objects with missing scripts", SelectMissing));
            root.Add(bottom);
            root.schedule.Execute(() => scanButtons?.SetEnabled(EditorToolUtility.CanEdit)).Every(250);
            ShowReport();
        }

        private void Scan(bool selection)
        {
            if (!EditorToolUtility.RequireEditMode()) return;
            var roots = selection ? new List<GameObject>(Selection.gameObjects) : EditorToolUtility.OpenSceneRoots();
            if (roots.Count == 0) { summary.text = "No scene objects selected/open. Select a GameObject in the Hierarchy."; return; }
            try
            {
                report = MobileAudit.Scenes(roots, selection ? "Selected scene object subtrees (including inactive)" : "Open scenes (including inactive)",
                    count => EditorUtility.DisplayCancelableProgressBar("Mobile scene audit", count + " objects inspected", Mathf.Min(.95f, count / (float)MobileAudit.MaxObjects)));
                ShowReport();
            }
            finally { EditorUtility.ClearProgressBar(); }
        }

        private void ScanTextures()
        {
            if (!EditorToolUtility.RequireEditMode()) return;
            var paths = new List<string>();
            foreach (string guid in Selection.assetGUIDs)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!string.IsNullOrEmpty(path)) paths.Add(path);
            }
            if (paths.Count == 0) { summary.text = "Select assets or a folder in the Project window first."; return; }
            try
            {
                report = MobileAudit.Textures(paths, count => EditorUtility.DisplayCancelableProgressBar("Mobile texture audit", count + " importers inspected", Mathf.Min(.95f, count / (float)MobileAudit.MaxTextures)));
                ShowReport();
            }
            finally { EditorUtility.ClearProgressBar(); }
        }

        private void ShowReport()
        {
            if (report == null || list == null) return;
            summary.text = report.scope + "\n" + report.summary;
            if (report.findings.Count == 0) summary.text += "\nNo findings in this scope; runtime profiling is still required.";
            list.itemsSource = report.findings;
            list.Rebuild();
        }

        private void Export()
        {
            if (report == null) { summary.text = "Run an audit before exporting."; return; }
            string path = EditorUtility.SaveFilePanel("Export mobile audit", Path.GetDirectoryName(Application.dataPath), "MobileAudit-" + DateTime.Now.ToString("yyyyMMdd-HHmmss"), "json");
            if (string.IsNullOrEmpty(path)) return;
            try { File.WriteAllText(path, JsonUtility.ToJson(report, true)); }
            catch (Exception ex) { Debug.LogException(ex); }
        }

        private void SelectMissing()
        {
            if (!EditorToolUtility.RequireEditMode()) return;
            var objects = new List<UnityEngine.Object>();
            foreach (var root in EditorToolUtility.OpenSceneRoots())
                foreach (var t in root.GetComponentsInChildren<Transform>(true))
                    if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject) > 0) objects.Add(t.gameObject);
            Selection.objects = objects.ToArray();
            summary.text = "Selected " + objects.Count + " GameObjects with missing scripts. No components were removed.";
        }
    }
}
