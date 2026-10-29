using System;
using System.Collections.Generic;

namespace Bulldozer.Player.Persistence
{
    public static class QuarryBalance
    {
        static readonly int[] pickaxe={10,70,180,350,650,1100,1800,2800,4200,6000};
        static readonly int[] rack={15,45,90,150,240,360,520,750,1050,1450,1950,2600,3400,4400,5600};
        static readonly int[] speed={20,75,180,360,650};
        public static int DamageCost(int level)=>pickaxe[Math.Min(Math.Max(level,0),pickaxe.Length-1)];
        public static int CapacityCost(int level)=>rack[Math.Min(Math.Max(level,0),rack.Length-1)];
        public static int SpeedCost(int level)=>speed[Math.Min(Math.Max(level,0),speed.Length-1)];
        public static int Profile(int round)=>round==0?0:1+(round-1)%3;
        public static int PremiumOre(int round)=>(round/3)%6;
        public static float ProcessingMultiplier(int round)=>Profile(round)==2?.8f:1f;
        public static string Shift(int round)
        {
            switch(Profile(round))
            {
                case 1:return "RICH VEINS: "+MiningResources.Names[PremiumOre(round)]+" +25%";
                case 2:return "QUICK SHIFT: refining 25% faster";
                case 3:return "TOUGH SEAM: delivery value +20%";
                default:return "FIRST SHIFT: earn your first pickaxe upgrade";
            }
        }
        // Round the premium once per delivery, so mixed cargo has consistent payouts.
        public static int DeliveryValue(IEnumerable<int> items,int round)
        {
            long hundredths=0;
            foreach(int item in items)
            {
                int rate=Profile(round)==3?120:Profile(round)==1&&item%6==PremiumOre(round)?125:100;
                hundredths+=(long)MiningResources.Prices[item%6]*rate;
            }
            return (int)Math.Min(int.MaxValue,(hundredths+50)/100);
        }
    }
}
