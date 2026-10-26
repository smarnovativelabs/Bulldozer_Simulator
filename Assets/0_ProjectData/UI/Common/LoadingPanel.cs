using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Bulldozer.UI
{
    /// <summary>Authored loading artwork. All motion uses unscaled time; hidden panels do no work.</summary>
    public sealed class LoadingPanel : MonoBehaviour
    {
        [SerializeField] private CanvasGroup cover;
        [SerializeField] private CanvasGroup contentGroup;
        [SerializeField] private RectTransform safeArea;
        [SerializeField] private RectTransform composition;
        [SerializeField] private RectTransform hero;
        [SerializeField] private RectTransform copy;
        [SerializeField] private RectTransform pickaxe;
        [SerializeField] private RectTransform rock;
        [SerializeField] private CanvasGroup impact;
        [SerializeField] private UnityEngine.UI.Image progressFill;
        [SerializeField] private TMP_Text status;
        [SerializeField] private TMP_Text percentage;
        [SerializeField] private TMP_Text tip;
        [SerializeField] private UnityEngine.UI.Button retryButton;
        [SerializeField, Min(0f)] private float revealSeconds = .35f;
        [SerializeField] private bool reducedMotion;

        private Sequence idle;
        private Tween fade;
        private Tween entrance;
        private float target;
        private float displayed;
        private int lastPercentage = -1;
        private Vector2 lastSize;
        private bool showingError;
        public event System.Action RetryRequested;
        public float DisplayedProgress => displayed;
        public bool IsVisible => gameObject.activeSelf;
        public bool BlocksInput => IsVisible && cover.blocksRaycasts;

        private void Awake()
        {
            retryButton.onClick.AddListener(Retry);
            HideImmediately();
        }

        private void Retry() => RetryRequested?.Invoke();

        public void Show()
        {
            StopMotion();
            gameObject.SetActive(true);
            showingError = false;
            displayed = target = 0f;
            lastPercentage = -1;
            progressFill.fillAmount = 0f;
            progressFill.rectTransform.anchorMax = new Vector2(0f, 1f);
            cover.alpha = 1f; // Opaque before the first expensive scene operation.
            cover.blocksRaycasts = true;
            cover.interactable = true;
            retryButton.gameObject.SetActive(false);
            status.text = "Preparing your worksite";
            tip.text = "Every great worksite starts with a single strike.";
            contentGroup.alpha = reducedMotion ? 1f : 0f;
            ApplyLayout(true);
            if (!reducedMotion)
            {
                entrance = contentGroup.DOFade(1f, .3f).SetUpdate(true);
                idle = DOTween.Sequence().SetUpdate(true).SetLoops(-1);
                idle.Append(pickaxe.DOLocalRotate(new Vector3(0, 0, -28), .45f).SetEase(Ease.InOutSine));
                idle.Append(pickaxe.DOLocalRotate(new Vector3(0, 0, 24), .18f).SetEase(Ease.InQuad));
                idle.AppendCallback(() => impact.alpha = 1f);
                idle.Append(rock.DOScale(.94f, .08f));
                idle.Join(impact.DOFade(0f, .25f));
                idle.Append(rock.DOScale(1f, .18f).SetEase(Ease.OutBack));
                idle.Append(pickaxe.DOLocalRotate(new Vector3(0, 0, 0), .4f).SetEase(Ease.OutSine));
                idle.AppendInterval(.2f);
            }
        }

        public void Report(float progress, string message)
        {
            if (!float.IsNaN(progress) && !float.IsInfinity(progress))
                target = Mathf.Max(target, Mathf.Clamp01(progress));
            if (!string.IsNullOrEmpty(message)) status.text = message;
        }

        private void Update()
        {
            ApplyLayout(false);
            if (showingError) return;
            displayed = Mathf.MoveTowards(displayed, target, Time.unscaledDeltaTime * 1.3f);
            progressFill.fillAmount = displayed;
            progressFill.rectTransform.anchorMax = new Vector2(displayed, 1f);
            int value = Mathf.FloorToInt(displayed * 100f);
            if (lastPercentage == value) return;
            lastPercentage = value;
            percentage.SetText("{0}%", value);
        }

        private void ApplyLayout(bool force)
        {
            Vector2 size = safeArea.rect.size;
            if (!force && size == lastSize) return;
            lastSize = size;
            bool wide = size.x > size.y * 1.15f;
            Vector2 design = wide ? new Vector2(1600, 740) : new Vector2(850, 1160);
            composition.sizeDelta = design;
            float scale = Mathf.Min((size.x - 72f) / design.x, (size.y - 72f) / design.y, 1.4f);
            composition.localScale = Vector3.one * Mathf.Max(.1f, scale);
            hero.anchoredPosition = wide ? new Vector2(-460, 0) : new Vector2(0, 310);
            copy.anchoredPosition = wide ? new Vector2(280, 0) : new Vector2(0, -220);
        }

        public IEnumerator Reveal()
        {
            Report(1f, "Ready to begin");
            while (displayed < .999f) yield return null;
            percentage.text = "100%";
            yield return new WaitForSecondsRealtime(.12f);
            idle?.Kill();
            idle = null;
            fade = cover.DOFade(0f, reducedMotion ? 0f : revealSeconds).SetUpdate(true);
            yield return fade.WaitForCompletion();
            HideImmediately();
        }

        public void ShowError(string message, bool canRetry)
        {
            StopMotion();
            gameObject.SetActive(true);
            showingError = true;
            cover.alpha = contentGroup.alpha = 1f;
            cover.blocksRaycasts = cover.interactable = true;
            status.text = "A little pause before we begin";
            percentage.text = "";
            tip.text = message;
            retryButton.gameObject.SetActive(canRetry);
            ApplyLayout(true);
        }

        public void HideImmediately()
        {
            StopMotion();
            cover.alpha = 0f;
            cover.blocksRaycasts = cover.interactable = false;
            gameObject.SetActive(false);
        }

        private void StopMotion()
        {
            idle?.Kill(); fade?.Kill(); entrance?.Kill();
            idle = null; fade = entrance = null;
            pickaxe.localRotation = Quaternion.identity;
            rock.localScale = Vector3.one;
            impact.alpha = 0f;
        }

        private void OnDisable() => StopMotion();
        private void OnDestroy()
        {
            if (retryButton != null) retryButton.onClick.RemoveListener(Retry);
        }
    }
}
