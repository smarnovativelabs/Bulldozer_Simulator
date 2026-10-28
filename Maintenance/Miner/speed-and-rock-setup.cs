if(EditorApplication.isPlaying)throw new System.Exception("Edit mode required");
var u=UnityEngine.Object.FindFirstObjectByType<Bulldozer.Player.MinerUpgradeArea>();Undo.RegisterFullObjectHierarchyUndo(u.panel,"Add movement speed upgrade");
if(!u.speedButton){u.speedButton=UnityEngine.Object.Instantiate(u.capacityButton,u.capacityButton.transform.parent);u.speedButton.name="SpeedUpgrade";u.speedButton.onClick=new UnityEngine.UI.Button.ButtonClickedEvent();u.speedText=u.speedButton.GetComponentInChildren<TMPro.TMP_Text>();}
u.panel.GetComponent<RectTransform>().sizeDelta=new Vector2(480,310);
u.panel.transform.Find("Heading").GetComponent<RectTransform>().anchoredPosition=new Vector2(0,120);
u.damageButton.GetComponent<RectTransform>().anchoredPosition=new Vector2(-115,60);u.capacityButton.GetComponent<RectTransform>().anchoredPosition=new Vector2(115,60);
u.speedButton.GetComponent<RectTransform>().anchoredPosition=new Vector2(0,-30);u.speedButton.GetComponent<RectTransform>().sizeDelta=new Vector2(440,70);u.speedText.rectTransform.sizeDelta=new Vector2(425,66);u.speedText.text="SPEED 80% > 88%\n$25";
u.feedback.rectTransform.anchoredPosition=new Vector2(0,-115);EditorUtility.SetDirty(u);
int easy=0,medium=0,hard=0;
foreach(var r in UnityEngine.Object.FindObjectsByType<Bulldozer.Player.MineableRock>(FindObjectsSortMode.None)){
var pos=r.transform.position;string tier;
if(pos.z<4.1f||Mathf.Abs(pos.x)>7.4f){r.maxHealth=2+r.resourceType%2;tier="Weathered";easy++;}
else if(Mathf.Abs(pos.x)<2.5f){r.maxHealth=8+r.resourceType%5;tier="Dense";hard++;}
else{r.maxHealth=4+r.resourceType%3;tier="Firm";medium++;}
r.name=tier+" "+Bulldozer.Player.MiningResources.Names[r.resourceType]+" ("+r.maxHealth+" HP)";EditorUtility.SetDirty(r);
}
UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(u.gameObject.scene);UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();AssetDatabase.SaveAssets();return "Saved speed UI; rocks easy="+easy+", medium="+medium+", dense="+hard;
