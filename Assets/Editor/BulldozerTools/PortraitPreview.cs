using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Bulldozer.EditorTools
{
    // Switches the Game view to a portrait phone resolution so the portrait HUD can be checked in the Editor.
    // Uses the active build target's size group (Android/iOS), adding the size once.
    public static class PortraitPreview
    {
        [MenuItem("Bulldozer/Game View/Portrait 1080x1920")]
        public static void Apply1080() => Apply(1080, 1920);

        public static void Apply(int width, int height)
        {
            string label = $"Portrait {width}x{height}";
            var sizesType = Type.GetType("UnityEditor.GameViewSizes,UnityEditor");
            var singleton = typeof(ScriptableSingleton<>).MakeGenericType(sizesType);
            var instance = singleton.GetProperty("instance").GetValue(null);
            // The current group follows the active build target (Android/iOS), not Standalone.
            var group = sizesType.GetProperty("currentGroup").GetValue(instance);
            var groupType = group.GetType();
            int total = (int)groupType.GetMethod("GetTotalCount").Invoke(group, null);
            int index = -1;
            var getSize = groupType.GetMethod("GetGameViewSize");
            for (int i = 0; i < total; i++)
            {
                var s = getSize.Invoke(group, new object[] { i });
                if ((string)s.GetType().GetProperty("baseText").GetValue(s) == label) { index = i; break; }
            }
            if (index < 0)
            {
                var sizeType = Type.GetType("UnityEditor.GameViewSize,UnityEditor");
                var kind = Type.GetType("UnityEditor.GameViewSizeType,UnityEditor");
                var size = sizeType.GetConstructor(new[] { kind, typeof(int), typeof(int), typeof(string) })
                    .Invoke(new object[] { Enum.ToObject(kind, 1), width, height, label });
                groupType.GetMethod("AddCustomSize").Invoke(group, new object[] { size });
                index = total;
            }
            var gameView = Type.GetType("UnityEditor.GameView,UnityEditor");
            var window = EditorWindow.GetWindow(gameView);
            gameView.GetMethod("SizeSelectionCallback", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Invoke(window, new object[] { index, null });
        }
    }
}
