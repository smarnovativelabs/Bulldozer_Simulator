using System;
using System.IO;
using System.Linq;
using UnityEngine;
namespace Bulldozer.Player.Persistence
{
    [Serializable]
    public sealed class MiningSnapshot
    {
        public string format;
        public int version;
        public int cargo,coins,damageLevel,capacityLevel;
        public int[] items=new int[0],refineryInput=new int[0],refineryOutput=new int[0],truckItems=new int[0];
        public string[] depletedBlocks=new string[0];
        public int trucksCompleted;
        public float refiningSeconds;
        public MiningSnapshot Copy()=>JsonUtility.FromJson<MiningSnapshot>(JsonUtility.ToJson(this));
        public static MiningSnapshot Create(int cargo,int coins)=>new MiningSnapshot{format="bulldozer-mining",version=2,cargo=cargo,coins=coins,items=new int[Mathf.Max(0,cargo)]};
    }
    public sealed class MiningProgressStore
    {
        public string Path {get;}
        public string Error {get;private set;}
        public bool RecoveredBackup {get;private set;}
        public MiningSnapshot Current {get;private set;}=MiningSnapshot.Create(0,0);
        readonly SaveFileWriter writer;long sequence;bool writable=true;
        public static string DefaultPath
        {
            get
            {
#if UNITY_EDITOR
                string testPath=UnityEditor.SessionState.GetString("Bulldozer.Mining.TestSavePath","");
                if(!string.IsNullOrEmpty(testPath))return testPath;
#endif
                return System.IO.Path.Combine(Application.persistentDataPath,"Mining","progress.json");
            }
        }
        public MiningProgressStore(string path)
        {
            Path=path;writer=new SaveFileWriter(path,path+".tmp",path+".bak");
            if(!File.Exists(path)&&!File.Exists(path+".bak"))return;
            if(Read(path,out var data,out bool future)){Current=data;return;}
            if(!future&&Read(path+".bak",out data,out _)){Current=data;RecoveredBackup=true;return;}
            writable=false;Error=future?"Save belongs to a newer game version":"Save could not be read; original files preserved";
        }
        static bool Valid(MiningSnapshot s)
        {
            return s!=null && s.format=="bulldozer-mining" && s.version==2 && s.cargo>=0 && s.cargo<=100000 && s.coins>=0
                && s.damageLevel>=0 && s.damageLevel<=10 && s.capacityLevel>=0 && s.capacityLevel<=15
                && s.items!=null && s.items.Length==s.cargo && s.items.All(MiningResources.Valid)
                && s.refineryInput!=null && s.refineryInput.Length<=100 && s.refineryInput.All(i=>i>=0&&i<6)
                && s.refineryOutput!=null && s.refineryOutput.Length<=100 && s.refineryOutput.All(i=>i>=6&&i<12)
                && s.truckItems!=null && s.truckItems.Length<=5 && s.truckItems.All(i=>i>=6&&i<12)
                && s.depletedBlocks!=null && s.trucksCompleted>=0 && s.refiningSeconds>=0 && !float.IsNaN(s.refiningSeconds) && !float.IsInfinity(s.refiningSeconds);
        }
        static bool Read(string path,out MiningSnapshot data,out bool future)
        {
            data=null;future=false;
            try
            {
                if(!File.Exists(path))return false;
                data=JsonUtility.FromJson<MiningSnapshot>(File.ReadAllText(path));
                future=data!=null&&data.format=="bulldozer-mining"&&data.version>2;
                if(data!=null&&data.format=="bulldozer-mining"&&data.version==1&&data.cargo>=0&&data.cargo<=100000&&data.coins>=0)
                    data=MiningSnapshot.Create(data.cargo,data.coins);
                return Valid(data);
            }
            catch(Exception){return false;}
        }
        // Legacy callers preserve all unrelated production and upgrade state.
        public bool TryCommit(int cargo,int coins)
        {
            if(cargo<0||cargo>100000||coins<0){Error="Invalid mining balances";return false;}
            var next=Current.Copy();next.coins=coins;Array.Resize(ref next.items,cargo);next.cargo=cargo;return TryCommit(next);
        }
        public bool TryCommit(MiningSnapshot next)
        {
            if(!writable)return false;
            if(!Valid(next)){Error="Invalid mining progress";return false;}
            var result=writer.Write(++sequence,JsonUtility.ToJson(next,true),!RecoveredBackup);
            if(!result.Succeeded){Error="Could not save mining progress: "+result.Error;return false;}
            Current=next.Copy();Error=null;RecoveredBackup=false;return true;
        }
    }
}

