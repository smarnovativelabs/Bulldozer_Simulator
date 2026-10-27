using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using TMPro;

namespace Bulldozer.Player.Editor
{
    public static class MinerSetup
    {
        const string Root = "Assets/0_ProjectData/Player/";
        static Material Mat(string name, Color color, Texture texture = null)
        {
            string path = Root + "Art/" + name + ".mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (!mat) { mat = new Material(Shader.Find("Universal Render Pipeline/Lit")); AssetDatabase.CreateAsset(mat, path); }
            mat.color = color; mat.mainTexture = texture; mat.SetFloat("_Smoothness", .18f); EditorUtility.SetDirty(mat); return mat;
        }
        [MenuItem("Tools/Bulldozer/Set Up Miner Player")]
        public static void Build()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play mode before setup.");
            var current = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (current.isDirty) EditorSceneManager.SaveScene(current);
            var scene = EditorSceneManager.OpenScene("Assets/0_ProjectData/0_Scenes/1_Gameplay.unity");
            if (GameObject.Find("MinerPlayer")) throw new InvalidOperationException("Miner already exists; edit the existing prefab instead.");
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
            PlayerSettings.allowedAutorotateToPortrait = false; PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true; PlayerSettings.allowedAutorotateToLandscapeRight = true;
            PlayerSettings.defaultScreenWidth = 1280; PlayerSettings.defaultScreenHeight = 720;
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(Root + "Art/Miner.fbx");
            if (!model) throw new InvalidOperationException("Miner FBX is missing.");
            var player = new GameObject("MinerPlayer"); player.transform.position = new Vector3(0, .1f, 0);
            var motor = player.AddComponent<CharacterController>(); motor.height = 1.85f; motor.radius = .3f; motor.center = Vector3.up * .925f; motor.stepOffset = .28f; motor.slopeLimit = 48; motor.skinWidth = .035f;
            var controller = player.AddComponent<MinerController>();
            var visual = (GameObject)PrefabUtility.InstantiatePrefab(model); visual.name = "MinerVisual"; visual.transform.SetParent(player.transform, false);
            controller.visual = visual.transform;
            var pose = visual.AddComponent<MinerPose>(); pose.controller = controller;
            var atlas = AssetDatabase.LoadAssetAtPath<Texture2D>(Root + "Art/MinerAtlas.png");
            foreach (var renderer in visual.GetComponentsInChildren<Renderer>())
            {
                renderer.sharedMaterials = renderer.sharedMaterials.Select(m =>
                {
                    if (!m) return Mat("MinerFallback", Color.gray);
                    string n = m.name;
                    return Mat(n, n.Contains("MinerAtlas") ? Color.white : m.color, n.Contains("MinerAtlas") ? atlas : null);
                }).ToArray();
            }
            var cam = Camera.main;
            if (!cam) { cam = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener)).GetComponent<Camera>(); cam.tag = "MainCamera"; }
            cam.fieldOfView = 58; cam.nearClipPlane = .08f; cam.farClipPlane = 180;
            var follow = cam.gameObject.AddComponent<MinerCamera>(); follow.target = controller; controller.followCamera = follow;
            cam.transform.SetPositionAndRotation(new Vector3(0,2.85f,-4.56f), Quaternion.Euler(18,0,0));
            if (!UnityEngine.Object.FindFirstObjectByType<EventSystem>()) new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            var canvasGO = new GameObject("MinerControls", typeof(RectTransform), typeof(Canvas), typeof(UnityEngine.UI.CanvasScaler), typeof(UnityEngine.UI.GraphicRaycaster));
            canvasGO.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGO.GetComponent<UnityEngine.UI.CanvasScaler>(); scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1280,720); scaler.matchWidthOrHeight = .5f;
            var controls = canvasGO.AddComponent<MinerTouchControls>(); controls.player = controller; controller.touchControls = controls;
            var safe = Rect("SafeArea", canvasGO.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero); controls.safeArea = safe;
            var look = Zone("LookArea", safe, new Vector2(.45f,0), Vector2.one, Vector2.zero, Vector2.zero, controls, MinerTouchZone.Action.Look, new Color(0,0,0,.001f));
            var move = Zone("Move", safe, Vector2.zero, Vector2.zero, new Vector2(150,145), new Vector2(180,180), controls, MinerTouchZone.Action.Move, new Color(.08f,.12f,.14f,.65f));
            var knob = Rect("Handle", move.transform, Vector2.one*.5f, Vector2.one*.5f, Vector2.zero, new Vector2(62,62)); knob.gameObject.AddComponent<UnityEngine.UI.Image>().color = new Color(.95f,.65f,.22f,.85f); knob.GetComponent<UnityEngine.UI.Image>().raycastTarget = false; move.knob = knob;
            Button("JUMP", new Vector2(-120,120), MinerTouchZone.Action.Jump, safe, controls);
            Button("SWING", new Vector2(-255,120), MinerTouchZone.Action.Mine, safe, controls);
            Button("RUN", new Vector2(-120,245), MinerTouchZone.Action.Sprint, safe, controls);
            Label("WASD / left stick: move   |   Shift: run   |   Space: jump   |   E: swing   |   RMB / right stick: look", safe, new Vector2(.5f,1), new Vector2(0,-28), new Vector2(1120,40), 18);
            // Small replaceable test yard: the gameplay scene had no level or colliders.
            var yard = new GameObject("PlayerTestYard");
            Box("Ground",new Vector3(0,-.25f,0),new Vector3(30,.5f,30),Mat("Ground",new Color(.31f,.29f,.24f)),yard.transform);
            Box("CameraCollisionWall",new Vector3(0,1,7),new Vector3(8,2,.5f),Mat("Concrete",new Color(.43f,.47f,.45f)),yard.transform);
            Box("Step",new Vector3(3,.12f,2),new Vector3(2,.24f,2),Mat("Concrete",new Color(.43f,.47f,.45f)),yard.transform);
            var ramp=Box("Slope",new Vector3(-4,.4f,2),new Vector3(2,.3f,4),Mat("Concrete",new Color(.43f,.47f,.45f)),yard.transform);ramp.transform.rotation=Quaternion.Euler(-15,0,0);
            PrefabUtility.SaveAsPrefabAsset(player, Root + "MinerPlayer.prefab");
            PrefabUtility.SaveAsPrefabAsset(canvasGO, Root + "MinerControls.prefab");
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets(); Selection.activeGameObject = player;
            Debug.Log("Miner setup complete: landscape, character, camera, controls and test yard.");
        }
        static RectTransform Rect(string name, Transform parent, Vector2 min, Vector2 max, Vector2 pos, Vector2 size)
        {
            var r = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(parent,false);r.anchorMin=min;r.anchorMax=max;r.anchoredPosition=pos;r.sizeDelta=size;return r;
        }
        static MinerTouchZone Zone(string name, Transform parent, Vector2 min, Vector2 max, Vector2 pos, Vector2 size, MinerTouchControls controls, MinerTouchZone.Action action, Color color)
        {
            var rect=Rect(name,parent,min,max,pos,size);rect.gameObject.AddComponent<UnityEngine.UI.Image>().color=color;var zone=rect.gameObject.AddComponent<MinerTouchZone>();zone.controls=controls;zone.action=action;return zone;
        }
        static void Button(string text, Vector2 pos, MinerTouchZone.Action action, Transform parent, MinerTouchControls controls)
        {
            var zone=Zone(text,parent,new Vector2(1,0),new Vector2(1,0),pos,new Vector2(110,90),controls,action,new Color(.1f,.15f,.18f,.8f));
            Label(text,zone.transform,Vector2.one*.5f,Vector2.zero,new Vector2(110,50),21);
        }
        static void Label(string text, Transform parent, Vector2 anchor, Vector2 pos, Vector2 size, int fontSize)
        {
            var rect=Rect(text,parent,anchor,anchor,pos,size);var label=rect.gameObject.AddComponent<TextMeshProUGUI>();label.text=text;label.fontSize=fontSize;label.alignment=TextAlignmentOptions.Center;label.color=Color.white;label.raycastTarget=false;
        }
        static GameObject Box(string name, Vector3 position, Vector3 scale, Material mat, Transform parent)
        {
            var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name=name;go.transform.SetParent(parent);go.transform.position=position;go.transform.localScale=scale;go.GetComponent<Renderer>().sharedMaterial=mat;return go;
        }
    }
}
