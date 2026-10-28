using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
namespace Bulldozer.Player.Editor
{
    public static class MiningParticlesVerification
    {
        public static string Run()
        {
            if(!EditorApplication.isPlaying||string.IsNullOrEmpty(SessionState.GetString("Bulldozer.Mining.TestSavePath","")))throw new Exception("Isolated Play mode required");
            var checks=new List<string>();Action<bool,string> check=(ok,label)=>checks.Add((ok?"PASS ":"FAIL ")+label);
            var d=UnityEngine.Object.FindFirstObjectByType<OreDropSystem>();var e=d.equipment;d.player.autoMine=false;d.player.CancelMining();var state=e.Snapshot;state.items=Array.Empty<int>();state.damageLevel=0;e.Commit(state);
            var rock=UnityEngine.Object.FindObjectsByType<MineableRock>(FindObjectsSortMode.None).Where(r=>!r.Depleted&&r.Health>1).OrderBy(r=>r.transform.position.z).First();
            Vector3 contact=rock.HitCollider.bounds.center+Vector3.back*.56f;int spark=d.SparkBursts,dust=d.DustBursts;
            check(rock.TryHit(e,contact)&&d.SparkBursts==spark+1&&d.DustBursts==dust,"Successful non-final hit emits sparks without completion dust");
            while(!rock.Depleted)if(!rock.TryHit(e,contact))throw new Exception("Final hit rejected unexpectedly");
            check(d.DustBursts==dust+1,"Exactly one dust burst on completed stone break");
            spark=d.SparkBursts;check(!rock.TryHit(e,contact)&&d.SparkBursts==spark&&d.DustBursts==dust+1,"Depleted stone cannot repeat effects");
            int children=d.transform.childCount;for(int i=0;i<100;i++)d.Impact(contact,Vector3.back,true,0);
            check(d.transform.childCount==children,"100 repeated bursts create no GameObjects");
            check(d.sparks.particleCount<=48&&d.dust.particleCount<=24&&d.chips.particleCount<=32,"Bounded particles: sparks <=48, dust <=24, chips <=32");
            check(!d.sparks.collision.enabled&&!d.sparks.lights.enabled&&!d.sparks.trails.enabled,"Sparks add no collisions, particle lights or trails");
            check(d.sparks.main.simulationSpace==ParticleSystemSimulationSpace.World&&d.dust.main.simulationSpace==ParticleSystemSimulationSpace.World,"Particles stay at impact when the player moves");
            foreach(var ps in new[]{d.sparks,d.dust,d.chips})ps.Clear();
            d.Impact(contact,Vector3.back,true,0);foreach(var ps in new[]{d.sparks,d.dust,d.chips}){ps.Simulate(.09f,false,false,true);ps.Pause();}
            check(d.sparks.particleCount>0&&d.dust.particleCount>0,"Sparks and dust remain visible during the break burst");
            checks.Add("Particle budget: at most 104 live mining particles across three reused renderers; sparks/dust are billboards. Device FPS not measured.");
            File.WriteAllLines("Maintenance/Miner/mining-particles-verification.txt",checks);return string.Join("\n",checks);
        }
    }
}
