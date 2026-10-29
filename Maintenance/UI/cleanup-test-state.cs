if(EditorApplication.isPlaying)throw new System.Exception("Stop Play before cleanup");
var prefix=SessionState.GetString("Quarry.UI.TestPrefs","");
if(prefix.StartsWith("Quarry.UI.Test."))foreach(var key in new[]{"Sound","Volume","Vibration","Performance"})PlayerPrefs.DeleteKey(prefix+key);
PlayerPrefs.Save();SessionState.EraseString("Quarry.UI.TestPrefs");SessionState.EraseString("Bulldozer.Mining.TestSavePath");
PlayerSettings.runInBackground=SessionState.GetBool("UI.Background",PlayerSettings.runInBackground);
Application.targetFrameRate=SessionState.GetInt("UI.FrameRate",Application.targetFrameRate);
AudioListener.volume=SessionState.GetFloat("UI.Volume",AudioListener.volume);
Time.captureDeltaTime=0;Time.timeScale=1;EditorApplication.isPaused=false;Crystal.SafeArea.Sim=Crystal.SafeArea.SimDevice.None;
var flags=System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.Static;
var asm=typeof(Editor).Assembly;var st=asm.GetType("UnityEditor.GameViewSizes");var sizes=st.BaseType.GetProperty("instance",flags).GetValue(null);var group=st.GetProperty("currentGroup",flags).GetValue(sizes);var gt=group.GetType();var vt=asm.GetType("UnityEditor.GameView");var window=EditorWindow.GetWindow(vt);var selected=vt.GetProperty("selectedSizeIndex",flags);
int original=SessionState.GetInt("UI.GameViewCount",-1),removed=0;
if(original>=0){selected.SetValue(window,SessionState.GetInt("UI.GameViewIndex",0));int count=(int)gt.GetMethod("GetTotalCount").Invoke(group,null);while(count>original){var size=gt.GetMethod("GetGameViewSize").Invoke(group,new object[]{count-1});var label=(string)size.GetType().GetProperty("baseText",flags).GetValue(size);if(!label.StartsWith("Quarry UI check"))throw new System.Exception("Unexpected size; retained "+label);gt.GetMethod("RemoveCustomSize").Invoke(group,new object[]{count-1});int next=(int)gt.GetMethod("GetTotalCount").Invoke(group,null);if(next!=count-1)throw new System.Exception("Unexpected size removal result");count=next;removed++;}}
SessionState.EraseBool("UI.Background");SessionState.EraseInt("UI.FrameRate");SessionState.EraseFloat("UI.Volume");SessionState.EraseInt("UI.GameViewCount");SessionState.EraseInt("UI.GameViewIndex");AssetDatabase.SaveAssets();return "Test settings cleared; original Game View restored; temporary sizes removed: "+removed;
