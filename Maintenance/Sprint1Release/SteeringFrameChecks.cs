using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Bulldozer.Player;
public static class SteeringFrameChecks {
public static string Main(){var c=UnityEngine.Object.FindFirstObjectByType<QuarryWheelbarrow>();var field=typeof(QuarryWheelbarrow).GetField("turnVelocity",BindingFlags.NonPublic|BindingFlags.Instance);var original=field.GetValue(c);var log=new List<string>();int failures=0;
try{foreach(int fps in new[]{60,30,15,10}){field.SetValue(c,0f);var q=Quaternion.Euler(0,180,0);float dt=1f/fps,time=0;bool bounded=true;while(time<2&&Quaternion.Angle(q,Quaternion.identity)>15){var next=c.SmoothFacing(Vector3.forward,q,dt);bounded&=Quaternion.Angle(q,next)<=c.maxTurnSpeed*dt+.01f;q=next;time+=dt;}bool ok=time<1&&bounded;if(!ok)failures++;log.Add((ok?"PASS ":"FAIL ")+fps+" Hz simulated delta: 180-degree turn within 15 degrees in "+time.ToString("F3")+"s; speed bounded="+bounded);}
log.Add("COMPLETE failures="+failures+" (math timestep checks, not device FPS)");File.WriteAllLines("Maintenance/Sprint1Release/steering-frame-checks.txt",log);if(failures>0)throw new Exception(log[log.Count-1]);return string.Join("\n",log);
}finally{field.SetValue(c,original);}}
}
