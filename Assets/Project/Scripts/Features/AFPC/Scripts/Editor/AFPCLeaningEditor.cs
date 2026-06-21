using UnityEditor;

namespace AFPC.Editor {

[CustomEditor(typeof(AFPCLeaning))]
public class AFPCLeaningEditor : AFPCEditor {

    private static readonly UnityEngine.Color accent = new UnityEngine.Color(0.95f, 0.65f, 0.3f, 1f);

    private SerializedProperty isDebugLog;
    private SerializedProperty order;
    private SerializedProperty leanLeftAction;
    private SerializedProperty leanRightAction;
    private SerializedProperty leanAngle;
    private SerializedProperty leanDistance;
    private SerializedProperty leanSpeed;
    private SerializedProperty onLeanStart;
    private SerializedProperty onLeanStop;

    private void OnEnable () {
        isDebugLog      = serializedObject.FindProperty("isDebugLog");
        order           = serializedObject.FindProperty("order");
        leanLeftAction  = serializedObject.FindProperty("leanLeftAction");
        leanRightAction = serializedObject.FindProperty("leanRightAction");
        leanAngle       = serializedObject.FindProperty("leanAngle");
        leanDistance    = serializedObject.FindProperty("leanDistance");
        leanSpeed       = serializedObject.FindProperty("leanSpeed");
        onLeanStart     = serializedObject.FindProperty("onLeanStart");
        onLeanStop      = serializedObject.FindProperty("onLeanStop");
    }

    public override void OnInspectorGUI () {
        serializedObject.Update();

        DrawHeader("LEANING", accent);
        BeginPanel();

        EditorGUILayout.PropertyField(isDebugLog);
        EditorGUILayout.PropertyField(order);
        EditorGUILayout.PropertyField(leanLeftAction);
        EditorGUILayout.PropertyField(leanRightAction);

        EditorGUILayout.Space(4);

        if (DrawToggleSection("Camera", leanAngle)) {
            EditorGUILayout.PropertyField(leanAngle);
            EditorGUILayout.PropertyField(leanDistance);
            EditorGUILayout.PropertyField(leanSpeed);
            EndSubPanel();
        }

        if (DrawToggleSectionFlat("Events", onLeanStart)) {
            EditorGUILayout.PropertyField(onLeanStart);
            EditorGUILayout.PropertyField(onLeanStop);
        }

        if (isDebugLog.boolValue) DrawDebugSection(target);

        EndPanel();

        serializedObject.ApplyModifiedProperties();
    }
}
}
