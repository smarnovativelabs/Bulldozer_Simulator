using UnityEngine;
using UnityEngine.EventSystems;

namespace Bulldozer.Player
{
    public sealed class MinerTouchZone : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        public enum Action { Move, Look, Sprint, Jump, Mine }
        public Action action;
        public MinerTouchControls controls;
        public RectTransform knob;
        // Optional floating joystick: this zone is a large invisible touch area and the
        // stick base jumps under the finger, then trails it when dragged past its radius.
        public RectTransform floatingStick;
        [Range(.2f, .6f)] public float stickRadius = .35f;
        Vector3 stickHome;
        bool homeCached;
        int pointer = int.MinValue;
        RectTransform StickRect => floatingStick ? floatingStick : (RectTransform)transform;
        float Radius => StickRect.rect.width * stickRadius;
        bool InputBlocked => !controls || !controls.player || controls.player.InputBlocked;
        void Update()
        {
            // A modal can take over while a finger still owns this control.
            // Release ownership too, so resuming never requires the old pointer-up.
            if (pointer != int.MinValue && InputBlocked) ResetInput();
        }
        public void OnPointerDown(PointerEventData e)
        {
            if (InputBlocked) { ResetInput(); return; }
            if (pointer != int.MinValue) return;
            pointer = e.pointerId;
            if (action == Action.Jump) controls.player.Jump();
            if (action == Action.Mine) controls.player.Mine();
            if (action == Action.Sprint) controls.Sprinting = true;
            if (action == Action.Move)
            {
                if (floatingStick) PlaceStick(e);
                OnDrag(e);
            }
        }
        void PlaceStick(PointerEventData e)
        {
            if (!homeCached) { stickHome = floatingStick.localPosition; homeCached = true; }
            var parent = (RectTransform)floatingStick.parent;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, e.position, e.pressEventCamera, out Vector2 local)) return;
            // Keep the whole base on screen when the touch lands near an edge.
            Vector2 half = floatingStick.rect.size * .5f;
            Rect bounds = parent.rect;
            local.x = Mathf.Clamp(local.x, bounds.xMin + half.x, bounds.xMax - half.x);
            local.y = Mathf.Clamp(local.y, bounds.yMin + half.y, bounds.yMax - half.y);
            floatingStick.localPosition = new Vector3(local.x, local.y, stickHome.z);
        }
        public void OnDrag(PointerEventData e)
        {
            if (InputBlocked) { ResetInput(); return; }
            if (pointer != e.pointerId) return;
            if (action == Action.Look) controls.AddLook(e.delta);
            if (action != Action.Move) return;
            var rect = StickRect;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, e.position, e.pressEventCamera, out Vector2 point);
            float radius = Radius;
            if (floatingStick && point.magnitude > radius)
            {
                // Drag the base along so reversing direction never needs a long thumb trip back.
                Vector2 excess = point - point.normalized * radius;
                floatingStick.localPosition += (Vector3)(excess * floatingStick.localScale.x);
                point -= excess;
            }
            Vector2 raw=Vector2.ClampMagnitude(point / radius, 1);
            controls.Move = raw.magnitude<.12f?Vector2.zero:raw.normalized*Mathf.InverseLerp(.12f,1,raw.magnitude);
            if (knob) knob.anchoredPosition = controls.Move * radius;
        }
        public void OnPointerUp(PointerEventData e) { if (pointer == e.pointerId) ResetInput(); }
        void OnDisable() => ResetInput();
        void OnApplicationFocus(bool focused) { if (!focused) ResetInput(); }
        void OnApplicationPause(bool paused) { if (paused) ResetInput(); }
        void ResetInput()
        {
            pointer = int.MinValue;
            if (!controls) return;
            if (action == Action.Move)
            {
                controls.Move = Vector2.zero; if (knob) knob.anchoredPosition = Vector2.zero;
                if (floatingStick && homeCached) floatingStick.localPosition = stickHome;
            }
            if (action == Action.Sprint) controls.Sprinting = false;
            if (action == Action.Look) controls.ConsumeLook();
        }
    }
}
