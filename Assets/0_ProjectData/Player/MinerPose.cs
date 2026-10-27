using System.Collections.Generic;
using UnityEngine;

namespace Bulldozer.Player
{
    // Initial procedural locomotion; replace with authored clips without changing controller or equipment.
    public sealed class MinerPose : MonoBehaviour
    {
        public MinerController controller;
        readonly Dictionary<string, Transform> bones = new Dictionary<string, Transform>();
        readonly Dictionary<string, Quaternion> rest = new Dictionary<string, Quaternion>();
        float phase, blend;
        void Awake()
        {
            foreach (Transform t in GetComponentsInChildren<Transform>())
            {
                string name = t.name.Substring(t.name.LastIndexOf(':') + 1);
                if (name == "LeftUpLeg" || name == "RightUpLeg" || name == "LeftLeg" || name == "RightLeg" || name == "LeftArm" || name == "RightArm" || name == "RightForeArm") { bones[name] = t; rest[name] = t.localRotation; }
            }
        }
        void Rotate(string name, float angle)
        {
            if (!bones.TryGetValue(name, out Transform bone)) return;
            bone.localRotation = rest[name];
            if (name == "LeftArm" || name == "RightArm")
                bone.rotation = Quaternion.AngleAxis(-Mathf.Sign(Vector3.Dot(bone.position - transform.position, transform.right)) * 25, transform.forward) * bone.rotation;
            bone.rotation = Quaternion.AngleAxis(angle, transform.right) * bone.rotation;
        }
        void LateUpdate()
        {
            if (!controller) return;
            blend = Mathf.Lerp(blend, controller.Grounded ? Mathf.Clamp01(controller.Speed / 3.2f) : 0, 12 * Time.deltaTime);
            phase += controller.Speed * Time.deltaTime * 3.7f;
            float stride = Mathf.Sin(phase) * 29 * blend;
            Rotate("LeftUpLeg", stride); Rotate("RightUpLeg", -stride);
            Rotate("LeftLeg", Mathf.Max(0, -Mathf.Sin(phase)) * 30 * blend);
            Rotate("RightLeg", Mathf.Max(0, Mathf.Sin(phase)) * 30 * blend);
            Rotate("LeftArm", -stride * .65f); Rotate("RightArm", stride * .35f - controller.Swing * 95);
            Rotate("RightForeArm", -controller.Swing * 25);
        }
    }
}

