using System;
using System.IO;
using System.Linq;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Bulldozer.Player;
public static class CartSaveResumeChecks {
public static string Main(){
var go=new GameObject("Cart save resume test");UnityEngine.Object.DontDestroyOnLoad(go);go.AddComponent<CartSaveResumeRunner>().StartCoroutine("Run");return "Waiting for full excavation test, then checking cart saves";
}}
public class CartSaveResumeRunner:MonoBehaviour {
void Check(bool ok,string name){File.AppendAllText("Maintenance/Sprint1Release/cart-save-checks.txt",(ok?"PASS ":"FAIL ")+name+"\n");if(!ok)throw new Exception(name);}
IEnumerator Run(){
while(!File.ReadAllText("Maintenance/Sprint1Release/gameplay-checks.txt").Contains("COMPLETE 3 excavations"))yield return null;
yield return new WaitForSeconds(.5f);
File.WriteAllText("Maintenance/Sprint1Release/cart-save-checks.txt","START\n");
var e=UnityEngine.Object.FindFirstObjectByType<MinerEquipment>();
var s=e.Snapshot;s.wheelbarrowUnlocked=true;s.usingWheelbarrow=true;s.items=new[]{0,6,11};s.backpackItems=new[]{2,8};s.barrowItems=Array.Empty<int>();s.barrowPosition=new Vector3(-6.8f,.05f,-2.2f);
Check(e.Commit(s),"Save active cart plus separately stored backpack");
var cash=e.Cash;int round=e.RoundIndex;
for(int i=0;i<2;i++){
int old=e.GetInstanceID();SceneManager.LoadSceneAsync("Assets/0_ProjectData/0_Scenes/1_Gameplay.unity");
float until=Time.realtimeSinceStartup+40;
do{if(Time.realtimeSinceStartup>until)throw new Exception("Cart reload timeout");yield return null;}
while(!UnityEngine.Object.FindFirstObjectByType<MinerEquipment>()||UnityEngine.Object.FindFirstObjectByType<MinerEquipment>().GetInstanceID()==old);
yield return null;yield return null;
e=UnityEngine.Object.FindFirstObjectByType<MinerEquipment>();var p=UnityEngine.Object.FindFirstObjectByType<MinerController>();p.autoMine=false;
s=e.Snapshot;
Check(!s.usingWheelbarrow&&s.wheelbarrowUnlocked,"Reload "+i+" parks unlocked cart safely");
Check(s.items.SequenceEqual(new[]{2,8})&&s.backpackItems.Length==0&&s.barrowItems.SequenceEqual(new[]{0,6,11}),"Reload "+i+" keeps both inventories exactly once");
Check(s.coins==cash&&s.roundIndex==round,"Reload "+i+" preserves cash and excavation");
}
File.AppendAllText("Maintenance/Sprint1Release/cart-save-checks.txt","COMPLETE\n");
Destroy(gameObject);
}}
