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
        int pointer = int.MinValue;
        public void OnPointerDown(PointerEventData e)
        {
            if (pointer != int.MinValue) return;
            pointer = e.pointerId;
            if (action == Action.Jump) controls.player.Jump();
            if (action == Action.Mine) controls.player.Mine();
            if (action == Action.Sprint) controls.Sprinting = true;
            if (action == Action.Move) OnDrag(e);
        }
        public void OnDrag(PointerEventData e)
        {
            if (pointer != e.pointerId) return;
            if (action == Action.Look) controls.AddLook(e.delta);
            if (action != Action.Move) return;
            var rect = (RectTransform)transform;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, e.position, e.pressEventCamera, out Vector2 point);
            controls.Move = Vector2.ClampMagnitude(point / (rect.rect.width * .35f), 1);
            if (knob) knob.anchoredPosition = controls.Move * rect.rect.width * .35f;
        }
        public void OnPointerUp(PointerEventData e) { if (pointer == e.pointerId) ResetInput(); }
        void OnDisable() => ResetInput();
        void ResetInput()
        {
            pointer = int.MinValue;
            if (!controls) return;
            if (action == Action.Move) { controls.Move = Vector2.zero; if (knob) knob.anchoredPosition = Vector2.zero; }
            if (action == Action.Sprint) controls.Sprinting = false;
        }
    }
}
