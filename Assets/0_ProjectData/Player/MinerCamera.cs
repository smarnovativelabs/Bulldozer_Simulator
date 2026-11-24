using UnityEngine;
using UnityEngine.InputSystem;

namespace Bulldozer.Player
{
    [DefaultExecutionOrder(250)]
    public sealed class MinerCamera : MonoBehaviour
    {
        public MinerController target;
        public float distance = 4.8f;
        public float sensitivity = .15f;
        public float startPitch=48f;
        public float startYaw;
        public bool fixedAngle=true;
        public LayerMask obstacleMask = ~0;
        public Renderer overheadOccluder;
        public float Yaw { get; private set; }
        float pitch = 18;
        readonly RaycastHit[] visibilityHits=new RaycastHit[32];
        readonly Renderer[] hiddenWalls=new Renderer[33];
        int hiddenCount;
        void OnEnable() { pitch=startPitch; Yaw=startYaw; }
        public void GetFollowPose(out Vector3 position,out Quaternion rotation)
        {
            rotation=Quaternion.Euler(fixedAngle?startPitch:pitch,fixedAngle?startYaw:Yaw,0);
            position=target.transform.position+Vector3.up*1.35f+rotation*Vector3.back*distance;
        }
        void LateUpdate()
        {
            if (!target) return;
            if (target.GuidanceInputBlocked) { RestoreWalls(); if(target.touchControls)target.touchControls.ConsumeLook(); return; }
            Vector2 look = target.touchControls ? target.touchControls.ConsumeLook() * .13f : Vector2.zero;
            if (Mouse.current != null && Mouse.current.rightButton.isPressed) look += Mouse.current.delta.ReadValue() * sensitivity;
            if (Gamepad.current != null) look += Gamepad.current.rightStick.ReadValue() * (110 * Time.deltaTime);
            if (target.InputBlocked) look = Vector2.zero;
            if(fixedAngle){Yaw=startYaw;pitch=startPitch;}
            else {Yaw += look.x; pitch = Mathf.Clamp(pitch - look.y, -20, 65);}
            Vector3 pivot = target.transform.position + Vector3.up * 1.35f;
            Quaternion rotation = Quaternion.Euler(pitch, Yaw, 0);
            Vector3 backward = rotation * Vector3.back;
            ShowPlayerThroughScenery(pivot,backward);
            // Stable arcade follow. Walls never pull the camera into the player;
            // one-time guidance still owns the camera while its input lock is held.
            transform.SetPositionAndRotation(pivot + backward * distance, rotation);
        }
        void RestoreWalls(){for(int i=0;i<hiddenCount;i++)if(hiddenWalls[i])hiddenWalls[i].forceRenderingOff=false;hiddenCount=0;}
        void ShowPlayerThroughScenery(Vector3 pivot,Vector3 backward)
        {
            RestoreWalls();
            if(overheadOccluder&&overheadOccluder.enabled&&!overheadOccluder.forceRenderingOff&&
                overheadOccluder.bounds.IntersectRay(new Ray(pivot,backward),out float roofDistance)&&roofDistance<=distance+.3f){
                overheadOccluder.forceRenderingOff=true;hiddenWalls[hiddenCount++]=overheadOccluder;
            }
            int count=Physics.RaycastNonAlloc(pivot,backward,visibilityHits,distance+.3f,obstacleMask,QueryTriggerInteraction.Ignore);
            for(int i=0;i<count;i++){
                var hit=visibilityHits[i];if(hit.transform.IsChildOf(target.transform))continue;
                var renderer=hit.collider.GetComponent<Renderer>();if(!renderer||renderer.forceRenderingOff||!renderer.enabled||renderer.bounds.size.y<2.5f)continue;
                if(hit.collider.GetComponentInParent<MineableRock>()||hit.collider.GetComponentInParent<TruckDelivery>())continue;
                string label=renderer.name;
                if(label.IndexOf("boundary",System.StringComparison.OrdinalIgnoreCase)<0&&label.IndexOf("bank",System.StringComparison.OrdinalIgnoreCase)<0&&label.IndexOf("Cut Rock",System.StringComparison.OrdinalIgnoreCase)<0&&label.IndexOf("blocker",System.StringComparison.OrdinalIgnoreCase)<0&&label.IndexOf("Entrance",System.StringComparison.OrdinalIgnoreCase)<0&&label.IndexOf("north cap",System.StringComparison.OrdinalIgnoreCase)<0)continue;
                renderer.forceRenderingOff=true;hiddenWalls[hiddenCount++]=renderer;
            }
        }
        void OnDisable(){RestoreWalls();}
    }
}
