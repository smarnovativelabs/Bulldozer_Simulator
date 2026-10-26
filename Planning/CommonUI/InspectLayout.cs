// Evaluate after selecting a fixed Game View size. Inspect on a player frame,
// rather than the Editor command's GUI context (which reports the panel size).
System.Collections.IEnumerator Inspect() {
yield return null;
var panel = Bulldozer.UI.CommonUI.Instance.GetComponentInChildren<Bulldozer.UI.LoadingPanel>(true);
var safe = panel.GetComponentInChildren<Crystal.SafeArea>().GetComponent<UnityEngine.RectTransform>();
UnityEngine.Canvas.ForceUpdateCanvases();
var labels = panel.GetComponentsInChildren<TMPro.TMP_Text>().Select(label => {
    label.ForceMeshUpdate();
    var corners = new UnityEngine.Vector3[4];
    label.rectTransform.GetWorldCorners(corners);
    return new { label.name, overflow = label.isTextOverflowing,
        insideSafeArea = corners.All(corner => safe.rect.Contains((UnityEngine.Vector2)safe.InverseTransformPoint(corner))) };
}).ToArray();
var result = new { UnityEngine.Screen.width, UnityEngine.Screen.height,
    frame = UnityEngine.Time.frameCount, simulatedDevice = Crystal.SafeArea.Sim.ToString(),
    safeMin = safe.anchorMin.ToString("F4"), safeMax = safe.anchorMax.ToString("F4"),
    progress = panel.DisplayedProgress, labels,
    passed = labels.All(label => !label.overflow && label.insideSafeArea) };
System.IO.File.AppendAllText("Planning/CommonUI/layout-validation.jsonl", Newtonsoft.Json.JsonConvert.SerializeObject(result) + "\n");
}
Bulldozer.UI.CommonUI.Instance.StartCoroutine(Inspect());
return "Layout inspection scheduled for the next player frame.";
