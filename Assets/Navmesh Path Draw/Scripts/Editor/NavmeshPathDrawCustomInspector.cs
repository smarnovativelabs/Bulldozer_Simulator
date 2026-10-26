using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(NavmeshPathDraw))]
public class NavmeshPathDrawCustomInspector : Editor
{
    SerializedProperty destination;
    SerializedProperty recalculatePath;
    SerializedProperty recalculationTime;
    SerializedProperty movementThreshold;
    SerializedProperty groundLayers;
    SerializedProperty groundRayDistance;
    SerializedProperty pathHeightOffset;
    SerializedProperty destinationOffset;
    SerializedProperty agentTypeIndex;
    SerializedProperty drawDirectFallback;

    void OnEnable()
    {
        destination = serializedObject.FindProperty("destination");
        recalculatePath = serializedObject.FindProperty("recalculatePath");
        recalculationTime = serializedObject.FindProperty("recalculationTime");
        movementThreshold = serializedObject.FindProperty("movementThreshold");
        groundLayers = serializedObject.FindProperty("groundLayers");
        groundRayDistance = serializedObject.FindProperty("groundRayDistance");
        pathHeightOffset = serializedObject.FindProperty("pathHeightOffset");
        destinationOffset = serializedObject.FindProperty("destinationOffset");
        agentTypeIndex = serializedObject.FindProperty("agentTypeIndex");
        drawDirectFallback = serializedObject.FindProperty("drawDirectFallback");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        var button = GUILayout.Button("Click for more tools");
        if (button) Application.OpenURL("https://assetstore.unity.com/publishers/39163");
        EditorGUILayout.Space(5);

        NavmeshPathDraw script = (NavmeshPathDraw) target;

        EditorGUILayout.PropertyField(destination, new GUIContent("Destination", "Transform position of the end destination"));
        EditorGUILayout.PropertyField(recalculatePath, new GUIContent("Recalculate Path", "If set to true, the pathfinding will be recalculated every set amount of time"));
        
        EditorGUI.BeginDisabledGroup(script.recalculatePath == false);
        EditorGUILayout.PropertyField(recalculationTime, new GUIContent("Recalculation Time", "The amount of time in seconds to recalculate the path. Values from 0.1 to 0.5 seconds are usually appropriate."));
        EditorGUILayout.PropertyField(movementThreshold);
        EditorGUI.EndDisabledGroup();

        EditorGUILayout.PropertyField(groundLayers);
        EditorGUILayout.PropertyField(groundRayDistance);
        EditorGUILayout.PropertyField(pathHeightOffset);
        EditorGUILayout.PropertyField(destinationOffset);
        EditorGUILayout.PropertyField(agentTypeIndex);
        EditorGUILayout.PropertyField(drawDirectFallback);

        serializedObject.ApplyModifiedProperties();
    }
}
