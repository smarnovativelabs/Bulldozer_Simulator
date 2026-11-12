using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using Bulldozer.Player;
public static class InspectSwing{
 public static string Main(){var p=UnityEngine.Object.FindFirstObjectByType<MinerController>();var clone=UnityEngine.Object.Instantiate(p.visual.gameObject);clone.transform.SetPositionAndRotation(Vector3.zero,Quaternion.identity);foreach(var s in clone.GetComponentsInChildren<MonoBehaviour>(true))s.enabled=false;var a=clone.GetComponent<Animator>();a.Rebind();a.Update(0);var clip=AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/0_ProjectData/Player/MiningGround.anim");var hand=a.GetBoneTransform(HumanBodyBones.RightHand);var m=clone.GetComponent<MinerMining>();var lines=new System.Collections.Generic.List<string>();try{for(int i=0;i<=10;i++){clip.SampleAnimation(clone,clip.length*i/10f);lines.Add(i+" hand="+hand.position.ToString("F3")+" head="+m.headProbes[0].position.ToString("F3")+" tips="+(m.headProbes[1].position-m.headProbes[2].position).ToString("F3"));}return string.Join("\n",lines);}finally{UnityEngine.Object.DestroyImmediate(clone);}}
}
