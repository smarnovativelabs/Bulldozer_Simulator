using System;
using System.IO;
using UnityEngine;
namespace Bulldozer.Services
{
    public sealed partial class QuarryOnlineServices
    {
        [Serializable] sealed class PendingAdReward { public string token; public int cash; }
        string RewardPath => Path.Combine(Application.persistentDataPath, "quarry-pending-ad-reward.json");
        bool JournalReward()
        {
            try {
                var json = JsonUtility.ToJson(new PendingAdReward { token = rewardToken, cash = rewardCash });
                File.WriteAllText(RewardPath + ".tmp", json);
                using (var stream = new FileStream(RewardPath + ".tmp", FileMode.Open, FileAccess.Write, FileShare.None)) stream.Flush(true);
                if (File.Exists(RewardPath)) File.Replace(RewardPath + ".tmp", RewardPath, RewardPath + ".bak");
                else File.Move(RewardPath + ".tmp", RewardPath);
                return true;
            } catch { return false; }
        }
        void RecoverReward()
        {
            foreach (var path in new[] { RewardPath, RewardPath + ".bak" })
                try {
                    if (!File.Exists(path)) continue;
                    var reward = JsonUtility.FromJson<PendingAdReward>(File.ReadAllText(path));
                    if (reward == null || string.IsNullOrWhiteSpace(reward.token) || !reward.token.StartsWith("ad:") || reward.cash <= 0) continue;
                    rewardToken = reward.token; rewardCash = reward.cash; rewardGranted = true;
                    rewardSaveAt = Time.realtimeSinceStartupAsDouble + 1; rewardCloseAt = Time.realtimeSinceStartupAsDouble + 2; return;
                } catch {}
        }
        void ClearRewardJournal()
        {
            // Wallet already contains the token; surviving backup files can only replay idempotently.
            try { if (File.Exists(RewardPath + ".bak")) File.Delete(RewardPath + ".bak"); if (File.Exists(RewardPath)) File.Delete(RewardPath); } catch {}
        }
    }
}
