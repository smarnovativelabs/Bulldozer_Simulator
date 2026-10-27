using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Bulldozer.Player.Editor
{
    public static class MinerVerification
    {
        static MinerController player;
        static Vector3 start;
        static float began, jumpStarted, moved, peak, maxSwing;
        static int stage, firstFrame;
        public static void Start() {
            if(string.IsNullOrEmpty(UnityEditor.SessionState.GetString("Bulldozer.Mining.TestSavePath","")))throw new System.InvalidOperationException("Prepare an isolated save with DepositVerification.Prepare before running destructive player tests.");
            if (!EditorApplication.isPlaying) throw new InvalidOperationException("Enter Play mode first.");
            player=UnityEngine.Object.FindFirstObjectByType<MinerController>();
            if (!player || !player.touchControls || !player.followCamera) throw new InvalidOperationException("Player references missing.");
            start=player.transform.position;began=Time.time;stage=0;peak=start.y;moved=maxSwing=0;firstFrame=Time.frameCount;
            player.touchControls.Move=Vector2.up;
            EditorApplication.update-=Tick;EditorApplication.update+=Tick;
        }
        static void Tick()
        {
            if (!EditorApplication.isPlaying || !player) { EditorApplication.update-=Tick; return; }
            if(stage==0 && Time.time-began>.6f)
            {
                player.touchControls.Move=Vector2.zero;moved=Vector3.Distance(start,player.transform.position);
                player.Jump();player.Mine();jumpStarted=Time.time;stage=1;
            }
            if(stage!=1)return;
            peak=Mathf.Max(peak,player.transform.position.y);maxSwing=Mathf.Max(maxSwing,player.Swing);
            if(Time.time-jumpStarted<1.7f)return;
            EditorApplication.update-=Tick;
            bool pass=moved>.7f && peak-start.y>.5f && player.Grounded && maxSwing>.5f && Time.frameCount-firstFrame>15;
            string report=$"passed={pass}\nframes={Time.frameCount-firstFrame}\nmovementMetres={moved:F3}\njumpRiseMetres={peak-start.y:F3}\nlanded={player.Grounded}\nswingPeak={maxSwing:F3}\nplayer={player.transform.position}\ncamera={player.followCamera.transform.position}\n";
            File.WriteAllText("Maintenance/Miner/playmode-verification.txt",report);Debug.Log(report);
        }
    }
}

