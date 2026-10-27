using UnityEngine;
using UnityEngine.InputSystem;

namespace Bulldozer.Player
{
    public sealed class MinerCamera : MonoBehaviour
    {
        public MinerController target;
        public float distance = 4.8f;
        public float sensitivity = .15f;
        public LayerMask obstacleMask = ~0;
        public float Yaw { get; private set; }
        float pitch = 18, actualDistance;
        readonly RaycastHit[] hits = new RaycastHit[32];
        void OnEnable() { actualDistance = distance; }
        void LateUpdate()
        {
            if (!target) return;
            Vector2 look = target.touchControls ? target.touchControls.ConsumeLook() * .13f : Vector2.zero;
            if (Mouse.current != null && Mouse.current.rightButton.isPressed) look += Mouse.current.delta.ReadValue() * sensitivity;
            if (Gamepad.current != null) look += Gamepad.current.rightStick.ReadValue() * (110 * Time.deltaTime);
            if (target.InputBlocked) look = Vector2.zero;
            Yaw += look.x; pitch = Mathf.Clamp(pitch - look.y, -20, 65);
            Vector3 pivot = target.transform.position + Vector3.up * 1.35f;
            Quaternion rotation = Quaternion.Euler(pitch, Yaw, 0);
            Vector3 backward = rotation * Vector3.back;
            float desired = distance;
            int count = Physics.SphereCastNonAlloc(pivot, .22f, backward, hits, distance, obstacleMask, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < count; i++) if (!hits[i].transform.IsChildOf(target.transform)) desired = Mathf.Min(desired, Mathf.Max(.3f, hits[i].distance - .08f));
            actualDistance = desired < actualDistance ? desired : Mathf.Lerp(actualDistance, desired, 1 - Mathf.Exp(-8 * Time.deltaTime));
            transform.SetPositionAndRotation(pivot + backward * actualDistance, rotation);
        }
    }
}

