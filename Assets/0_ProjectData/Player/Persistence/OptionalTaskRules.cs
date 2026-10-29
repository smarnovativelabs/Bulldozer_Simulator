using System;

namespace Bulldozer.Player.Persistence
{
    // Applied inside the same durable transaction as the action; no claim button or timer.
    public static class OptionalTaskRules
    {
        public static int Target(int round,int slot)=>slot==0?(round==0?2:round%3==1?6:3):slot==1?(round==0?6:9+(round%3)*3):(round==0?2:3+round%2);
        public static int Reward(int round,int slot)=>round==0?10+slot*5:25+slot*10+Math.Min(round,5)*5;
        public static int Progress(MiningSnapshot s,int slot)
        {
            if(slot==1)return s.taskRefined;
            if(slot==2)return s.taskDeliveries;
            if(s.roundIndex==0||s.roundIndex%3==1)return s.taskRocks;
            if(s.roundIndex%3==0)return s.taskDense;
            int count=0;for(int i=0;i<6;i++)if((s.taskOreMask&(1<<i))!=0)count++;return count;
        }
        public static string Label(int round,int slot)=>slot==1?"Refine stones":slot==2?"Complete deliveries":round==0||round%3==1?"Break rocks":round%3==0?"Break tough rocks (6+ HP)":"Mine different ore types";
        public static void Rock(MiningSnapshot s,int ore,bool tough){s.taskRocks=Math.Min(1000,s.taskRocks+1);s.taskOreMask|=1<<ore;if(tough)s.taskDense=Math.Min(1000,s.taskDense+1);Award(s);}
        public static void Refined(MiningSnapshot s){s.taskRefined=Math.Min(1000,s.taskRefined+1);Award(s);}
        public static void Delivered(MiningSnapshot s){s.taskDeliveries=Math.Min(1000,s.taskDeliveries+1);Award(s);}
        public static void Award(MiningSnapshot s)
        {
            for(int slot=0;slot<3;slot++)
            {
                int bit=1<<slot,reward=Reward(s.roundIndex,slot);
                if((s.taskClaimedMask&bit)!=0||Progress(s,slot)<Target(s.roundIndex,slot)||s.coins>int.MaxValue-reward)continue;
                s.coins+=reward;s.taskClaimedMask|=bit;
            }
        }
        public static void Reset(MiningSnapshot s){s.taskRocks=s.taskDense=s.taskOreMask=s.taskRefined=s.taskDeliveries=s.taskClaimedMask=0;}
    }
}
