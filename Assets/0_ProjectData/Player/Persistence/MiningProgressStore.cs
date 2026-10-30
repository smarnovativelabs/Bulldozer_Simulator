using System;
using System.IO;
using System.Linq;
using UnityEngine;
namespace Bulldozer.Player.Persistence
{
    [Serializable]
    public sealed class GroundOreState
    {
        public string id;
        public int item;
        public Vector3 position;
    }
    [Serializable]
    public sealed class RockDamageState
    {
        public string id;
        public int damage;
    }
    [Serializable]
    public sealed class MiningSnapshot
    {
        public string format;
        public int version;
        public int cargo,coins,damageLevel,capacityLevel;
        public int speedLevel;
        public int roundIndex;
        public bool roundBonusClaimed;
        public bool copperRidgeUnlocked,contractClaimed;
        public QuarryContract contractKind;
        public int contractProgress;
        public int taskRocks,taskDense,taskOreMask,taskRefined,taskDeliveries,taskClaimedMask;
        public int[] items=new int[0],refineryInput=new int[0],refineryOutput=new int[0],truckItems=new int[0];
        public string[] depletedBlocks=new string[0];
        public RockDamageState[] damagedBlocks=Array.Empty<RockDamageState>();
        public GroundOreState[] groundDrops=new GroundOreState[0];
        public int trucksCompleted;
        public int[] loadingQueue=new int[0];
        public bool wheelbarrowUnlocked,usingWheelbarrow;
        public int[] backpackItems=new int[0],barrowItems=new int[0];
        public Vector3 barrowPosition;
        public float refiningSeconds;
        public bool removeAds;
        public string[] fulfilledPurchases = Array.Empty<string>();
        public MiningSnapshot Copy()=>JsonUtility.FromJson<MiningSnapshot>(JsonUtility.ToJson(this));
        public const int CurrentVersion = 8;
        public static MiningSnapshot Create(int cargo,int coins)=>new MiningSnapshot{format="bulldozer-mining",version=CurrentVersion,cargo=cargo,coins=coins,items=new int[Mathf.Max(0,cargo)]};
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
            return s!=null && s.format=="bulldozer-mining" && s.version==MiningSnapshot.CurrentVersion && s.roundIndex>=0 && s.cargo>=0 && s.cargo<=100000 && s.coins>=0
                && s.fulfilledPurchases!=null && s.fulfilledPurchases.All(id=>!string.IsNullOrWhiteSpace(id)&&id.Length<=128)
                && s.fulfilledPurchases.Distinct().Count()==s.fulfilledPurchases.Length
                && s.contractKind>=QuarryContract.DeliverStone && s.contractKind<=QuarryContract.FinishExcavation
                && s.contractProgress>=0 && s.contractProgress<=1000
                && s.taskRocks>=0&&s.taskRocks<=1000&&s.taskDense>=0&&s.taskDense<=1000&&s.taskOreMask>=0&&s.taskOreMask<=63
                && s.taskRefined>=0&&s.taskRefined<=1000&&s.taskDeliveries>=0&&s.taskDeliveries<=1000&&s.taskClaimedMask>=0&&s.taskClaimedMask<=7
                && s.damageLevel>=0 && s.damageLevel<=10 && s.capacityLevel>=0 && s.capacityLevel<=15
                && s.speedLevel>=0 && s.speedLevel<=5
                && s.items!=null && s.items.Length==s.cargo && s.items.All(MiningResources.Valid)
                && s.backpackItems!=null && s.backpackItems.Length<=100000 && s.backpackItems.All(MiningResources.Valid)
                && s.barrowItems!=null && s.barrowItems.Length<=12 && s.barrowItems.All(MiningResources.Valid) && Finite(s.barrowPosition)
                && (!s.usingWheelbarrow || s.wheelbarrowUnlocked&&s.items.Length<=12)
                && s.refineryInput!=null && s.refineryInput.Length<=100 && s.refineryInput.All(i=>i>=0&&i<6)
                && s.refineryOutput!=null && s.refineryOutput.Length<=100 && s.refineryOutput.All(i=>i>=6&&i<12)
                && s.truckItems!=null && s.truckItems.Length<=5 && s.truckItems.All(i=>i>=6&&i<12)
                && s.loadingQueue!=null && s.loadingQueue.Length<=100 && s.loadingQueue.All(i=>i>=6&&i<12)
                && s.depletedBlocks!=null && s.groundDrops!=null && s.groundDrops.Length<=96
                && s.damagedBlocks!=null && s.damagedBlocks.All(d=>d!=null&&!string.IsNullOrWhiteSpace(d.id)&&d.damage>0&&Array.IndexOf(s.depletedBlocks,d.id)<0)
                && s.damagedBlocks.Select(d=>d.id).Distinct().Count()==s.damagedBlocks.Length
                && s.groundDrops.All(d=>d!=null&&!string.IsNullOrEmpty(d.id)&&d.item>=0&&d.item<6&&Finite(d.position))
                && s.groundDrops.Select(d=>d.id).Distinct().Count()==s.groundDrops.Length
                && s.trucksCompleted>=0 && s.refiningSeconds>=0 && !float.IsNaN(s.refiningSeconds) && !float.IsInfinity(s.refiningSeconds);
        }
        static bool Finite(Vector3 p)=>!float.IsNaN(p.x)&&!float.IsNaN(p.y)&&!float.IsNaN(p.z)&&!float.IsInfinity(p.x)&&!float.IsInfinity(p.y)&&!float.IsInfinity(p.z);
        static bool Read(string path,out MiningSnapshot data,out bool future)
        {
            data=null;future=false;
            try
            {
                if(!File.Exists(path))return false;
                data=JsonUtility.FromJson<MiningSnapshot>(File.ReadAllText(path));
                if(data!=null && data.loadingQueue==null)data.loadingQueue=Array.Empty<int>();
                if(data!=null&&data.backpackItems==null)data.backpackItems=Array.Empty<int>();
                if(data!=null&&data.barrowItems==null)data.barrowItems=Array.Empty<int>();
                future=data!=null&&data.format=="bulldozer-mining"&&data.version>MiningSnapshot.CurrentVersion;
                if(data!=null&&data.format=="bulldozer-mining"&&data.version==1&&data.cargo>=0&&data.cargo<=100000&&data.coins>=0)
                    data=MiningSnapshot.Create(data.cargo,data.coins);
                if(data!=null&&data.format=="bulldozer-mining"&&data.version==2){data.version=3;data.groundDrops=Array.Empty<GroundOreState>();}
                if(data!=null&&data.format=="bulldozer-mining"&&data.version==3){data.version=4;data.roundIndex=0;data.roundBonusClaimed=false;}
                if(data!=null&&data.format=="bulldozer-mining"&&data.version==4){data.version=5;OptionalTaskRules.Reset(data);}
                if(data!=null&&data.format=="bulldozer-mining"&&data.version==5){data.version=6;data.fulfilledPurchases=Array.Empty<string>();data.removeAds=false;}
                if(data!=null&&data.format=="bulldozer-mining"&&data.version==6){data.version=7;data.damagedBlocks=Array.Empty<RockDamageState>();}
                if(data!=null&&data.format=="bulldozer-mining"&&data.version==7){data.version=8;QuarryProgressionRules.MigrateExisting(data);}
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


