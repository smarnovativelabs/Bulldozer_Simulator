using UnityEngine;
using UnityEngine.UI;
using TMPro;
namespace Bulldozer.Services
{
    public sealed class QuarryPrivacyPanel : MonoBehaviour
    {
        public Toggle analytics, crashReports, attribution;
        public TMP_Text status;
        void OnEnable() { var s=QuarryOnlineServices.Instance; if(!s)return; analytics.SetIsOnWithoutNotify(s.Analytics);crashReports.SetIsOnWithoutNotify(s.CrashReports);attribution.SetIsOnWithoutNotify(s.Attribution);status.text=s.Status; }
        public void Analytics(bool value){QuarryOnlineServices.Instance?.SetAnalytics(value);Click();}
        public void CrashReports(bool value){QuarryOnlineServices.Instance?.SetCrashReports(value);Click();}
        public void Attribution(bool value){QuarryOnlineServices.Instance?.SetAttribution(value);Click();}
        public void AdPrivacy(){QuarryOnlineServices.Instance?.PrivacyOptions();Click();}
        public void Retry(){QuarryOnlineServices.Instance?.Retry();Click();}
        public void Policy(){var s=QuarryOnlineServices.Instance;if(s&&System.Uri.TryCreate(s.settings.privacyPolicyUrl,System.UriKind.Absolute,out var u)&&u.Scheme=="https")Application.OpenURL(u.AbsoluteUri);else status.text="Privacy policy URL has not been configured.";Click();}
        void Click(){Bulldozer.UI.QuarryGameUI.Instance?.feedback.Play();}
    }
}
