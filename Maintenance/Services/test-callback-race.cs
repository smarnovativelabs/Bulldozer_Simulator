if(!EditorApplication.isPlaying||string.IsNullOrEmpty(SessionState.GetString("Bulldozer.Mining.TestSavePath","")))throw new System.Exception("Isolated Play session required");
var store=Bulldozer.Services.QuarryStore.Instance;if(store.Busy)throw new System.Exception("Wait for store fixture to finish");
var flags=System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance;var type=store.GetType();
var controller=(UnityEngine.Purchasing.StoreController)type.GetField("store",flags).GetValue(store);
var product=controller.GetProductById("test.quarry.cash1000");
var cart=new UnityEngine.Purchasing.Cart(new UnityEngine.Purchasing.CartItem(product));
var stale=new UnityEngine.Purchasing.ConfirmedOrder(cart,(UnityEngine.Purchasing.IOrderInfo)System.Activator.CreateInstance(typeof(UnityEngine.Purchasing.ConfirmedOrder).Assembly.GetType("UnityEngine.Purchasing.OrderInfo"),new object[]{"","previous-transaction","fake"}));
type.GetMethod("SetBusy",flags).Invoke(store,new object[]{"Simulated newer checkout"});
type.GetField("activeProduct",flags).SetValue(store,product.definition.id);type.GetField("activeTransaction",flags).SetValue(store,"current-transaction");
type.GetMethod("Confirmed",flags).Invoke(store,new object[]{stale});
if(!store.Busy)throw new System.Exception("Stale confirmation cleared newer purchase state");
var own=new UnityEngine.Purchasing.ConfirmedOrder(cart,(UnityEngine.Purchasing.IOrderInfo)System.Activator.CreateInstance(typeof(UnityEngine.Purchasing.ConfirmedOrder).Assembly.GetType("UnityEngine.Purchasing.OrderInfo"),new object[]{"","current-transaction","fake"}));
type.GetMethod("Confirmed",flags).Invoke(store,new object[]{own});
if(store.Busy)throw new System.Exception("Matching confirmation did not finish its purchase");
type.GetMethod("SetBusy",flags).Invoke(store,new object[]{"Simulated restore"});type.GetMethod("Confirmed",flags).Invoke(store,new object[]{stale});
if(!store.Busy)throw new System.Exception("Late confirmation interrupted restore");
type.GetMethod("Finish",flags).Invoke(store,new object[]{"Store ready"});
System.IO.File.WriteAllText("Maintenance/Services/callback-race-results.txt","PASS stale confirmation preserves newer checkout\nPASS matching confirmation completes its checkout\nPASS late confirmation preserves restore operation\n");return "3 callback-race checks passed";

