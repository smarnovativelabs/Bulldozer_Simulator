var c=UnityEngine.Object.FindFirstObjectByType<Bulldozer.Player.QuarryWheelbarrow>();var labels=UnityEngine.Object.FindObjectsByType<TMPro.TMP_Text>(FindObjectsSortMode.None);var enabled=new bool[labels.Length];for(int i=0;i<labels.Length;i++){enabled[i]=labels[i].enabled;labels[i].enabled=false;}
string output;
try{output=Bulldozer.Player.Editor.HandMiningPolish.Capture("equipment-polish-final",true);}finally{for(int i=0;i<labels.Length;i++)labels[i].enabled=enabled[i];}
Time.captureDeltaTime=0;EditorApplication.isPaused=false;EditorApplication.isPlaying=false;return output;
