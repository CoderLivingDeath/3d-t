using UnityEngine;
using UnityEditor;
using System.Reflection;

namespace AFPC.Editor {

/// <summary>
/// Base class for all AFPC custom editors. Provides shared styles and layout helpers.
/// </summary>
public class AFPCEditor : UnityEditor.Editor {

    protected static readonly Color headerColor  = new Color(0.18f, 0.18f, 0.18f, 1f);
    protected static readonly Color toggleOffColor = new Color(0.22f, 0.22f, 0.22f, 1f);

    private static GUIStyle _panelStyle;
    protected static GUIStyle PanelStyle {
        get {
            if (_panelStyle == null) {
                _panelStyle = new GUIStyle(EditorStyles.helpBox);
                _panelStyle.padding = new RectOffset(8, 4, 4, 4);
                _panelStyle.margin  = new RectOffset(0, 0, 0, 0);
            }
            return _panelStyle;
        }
    }

    private static GUIStyle _subPanelStyle;
    protected static GUIStyle SubPanelStyle {
        get {
            if (_subPanelStyle == null || _subPanelStyle.normal.background == null) {
                _subPanelStyle = new GUIStyle();
                _subPanelStyle.padding = new RectOffset(8, 8, 4, 8);
                _subPanelStyle.margin  = new RectOffset(0, 0, 0, 0);
                Texture2D bg = new Texture2D(1, 1);
                bg.hideFlags = HideFlags.HideAndDontSave;
                bg.SetPixel(0, 0, new Color(0.15f, 0.15f, 0.15f, 1f));
                bg.Apply();
                _subPanelStyle.normal.background = bg;
            }
            return _subPanelStyle;
        }
    }

    // ─── Info ───

    private void DrawInfoSection () {
        var info = serializedObject.FindProperty("info");
        if (info == null) return;

        if (DrawToggleSection("Info", info)) {
            EditorGUILayout.PropertyField(info.FindPropertyRelative("ID"));
            EditorGUILayout.PropertyField(info.FindPropertyRelative("Icon"));

            var desc = info.FindPropertyRelative("Description");
            var style = new GUIStyle(EditorStyles.textArea) { wordWrap = true };
            if (string.IsNullOrEmpty(desc.stringValue)) {
                var placeholder = new GUIStyle(style) { fontStyle = FontStyle.Italic };
                placeholder.normal.textColor = new Color(0.5f, 0.5f, 0.5f);
                string result = EditorGUILayout.TextArea("Description", placeholder, GUILayout.MinHeight(40));
                if (result != "Description") desc.stringValue = result;
            } else {
                desc.stringValue = EditorGUILayout.TextArea(desc.stringValue, style, GUILayout.MinHeight(40));
            }

            DrawInfoList("String Data", info.FindPropertyRelative("StringData"));
            DrawInfoList("Int Data", info.FindPropertyRelative("IntData"));
            DrawInfoList("Float Data", info.FindPropertyRelative("FloatData"));
            EndSubPanel();
        }
    }

    private static GUIStyle _infoListStyle;
    private static GUIStyle InfoListStyle {
        get {
            if (_infoListStyle == null) {
                _infoListStyle = new GUIStyle(EditorStyles.helpBox);
                _infoListStyle.padding = new RectOffset(2, 2, 2, 2);
                _infoListStyle.margin  = new RectOffset(0, 0, 0, 0);
            }
            return _infoListStyle;
        }
    }

