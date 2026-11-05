using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
public static class OptimizeCartClips {
 static void Reduce(float[] values,int start,int end,float tolerance,SortedSet<int> keep){if(end-start<=1)return;float largest=tolerance;int at=-1;for(int i=start+1;i<end;i++){float error=Mathf.Abs(values[i]-Mathf.Lerp(values[start],values[end],(float)(i-start)/(end-start)));if(error>largest){largest=error;at=i;}}if(at>=0){keep.Add(at);Reduce(values,start,at,tolerance,keep);Reduce(values,at,end,tolerance,keep);}}
 public static object Main(){if(Application.isPlaying)throw new Exception("Edit mode only");var rows=new List<object>();
 foreach(string name in new[]{"CartIdle","CartPush","CartPull"}){
  var clip=AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/0_ProjectData/Player/Production/CartRelease/"+name+".anim");int before=0,after=0;float maxError=0;
  foreach(var binding in AnimationUtility.GetCurveBindings(clip)){
   var source=AnimationUtility.GetEditorCurve(clip,binding);before+=source.length;int count=Mathf.CeilToInt(clip.length*60);var values=new float[count+1];for(int i=0;i<=count;i++)values[i]=source.Evaluate(clip.length*i/count);
   var keep=new SortedSet<int>{0,count};Reduce(values,0,count,.0005f,keep);var curve=new AnimationCurve(keep.Select(i=>new Keyframe(clip.length*i/count,values[i])).ToArray());
   for(int i=0;i<curve.length;i++){AnimationUtility.SetKeyLeftTangentMode(curve,i,AnimationUtility.TangentMode.Linear);AnimationUtility.SetKeyRightTangentMode(curve,i,AnimationUtility.TangentMode.Linear);}
   float curveError=0;for(int i=0;i<=count*2;i++){float t=clip.length*i/(count*2);curveError=Mathf.Max(curveError,Mathf.Abs(curve.Evaluate(t)-source.Evaluate(t)));}if(curve.length>=source.length||curveError>.001f){after+=source.length;continue;}maxError=Mathf.Max(maxError,curveError);
   after+=curve.length;AnimationUtility.SetEditorCurve(clip,binding,curve);
  }
  EditorUtility.SetDirty(clip);rows.Add(new{name,before,after,maxError});
 }
 AssetDatabase.SaveAssets();var json=Newtonsoft.Json.JsonConvert.SerializeObject(rows,Newtonsoft.Json.Formatting.Indented);System.IO.File.WriteAllText("Maintenance/CartReleaseFix/clip-budget.json",json);return rows;
 }
}

