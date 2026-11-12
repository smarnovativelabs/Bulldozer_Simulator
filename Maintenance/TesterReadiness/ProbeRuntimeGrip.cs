using System;
using System.Linq;
using UnityEngine;
using Bulldozer.Player;
public static class ProbeRuntimeGrip {
 public static string Main(){var p=UnityEngine.Object.FindFirstObjectByType<MinerController>();var driver=p.visual.GetComponent<MinerAnimator>();var a=p.visual.GetComponent<Animator>();var m=p.visual.GetComponent<MinerMining>();var up=m.headProbes[0].localPosition.normalized;var lines=new System.Collections.Generic.List<string>();p.autoMine=false;p.CancelMining();driver.enabled=false;int layer=a.GetLayerIndex("Pickaxe Carry");a.SetLayerWeight(layer,0);a.SetBool("Mining",true);try{for(int i=0;i<=10;i++){a.Play("Mine Wall",0,i/10f);a.Update(0);var target=driver.toolGrip.TransformPoint(-up*.22f);var hand=a.GetBoneTransform(HumanBodyBones.LeftHand);lines.Add(i+" error="+Vector3.Distance(hand.position,target)+" hand="+hand.position+" target="+target+" carry="+a.GetLayerWeight(layer));}return string.Join("\n",lines);}finally{driver.enabled=true;a.SetBool("Mining",false);a.Play("Locomotion",0,0);a.Update(0);}}
}
