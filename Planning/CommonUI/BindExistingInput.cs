// Use persistent references to the project's existing actions, including touch bindings.
const string path = "Assets/0_ProjectData/UI/Common/CommonUI.prefab";
const string actionsPath = "Assets/InputSystem_Actions.inputactions";
if (UnityEditor.EditorApplication.isPlaying) throw new System.Exception("Exit Play Mode first.");
var actions = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.InputSystem.InputActionAsset>(actionsPath);
var references = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(actionsPath).OfType<UnityEngine.InputSystem.InputActionReference>().ToArray();
UnityEngine.InputSystem.InputActionReference Action(string name) => references.First(r => r.action.actionMap.name == "UI" && r.action.name == name);
var root = UnityEditor.PrefabUtility.LoadPrefabContents(path);
try {
    var module = root.GetComponentInChildren<UnityEngine.InputSystem.UI.InputSystemUIInputModule>(true);
    module.actionsAsset = actions;
    module.point = Action("Point"); module.leftClick = Action("Click");
    module.rightClick = Action("RightClick"); module.middleClick = Action("MiddleClick");
    module.scrollWheel = Action("ScrollWheel"); module.move = Action("Navigate");
    module.submit = Action("Submit"); module.cancel = Action("Cancel");
    module.trackedDevicePosition = Action("TrackedDevicePosition");
    module.trackedDeviceOrientation = Action("TrackedDeviceOrientation");
    UnityEditor.PrefabUtility.SaveAsPrefabAsset(root, path);
} finally { UnityEditor.PrefabUtility.UnloadPrefabContents(root); }
var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
if (scene.path != "Assets/0_ProjectData/0_Scenes/0_MainMenu.unity" || scene.isDirty)
    throw new System.Exception("Expected saved MainMenu scene.");
var sceneInput = UnityEngine.Object.FindFirstObjectByType<Bulldozer.UI.CommonUI>().GetComponentInChildren<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
UnityEditor.PrefabUtility.RevertObjectOverride(sceneInput, UnityEditor.InteractionMode.AutomatedAction);
UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
return "CommonUI uses the existing InputSystem_Actions asset; transient scene action copies removed.";
