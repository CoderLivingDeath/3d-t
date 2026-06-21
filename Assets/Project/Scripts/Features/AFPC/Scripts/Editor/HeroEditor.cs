using UnityEngine;
using UnityEditor;

namespace AFPC.Editor {

[CustomEditor(typeof(Hero))]
public class HeroEditor : AFPCEditor {

    private static readonly Color accentLifecycle  = new Color(0.85f, 0.3f,  0.3f,  1f);
    private static readonly Color accentMovement   = new Color(0.3f,  0.65f, 0.85f, 1f);
    private static readonly Color accentOverview   = new Color(0.3f,  0.8f,  0.45f, 1f);
    private static readonly Color accentRuntime    = new Color(0.9f,  0.75f, 0.2f,  1f);
    private static readonly Color accentExtensions = new Color(0.7f,  0.4f,  0.9f,  1f);

    private static GUIStyle _extensionsPanelStyle;
    private static GUIStyle ExtensionsPanelStyle {
        get {
            if (_extensionsPanelStyle == null) {
                _extensionsPanelStyle = new GUIStyle(EditorStyles.helpBox);
                _extensionsPanelStyle.padding = new RectOffset(2, 2, 2, 2);
                _extensionsPanelStyle.margin  = new RectOffset(0, 0, 0, 0);
            }
            return _extensionsPanelStyle;
        }
    }

    private void OnEnable () {
        Hero hero = (Hero)target;
        hero.extensions.Sort((a, b) => {
            if (a == null) return 1;
            if (b == null) return -1;
            return a.order.CompareTo(b.order);
        });
        EditorUtility.SetDirty(target);
    }

    public override void OnInspectorGUI () {
        serializedObject.Update();

        DrawReferences();
        EditorGUILayout.Space(2);
        DrawLifecycle();
        EditorGUILayout.Space(2);
        DrawMovement();
        EditorGUILayout.Space(2);
        DrawOverview();
        EditorGUILayout.Space(2);
        DrawExtensions();

        if (Application.isPlaying) {
            EditorGUILayout.Space(2);
            DrawRuntime();
        }

        serializedObject.ApplyModifiedProperties();

        EditorGUILayout.Space(4);
        if (GUILayout.Button("Documentation", GUILayout.Height(32))) {
            string path = System.IO.Path.Combine(Application.dataPath, "AFPC/Documentation.html");
            Application.OpenURL("file:///" + path.Replace("\\", "/"));
        }
    }

    // ─── Panels ───