    private void DrawInfoList (string title, SerializedProperty list) {
        EditorGUILayout.Space(1);
        Rect rect = EditorGUILayout.GetControlRect(false, 24);
        EditorGUI.DrawRect(rect, list.isExpanded ? new Color(0.12f, 0.12f, 0.12f, 1f) : toggleOffColor);
        EditorGUI.LabelField(new Rect(rect.x + 4, rect.y, 16, rect.height), list.isExpanded ? "\u25BC" : "\u25B6", EditorStyles.miniLabel);
        EditorGUI.LabelField(new Rect(rect.x + 18, rect.y, rect.width - 50, rect.height), title, EditorStyles.miniLabel);
        EditorGUI.LabelField(new Rect(rect.xMax - 28, rect.y, 28, rect.height), list.arraySize.ToString(), new GUIStyle(EditorStyles.miniLabel) { alignment = TextAnchor.MiddleRight });
        if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition)) {
            list.isExpanded = !list.isExpanded;
            Event.current.Use();
        }
        if (!list.isExpanded) return;
        EditorGUILayout.BeginVertical(InfoListStyle);
        if (list.arraySize > 0) {
            Rect headerRect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight);
            float half = (headerRect.width - 16) / 2f;
            EditorGUI.LabelField(new Rect(headerRect.x, headerRect.y, half, headerRect.height), "Key", EditorStyles.miniLabel);
            EditorGUI.LabelField(new Rect(headerRect.x + half, headerRect.y, half, headerRect.height), "Value", EditorStyles.miniLabel);
        }
        int removeIndex = -1;
        for (int i = 0; i < list.arraySize; i++) {
            var element = list.GetArrayElementAtIndex(i);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(element.FindPropertyRelative("Key"), new GUIContent("", "Key identifier for this entry"));
            EditorGUILayout.PropertyField(element.FindPropertyRelative("Value"), new GUIContent("", "Value of this entry"));
            if (GUILayout.Button("\u00D7", GUILayout.Width(16), GUILayout.Height(16))) removeIndex = i;
            EditorGUILayout.EndHorizontal();
        }
        if (removeIndex >= 0) list.DeleteArrayElementAtIndex(removeIndex);
        if (GUILayout.Button("+", GUILayout.Height(24))) {
            list.InsertArrayElementAtIndex(list.arraySize);
        }
        EditorGUILayout.EndVertical();
    }

    // ─── Helpers ───

    protected bool DrawHeader (string title, SerializedProperty prop, Color accent) {
        Rect rect = EditorGUILayout.GetControlRect(false, 32);
        EditorGUI.DrawRect(rect, headerColor);
        EditorGUI.DrawRect(new Rect(rect.x, rect.y, 4, rect.height), accent);
        EditorGUI.LabelField(new Rect(rect.x + 8, rect.y + 8, 16, 16), prop.isExpanded ? "\u25BC" : "\u25B6", EditorStyles.miniLabel);
        GUIStyle style = new GUIStyle(EditorStyles.boldLabel) { fontSize = 16, alignment = TextAnchor.MiddleLeft };
        style.normal.textColor = accent;
        EditorGUI.LabelField(new Rect(rect.x + 32, rect.y, rect.width - 32, rect.height), title, style);
        if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition)) {
            prop.isExpanded = !prop.isExpanded;
            Event.current.Use();
        }
        return prop.isExpanded;
    }

    protected void DrawHeader (string title, Color accent) {
        Rect rect = EditorGUILayout.GetControlRect(false, 32);
        EditorGUI.DrawRect(rect, headerColor);
        EditorGUI.DrawRect(new Rect(rect.x, rect.y, 4, rect.height), accent);
        GUIStyle style = new GUIStyle(EditorStyles.boldLabel) { fontSize = 16, alignment = TextAnchor.MiddleLeft };
        style.normal.textColor = accent;
        EditorGUI.LabelField(new Rect(rect.x + 12, rect.y, rect.width - 12, rect.height), title, style);
    }

    protected bool DrawToggleSection (string title, SerializedProperty prop) {
        EditorGUILayout.Space(2);
        Rect rect = EditorGUILayout.GetControlRect(false, 32);
        EditorGUI.DrawRect(rect, prop.isExpanded ? new Color(0.12f, 0.12f, 0.12f, 1f) : toggleOffColor);
        EditorGUI.LabelField(new Rect(rect.x + 8, rect.y, rect.width - 8, rect.height), title, EditorStyles.boldLabel);
        if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition)) {
            prop.isExpanded = !prop.isExpanded;
            Event.current.Use();
        }
        if (prop.isExpanded) { EditorGUILayout.Space(-2); BeginSubPanel(); }
        return prop.isExpanded;
    }

    protected bool DrawToggleSectionFlat (string title, SerializedProperty prop) {
        EditorGUILayout.Space(2);
        Rect rect = EditorGUILayout.GetControlRect(false, 32);
        EditorGUI.DrawRect(rect, prop.isExpanded ? new Color(0.12f, 0.12f, 0.12f, 1f) : toggleOffColor);
        EditorGUI.LabelField(new Rect(rect.x + 8, rect.y, rect.width - 8, rect.height), title, EditorStyles.boldLabel);
        if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition)) {
            prop.isExpanded = !prop.isExpanded;
            Event.current.Use();
        }
        return prop.isExpanded;
    }

    protected bool DrawToggleSectionWithProp (SerializedProperty parent, string title, string togglePropName) {
        var toggleProp = parent.FindPropertyRelative(togglePropName);
        EditorGUILayout.Space(2);
        Rect rect = EditorGUILayout.GetControlRect(false, 32);
        EditorGUI.DrawRect(rect, toggleProp.isExpanded ? new Color(0.12f, 0.12f, 0.12f, 1f) : toggleOffColor);
        toggleProp.boolValue = EditorGUI.Toggle(new Rect(rect.x + 4, rect.y + 8, 16, 16), toggleProp.boolValue);
        EditorGUI.LabelField(new Rect(rect.x + 32, rect.y, rect.width - 32, rect.height), title, EditorStyles.boldLabel);
        if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition)) {
            toggleProp.isExpanded = !toggleProp.isExpanded;
            Event.current.Use();
        }
        if (toggleProp.isExpanded) { EditorGUILayout.Space(-2); BeginSubPanel(); }
        return toggleProp.isExpanded;
    }

    protected void BeginPanel    () {
        EditorGUILayout.BeginVertical(PanelStyle);
        DrawInfoSection();
    }
    protected void EndPanel      () => EditorGUILayout.EndVertical();
    protected void BeginSubPanel () => EditorGUILayout.BeginVertical(SubPanelStyle);
    protected void EndSubPanel   () => EditorGUILayout.EndVertical();

    // ─── Debug Section ───

    protected void DrawDebugSection (object obj) {
        EditorGUILayout.Space(2);
        Rect rect = EditorGUILayout.GetControlRect(false, 22);
        EditorGUI.DrawRect(rect, new Color(0.08f, 0.08f, 0.08f, 1f));
        var headerStyle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 11, normal = { textColor = new Color(0.4f, 1f, 0.4f) } };
        EditorGUI.LabelField(new Rect(rect.x + 8, rect.y, rect.width, rect.height), "DEBUG", headerStyle);
        BeginSubPanel();
        if (!Application.isPlaying) {
            EditorGUILayout.LabelField("Values shown in Play Mode.", EditorStyles.centeredGreyMiniLabel);
            EndSubPanel();
            return;
        }
        var nameStylePublic  = new GUIStyle(EditorStyles.miniLabel) { normal = { textColor = new Color(0.85f, 0.85f, 0.85f) } };
        var nameStylePrivate = new GUIStyle(EditorStyles.miniLabel) { normal = { textColor = new Color(0.45f, 0.45f, 0.45f) } };
        var valueStyle       = new GUIStyle(EditorStyles.miniLabel) { normal = { textColor = Color.white }, alignment = TextAnchor.MiddleRight };
        foreach (var f in obj.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)) {
            if (ShouldSkipDebugField(f)) continue;
            Rect row  = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight);
            float half = row.width / 2f;
            EditorGUI.LabelField(new Rect(row.x, row.y, half, row.height), (f.IsPublic ? "" : "~") + f.Name, f.IsPublic ? nameStylePublic : nameStylePrivate);
            EditorGUI.LabelField(new Rect(row.x + half, row.y, half, row.height), FormatDebugValue(f.GetValue(obj)), valueStyle);
        }
        EndSubPanel();
        Repaint();
    }

    private static bool ShouldSkipDebugField (FieldInfo f) {
        if (f.Name.StartsWith("m_") || f.Name.StartsWith("k_")) return true;
        if (typeof(UnityEngine.Events.UnityEventBase).IsAssignableFrom(f.FieldType)) return true;
        if (f.FieldType.Namespace != null && f.FieldType.Namespace.StartsWith("UnityEngine.InputSystem")) return true;
        return false;
    }

    private static string FormatDebugValue (object value) {
        if (value == null) return "null";
        if (value is float  f)  return f.ToString("F2");
        if (value is bool   b)  return b.ToString();
        if (value is int    i)  return i.ToString();
        if (value is Vector2 v2) return $"({v2.x:F1}, {v2.y:F1})";
        if (value is Vector3 v3) return $"({v3.x:F1}, {v3.y:F1}, {v3.z:F1})";
        if (value is Quaternion q) { var e = q.eulerAngles; return $"({e.x:F0}, {e.y:F0}, {e.z:F0})"; }
        if (value is LayerMask lm) return lm.value.ToString();
        if (value is UnityEngine.Object o) return o ? o.name : "null";
        return value.ToString();
    }
}
}
