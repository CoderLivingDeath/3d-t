using UnityEditor;

namespace AFPC.Editor {

[CustomEditor(typeof(AFPCMovingPlatformSupport))]
public class AFPCMovingPlatformSupportEditor : AFPCEditor {

    private static readonly UnityEngine.Color accent = new UnityEngine.Color(0.95f, 0.6f, 0.3f, 1f);

    private SerializedProperty isDebugLog;
    private SerializedProperty order;
    private SerializedProperty velocityInheritance;
    private SerializedProperty maxInheritedSpeed;
    private SerializedProperty onPlatformEnter;
    private SerializedProperty onPlatformExit;

    private void OnEnable () {
        isDebugLog          = serializedObject.FindProperty("isDebugLog");
        order               = serializedObject.FindProperty("order");
        velocityInheritance = serializedObject.FindProperty("velocityInheritance");
        maxInheritedSpeed   = serializedObject.FindProperty("maxInheritedSpeed");
        onPlatformEnter     = serializedObject.FindProperty("onPlatformEnter");
        onPlatformExit      = serializedObject.FindProperty("onPlatformExit");
    }

    public override void OnInspectorGUI () {
        serializedObject.Update();

        DrawHeader("MOVING PLATFORM", accent);
        BeginPanel();

        EditorGUILayout.PropertyField(isDebugLog);
        EditorGUILayout.PropertyField(order);

        EditorGUILayout.Space(4);

        if (DrawToggleSection("Velocity Inheritance", velocityInheritance)) {
            EditorGUILayout.PropertyField(velocityInheritance);
            EditorGUILayout.PropertyField(maxInheritedSpeed);
            EndSubPanel();
        }

        if (DrawToggleSectionFlat("Events", onPlatformEnter)) {
            EditorGUILayout.PropertyField(onPlatformEnter);
            EditorGUILayout.PropertyField(onPlatformExit);
        }

        if (isDebugLog.boolValue) DrawDebugSection(target);

        EndPanel();

        serializedObject.ApplyModifiedProperties();
    }
}
}
