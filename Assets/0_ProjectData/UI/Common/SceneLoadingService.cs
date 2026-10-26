using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Bulldozer.UI
{
    /// <summary>One asynchronous scene transition at a time, owned by the persistent CommonUI.</summary>
    public sealed class SceneLoadingService : MonoBehaviour
    {
        [SerializeField] private LoadingPanel panel;
        [SerializeField, Min(0f)] private float minimumDisplaySeconds = .8f;
        [SerializeField, Min(10f)] private float slowLoadNoticeSeconds = 30f;
        private AsyncOperation operation;
        private Coroutine routine;
        private string lastTarget;
        public bool IsLoading { get; private set; }
        public string LastError { get; private set; }
        public float Progress { get; private set; }

        private void Awake() => panel.RetryRequested += Retry;
        private void Retry() { if (!IsLoading) TryLoadScene(lastTarget); }

        public bool TryLoadScene(string scenePath)
        {
            if (!isActiveAndEnabled || IsLoading || (operation != null && !operation.isDone)) return false;
            lastTarget = scenePath;
            LastError = null;
            Progress = 0f;
            if (string.IsNullOrWhiteSpace(scenePath) || !Application.CanStreamedLevelBeLoaded(scenePath))
            {
                Fail("We couldn't open the worksite. Please try again.");
                Debug.LogError("[SceneLoading] Scene is missing from the active build scene list: " + scenePath, this);
                return false;
            }
            IsLoading = true; // Set before StartCoroutine so simultaneous requests cannot overlap.
            panel.Show();
            routine = StartCoroutine(Load(scenePath));
            return true;
        }

        private IEnumerator Load(string path)
        {
            float shownAt = Time.realtimeSinceStartup;
            // Let the opaque cover render before loading work starts.
            yield return null;
            yield return null;
            Exception failure = null;
            try { operation = SceneManager.LoadSceneAsync(path, LoadSceneMode.Single); }
            catch (Exception exception) { failure = exception; }
            if (failure != null || operation == null)
            {
                Fail("We couldn't open the worksite. Please try again.");
                Debug.LogError("[SceneLoading] " + (failure?.Message ?? "No load operation returned."), this);
                yield break;
            }
            operation.allowSceneActivation = false;
            try
            {
                while (operation.progress < .9f || Time.realtimeSinceStartup - shownAt < minimumDisplaySeconds)
                {
                    Progress = Mathf.Clamp01(operation.progress / .9f) * .9f;
                    panel.Report(Progress, Time.realtimeSinceStartup - shownAt > slowLoadNoticeSeconds
                        ? "Still preparing your worksite..." : "Loading your worksite");
                    yield return null;
                }
                // 90% means scene data is ready, not that activation has finished.
                panel.Report(.9f, "Opening the worksite");
                operation.allowSceneActivation = true;
                while (!operation.isDone) yield return null;
                // Keep the cover over Awake/OnEnable/Start and the first destination frames.
                yield return null;
                yield return null;
                Progress = 1f;
                yield return panel.Reveal();
            }
            finally
            {
                // Unity AsyncOperation cannot be cancelled. Never strand it at the 0.9 gate.
                if (operation != null) operation.allowSceneActivation = true;
                if (operation == null || operation.isDone) operation = null;
                IsLoading = false;
                routine = null;
            }
        }

        private void Fail(string message)
        {
            LastError = message;
            IsLoading = false;
            routine = null;
            panel.ShowError(message, operation == null || operation.isDone);
        }

        private void OnDisable()
        {
            if (routine != null) StopCoroutine(routine);
            routine = null;
            if (operation != null) operation.allowSceneActivation = true;
            IsLoading = false;
            if (panel != null) panel.HideImmediately();
        }

        private void OnDestroy() { if (panel != null) panel.RetryRequested -= Retry; }
    }
}
