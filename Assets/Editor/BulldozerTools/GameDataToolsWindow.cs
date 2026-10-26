using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Bulldozer.EditorTools
{
    internal static class SaveFolderOperations
    {
        internal const string BackupFolder = "EditorToolBackups";

        internal static bool TryResolve(string root, string relative, out string path, out string error)
        {
            path = null;
            error = "Enter a save subfolder relative to persistentDataPath. The root itself cannot be reset.";
            if (string.IsNullOrWhiteSpace(relative) || Path.IsPathRooted(relative)) return false;
            string normalized = relative.Replace('\\', '/');
            var parts = normalized.Split('/');
            if (string.Equals(parts[0], BackupFolder, StringComparison.OrdinalIgnoreCase)) return false;
            foreach (var part in parts)
            {
                if (string.IsNullOrWhiteSpace(part) || part == "." || part == ".." || part.Contains(":")) return false;
                if (part.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0) return false;
            }
            try
            {
                string fullRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
                path = Path.GetFullPath(Path.Combine(fullRoot, normalized));
                if (!path.StartsWith(fullRoot, StringComparison.Ordinal)) { path = null; return false; }
                // Never traverse a link/junction when resolving the configured data location.
                var check = new DirectoryInfo(path);
                while (check != null && (check.FullName + Path.DirectorySeparatorChar).StartsWith(fullRoot, StringComparison.Ordinal))
                {
                    if (check.Exists && (check.Attributes & FileAttributes.ReparsePoint) != 0)
                    {
                        path = null;
                        error = "Symlink/junction save paths are not supported.";
                        return false;
                    }
                    check = check.Parent;
                }
                error = "";
                return true;
            }
            catch (Exception ex) when (ex is ArgumentException || ex is IOException || ex is UnauthorizedAccessException || ex is NotSupportedException)
            {
                path = null;
                error = ex.Message;
                return false;
            }
        }

        internal static string Archive(string root, string relative)
        {
            if (!TryResolve(root, relative, out var source, out var error)) throw new IOException(error);
            if (!Directory.Exists(source)) throw new DirectoryNotFoundException(source);
            string backupRoot = Path.Combine(Path.GetFullPath(root), BackupFolder);
            if (Directory.Exists(backupRoot) && (File.GetAttributes(backupRoot) & FileAttributes.ReparsePoint) != 0)
                throw new IOException("The backup location must not be a link/junction.");
            Directory.CreateDirectory(backupRoot);
            string destination = Path.Combine(backupRoot, DateTime.UtcNow.ToString("yyyyMMdd-HHmmss") + "-" + Guid.NewGuid().ToString("N"));
            Directory.Move(source, destination);
            return destination;
        }

        internal static void Restore(string root, string relative, string backup)
        {
            if (!TryResolve(root, relative, out var destination, out var error)) throw new IOException(error);
            string backupRoot = Path.Combine(Path.GetFullPath(root), BackupFolder);
            string fullBackup = Path.GetFullPath(backup);
            if (!string.Equals(Path.GetDirectoryName(fullBackup), backupRoot, StringComparison.Ordinal) || !Directory.Exists(fullBackup))
                throw new IOException("Backup is missing or outside this project's backup folder.");
            if ((File.GetAttributes(backupRoot) & FileAttributes.ReparsePoint) != 0 ||
                (File.GetAttributes(fullBackup) & FileAttributes.ReparsePoint) != 0)
                throw new IOException("Backup links/junctions are not supported.");
            if (Directory.Exists(destination) || File.Exists(destination))
                throw new IOException("The destination already exists. Archive the new save before restoring an older one.");
            Directory.CreateDirectory(Path.GetDirectoryName(destination));
            Directory.Move(fullBackup, destination);
        }
    }

    public sealed class GameDataToolsWindow : EditorWindow
    {
        [Serializable] private sealed class BackupRecord { public string relative; public string backup; }
        private TextField keyField, folderField;
        private Label targetLabel, statusLabel;
        private VisualElement actions;
        private string FolderKey => EditorToolUtility.PreferencePrefix + "SaveSubfolder";
        private string BackupKey => EditorToolUtility.PreferencePrefix + "LastSaveBackup";

        [MenuItem(EditorToolUtility.MenuRoot + "Game Data", false, 10)]
        [MenuItem("Tools/Clear Game Data", false, 101)]
        public static void Open()
        {
            var window = GetWindow<GameDataToolsWindow>("Game Data");
            window.minSize = new Vector2(440, 420);
        }

        [MenuItem(EditorToolUtility.MenuRoot + "Reset PlayerPrefs...", false, 11)]
        [MenuItem("Tools/Delete Prefs", false, 102)]
        public static void ConfirmResetPlayerPrefs()
        {
            if (!EditorToolUtility.RequireEditMode()) return;
            if (!EditorUtility.DisplayDialog("Reset Editor PlayerPrefs?",
                    "Company: " + Application.companyName + "\nProduct: " + Application.productName +
                    "\nProject: " + Application.dataPath +
                    "\n\nDelete ALL PlayerPrefs for this Editor game identity? This cannot be undone. Other projects using the same identity may share this store. It does not reset an installed phone/iPad app or EditorPrefs.",
                    "Delete PlayerPrefs", "Cancel")) return;
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            Debug.Log("[Bulldozer Tools] Editor PlayerPrefs reset for " + Application.companyName + "/" + Application.productName);
        }

        [MenuItem(EditorToolUtility.MenuRoot + "Reset PlayerPrefs...", true)]
        [MenuItem("Tools/Delete Prefs", true)]
        private static bool CanReset() => EditorToolUtility.CanEdit;

        public void CreateGUI()
        {
            var root = rootVisualElement;
            root.Clear();
            EditorToolUtility.Style(root);
            var scroll = new ScrollView();
            scroll.AddToClassList("grow");
            root.Add(scroll);
            scroll.Add(EditorToolUtility.Text("Editor game data", "title"));
            scroll.Add(EditorToolUtility.Text("Company: " + Application.companyName + "  •  Product: " + Application.productName));
            scroll.Add(EditorToolUtility.Text("This affects local Editor data only, not the connected iPad or an installed Android build. PlayerPrefs can be shared by projects with the same game identity."));
            scroll.Add(EditorToolUtility.Text("Persistent data: " + Application.persistentDataPath));
            scroll.Add(EditorToolUtility.Button("Reveal persistent data folder", () => EditorToolUtility.RevealExisting(Application.persistentDataPath)));
            actions = new VisualElement();
            scroll.Add(actions);
            keyField = new TextField("Exact PlayerPrefs key") { name = "prefsKey" };
            actions.Add(keyField);
            actions.Add(EditorToolUtility.Button("Delete this key...", DeleteKey));
            actions.Add(EditorToolUtility.Button("Reset all PlayerPrefs...", ConfirmResetPlayerPrefs));
            actions.Add(EditorToolUtility.Text("Save files — archive instead of permanently deleting", "section-title"));
            actions.Add(EditorToolUtility.Text("Choose the actual subfolder used by your save system. No Janitor folder is assumed; file-only saves directly in the root are not reset by this tool."));
            folderField = new TextField("Save subfolder") { name = "saveSubfolder", value = EditorPrefs.GetString(FolderKey, "") };
            folderField.RegisterValueChangedCallback(evt => { EditorPrefs.SetString(FolderKey, evt.newValue); UpdatePath(); });
            actions.Add(folderField);
            targetLabel = EditorToolUtility.Text("");
            actions.Add(targetLabel);
            actions.Add(EditorToolUtility.Button("Archive save folder and reset...", Archive));
            actions.Add(EditorToolUtility.Button("Restore last archived folder...", Restore));
            actions.Add(EditorToolUtility.Button("Reveal save backups", () => EditorToolUtility.RevealExisting(Path.Combine(Application.persistentDataPath, SaveFolderOperations.BackupFolder))));
            statusLabel = EditorToolUtility.Text("No data has been changed.");
            scroll.Add(statusLabel);
            UpdatePath();
            root.schedule.Execute(() => actions?.SetEnabled(EditorToolUtility.CanEdit)).Every(250);
        }

        private void UpdatePath()
        {
            bool valid = SaveFolderOperations.TryResolve(Application.persistentDataPath, folderField.value, out var path, out var error);
            targetLabel.text = valid ? "Target: " + path + (Directory.Exists(path) ? "" : " (not created)") : error;
        }

        private void DeleteKey()
        {
            if (!EditorToolUtility.RequireEditMode()) return;
            string key = keyField.value;
            if (string.IsNullOrEmpty(key) || !PlayerPrefs.HasKey(key)) { statusLabel.text = "That exact key does not exist."; return; }
            if (!EditorUtility.DisplayDialog("Delete PlayerPrefs key?", Application.companyName + "/" + Application.productName + "\n\nKey: " + key + "\nCannot be undone.", "Delete key", "Cancel")) return;
            PlayerPrefs.DeleteKey(key);
            PlayerPrefs.Save();
            statusLabel.text = "Deleted the selected key.";
        }

        private void Archive()
        {
            if (!EditorToolUtility.RequireEditMode()) return;
            if (!SaveFolderOperations.TryResolve(Application.persistentDataPath, folderField.value, out var path, out var error)) { statusLabel.text = error; return; }
            if (!Directory.Exists(path)) { statusLabel.text = "No save folder exists at that path."; return; }
            if (!EditorUtility.DisplayDialog("Archive this save folder?", path + "\n\nMove it to EditorToolBackups so the next run starts fresh. PlayerPrefs remain unchanged. The archive can be restored if the target folder is absent.", "Archive and reset", "Cancel")) return;
            try
            {
                string backup = SaveFolderOperations.Archive(Application.persistentDataPath, folderField.value);
                EditorPrefs.SetString(BackupKey, JsonUtility.ToJson(new BackupRecord { relative = folderField.value, backup = backup }));
                statusLabel.text = "Archived to " + backup;
                UpdatePath();
            }
            catch (Exception ex) { statusLabel.text = ex.Message; Debug.LogException(ex); }
        }

        private void Restore()
        {
            if (!EditorToolUtility.RequireEditMode()) return;
            try
            {
                var record = JsonUtility.FromJson<BackupRecord>(EditorPrefs.GetString(BackupKey, "{}"));
                if (record == null || string.IsNullOrEmpty(record.backup)) { statusLabel.text = "No archive recorded for this project."; return; }
                if (!EditorUtility.DisplayDialog("Restore the last archive?", record.backup + "\n\nRestore to: " + record.relative + "\nExisting data will never be overwritten.", "Restore", "Cancel")) return;
                SaveFolderOperations.Restore(Application.persistentDataPath, record.relative, record.backup);
                EditorPrefs.DeleteKey(BackupKey);
                statusLabel.text = "Archive restored.";
                UpdatePath();
            }
            catch (Exception ex) { statusLabel.text = ex.Message; Debug.LogException(ex); }
        }
    }
}
