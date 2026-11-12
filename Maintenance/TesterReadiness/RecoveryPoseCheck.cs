using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Bulldozer.Player;
public static class RecoveryPoseCheck {
 public static string Main(){
  if(Application.isPlaying)throw new Exception("Edit mode required");var p=UnityEngine.Object.FindFirstObjectByType<MinerController>();var clone=UnityEngine.Object.Instantiate(p.visual.gameObject);clone.transform.SetPositionAndRotation(Vector3.zero,Quaternion.identity);foreach(var s in clone.GetComponentsInChildren<MonoBehaviour>(true))s.enabled=false;var a=clone.GetComponent<Animator>();a.Rebind();a.Update(0);var tool=clone.GetComponent<MinerAnimator>().toolGrip;var log=new List<string>();
  try{foreach(string kind in new[]{"Ground","Wall"}){
   var clip=AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/0_ProjectData/Player/Production/VerticalMining/Vertical"+kind+".anim");float maxAngle=0,maxPosition=0;
   for(int i=0;i<=20;i++){float u=i/20f;clip.SampleAnimation(clone,(.10f+.30f*u)*clip.length);var position=tool.position;var rotation=tool.rotation;clip.SampleAnimation(clone,(.85f-.30f*u)*clip.length);maxAngle=Mathf.Max(maxAngle,Quaternion.Angle(rotation,tool.rotation));maxPosition=Mathf.Max(maxPosition,Vector3.Distance(position,tool.position));}
   log.Add((maxAngle<4&&maxPosition<.015f?"PASS ":"FAIL ")+kind+" recovery retraces strike; max angle="+maxAngle+" position="+maxPosition);
   clip.SampleAnimation(clone,0);var start=tool.rotation;clip.SampleAnimation(clone,clip.length);float seam=Quaternion.Angle(start,tool.rotation);log.Add((seam<.1f?"PASS ":"FAIL ")+kind+" matching repeated-swing seam="+seam);
   float grip=0;string detail="";var localUp=clone.GetComponent<MinerMining>().headProbes[0].localPosition.normalized;for(int i=0;i<=120;i++){clip.SampleAnimation(clone,clip.length*i/120f);var hand=a.GetBoneTransform(HumanBodyBones.LeftHand);var target=tool.TransformPoint(-localUp*.22f);float error=Vector3.Distance(hand.position,target);if(error>grip){grip=error;var shoulder=a.GetBoneTransform(HumanBodyBones.LeftUpperArm);var elbow=a.GetBoneTransform(HumanBodyBones.LeftLowerArm);detail=" frame="+i+" target="+target+" shoulder="+shoulder.position+" reach="+(Vector3.Distance(shoulder.position,elbow.position)+Vector3.Distance(elbow.position,hand.position))+" distance="+Vector3.Distance(shoulder.position,target);}}log.Add((grip<.06f?"PASS ":"FAIL ")+kind+" final-clip support grip="+grip+detail);
  }File.WriteAllLines("Maintenance/TesterReadiness/recovery-pose-checks.txt",log);return string.Join("\n",log);}finally{UnityEngine.Object.DestroyImmediate(clone);}
 }
}

