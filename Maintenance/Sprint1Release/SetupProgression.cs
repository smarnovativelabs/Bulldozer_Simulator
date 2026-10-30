using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using Bulldozer.Player;
public static class SetupProgression {
static GameObject Box(Transform parent,string name,Vector3 pos,Vector3 scale,Material mat){
var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=name;g.transform.SetParent(parent);g.transform.position=pos;g.transform.localScale=scale;g.GetComponent<Renderer>().sharedMaterial=mat;g.isStatic=true;return g;
}
static TMP_Text Label(TMP_Text source,Transform parent,string name,float y,float height){
var t=UnityEngine.Object.Instantiate(source,parent);t.name=name;t.rectTransform.anchorMin=t.rectTransform.anchorMax=new Vector2(0,1);t.rectTransform.pivot=new Vector2(0,1);t.rectTransform.anchoredPosition=new Vector2(16,y);t.rectTransform.sizeDelta=new Vector2(288,height);t.fontSize=14;t.enableAutoSizing=false;t.textWrappingMode=TextWrappingModes.Normal;t.raycastTarget=false;return t;
}
public static string Main(){
if(EditorApplication.isPlaying)throw new Exception("Edit mode required");
if(GameObject.Find("Copper Ridge Expansion"))throw new Exception("Already installed");
var equipment=UnityEngine.Object.FindFirstObjectByType<MinerEquipment>();
var source=UnityEngine.Object.FindObjectsByType<MineableRock>(FindObjectsSortMode.None).First(r=>r.resourceType==1&&!string.IsNullOrEmpty(r.blockId)&&r.maxHealth==5);
var root=new GameObject("Copper Ridge Expansion");Undo.RegisterCreatedObjectUndo(root,"Add Sprint 1 progression");
var expansion=root.AddComponent<QuarryExpansion>();expansion.equipment=equipment;
var mat=source.rockRenderer.sharedMaterial;
// Cheap shared-material barriers; full-height colliders prevent jumping over the locked area.
Box(root.transform,"West Ridge Boundary",new Vector3(17.5f,2,6),new Vector3(.35f,4,9),mat);
Box(root.transform,"East Ridge Boundary",new Vector3(23.5f,2,6),new Vector3(.35f,4,9),mat);
Box(root.transform,"North Ridge Boundary",new Vector3(20.5f,2,10.5f),new Vector3(6.35f,4,.35f),mat);
Box(root.transform,"Entrance Left",new Vector3(18.1f,2,1.5f),new Vector3(1.55f,4,.35f),mat);
Box(root.transform,"Entrance Right",new Vector3(22.9f,2,1.5f),new Vector3(1.55f,4,.35f),mat);
expansion.gate=Box(root.transform,"Locked Copper Ridge Gate",new Vector3(20.5f,2,1.5f),new Vector3(3.3f,4,.35f),mat);expansion.gate.isStatic=false;
expansion.deposits=new MineableRock[6];
for(int i=0;i<6;i++){
 var rock=UnityEngine.Object.Instantiate(source,root.transform);rock.name="Copper Ridge Deposit "+(i+1);rock.blockId="copper-ridge-"+(i+1).ToString("00");rock.requiresCopperRidge=true;rock.transform.position=new Vector3(19.2f+(i%2)*2.5f,0,4+(i/2)*2.2f);expansion.deposits[i]=rock;
}
var signGo=new GameObject("Copper Ridge Placeholder Sign");signGo.transform.SetParent(root.transform);signGo.transform.position=new Vector3(20.5f,3.5f,1.25f);var sign=signGo.AddComponent<TextMeshPro>();sign.fontSize=3;sign.alignment=TextAlignmentOptions.Center;sign.rectTransform.sizeDelta=new Vector2(6,1.3f);sign.text="COPPER RIDGE\nFINISH 3 EXCAVATIONS TO OPEN";expansion.sign=sign;
var hud=UnityEngine.Object.FindFirstObjectByType<QuarryOptionalTasks>();Undo.RecordObject(hud,"Wire progression HUD");
hud.contractLabel=Label(hud.rows[0],hud.card,"Special Contract",-157,45);hud.milestoneLabel=Label(hud.rows[0],hud.card,"Excavation Milestone",-205,45);hud.milestoneLabel.color=new Color(1,.8f,.4f);hud.card.sizeDelta=new Vector2(320,254);
var rounds=UnityEngine.Object.FindFirstObjectByType<ExcavationRounds>();
((RectTransform)rounds.summary.transform.parent).sizeDelta=new Vector2(650,480);
rounds.heading.rectTransform.anchoredPosition=new Vector2(0,185);rounds.heading.rectTransform.sizeDelta=new Vector2(610,60);
rounds.summary.rectTransform.sizeDelta=new Vector2(590,290);rounds.summary.rectTransform.anchoredPosition=new Vector2(0,12);rounds.summary.fontSize=19;
((RectTransform)rounds.nextButton.transform).anchoredPosition=new Vector2(0,-190);
EditorSceneManager.MarkSceneDirty(root.scene);EditorSceneManager.SaveScene(root.scene);
return "Installed Copper Ridge (6 shared-mesh deposits, 6 box barriers), contract HUD and next preview";
}
}
