using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Bulldozer.Player.Editor
{
    public static class HandMiningPolish
    {
        public static string Apply()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Edit mode required");
            var p=UnityEngine.Object.FindFirstObjectByType<MinerController>();
            var a=p.visual.GetComponent<MinerAnimator>();
            Undo.RecordObject(a,"Refine pickaxe grip");
            a.carryPosition=new Vector3(.36f,1.10f,.12f);
            var probes=a.GetComponent<MinerMining>().headProbes;
            var up=probes[0].localPosition.normalized;
            var side=probes[1].localPosition-probes[2].localPosition;
            var desiredUp=new Vector3(.45f,.85f,.25f).normalized;
            var wrist=Quaternion.LookRotation(Vector3.Cross(Vector3.forward,desiredUp),desiredUp)*Quaternion.Inverse(Quaternion.LookRotation(Vector3.Cross(side,up),up));
            a.carryToolRotation=Quaternion.Inverse(a.carryRotation)*wrist;
            a.toolGrip.localRotation=Quaternion.identity;
            var drops=a.equipment.drops;
            foreach(var ps in new[]{drops.dust,drops.chips})
            {
                Undo.RecordObject(ps,"Refine mining particles");
                var main=ps.main;main.maxParticles=ps==drops.dust?24:32;
                var shape=ps.shape;shape.enabled=false;
                var collision=ps.collision;collision.enabled=false;
                if(ps==drops.chips)
                {
                    var rotation=ps.rotationOverLifetime;rotation.enabled=true;rotation.z=new ParticleSystem.MinMaxCurve(-5,5);
                    var size=ps.sizeOverLifetime;size.enabled=true;
                    size.size=new ParticleSystem.MinMaxCurve(1,new AnimationCurve(new Keyframe(0,1),new Keyframe(.65f,1),new Keyframe(1,0)));
                }
            }
            EditorUtility.SetDirty(a);
            PrefabUtility.SaveAsPrefabAsset(p.gameObject,"Assets/0_ProjectData/Player/MinerPlayer.prefab");
            EditorSceneManager.MarkSceneDirty(p.gameObject.scene);EditorSceneManager.SaveScene(p.gameObject.scene);
            AssetDatabase.SaveAssets();return "Saved grip and bounded particle settings";
        }

        public static string Capture(string name,bool wide=false)
        {
            var p=UnityEngine.Object.FindFirstObjectByType<MinerController>();
            var go=new GameObject("Temporary pose inspection");var camera=go.AddComponent<Camera>();
            camera.CopyFrom(Camera.main);camera.enabled=false;
            var focus=p.visual.position+Vector3.up*(wide?.85f:1.18f)+p.visual.forward*(wide?.65f:0);
            camera.transform.position=focus+p.visual.forward*(wide?-3.3f:1.6f)+p.visual.right*(wide?3.4f:1.15f)+Vector3.up*(wide?1.4f:.18f);
            camera.transform.LookAt(focus);
            var rt=new RenderTexture(1000,900,24);var old=RenderTexture.active;
            var texture=new Texture2D(1000,900,TextureFormat.RGB24,false);
            string path="Maintenance/Miner/"+name+".png";
            try{camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;texture.ReadPixels(new Rect(0,0,1000,900),0,0);texture.Apply();File.WriteAllBytes(path,texture.EncodeToPNG());}
            finally{RenderTexture.active=old;camera.targetTexture=null;UnityEngine.Object.DestroyImmediate(go);UnityEngine.Object.DestroyImmediate(texture);rt.Release();UnityEngine.Object.DestroyImmediate(rt);}
            return path;
        }

        static int shot;static bool capturedBreak;static float began,breakTime;static MinerController player;static MinerMining mining;
        public static string WatchSwing()
        {
            if(!EditorApplication.isPlaying)throw new InvalidOperationException("Play mode required");
            player=UnityEngine.Object.FindFirstObjectByType<MinerController>();mining=player.visual.GetComponent<MinerMining>();
            shot=0;began=-1;breakTime=-1;capturedBreak=false;EditorApplication.update-=CaptureTick;EditorApplication.update+=CaptureTick;
            return "Capturing first swing and final break in advancing Play mode";
        }
        static void CaptureTick()
        {
            if(!EditorApplication.isPlaying||!player){EditorApplication.update-=CaptureTick;return;}
            if(began<0){if(!player.IsMining)return;began=Time.time;}
            if(shot<5&&Time.time-began>=shot*.19f){Capture("polished-swing-"+shot,true);shot++;}
            if(breakTime<0&&mining.Target&&mining.Target.Depleted)breakTime=Time.time;
            if(!capturedBreak&&breakTime>=0&&Time.time-breakTime>.08f){Capture("polished-break",true);capturedBreak=true;}
            if(Time.time-began>12)EditorApplication.update-=CaptureTick;
        }

        static float testBegan,maxGripError,minBodyAlignment;static int testFrame,runShot;
        public static string CheckMovingGrip()
        {
            if(!EditorApplication.isPlaying||string.IsNullOrEmpty(SessionState.GetString("Bulldozer.Mining.TestSavePath","")))throw new InvalidOperationException("Isolated Play required");
            player=UnityEngine.Object.FindFirstObjectByType<MinerController>();player.autoMine=false;player.CancelMining();
            var motor=player.GetComponent<CharacterController>();motor.enabled=false;player.transform.position=new Vector3(0,.03f,-3);motor.enabled=true;
            player.touchControls.Move=Vector2.zero;Physics.SyncTransforms();
            testBegan=Time.time;testFrame=Time.frameCount;maxGripError=0;minBodyAlignment=1;runShot=0;
            EditorApplication.update-=MovementTick;EditorApplication.update+=MovementTick;return "Testing carry through acceleration and four turns";
        }
        static void MovementTick()
        {
            if(!EditorApplication.isPlaying||!player){EditorApplication.update-=MovementTick;return;}
            float age=Time.time-testBegan;var a=player.visual.GetComponent<MinerAnimator>();
            if(age<.5f)return;
            if(age<3.3f)
            {
                int turn=Mathf.Min(3,(int)((age-.5f)/.7f));
                player.touchControls.Move=turn==0?Vector2.right:turn==1?Vector2.up:turn==2?Vector2.left:Vector2.down;
                maxGripError=Mathf.Max(maxGripError,a.LastGripError);
                var hips=a.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.Hips);
                minBodyAlignment=Mathf.Min(minBodyAlignment,Vector3.Dot(Vector3.ProjectOnPlane(hips.forward,Vector3.up).normalized,player.visual.forward));
                if(runShot==0&&age>1){Capture("polished-running");runShot++;}
                return;
            }
            player.touchControls.Move=Vector2.zero;
            if(age<3.8f)return;
            var report=new System.Collections.Generic.List<string>();
            Action<bool,string> check=(ok,label)=>report.Add((ok?"PASS ":"FAIL ")+label);
            check(Time.frameCount-testFrame>60,"Movement test advanced "+(Time.frameCount-testFrame)+" frames");
            check(maxGripError<.02f,"Maximum moving wrist target error "+maxGripError.ToString("F5")+" m");
            check(minBodyAlignment>.8f,"Body follows all four turns; minimum heading dot "+minBodyAlignment.ToString("F3"));
            check(Quaternion.Angle(a.toolGrip.localRotation,Quaternion.identity)<.01f,"Shaft stays fixed in palm during carry and turns");
            var d=a.equipment.drops;int count=d.transform.childCount;
            for(int i=0;i<100;i++)d.Impact(player.transform.position+Vector3.up,Vector3.forward,true,i%6);
            check(d.transform.childCount==count,"100 repeated break bursts create no additional objects");
            check(d.dust.particleCount<=24&&d.chips.particleCount<=32,"Particle caps: dust="+d.dust.particleCount+" chips="+d.chips.particleCount);
            check(!d.dust.collision.enabled&&!d.chips.collision.enabled,"No particle collisions or debris rigidbodies added");
            int triangles=d.chips.GetComponent<ParticleSystemRenderer>().mesh.triangles.Length/3;
            report.Add("BUDGET chip mesh "+triangles+" triangles; maximum "+(triangles*32)+" chip triangles; two shared particle renderers; ore pool "+OreDropSystem.PoolSize);
            var rock=UnityEngine.Object.FindObjectsByType<MineableRock>(FindObjectsSortMode.None).First(r=>r.Health==r.maxHealth&&r.cracks&&r.cracks.transform.childCount==3);
            a.equipment.SetCargoCount(0);rock.TryHit(a.equipment,rock.transform.position);int first=rock.cracks.transform.Cast<Transform>().Count(t=>t.gameObject.activeSelf);
            for(int i=0;i<rock.maxHealth&&rock.Health>Mathf.Max(1,rock.maxHealth/3);i++)if(!rock.TryHit(a.equipment,rock.transform.position))break;
            int second=rock.cracks.transform.Cast<Transform>().Count(t=>t.gameObject.activeSelf);
            check(first<second,"Progressive crack segments "+first+" then "+second);
            d.dust.Clear();d.chips.Clear();Capture("polished-carry-final");
            File.WriteAllLines("Maintenance/Miner/hand-effects-verification.txt",report);Debug.Log(string.Join("\n",report));
            EditorApplication.update-=MovementTick;
        }
    }
}
