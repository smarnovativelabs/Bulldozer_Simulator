using System;
using System.Linq;

namespace Bulldozer.Player.Persistence
{
    // Pure transaction rules shared by gameplay and regression checks.
    public static class ExcavationRules
    {
        public static int Bonus(int roundIndex)=>50+Math.Min(roundIndex,10)*10;
        public static bool CargoSettled(MiningSnapshot s)=>s.items.Length==0&&s.backpackItems.Length==0&&s.barrowItems.Length==0&&s.groundDrops.Length==0&&s.refineryInput.Length==0&&s.refineryOutput.Length==0&&s.loadingQueue.Length==0&&s.truckItems.Length==0;
        public static bool Cleared(MiningSnapshot s,string[] ids)=>ids!=null&&ids.Length>0&&ids.All(id=>Array.IndexOf(s.depletedBlocks,id)>=0)&&CargoSettled(s);
        public static bool TryComplete(MiningSnapshot s,string[] ids,out MiningSnapshot next)
        {
            next=null;if(s.roundBonusClaimed||!Cleared(s,ids)||s.coins>int.MaxValue-Bonus(s.roundIndex))return false;
            next=s.Copy();next.coins+=Bonus(s.roundIndex);next.roundBonusClaimed=true;return true;
        }
        public static bool TryAdvance(MiningSnapshot s,string[] ids,out MiningSnapshot next)
        {
            next=null;if(!s.roundBonusClaimed||!Cleared(s,ids)||s.roundIndex==int.MaxValue)return false;
            next=s.Copy();next.roundIndex++;next.roundBonusClaimed=false;next.depletedBlocks=Array.Empty<string>();next.refiningSeconds=0;
            // A new excavation starts on foot. Both inventories are already empty.
            next.usingWheelbarrow=false;next.barrowPosition=UnityEngine.Vector3.zero;OptionalTaskRules.Reset(next);return true;
        }
        public static int Health(int original,int roundIndex,string id,bool easy)
        {
            if(roundIndex==0)return original;
            uint hash=2166136261;unchecked{foreach(char c in id)hash=(hash^c)*16777619;hash^=(uint)roundIndex*2654435761;}
            int seam=QuarryBalance.Profile(roundIndex)==3?2:0;
            return easy?2+(int)(hash%2):Math.Min(16,Math.Max(4,original)+(int)(hash%3)-1+Math.Min(roundIndex/3,4)+seam);
        }
    }
}