    private void DrawReferences () {
        var prop = serializedObject.FindProperty("UI");
        if (!DrawHeader("REFERENCES", prop, Color.gray)) return;
        BeginPanel();
        EditorGUILayout.PropertyField(prop, new GUIContent("UI", "Optional AFPCUI reference for health, shield, endurance bars and damage effects"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("inputActions"), new GUIContent("Input Actions", "Input Action Asset containing Player action map with Move, Look, Jump, Run, Aim, Crouch, Damage, Heal, Respawn actions"));
        var mv = serializedObject.FindProperty("movement");
        EditorGUILayout.PropertyField(mv.FindPropertyRelative("rb"), new GUIContent("Rigidbody", "Character rigidbody used for physics-based movement and rotation"));
        EditorGUILayout.PropertyField(mv.FindPropertyRelative("cc"), new GUIContent("Collider", "Capsule collider used for ground detection and physics material generation"));
        var ov = serializedObject.FindProperty("overview");
        EditorGUILayout.PropertyField(ov.FindPropertyRelative("camera"), new GUIContent("Camera", "Main camera used for looking, aiming, FOV effects and shaking"));
        EndPanel();
    }

    private void DrawLifecycle () {
        var lc = serializedObject.FindProperty("lifecycle");
        if (!DrawHeader("LIFECYCLE", lc, accentLifecycle)) return;
        BeginPanel();
        EditorGUILayout.PropertyField(lc.FindPropertyRelative("isDebugLog"), new GUIContent("Debug", "Log lifecycle events to the console"));
        EditorGUILayout.PropertyField(lc.FindPropertyRelative("ID"), new GUIContent("ID", "Identifier used in debug log messages"));

        if (DrawToggleSection("Health", lc.FindPropertyRelative("referenceHealth"))) {
            EditorGUILayout.PropertyField(lc.FindPropertyRelative("referenceHealth"), new GUIContent("Max", "Maximum health value. Health recovers toward this over time"));
            EditorGUILayout.PropertyField(lc.FindPropertyRelative("healthRecoveryInterval"), new GUIContent("Recovery (s)", "Seconds between each +1 health recovery tick. Set to 0 to disable"));
            EndSubPanel();
        }

        if (DrawToggleSection("Shield", lc.FindPropertyRelative("referenceShield"))) {
            EditorGUILayout.PropertyField(lc.FindPropertyRelative("referenceShield"), new GUIContent("Max", "Maximum shield value. Shield absorbs damage before health"));
            EditorGUILayout.PropertyField(lc.FindPropertyRelative("shieldRecoveryInterval"), new GUIContent("Recovery (s)", "Seconds between each +1 shield recovery tick. Set to 0 to disable"));
            EndSubPanel();
        }

        if (DrawToggleSection("Frenzy", lc.FindPropertyRelative("frenzyThreshold"))) {
            EditorGUILayout.PropertyField(lc.FindPropertyRelative("frenzyThreshold"), new GUIContent("Threshold (%)", "Health percentage below which frenzy state activates. Use for low-health special effects"));
            EndSubPanel();
        }

        if (DrawToggleSectionFlat("Events", lc.FindPropertyRelative("onDamage"))) {
            EditorGUILayout.PropertyField(lc.FindPropertyRelative("onDamage"),        new GUIContent("On Damage (float)", "Fired when the character takes damage. Parameter: damage amount"));
            EditorGUILayout.PropertyField(lc.FindPropertyRelative("onHeal"),          new GUIContent("On Heal (float)",   "Fired when the character is healed. Parameter: heal amount"));
            EditorGUILayout.PropertyField(lc.FindPropertyRelative("onDeath"),         new GUIContent("On Death",          "Fired when health reaches zero"));
            EditorGUILayout.PropertyField(lc.FindPropertyRelative("onRespawn"),       new GUIContent("On Respawn",        "Fired when the character respawns with full health and shield"));
            EditorGUILayout.PropertyField(lc.FindPropertyRelative("onActivate"),      new GUIContent("On Activate",       "Fired when the character becomes available"));
            EditorGUILayout.PropertyField(lc.FindPropertyRelative("onDeactivate"),    new GUIContent("On Deactivate",     "Fired when the character becomes unavailable"));
            EditorGUILayout.PropertyField(lc.FindPropertyRelative("onFrenzyChanged"), new GUIContent("On Frenzy (bool)",  "Fired when frenzy state changes. Parameter: is frenzy active"));
            EditorGUILayout.PropertyField(lc.FindPropertyRelative("onHealthChanged"), new GUIContent("On Health (float)", "Fired when health value changes. Parameter: current health"));
            EditorGUILayout.PropertyField(lc.FindPropertyRelative("onShieldChanged"), new GUIContent("On Shield (float)", "Fired when shield value changes. Parameter: current shield"));
        }

        if (lc.FindPropertyRelative("isDebugLog").boolValue) DrawDebugSection(((Hero)target).lifecycle);

        EndPanel();
    }

    private void DrawMovement () {
        var mv = serializedObject.FindProperty("movement");
        if (!DrawHeader("MOVEMENT", mv, accentMovement)) return;
        BeginPanel();
        EditorGUILayout.PropertyField(mv.FindPropertyRelative("isDebugLog"), new GUIContent("Debug", "Log movement events to the console"));

        if (DrawToggleSection("Acceleration", mv.FindPropertyRelative("referenceAcceleration"))) {
            EditorGUILayout.PropertyField(mv.FindPropertyRelative("referenceAcceleration"), new GUIContent("Walk Speed",     "Base movement speed when walking"));
            EditorGUILayout.PropertyField(mv.FindPropertyRelative("runningAcceleration"),   new GUIContent("Run Speed",      "Movement speed when holding the run button"));
            EditorGUILayout.PropertyField(mv.FindPropertyRelative("startingSharpness"),     new GUIContent("Start Sharpness","How quickly the character reaches target speed. Higher = snappier"));
            EditorGUILayout.PropertyField(mv.FindPropertyRelative("stoppingSharpness"),     new GUIContent("Stop Sharpness", "How quickly the character stops when input is released. Higher = less slide"));
            EndSubPanel();
        }

        if (DrawToggleSection("Endurance", mv.FindPropertyRelative("referenceEndurance"))) {
            EditorGUILayout.PropertyField(mv.FindPropertyRelative("referenceEndurance"),    new GUIContent("Max",           "Maximum endurance value. Depletes while running, recovers when not"));
            EditorGUILayout.PropertyField(mv.FindPropertyRelative("enduranceDrainRate"),    new GUIContent("Drain Rate",    "How fast endurance drains per second while running"));
            EditorGUILayout.PropertyField(mv.FindPropertyRelative("enduranceRecoveryRate"), new GUIContent("Recovery Rate", "How fast endurance recovers per second when not running"));
            EndSubPanel();
        }

        if (DrawToggleSection("Jumping", mv.FindPropertyRelative("jumpForce"))) {
            EditorGUILayout.PropertyField(mv.FindPropertyRelative("jumpForce"),         new GUIContent("Force",         "Upward velocity applied on each jump"));
            EditorGUILayout.PropertyField(mv.FindPropertyRelative("jumpEnduranceCost"), new GUIContent("Endurance Cost","Endurance consumed per jump. Jump is blocked if endurance is too low"));
            EditorGUILayout.PropertyField(mv.FindPropertyRelative("maxJumpCount"),      new GUIContent("Max Jumps",     "Number of jumps allowed before landing. Set to 2 for double jump"));
            EditorGUILayout.PropertyField(mv.FindPropertyRelative("groundDetectionMode"), new GUIContent("Ground Detection", "Raycast: single ray, works on all geometry. MultiRaycast: center + ring for better edge detection. SphereCast: most forgiving but fails on modular seams"));
            EditorGUILayout.PropertyField(mv.FindPropertyRelative("groundCheckRadius"), new GUIContent("Ground Radius", "Radius used for SphereCast sphere and MultiRaycast ring"));
            if (mv.FindPropertyRelative("groundDetectionMode").enumValueIndex == (int)GroundDetectionMode.MultiRaycast) {
                EditorGUILayout.PropertyField(mv.FindPropertyRelative("multiRaycastRayCount"), new GUIContent("Ray Count", "Number of rays in the ring for MultiRaycast mode"));
            }
            EditorGUILayout.PropertyField(mv.FindPropertyRelative("groundCheckSkinWidth"), new GUIContent("Skin Width", "Extra distance beyond the collider used for ground detection"));
            EditorGUILayout.Slider(mv.FindPropertyRelative("airControlFactor"), 0f, 1f, new GUIContent("Air Control", "How much input influence the character has while airborne. 0 = no air control, 1 = full control"));
            EndSubPanel();
        }

        if (DrawToggleSection("Crouch", mv.FindPropertyRelative("crouchMode"))) {
            EditorGUILayout.PropertyField(mv.FindPropertyRelative("crouchMode"),            new GUIContent("Mode",             "Hold: crouch while button held. Toggle: press once to crouch, press again to stand"));
            EditorGUILayout.PropertyField(mv.FindPropertyRelative("crouchHeight"),          new GUIContent("Height",           "Capsule collider height while crouching. Must be less than standing Height"));
            EditorGUILayout.PropertyField(mv.FindPropertyRelative("crouchSpeedReduction"),  new GUIContent("Speed Reduction",  "Fraction of walk speed removed while crouching. 0 = no reduction, 1 = cannot move"));
            EditorGUILayout.PropertyField(mv.FindPropertyRelative("crouchTransitionSpeed"), new GUIContent("Transition Speed", "How fast the collider and camera lerp between standing and crouching. Shared by both"));
            EditorGUILayout.PropertyField(mv.FindPropertyRelative("ceilingMask"),           new GUIContent("Ceiling Mask",     "Layers checked for ceiling clearance before standing up. Set this to your geometry layer"));
            EndSubPanel();
        }

        if (DrawToggleSection("Physics", mv.FindPropertyRelative("isGeneratePhysicMaterial"))) {
            EditorGUILayout.PropertyField(mv.FindPropertyRelative("isGeneratePhysicMaterial"), new GUIContent("Gen. Material","Auto-generate a frictionless physic material on initialize"));
            EditorGUILayout.PropertyField(mv.FindPropertyRelative("mass"),                     new GUIContent("Mass",         "Rigidbody mass in kg. Affects physics interactions"));
            EditorGUILayout.PropertyField(mv.FindPropertyRelative("height"),                   new GUIContent("Height",       "Capsule collider standing height. Must be greater than Crouch Height"));
            EditorGUILayout.PropertyField(mv.FindPropertyRelative("drag"),                     new GUIContent("Ground Drag",  "Rigidbody drag while grounded. Higher = more friction"));
            EditorGUILayout.PropertyField(mv.FindPropertyRelative("airDrag"),                  new GUIContent("Air Drag",     "Rigidbody drag while airborne. Lower than ground drag for momentum"));
            EditorGUILayout.PropertyField(mv.FindPropertyRelative("groundMask"),               new GUIContent("Ground Mask",  "Layer mask for ground detection. Only these layers count as ground"));
            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("Slopes", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(mv.FindPropertyRelative("slopeLimit"),             new GUIContent("Slope Limit",      "Max walkable angle in degrees. Steeper surfaces cause sliding"));
            EditorGUILayout.PropertyField(mv.FindPropertyRelative("slopeSlideForceMul"),     new GUIContent("Slide Force",      "Multiplier for gravity slide on steep slopes. 0 disables sliding"));
            EditorGUILayout.Slider(mv.FindPropertyRelative("uphillSpeedReduction"),  0f, 1f, new GUIContent("Uphill Reduction", "Speed reduction going uphill on walkable slopes"));
            EditorGUILayout.Slider(mv.FindPropertyRelative("downhillSpeedBoost"),    0f, 1f, new GUIContent("Downhill Boost",   "Speed boost going downhill on walkable slopes"));
            EndSubPanel();
        }

        if (DrawToggleSectionFlat("Events", mv.FindPropertyRelative("onLanded"))) {
            EditorGUILayout.PropertyField(mv.FindPropertyRelative("onLanded"),           new GUIContent("On Landed",            "Fired when the character lands on the ground"));
            EditorGUILayout.PropertyField(mv.FindPropertyRelative("onAirborne"),         new GUIContent("On Airborne",          "Fired when the character leaves the ground"));
            EditorGUILayout.PropertyField(mv.FindPropertyRelative("onJump"),             new GUIContent("On Jump (int)",        "Fired on each jump. Parameter: current jump count (1 = first, 2 = double)"));
            EditorGUILayout.PropertyField(mv.FindPropertyRelative("onRunStart"),         new GUIContent("On Run Start",         "Fired when the character starts running"));
            EditorGUILayout.PropertyField(mv.FindPropertyRelative("onRunStop"),          new GUIContent("On Run Stop",          "Fired when the character stops running"));
            EditorGUILayout.PropertyField(mv.FindPropertyRelative("onEnduranceDepleted"),new GUIContent("On Endurance Depleted","Fired when endurance reaches zero while running"));
            EditorGUILayout.PropertyField(mv.FindPropertyRelative("onCrouchStart"),      new GUIContent("On Crouch Start",      "Fired when the character starts crouching"));
            EditorGUILayout.PropertyField(mv.FindPropertyRelative("onCrouchStop"),       new GUIContent("On Crouch Stop",       "Fired when the character stands back up"));
        }

        if (mv.FindPropertyRelative("isDebugLog").boolValue) DrawDebugSection(((Hero)target).movement);

        EndPanel();
    }

    private void DrawOverview () {
        var ov = serializedObject.FindProperty("overview");
        if (!DrawHeader("OVERVIEW", ov, accentOverview)) return;
        BeginPanel();
        EditorGUILayout.PropertyField(ov.FindPropertyRelative("isDebugLog"), new GUIContent("Debug", "Log overview events to the console"));

        if (DrawToggleSection("Following", ov.FindPropertyRelative("isFollowingInstant"))) {
            EditorGUILayout.PropertyField(ov.FindPropertyRelative("isFollowingInstant"), new GUIContent("Instant", "Camera snaps to target position. Disable for smooth follow with damping"));
            EditorGUILayout.PropertyField(ov.FindPropertyRelative("damping"),            new GUIContent("Damping", "Smooth follow speed when Instant is off. Higher = faster catch-up"));
            EditorGUILayout.PropertyField(ov.FindPropertyRelative("cameraOffset"),       new GUIContent("Offset",  "Camera position offset from the character. Y is eye height"));
            EndSubPanel();
        }

        if (DrawToggleSection("Looking", ov.FindPropertyRelative("sensitivity"))) {
            EditorGUILayout.PropertyField(ov.FindPropertyRelative("hideCursor"),          new GUIContent("Hide Cursor",      "Lock and hide the cursor when the game starts. Disable to keep the cursor visible"));
            EditorGUILayout.PropertyField(ov.FindPropertyRelative("sensitivity"),        new GUIContent("Sensitivity",      "Look sensitivity. Mouse uses a fixed scale (framerate-independent), gamepad sticks use Time.deltaTime (degrees per second)"));
            EditorGUILayout.PropertyField(ov.FindPropertyRelative("isHorizontalInverted"),new GUIContent("Invert Horizontal","Invert horizontal mouse axis"));
            EditorGUILayout.PropertyField(ov.FindPropertyRelative("isVerticalInverted"), new GUIContent("Invert Vertical",  "Invert vertical mouse axis"));
            EditorGUILayout.PropertyField(ov.FindPropertyRelative("horizontalRange"),    new GUIContent("Horizontal Range", "Max horizontal rotation in degrees. Set to 0 for unlimited"));
            EditorGUILayout.PropertyField(ov.FindPropertyRelative("verticalRange"),      new GUIContent("Vertical Range",   "Max vertical rotation in degrees. Prevents looking too far up or down"));
            EndSubPanel();
        }

        if (DrawToggleSection("Aiming", ov.FindPropertyRelative("defaultFOV"))) {
            EditorGUILayout.PropertyField(ov.FindPropertyRelative("defaultFOV"),         new GUIContent("Default FOV",    "Normal field of view in degrees"));
            EditorGUILayout.PropertyField(ov.FindPropertyRelative("aimingFOV"),          new GUIContent("Aim FOV",        "Field of view when aiming (right mouse). Lower = more zoom"));
            EditorGUILayout.PropertyField(ov.FindPropertyRelative("aimingFOVSmoothing"), new GUIContent("Aim Smoothing",  "Speed of the FOV transition when pressing or releasing aim"));
            EditorGUILayout.PropertyField(ov.FindPropertyRelative("searchMask"),         new GUIContent("Search Mask",    "Layer mask for the Search() raycast. Use for interaction or shooting targets"));
            EditorGUILayout.PropertyField(ov.FindPropertyRelative("searchDistance"),     new GUIContent("Search Distance","Maximum raycast distance for Search()"));
            EndSubPanel();
        }

        if (DrawToggleSectionWithProp(ov, "Speed FOV", "isSpeedFOV")) {
            EditorGUILayout.PropertyField(ov.FindPropertyRelative("speedFOVMax"),       new GUIContent("Max FOV",     "Maximum FOV when moving at full run speed"));
            EditorGUILayout.PropertyField(ov.FindPropertyRelative("speedFOVSmoothing"), new GUIContent("Smoothing",   "How fast FOV transitions between values"));
            EditorGUILayout.PropertyField(ov.FindPropertyRelative("speedFOVThreshold"), new GUIContent("Threshold",   "Minimum speed (0-1) before FOV starts increasing"));
            EditorGUILayout.Slider(ov.FindPropertyRelative("speedFOVGlobalWeight"),  0f, 1f, new GUIContent("Global Weight","Overall multiplier for the speed FOV effect"));
            EditorGUILayout.Slider(ov.FindPropertyRelative("speedFOVWeightForward"), 0f, 1f, new GUIContent("Forward",     "How much forward velocity contributes to the FOV effect"));
            EditorGUILayout.Slider(ov.FindPropertyRelative("speedFOVWeightStrafe"),  0f, 1f, new GUIContent("Strafe",      "How much lateral (left/right) velocity contributes to the FOV effect"));
            EditorGUILayout.Slider(ov.FindPropertyRelative("speedFOVWeightVertical"),0f, 1f, new GUIContent("Vertical",    "How much vertical (jump/fall) velocity contributes to the FOV effect"));
            EndSubPanel();
        }

        if (DrawToggleSectionWithProp(ov, "Head Bobbing", "isHeadBobbing")) {
            EditorGUILayout.PropertyField(ov.FindPropertyRelative("bobbingSpeed"),         new GUIContent("Speed",          "How fast the camera bobs while moving"));
            EditorGUILayout.PropertyField(ov.FindPropertyRelative("bobbingAmount"),        new GUIContent("Amount",         "Vertical amplitude of the head bob effect"));
            EditorGUILayout.PropertyField(ov.FindPropertyRelative("bobbingRunMultiplier"), new GUIContent("Run Multiplier", "Multiplies bobbing speed and amount while running"));
            EndSubPanel();
        }

        if (DrawToggleSection("Crouch Camera", ov.FindPropertyRelative("crouchCameraOffset"))) {
            EditorGUILayout.PropertyField(ov.FindPropertyRelative("crouchCameraOffset"), new GUIContent("Y Offset", "How far the camera drops when crouching. Negative = down"));
            EndSubPanel();
        }

        EditorGUILayout.Space(4);
        EditorGUILayout.PropertyField(ov.FindPropertyRelative("referenceShakingAmount"), new GUIContent("Shake Amount",    "Base camera shake intensity. Camera lens shifts by this amount. Requires Physical Camera mode"));
        EditorGUILayout.PropertyField(ov.FindPropertyRelative("shakeAmplitude"),         new GUIContent("Shake Amplitude",   "Global multiplier applied to every Shake() call. 1 = normal, 0 = no shake, 2 = double intensity"));
        EditorGUILayout.PropertyField(ov.FindPropertyRelative("shakeDecayRate"),         new GUIContent("Shake Decay Rate",  "Units per second the shake intensity fades toward the reference amount. Higher = shorter shakes"));

        if (DrawToggleSectionFlat("Events", ov.FindPropertyRelative("onAimStart"))) {
            EditorGUILayout.PropertyField(ov.FindPropertyRelative("onAimStart"), new GUIContent("On Aim Start",   "Fired when the player starts aiming (right mouse down)"));
            EditorGUILayout.PropertyField(ov.FindPropertyRelative("onAimStop"),  new GUIContent("On Aim Stop",    "Fired when the player stops aiming (right mouse up)"));
            EditorGUILayout.PropertyField(ov.FindPropertyRelative("onShake"),    new GUIContent("On Shake (float)","Fired when the camera is shaken. Parameter: shake amount"));
        }

        if (ov.FindPropertyRelative("isDebugLog").boolValue) DrawDebugSection(((Hero)target).overview);

        EndPanel();
    }

    private void DrawExtensions () {
        var list = serializedObject.FindProperty("extensions");
        if (!DrawHeader("EXTENSIONS", list, accentExtensions)) return;
        EditorGUILayout.BeginVertical(ExtensionsPanelStyle);

        int removeIndex = -1;
        EditorGUI.BeginChangeCheck();
        for (int i = 0; i < list.arraySize; i++) {
            var element = list.GetArrayElementAtIndex(i);
            var ext = element.objectReferenceValue as AFPCExtension;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(element, GUIContent.none);
            if (ext != null) {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.IntField(ext.order, GUILayout.Width(32));
                EditorGUI.EndDisabledGroup();
            }
            if (GUILayout.Button("\u00D7", GUILayout.Width(16), GUILayout.Height(16))) removeIndex = i;
            EditorGUILayout.EndHorizontal();
        }
        if (removeIndex >= 0) list.DeleteArrayElementAtIndex(removeIndex);
        if (EditorGUI.EndChangeCheck()) {
            serializedObject.ApplyModifiedProperties();
            Hero hero = (Hero)target;
            hero.extensions.Sort((a, b) => {
                if (a == null) return 1;
                if (b == null) return -1;
                return a.order.CompareTo(b.order);
            });
            for (int i = 0; i < hero.extensions.Count; i++)
                list.GetArrayElementAtIndex(i).objectReferenceValue = hero.extensions[i];
            EditorUtility.SetDirty(target);
        }

        if (GUILayout.Button("+", GUILayout.Height(32))) {
            list.InsertArrayElementAtIndex(list.arraySize);
            list.GetArrayElementAtIndex(list.arraySize - 1).objectReferenceValue = null;
        }

        EndPanel();
    }

    private void DrawRuntime () {
        DrawHeader("RUNTIME", accentRuntime);
        BeginPanel();
        Hero hero = (Hero)target;
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.FloatField(new GUIContent("Health",   "Current health value"),                           hero.lifecycle.GetHealthValue());
        EditorGUILayout.FloatField(new GUIContent("Shield",   "Current shield value"),                           hero.lifecycle.GetShieldValue());
        EditorGUILayout.FloatField(new GUIContent("Endurance","Current endurance value"),                        hero.movement.GetEnduranceValue());
        EditorGUILayout.FloatField(new GUIContent("Speed",    "Current rigidbody velocity magnitude"),           hero.movement.rb ? hero.movement.rb.linearVelocity.magnitude : 0);
        EditorGUILayout.Toggle    (new GUIContent("Grounded",    "Whether the character is currently on the ground"),hero.movement.IsGrounded());
        EditorGUILayout.Toggle    (new GUIContent("Steep Slope", "On an unwalkable slope"),                       hero.movement.IsOnSteepSlope());
        EditorGUILayout.FloatField(new GUIContent("Slope Angle", "Ground surface angle in degrees"),              hero.movement.GetGroundSlopeAngle());
        EditorGUILayout.Toggle    (new GUIContent("Crouching",   "Whether the character is currently crouching"), hero.movement.IsCrouching());
        EditorGUILayout.Toggle    (new GUIContent("Frenzy",   "Whether health is below the frenzy threshold"),   hero.lifecycle.IsFrenzy());
        EditorGUI.EndDisabledGroup();
        EndPanel();
        Repaint();
    }
}
}
