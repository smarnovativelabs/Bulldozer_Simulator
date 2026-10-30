using System;
using System.Linq;

namespace Bulldozer.Player.Persistence
{
    // Persisted numeric IDs: never renumber these when adding future contracts.
    public enum QuarryContract { DeliverStone, CartPickup, DeliverCopper, ToughRocks, FinishExcavation }

    public static class QuarryProgressionRules
    {
        public static QuarryContract ForRound(int round) => round == 0 ? QuarryContract.DeliverStone :
            round == 1 ? QuarryContract.CartPickup : round == 2 ? QuarryContract.DeliverCopper :
            (round - 3) % 3 == 0 ? QuarryContract.DeliverStone : (round - 3) % 3 == 1 ? QuarryContract.DeliverCopper : QuarryContract.ToughRocks;
        public static int Target(MiningSnapshot s) => s.contractKind == QuarryContract.FinishExcavation ? 1 :
            s.contractKind == QuarryContract.ToughRocks ? 6 : s.roundIndex < 3 ? 12 : 18;
        public static int Reward(MiningSnapshot s) => s.roundIndex == 0 ? 40 : s.roundIndex == 1 ? 60 : s.roundIndex == 2 ? 90 : 100;
        public static string ContractName(QuarryContract kind)
        {
            switch (kind)
            {
                case QuarryContract.CartPickup: return "Collect ore with the wheelbarrow";
                case QuarryContract.DeliverCopper: return "Deliver copper ingots";
                case QuarryContract.ToughRocks: return "Break tough rocks (6+ HP)";
                case QuarryContract.FinishExcavation: return "Finish this excavation";
                default: return "Deliver refined stones";
            }
        }
        public static void Add(MiningSnapshot s, QuarryContract kind, int amount)
        {
            if (s.contractClaimed || s.contractKind != kind || amount <= 0) return;
            s.contractProgress = (int)Math.Min(Target(s), (long)s.contractProgress + amount);
            Award(s);
        }
        public static void Award(MiningSnapshot s)
        {
            int reward = Reward(s);
            if (s.contractClaimed || s.contractProgress < Target(s) || s.coins > int.MaxValue - reward) return;
            s.coins += reward;
            s.contractClaimed = true;
        }
        public static void Delivered(MiningSnapshot s, int[] items)
        {
            Add(s, QuarryContract.DeliverStone, items.Length);
            Add(s, QuarryContract.DeliverCopper, items.Count(item => item == 7));
        }
        public static void Complete(MiningSnapshot s)
        {
            // These unlocks share the durable completion transaction. No scene-only ownership.
            s.wheelbarrowUnlocked = true;
            if (s.roundIndex >= 2) s.copperRidgeUnlocked = true;
            Add(s, QuarryContract.FinishExcavation, 1);
        }
        public static void ResetContract(MiningSnapshot s)
        {
            s.contractKind = ForRound(s.roundIndex); s.contractProgress = 0; s.contractClaimed = false;
        }
        public static bool AreaAvailable(MiningSnapshot s) => s.copperRidgeUnlocked && s.roundIndex >= 3;
        public static string Phase(int round) => round == 0 ? "1 / FOUNDATION" : round == 1 ? "2 / WORK SMARTER" :
            round == 2 ? "3 / OPEN THE RIDGE" : "COPPER RIDGE OPERATIONS";
        public static string Milestone(MiningSnapshot s)
        {
            if (s.roundIndex == 0 && !s.roundBonusClaimed) return s.wheelbarrowUnlocked ? "Finish shift 1 to progress toward Copper Ridge" : "Finish shift 1: FREE WHEELBARROW";
            if (s.copperRidgeUnlocked) return AreaAvailable(s) ? "COPPER RIDGE OPEN - follow the east signs" : "COPPER RIDGE UNLOCKED - opens next excavation";
            return "COPPER RIDGE  " + Math.Min(3, s.roundIndex + (s.roundBonusClaimed ? 1 : 0)) + "/3 excavations complete";
        }
        public static string NextPreview(MiningSnapshot s)
        {
            if (s.roundIndex == int.MaxValue) return "All excavations completed.";
            var next = s.Copy(); next.roundIndex++; ResetContract(next);
            string destination = next.roundIndex == 1 ? "Your wheelbarrow is ready. Collect more ore per trip." :
                next.roundIndex == 2 ? "Complete this shift to unlock Copper Ridge." :
                next.roundIndex == 3 ? "COPPER RIDGE OPENS: six new copper deposits beyond the east gate." : QuarryBalance.Shift(next.roundIndex);
            return "NEXT: EXCAVATION " + ((long)next.roundIndex + 1) + "\n" + destination + "\nContract: " +
                ContractName(next.contractKind) + "  " + Target(next) + "  |  +$" + Reward(next);
        }
        public static void MigrateExisting(MiningSnapshot s)
        {
            // Existing near-complete saves get an achievable goal, never a retroactive cash payout.
            s.contractKind = QuarryContract.FinishExcavation;
            s.contractProgress = s.roundBonusClaimed ? 1 : 0; s.contractClaimed = s.roundBonusClaimed;
            if (s.roundIndex > 0 || s.roundBonusClaimed) s.wheelbarrowUnlocked = true;
            s.copperRidgeUnlocked = s.roundIndex >= 3 || (s.roundIndex == 2 && s.roundBonusClaimed);
        }
    }
}
