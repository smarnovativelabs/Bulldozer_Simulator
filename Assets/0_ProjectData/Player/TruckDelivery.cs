using UnityEngine;

namespace Bulldozer.Player
{
    // Presentation only: the production transaction pays and saves before Dispatch.
    // Reuse the same truck off screen; never instantiate a vehicle per delivery.
    public sealed class TruckDelivery : MonoBehaviour
    {
        public enum DeliveryPhase { Parked, Loading, Departing, Away, Arriving, Queued }
        public bool singleLane;
        public TruckDelivery queuePeer;
        public Vector3 queueBay;
        public float queueGap=5f;
        public Transform[] wheels;
        public SkinnedMeshRenderer body;
        public Collider parkingCollider;
        public Transform cargo;
        public TMPro.TMP_Text counter;
        public int loadedCount,orderCapacity=2;
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

        void Awake() { home = transform.localPosition; initialized = true; if(singleLane&&Vector3.Distance(home,queueBay)>.1f)Phase=DeliveryPhase.Queued; }
        public void Dispatch()
        {
            if (IsBusy) return;
            Phase = DeliveryPhase.Loading;
            elapsed = 0;
        }
        void Update()
        {
            if(singleLane){UpdateQueue();return;}
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
        void UpdateQueue()
        {
            elapsed+=Time.deltaTime;
            Vector3 direction=localTravelDirection.normalized;
            float progress=Vector3.Dot(transform.localPosition-queueBay,direction);
            if(Phase==DeliveryPhase.Parked)return;
            if(Phase==DeliveryPhase.Loading){if(elapsed>=.85f){parkingCollider.enabled=false;Next(DeliveryPhase.Departing);}return;}
            if(Phase==DeliveryPhase.Departing)
            {
                MoveTo(transform.localPosition+direction*(Mathf.Min(12,2+elapsed*5)*Time.deltaTime));
                if(progress>=travelDistance){body.enabled=false;cargo.gameObject.SetActive(false);Next(DeliveryPhase.Away);}return;
            }
            if(Phase==DeliveryPhase.Away)
            {
                if(elapsed>=.7f){transform.localPosition=queueBay-direction*travelDistance;body.enabled=true;cargo.gameObject.SetActive(true);loadedCount=0;Next(DeliveryPhase.Arriving);}return;
            }
            float target=0;
            if(queuePeer&&queuePeer.Phase!=DeliveryPhase.Away)
            {
                float ahead=Vector3.Dot(queuePeer.transform.localPosition-queueBay,direction);
                if(ahead>progress+.1f)target=Mathf.Min(0,ahead-queueGap);
            }
            Vector3 destination=queueBay+direction*target;
            MoveTo(Vector3.MoveTowards(transform.localPosition,destination,3.5f*Time.deltaTime));
            parkingCollider.enabled=true;
            if(Vector3.Distance(transform.localPosition,queueBay)<.025f)Next(DeliveryPhase.Parked);
            else if(Phase==DeliveryPhase.Arriving&&Mathf.Abs(progress+queueGap)<.1f)Next(DeliveryPhase.Queued);
        }
        void LateUpdate()
        {
            if(!counter)return;
            counter.gameObject.SetActive(Phase!=DeliveryPhase.Away);
            counter.text=Phase==DeliveryPhase.Queued?"QUEUED":(Phase==DeliveryPhase.Arriving?"0/"+orderCapacity+"\nRETURNING":loadedCount+"/"+orderCapacity+(Phase==DeliveryPhase.Departing?"\nDELIVERING":""));
            var camera=Camera.main;if(camera)counter.transform.rotation=camera.transform.rotation;
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


