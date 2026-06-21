using UnityEditor;

namespace AFPC.Editor {

[CustomEditor(typeof(AFPCInteraction))]
public class AFPCInteractionEditor : AFPCEditor {

    private static readonly UnityEngine.Color accent = new UnityEngine.Color(0.4f, 0.75f, 1f, 1f);

    private SerializedProperty isDebugLog;
    private SerializedProperty order;
    private SerializedProperty interactAction;
    private SerializedProperty interactDistance;
    private SerializedProperty interactMask;
    private SerializedProperty onFocus;
    private SerializedProperty onUnfocus;
    private SerializedProperty onInteract;
    private SerializedProperty onInteractFailed;

    private void OnEnable () {
        isDebugLog       = serializedObject.FindProperty("isDebugLog");
        order            = serializedObject.FindProperty("order");
        interactAction   = serializedObject.FindProperty("interactAction");
        interactDistance = serializedObject.FindProperty("interactDistance");
        interactMask     = serializedObject.FindProperty("interactMask");
        onFocus          = serializedObject.FindProperty("onFocus");
        onUnfocus        = serializedObject.FindProperty("onUnfocus");
        onInteract       = serializedObject.FindProperty("onInteract");
        onInteractFailed = serializedObject.FindProperty("onInteractFailed");
    }

    public override void OnInspectorGUI () {
        serializedObject.Update();

        DrawHeader("INTERACTION", accent);
        BeginPanel();

        EditorGUILayout.PropertyField(isDebugLog);
        EditorGUILayout.PropertyField(order);
        EditorGUILayout.PropertyField(interactAction);

        EditorGUILayout.Space(4);

        if (DrawToggleSection("Detection", interactDistance)) {
            EditorGUILayout.PropertyField(interactDistance);
            EditorGUILayout.PropertyField(interactMask);
            EndSubPanel();
        }

        if (DrawToggleSectionFlat("Events", onFocus)) {
            EditorGUILayout.PropertyField(onFocus);
            EditorGUILayout.PropertyField(onUnfocus);
            EditorGUILayout.PropertyField(onInteract);
            EditorGUILayout.PropertyField(onInteractFailed);
        }

        if (isDebugLog.boolValue) DrawDebugSection(target);

        EndPanel();

        serializedObject.ApplyModifiedProperties();
    }
}
}
