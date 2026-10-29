using System;
using System.Linq;
using Bulldozer.Player.Persistence;

namespace Bulldozer.Services
{
    // The receipt ledger and reward share the same atomic mining save transaction.
    public static class QuarryPurchaseGrant
    {
        public static bool Prepare(MiningSnapshot current, string verifiedTransaction, int cash, bool removeAds, out MiningSnapshot next)
        {
            next = null;
            if (current == null || string.IsNullOrWhiteSpace(verifiedTransaction) || verifiedTransaction.Length > 128 || cash < 0 || (removeAds && cash != 0) || (!removeAds && cash == 0)) return false;
            next = current.Copy();
            if (Array.IndexOf(next.fulfilledPurchases, verifiedTransaction) >= 0) return true;
            if ((long)next.coins + cash > int.MaxValue) { next = null; return false; }
            next.coins += cash;
            next.removeAds |= removeAds;
            next.fulfilledPurchases = next.fulfilledPurchases.Append(verifiedTransaction).ToArray();
            return true;
        }
    }
}
