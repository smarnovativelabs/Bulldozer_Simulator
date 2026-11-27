using Lofelt.NiceVibrations;
using UnityEngine;

namespace Bulldozer.UI
{
    // Short preset haptics for gameplay moments. Respects the Vibration setting and is
    // throttled so rapid pickups never turn into a continuous buzz on low-end phones.
    public static class QuarryHaptics
    {
        public enum Kind { Hit, Break, Pickup, Cash, Confirm, Error }
        static float nextLight;

        public static void Play(Kind kind)
        {
            if (!Application.isMobilePlatform || !QuarryUIFeedback.Vibration) return;
            bool important = kind == Kind.Break || kind == Kind.Cash || kind == Kind.Error;
            float now = Time.unscaledTime;
            if (!important)
            {
                if (now < nextLight) return;
                nextLight = now + (kind == Kind.Pickup ? .09f : .05f);
            }
            HapticPatterns.PlayPreset(Preset(kind));
        }

        static HapticPatterns.PresetType Preset(Kind kind)
        {
            switch (kind)
            {
                case Kind.Hit: return HapticPatterns.PresetType.LightImpact;
                case Kind.Break: return HapticPatterns.PresetType.MediumImpact;
                case Kind.Cash: return HapticPatterns.PresetType.Success;
                case Kind.Error: return HapticPatterns.PresetType.Warning;
                default: return HapticPatterns.PresetType.Selection;
            }
        }
    }
}
