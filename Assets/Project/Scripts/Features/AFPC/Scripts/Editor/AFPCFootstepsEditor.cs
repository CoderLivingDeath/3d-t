using UnityEditor;

namespace AFPC.Editor {

[CustomEditor(typeof(AFPCFootsteps))]
public class AFPCFootstepsEditor : AFPCEditor {

    private static readonly UnityEngine.Color accent = new UnityEngine.Color(0.55f, 0.85f, 0.4f, 1f);

    private SerializedProperty isDebugLog;
    private SerializedProperty order;
    private SerializedProperty walkStepDistance;
    private SerializedProperty runStepDistance;
    private SerializedProperty groundMask;
    private SerializedProperty rayDistance;
    private SerializedProperty onStep;

    private void OnEnable () {
        isDebugLog        = serializedObject.FindProperty("isDebugLog");
        order             = serializedObject.FindProperty("order");
        walkStepDistance  = serializedObject.FindProperty("walkStepDistance");
        runStepDistance   = serializedObject.FindProperty("runStepDistance");
        groundMask        = serializedObject.FindProperty("groundMask");
        rayDistance       = serializedObject.FindProperty("rayDistance");
        onStep            = serializedObject.FindProperty("onStep");
    }

    public override void OnInspectorGUI () {
        serializedObject.Update();

        DrawHeader("FOOTSTEPS", accent);
        BeginPanel();

        EditorGUILayout.PropertyField(isDebugLog);
        EditorGUILayout.PropertyField(order);

        EditorGUILayout.Space(4);

        if (DrawToggleSection("Steps", walkStepDistance)) {
            EditorGUILayout.PropertyField(walkStepDistance);
            EditorGUILayout.PropertyField(runStepDistance);
            EndSubPanel();
        }

        if (DrawToggleSection("Surface Detection", groundMask)) {
            EditorGUILayout.PropertyField(groundMask);
            EditorGUILayout.PropertyField(rayDistance);
            EndSubPanel();
        }

        if (DrawToggleSectionFlat("Events", onStep)) {
            EditorGUILayout.PropertyField(onStep);
        }

        if (isDebugLog.boolValue) DrawDebugSection(target);

        EndPanel();

        serializedObject.ApplyModifiedProperties();
    }
}
}
