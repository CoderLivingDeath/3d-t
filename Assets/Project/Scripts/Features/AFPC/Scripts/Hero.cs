using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AFPC {

/// <summary>
/// Example of setup AFPC with Lifecycle, Movement and Overview classes.
/// </summary>
public class Hero : MonoBehaviour {

    /* Info */
    public AFPCObjectInfo info = new AFPCObjectInfo();

    /* UI Reference */
    public AFPCUI UI;

    /* Lifecycle class. Damage, Heal, Death, Respawn... */
    public Lifecycle lifecycle;

    /* Movement class. Move, Jump, Run, Crouch... */
    public Movement movement;

    /* Overview class. Look, Aim, Shake... */
    public Overview overview;

    /* Extensions */
    public List<AFPCExtension> extensions = new List<AFPCExtension>();

    /* Input */
    public InputActionAsset inputActions;
    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction jumpAction;
    private InputAction runAction;
    private InputAction aimAction;
    private InputAction crouchAction;
    private InputAction damageAction;
    private InputAction healAction;
    private InputAction respawnAction;
    private int initSkipFrames;

    /* Optional assign the UI */
    private void Awake () {
        if (UI) {
            UI.hero = this;
        }
    }

    /* Some classes need to initialize */
    private void Start () {

        if (overview.hideCursor) overview.HideCursor();
        initSkipFrames = 2;

        /* Initialize overview: sync rotation and FOV */
        overview.Initialize();

        /* Initialize lifecycle and add Damage FX */
        lifecycle.Initialize();
        lifecycle.onDamage.AddListener((_) => DamageFX());
        lifecycle.onDeath.AddListener(() => movement.StandUp());

        /* Initialize movement and add camera shake when landing */
        movement.Initialize();
        overview.crouchTransitionSpeed = movement.crouchTransitionSpeed;

        /* Initialize extensions sorted by order */
        extensions.Sort((a, b) => {
            if (a == null) return 1;
            if (b == null) return -1;
            return a.order.CompareTo(b.order);
        });
        foreach (var ext in extensions) {
            if (ext == null) continue;
            ext.hero = this;
            ext.Initialize();
        }


        /* Bind input actions */
        if (!inputActions) {
            Debug.LogWarning("Hero: InputActionAsset is not assigned.", this);
            return;
        }
        var playerMap = inputActions.FindActionMap("Player");
        if (playerMap == null) {
            Debug.LogWarning("Hero: 'Player' action map not found in InputActionAsset.", this);
            return;
        }
        moveAction    = playerMap.FindAction("Move");
        lookAction    = playerMap.FindAction("Look");
        jumpAction    = playerMap.FindAction("Jump");
        runAction     = playerMap.FindAction("Run");
        aimAction     = playerMap.FindAction("Aim");
        crouchAction  = playerMap.FindAction("Crouch");
        damageAction  = playerMap.FindAction("Damage");
        healAction    = playerMap.FindAction("Heal");
        respawnAction = playerMap.FindAction("Respawn");
        inputActions.Enable();
    }

    private void OnDestroy () {
        if (inputActions) inputActions.Disable();
    }

    private void Update () {

        /* Skip two frames so the cursor-lock warp delta fully clears before input is read */
        if (initSkipFrames > 0) {
            initSkipFrames--;
            return;
        }

        /* Read player input before check availability */
        ReadInput();

        /* Block controller when unavailable */
        if (!lifecycle.Availability()) return;

        /* Mouse look state */
        overview.Looking();

        /* Speed FOV: normalize velocity to 0-1 range based on run speed */
        if (movement.rb) {
            Vector3 local = movement.rb.transform.InverseTransformDirection (movement.rb.linearVelocity);
            float weighted = Mathf.Max (0f, local.z) * overview.speedFOVWeightForward
                           + Mathf.Abs (local.x) * overview.speedFOVWeightStrafe
                           + Mathf.Abs (local.y) * overview.speedFOVWeightVertical;
            overview.speedInput = Mathf.Clamp01 (weighted / movement.runningAcceleration * overview.speedFOVGlobalWeight);
        } else {
            overview.speedInput = 0;
        }
        overview.isGrounded = movement.IsGrounded();

        /* Change camera FOV state */
        overview.Aiming();

        /* Shake camera state. Required "physical camera" mode on */
        overview.Shaking();

        /* Control the speed */
        movement.Running();

        /* Sync camera crouch offset */
        overview.SetCrouching(movement.IsCrouching());

        /* Control the jumping, ground search... */
        movement.Jumping();

        /* Control the health and shield recovery */
        lifecycle.Runtime();

        /* Update extensions */
        foreach (var ext in extensions) {
            if (ext != null && ext.enabled) ext.OnUpdate();
        }
    }

    private void FixedUpdate () {

        /* Block controller when unavailable */
        if (!lifecycle.Availability()) return;

        /* Control crouching */
        movement.Crouching();

        /* Physical movement */
        movement.Accelerate();

        /* Physical rotation with camera */
        overview.RotateRigidbodyToLookDirection (movement.rb);

        /* FixedUpdate extensions */
        foreach (var ext in extensions) {
            if (ext != null && ext.enabled) ext.OnFixedUpdate();
        }
    }

    private void LateUpdate () {

        /* Block controller when unavailable */
        if (!lifecycle.Availability()) return;

        /* Camera following */
        overview.Follow (transform.position);

        /* LateUpdate extensions */
        foreach (var ext in extensions) {
            if (ext != null && ext.enabled) ext.OnLateUpdate();
        }
    }

    private void OnDrawGizmos () {
        if (!Application.isPlaying) return;
        movement?.DrawGroundCheckGizmos();
    }

    public T GetExtension<T> () where T : AFPCExtension {
        foreach (var ext in extensions)
            if (ext is T match) return match;
        return null;
    }

    public AFPCExtension GetExtension (string id) {
        foreach (var ext in extensions)
            if (ext != null && ext.info.ID == id) return ext;
        return null;
    }

    private void ReadInput () {
        if (moveAction == null) return;
        if (damageAction.WasPerformedThisFrame()) lifecycle.Damage(50);
        if (healAction.WasPerformedThisFrame()) lifecycle.Heal(50);
        if (respawnAction.WasPerformedThisFrame()) lifecycle.Respawn();
        Vector2 look = lookAction.ReadValue<Vector2>();
        overview.lookingInputValues.x = look.x;
        overview.lookingInputValues.y = look.y;
        var activeControl = lookAction.activeControl;
        overview.lookingIsMouseDelta = activeControl != null && activeControl.device is UnityEngine.InputSystem.Mouse;
        overview.aimingInputValue = aimAction.IsPressed();
        Vector2 move = moveAction.ReadValue<Vector2>();
        movement.movementInputValues.x = move.x;
        movement.movementInputValues.y = move.y;
        movement.jumpingInputValue  = jumpAction.WasPerformedThisFrame();
        movement.runningInputValue  = runAction.IsPressed();
        movement.crouchingInputValue   = crouchAction != null && crouchAction.IsPressed();
        movement.crouchingInputPressed = crouchAction != null && crouchAction.WasPerformedThisFrame();
    }

    private void DamageFX () {
        if (UI) UI.DamageFX();
        overview.Shake(0.75f);
    }
}
}
