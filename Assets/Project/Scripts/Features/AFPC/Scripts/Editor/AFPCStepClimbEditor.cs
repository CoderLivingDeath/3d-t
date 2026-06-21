using UnityEditor;

namespace AFPC.Editor {

[CustomEditor(typeof(AFPCStepClimb))]
public class AFPCStepClimbEditor : AFPCEditor {

    private static readonly UnityEngine.Color accent = new UnityEngine.Color(0.8f, 0.55f, 0.95f, 1f);

    private SerializedProperty isDebugLog;
    private SerializedProperty order;
    private SerializedProperty maxStepHeight;
    private SerializedProperty stepUpForce;
    private SerializedProperty checkDistance;
    private SerializedProperty cooldown;
    private SerializedProperty stepMask;
    private SerializedProperty onStep;

    private void OnEnable () {
        isDebugLog    = serializedObject.FindProperty("isDebugLog");
        order         = serializedObject.FindProperty("order");
        maxStepHeight = serializedObject.FindProperty("maxStepHeight");
        stepUpForce   = serializedObject.FindProperty("stepUpForce");
        checkDistance = serializedObject.FindProperty("checkDistance");
        cooldown      = serializedObject.FindProperty("cooldown");
        stepMask      = serializedObject.FindProperty("stepMask");
        onStep        = serializedObject.FindProperty("onStep");
    }

    public override void OnInspectorGUI () {
        serializedObject.Update();

        DrawHeader("STEP CLIMB", accent);
        BeginPanel();

        EditorGUILayout.PropertyField(isDebugLog);
        EditorGUILayout.PropertyField(order);

        EditorGUILayout.Space(4);

        if (DrawToggleSection("Detection", maxStepHeight)) {
            EditorGUILayout.PropertyField(maxStepHeight);
            EditorGUILayout.PropertyField(stepUpForce);
            EditorGUILayout.PropertyField(checkDistance);
            EditorGUILayout.PropertyField(cooldown);
            EditorGUILayout.PropertyField(stepMask);
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
