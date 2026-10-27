using UnityEngine;

namespace Bulldozer.Player
{
    // Presentation only: the production transaction pays and saves before Dispatch.
    // Reuse the same truck off screen; never instantiate a vehicle per delivery.
    public sealed class TruckDelivery : MonoBehaviour
    {
        public enum DeliveryPhase { Parked, Loading, Departing, Away, Arriving }
        public Transform[] wheels;
        public SkinnedMeshRenderer body;
        public Collider parkingCollider;
        public Transform cargo;
        public Vector3 localTravelDirection = Vector3.right;
        [Min(.01f)] public float wheelRadius = .265f;
        [Min(5f)] public float travelDistance = 32f;
        [Min(.1f)] public float travelSeconds = 5f;
        public DeliveryPhase Phase { get; private set; }
        public bool IsBusy => Phase != DeliveryPhase.Parked;
        public bool ShowDepartingCargo => Phase == DeliveryPhase.Loading || Phase == DeliveryPhase.Departing;
        public float DistanceTravelled { get; private set; }
        public float WheelDegrees { get; private set; }
        Vector3 home;
        float elapsed;
        bool initialized;

        void Awake() { home = transform.localPosition; initialized = true; }
        public void Dispatch()
        {
            if (IsBusy) return;
            Phase = DeliveryPhase.Loading;
            elapsed = 0;
        }
        void Update()
        {
            if (!IsBusy) return;
            elapsed += Time.deltaTime;
            switch (Phase)
            {
                case DeliveryPhase.Loading:
                    // Allow the final .65-second cargo flight to land first.
                    if (elapsed >= .85f) { parkingCollider.enabled = false; Next(DeliveryPhase.Departing); }
                    break;
                case DeliveryPhase.Departing:
                    float depart = Mathf.Clamp01(elapsed / travelSeconds);
                    MoveTo(home + localTravelDirection.normalized * (travelDistance * depart * depart));
                    if (depart >= 1) { body.enabled = false; cargo.gameObject.SetActive(false); Next(DeliveryPhase.Away); }
                    break;
                case DeliveryPhase.Away:
                    if (elapsed >= .7f)
                    {
                        // Teleport only while hidden. The next vehicle enters from the opposite side.
                        transform.localPosition = home - localTravelDirection.normalized * travelDistance;
                        body.enabled = true;
                        Next(DeliveryPhase.Arriving);
                    }
                    break;
                case DeliveryPhase.Arriving:
                    float arrive = Mathf.Clamp01(elapsed / travelSeconds);
                    MoveTo(home - localTravelDirection.normalized * (travelDistance * (1-arrive) * (1-arrive)));
                    if (arrive >= 1) { parkingCollider.enabled = true; cargo.gameObject.SetActive(true); Next(DeliveryPhase.Parked); }
                    break;
            }
        }
        void Next(DeliveryPhase phase) { Phase = phase; elapsed = 0; }
        void MoveTo(Vector3 position)
        {
            Vector3 previous = transform.position;
            transform.localPosition = position;
            Vector3 displacement = transform.position - previous;
            float distance = displacement.magnitude;
            if (distance < .000001f) return;
            float degrees = distance / Mathf.Max(.01f, wheelRadius) * Mathf.Rad2Deg;
            Vector3 axle = Vector3.Cross(Vector3.up, displacement / distance);
            for (int i = 0; i < wheels.Length; i++) wheels[i].Rotate(axle, degrees, Space.World);
            DistanceTravelled += distance;
            WheelDegrees += degrees;
        }
        void OnDisable()
        {
            if (!initialized) return;
            transform.localPosition = home;
            Phase = DeliveryPhase.Parked;
            if (body) body.enabled = true;
            if (parkingCollider) parkingCollider.enabled = true;
            if (cargo) cargo.gameObject.SetActive(true);
        }
    }
}
