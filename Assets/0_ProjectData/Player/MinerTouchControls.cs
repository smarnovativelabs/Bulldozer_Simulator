using UnityEngine;
using UnityEngine.EventSystems;

namespace Bulldozer.Player
{
    public sealed class MinerTouchControls : MonoBehaviour
    {
        public MinerController player;
        public RectTransform safeArea;
        public bool externalSafeArea;
        public Vector2 Move { get; set; }
        public bool Sprinting { get; set; }
        Vector2 look;
        Rect lastSafe;
        Vector2Int lastSize;
        public void AddLook(Vector2 delta) => look += delta;
        public Vector2 ConsumeLook() { var value = look; look = Vector2.zero; return value; }
        void Update()
        {
            if(externalSafeArea)return;
            Rect area = Screen.safeArea;
            var size = new Vector2Int(Screen.width, Screen.height);
            if (!safeArea || (area == lastSafe && size == lastSize)) return;
            lastSafe = area; lastSize = size;
            safeArea.anchorMin = new Vector2(area.xMin / Screen.width, area.yMin / Screen.height);
            safeArea.anchorMax = new Vector2(area.xMax / Screen.width, area.yMax / Screen.height);
            safeArea.offsetMin = safeArea.offsetMax = Vector2.zero;
        }
        void OnDisable() { Move = look = Vector2.zero; Sprinting = false; }
        void OnApplicationFocus(bool focus) { if (!focus) { Move = look = Vector2.zero; Sprinting = false; } }
    }
}

