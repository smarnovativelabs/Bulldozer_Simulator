using UnityEngine;

namespace Bulldozer.Services
{
    [CreateAssetMenu(menuName = "Quarry/Service Settings")]
    public sealed class QuarryServiceSettings : ScriptableObject
    {
        [Header("This game's accounts only — leave blank until registered")]
        public string androidRemoveAdsId, iosRemoveAdsId;
        public string androidCashPackId, iosCashPackId;
        [Min(1)] public int cashPackAmount = 1000;
        [TextArea] public string googlePlayPublicKey;
        public string maxSdkKey, androidInterstitialId, iosInterstitialId;
        [Header("MAX rewarded ad units")]
        public string androidRewardedId, iosRewardedId;
        [Header("AdMob ad units")]
        public string androidAdMobInterstitialId, iosAdMobInterstitialId;
        public string androidAdMobRewardedId, iosAdMobRewardedId;
        [Header("Attribution and service configuration")]
        public string androidTenjinKey, iosTenjinKey;
        public bool firebaseConfigurationAdded;
        [Tooltip("Set after adding this game's app IDs in Google Mobile Ads settings.")]
        public bool adApplicationIdsConfigured;
        public bool underAgeOfConsent;
        [Min(60)] public float interstitialCooldown = 180;
        public string privacyPolicyUrl;
        [Min(1)] public int rewardedCashAmount = 50;
        [Range(1, 10)] public int interstitialWarningSeconds = 5;
        public string[] testDeviceAdvertisingIds = new string[0];
        public string RewardedId => IsApple ? iosRewardedId : androidRewardedId;
        public string RemoveAdsId => IsApple ? iosRemoveAdsId : androidRemoveAdsId;
        public string CashPackId => IsApple ? iosCashPackId : androidCashPackId;
        public string InterstitialId => IsApple ? iosInterstitialId : androidInterstitialId;
        public string TenjinKey => IsApple ? iosTenjinKey : androidTenjinKey;
        public static bool IsApple {
            get {
#if UNITY_IOS
                return true;
#else
                return false;
#endif
            }
        }
    }
}
