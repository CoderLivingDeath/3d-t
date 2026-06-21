using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AFPC.Editor {

    public class AFPCWizard : EditorWindow {

        [MenuItem("Window/AFPC/Wizard")]
        public static void Open () {
            GetWindow<AFPCWizard>("AFPC Wizard");
        }

        // ─── Data ────────────────────────────────────────────────────────────────────

        private enum Level { OK, Warning, Error, Info }

        private struct Entry {
            public Level  level;
            public string text;
            public Object ping;
        }

        private class Panel {
            public string title;
            public readonly List<Entry> entries = new List<Entry>();
        }

        private class HeroBlock {
            public string name;
            public bool   inactive;
            public Object ping;
            public readonly List<Panel> panels = new List<Panel>();
        }

        private readonly List<Entry>     _top    = new List<Entry>();
        private readonly List<HeroBlock> _blocks = new List<HeroBlock>();
        private HeroBlock _currentBlock;
        private Panel     _currentPanel;

        private Vector2 _scroll;

        // ─── Colors ──────────────────────────────────────────────────────────────────

        private static readonly Color colOK      = new Color(0.60f, 1.00f, 0.65f);
        private static readonly Color colWarning = new Color(1.00f, 0.90f, 0.35f);
        private static readonly Color colError   = new Color(1.00f, 0.50f, 0.50f);
        private static readonly Color colInfo    = new Color(0.88f, 0.88f, 0.88f);
        private static readonly Color colHero    = new Color(1.00f, 1.00f, 1.00f);
        private static readonly Color colPanel   = new Color(0.95f, 0.95f, 0.95f);
        private static readonly Color colAccent  = new Color(0.30f, 0.65f, 0.90f);
        private static readonly Color colHeader  = new Color(0.18f, 0.18f, 0.18f);

        // ─── Styles ──────────────────────────────────────────────────────────────────

        private GUIStyle _entryStyle;
        private GUIStyle _heroStyle;
        private GUIStyle _panelTitleStyle;
        private GUIStyle _panelBoxStyle;

        // ─── Lifecycle ───────────────────────────────────────────────────────────────

        private void OnEnable () => Scan();

        // ─── GUI ─────────────────────────────────────────────────────────────────────

        private void OnGUI () {
            EnsureStyles();

            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            EditorGUILayout.Space(6);

            // Top entries (found / error)
            foreach (var e in _top) DrawEntry(e);

            // Hero blocks
            foreach (var block in _blocks) {
                EditorGUILayout.Space(4);
                DrawHeroHeader(block);

                foreach (var panel in block.panels) {
                    EditorGUILayout.Space(2);
                    DrawPanel(panel);
                }
            }

            EditorGUILayout.Space(6);
            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("Scan Scene", GUILayout.Height(32))) Scan();
            if (GUILayout.Button("Documentation", GUILayout.Height(28))) {
                string path = System.IO.Path.Combine(Application.dataPath, "AFPC/Documentation.html");
                Application.OpenURL("file:///" + path.Replace("\\", "/"));
            }
            if (GUILayout.Button("Asset Store", GUILayout.Height(28)))
                Application.OpenURL("https://assetstore.unity.com/publishers/21782");
        }

        private void DrawHeroHeader (HeroBlock block) {
            Rect rect = EditorGUILayout.GetControlRect(false, 28);
            EditorGUI.DrawRect(rect, colHeader);
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, 4, rect.height), colAccent);
            GUI.contentColor = colHero;
            string label = block.name + (block.inactive ? "  [inactive]" : "");
            EditorGUI.LabelField(new Rect(rect.x + 10, rect.y, rect.width - 10, rect.height), label, _heroStyle);
            GUI.contentColor = Color.white;
            if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
                if (block.ping) EditorGUIUtility.PingObject(block.ping);
        }

        private void DrawPanel (Panel panel) {
            EditorGUILayout.BeginVertical(_panelBoxStyle);

            // Panel title
            GUI.contentColor = colPanel;
            EditorGUILayout.LabelField(panel.title, _panelTitleStyle);
            GUI.contentColor = Color.white;

            // Entries
            foreach (var entry in panel.entries) DrawEntry(entry);

            EditorGUILayout.EndVertical();
        }

        private void DrawEntry (Entry entry) {
            Color c = entry.level switch {
                Level.OK      => colOK,
                Level.Warning => colWarning,
                Level.Error   => colError,
                _             => colInfo,
            };
            GUI.contentColor = c;
            Rect rect = GUILayoutUtility.GetRect(new GUIContent(entry.text), _entryStyle, GUILayout.ExpandWidth(true));
            if (GUI.Button(rect, entry.text, _entryStyle))
                if (entry.ping) EditorGUIUtility.PingObject(entry.ping);
            GUI.contentColor = Color.white;
        }

        // ─── Scan ────────────────────────────────────────────────────────────────────

        private void Scan () {
            _top.Clear();
            _blocks.Clear();
            _currentBlock = null;
            _currentPanel = null;

            var heroes = FindObjectsByType<Hero>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            if (heroes.Length == 0) {
                _top.Add(new Entry { level = Level.Error, text = "No Hero found in the scene." });
                return;
            }

            _top.Add(new Entry { level = Level.Info, text = $"Found {heroes.Length} Hero{(heroes.Length > 1 ? "s" : "")} in scene" });

            foreach (var hero in heroes) ScanHero(hero);
        }

        private void ScanHero (Hero hero) {
            _currentBlock = new HeroBlock {
                name     = hero.name,
                inactive = !hero.gameObject.activeInHierarchy,
                ping     = hero.gameObject,
            };
            _blocks.Add(_currentBlock);

            BeginPanel("References"); ScanReferences(hero);
            BeginPanel("Movement");   ScanMovement(hero);
            BeginPanel("Overview");   ScanOverview(hero);
            BeginPanel("Lifecycle");  ScanLifecycle(hero);
            BeginPanel("Extensions"); ScanExtensions(hero);
        }

        // ─── Core ────────────────────────────────────────────────────────────────────

        private void ScanReferences (Hero hero) {
            Check(hero.movement.rb,     "Rigidbody",        Level.Error, hero.gameObject);
            Check(hero.movement.cc,     "CapsuleCollider",  Level.Error, hero.gameObject);
            Check(hero.overview.camera, "Camera",           Level.Error, hero.gameObject);
            Check(hero.inputActions,    "InputActionAsset", Level.Error, hero.gameObject);
            if (hero.UI) Log(Level.OK, $"✓  UI  [{hero.UI.name}]", hero.UI.gameObject);
        }

        private void ScanMovement (Hero hero) {
            var mv = hero.movement;

            Log(Level.Info, $"Walk {mv.referenceAcceleration:0.##}  ·  Run {mv.runningAcceleration:0.##}  ·  Start sharpness {mv.startingSharpness:0.##}  ·  Stop sharpness {mv.stoppingSharpness:0.##}");

            if (mv.maxJumpCount == 0)
                Log(Level.Warning, "⚠  Jump: disabled (maxJumpCount = 0)");
            else
                Log(Level.Info, $"Jump force {mv.jumpForce:0.##}  ×{mv.maxJumpCount}  ·  Endurance cost {mv.jumpEnduranceCost:0.##}");

            Log(Level.Info, $"Endurance {mv.referenceEndurance:0.##}  ·  Drain {mv.enduranceDrainRate:0.##}/s  ·  Recover {mv.enduranceRecoveryRate:0.##}/s");

            if (mv.crouchHeight < mv.height)
                Log(Level.Info, $"Crouch {mv.crouchMode}  ·  Height {mv.height:0.##} → {mv.crouchHeight:0.##}  ·  Speed reduction {mv.crouchSpeedReduction * 100:0}%");
            else
                Log(Level.Warning, $"⚠  Crouch height ({mv.crouchHeight:0.##}) >= standing height ({mv.height:0.##})");

            Log(Level.Info, $"Mass {mv.mass:0.##}  ·  Drag {mv.drag:0.##}  ·  Air drag {mv.airDrag:0.##}");

            if (mv.groundMask.value == 0)
                Log(Level.Error, "✗  Ground Mask: Nothing — ground detection will never trigger");
            else
                Log(Level.Info, $"Ground Mask: {mv.groundMask.value}  ·  Ceiling Mask: {mv.ceilingMask.value}");
        }

        private void ScanOverview (Hero hero) {
            var ov = hero.overview;

            Log(Level.Info, $"FOV {ov.defaultFOV:0.##}° → {ov.aimingFOV:0.##}° (aim)");

            if (ov.sensitivity <= 0f)
                Log(Level.Error, $"✗  Sensitivity is {ov.sensitivity:0.##} — camera won't move");
            else
                Log(Level.Info, $"Sensitivity {ov.sensitivity:0.##} (mouse: fixed scale, gamepad: ×deltaTime)  ·  V-range ±{ov.verticalRange:0.##}°{(ov.horizontalRange > 0 ? $"  ·  H-range ±{ov.horizontalRange:0.##}°" : "")}");

            Log(Level.Info, ov.isSpeedFOV
                ? $"Speed FOV: on  ·  Max {ov.speedFOVMax:0.##}°  threshold {ov.speedFOVThreshold:0.##}"
                : "Speed FOV: off");

            Log(Level.Info, ov.isHeadBobbing
                ? $"Head Bobbing: on  ·  Speed {ov.bobbingSpeed:0.##}  amount {ov.bobbingAmount:0.###}"
                : "Head Bobbing: off");

            if (ov.referenceShakingAmount > 0f)
                Log(Level.Info, $"Shake amount {ov.referenceShakingAmount:0.##}");
        }

        private void ScanLifecycle (Hero hero) {
            var lc = hero.lifecycle;

            if (lc.referenceHealth <= 0f)
                Log(Level.Error, $"✗  Health max is {lc.referenceHealth:0.##}");
            else
                Log(Level.Info, $"Health {lc.referenceHealth:0.##}  ·  Recovery {(lc.healthRecoveryInterval > 0 ? $"+1 every {lc.healthRecoveryInterval:0.##}s" : "off")}");

            if (lc.referenceShield <= 0f)
                Log(Level.Info, "Shield: none");
            else
                Log(Level.Info, $"Shield {lc.referenceShield:0.##}  ·  Recovery {(lc.shieldRecoveryInterval > 0 ? $"+1 every {lc.shieldRecoveryInterval:0.##}s" : "off")}");

            var field = typeof(Lifecycle).GetField("frenzyThreshold", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null) {
                float threshold = (float)field.GetValue(lc);
                Log(Level.Info, $"Frenzy below {threshold:0.##}% health");
            }
        }

        // ─── Extensions ──────────────────────────────────────────────────────────────

        private void ScanExtensions (Hero hero) {
            var exts = hero.extensions;

            if (exts == null || exts.Count == 0) {
                Log(Level.Info, "none");
                return;
            }

            for (int i = 0; i < exts.Count; i++) {
                var ext = exts[i];
                if (ext == null) {
                    Log(Level.Error, $"✗  [{i}] null slot");
                    continue;
                }
                string label = $"✓  [{i}] {ext.GetType().Name}  ·  order {ext.order}{(!ext.enabled ? "  [disabled]" : "")}";
                Log(ext.enabled ? Level.OK : Level.Info, label, ext.gameObject);
            }
        }

        // ─── Helpers ─────────────────────────────────────────────────────────────────

        private void BeginPanel (string title) {
            _currentPanel = new Panel { title = title };
            _currentBlock.panels.Add(_currentPanel);
        }

        private void Check (Object obj, string label, Level failLevel, Object ping = null) {
            if (obj) Log(Level.OK,  $"✓  {label}  [{obj.name}]", ping);
            else     Log(failLevel, $"✗  {label}: not assigned",  ping);
        }

        private void Log (Level level, string text, Object ping = null) {
            _currentPanel?.entries.Add(new Entry { level = level, text = text, ping = ping });
        }

        // ─── Styles ──────────────────────────────────────────────────────────────────

        private void EnsureStyles () {
            if (_entryStyle != null) return;

            _entryStyle = new GUIStyle(EditorStyles.label) {
                fontSize  = 11,
                richText  = true,
                padding   = new RectOffset(6, 0, 1, 1),
                font      = EditorStyles.miniFont,
            };

            _heroStyle = new GUIStyle(EditorStyles.boldLabel) {
                fontSize  = 13,
                alignment = TextAnchor.MiddleLeft,
            };

            _panelTitleStyle = new GUIStyle(EditorStyles.boldLabel) {
                fontSize = 11,
                padding  = new RectOffset(4, 0, 2, 2),
            };

            _panelBoxStyle = new GUIStyle(EditorStyles.helpBox) {
                padding = new RectOffset(6, 6, 4, 4),
                margin  = new RectOffset(4, 4, 0, 0),
            };
        }
    }
}
