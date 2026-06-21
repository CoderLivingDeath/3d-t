using UnityEngine;
using UnityEngine.Events;

namespace AFPC {

    public enum CrouchMode { Hold, Toggle }
    public enum GroundDetectionMode { Raycast, MultiRaycast, SphereCast }

    /// <summary>
    /// This class allows the user to move.
    /// </summary>
    [System.Serializable]
    public class Movement {

        public bool isDebugLog;

        [HideInInspector] public Vector3 movementInputValues;
        [HideInInspector] public bool runningInputValue;
        [HideInInspector] public bool jumpingInputValue;
        [HideInInspector] public bool crouchingInputValue;
        [HideInInspector] public bool crouchingInputPressed;

        public float referenceAcceleration = 2.66f;
        private float currentAcceleration = 2.66f;
        public float startingSharpness = 15.0f;
        public float stoppingSharpness = 20.0f;
        private Vector3 _moveVelocity;
        private bool isMovementAvailable = true;
        private bool releaseAcceleration = true;

        public float runningAcceleration = 5.32f;
        private bool isRunningAvailable = true;

        public float referenceEndurance = 20.0f;
        public float enduranceDrainRate = 2.0f;
        public float enduranceRecoveryRate = 1.0f;
        private float endurance = 20.0f;

        public float jumpForce = 7.5f;
        public float jumpEnduranceCost = 3.0f;
        public int maxJumpCount = 1;
        private int jumpCount;
        private bool isJumpingAvailable = true;
        [Range(0f, 1f)] public float airControlFactor = 1f;
        private float savedAirControlFactor = 1f;
        [HideInInspector] public bool jumpBuffered;
        [HideInInspector] public bool suppressEnduranceRecovery;
        [Tooltip("Seconds after full endurance depletion before recovery can begin.")]
        public float depletionRecoveryDelay = 2f;
        private float depletionTimer;
        private Vector3 groundCheckPosition;
        public float groundCheckRadius = 0.25f;
        [Tooltip("Extra distance beyond the collider used for ground detection.")]
        public float groundCheckSkinWidth = 0.1f;
        [Tooltip("Raycast: single ray, works on all geometry. MultiRaycast: center + ring for better edge detection. SphereCast: most forgiving but fails on modular seams.")]
        public GroundDetectionMode groundDetectionMode = GroundDetectionMode.MultiRaycast;
        [Tooltip("Number of rays in the ring for MultiRaycast mode.")]
        [Range(3, 8)] public int multiRaycastRayCount = 4;
        private RaycastHit groundHit;
        private bool hasGroundHit;
        private bool isOnSteepSlope;
        private float groundSlopeAngle;
        private Vector3 groundNormal = Vector3.up;
        private bool wasRunning;

        private bool isCrouching;
        private bool isCrouchingAvailable = true;
        private Vector3 initialColliderCenter;
        private float crouchMultiplier = 1f;
        private float targetColliderHeight;
        private float currentColliderHeight;

        /// <summary> Fired when the character lands on the ground. </summary>
        public UnityEvent onLanded;
        /// <summary> Fired when the character leaves the ground. </summary>
        public UnityEvent onAirborne;
        /// <summary> Fired when the character jumps. Parameter: current jump count. </summary>
        public UnityEvent<int> onJump;
        /// <summary> Fired when running starts. </summary>
        public UnityEvent onRunStart;
        /// <summary> Fired when running stops. </summary>
        public UnityEvent onRunStop;
        /// <summary> Fired when endurance is fully depleted. </summary>
        public UnityEvent onEnduranceDepleted;
        /// <summary> Fired when the character starts crouching. </summary>
        public UnityEvent onCrouchStart;
        /// <summary> Fired when the character stops crouching. </summary>
        public UnityEvent onCrouchStop;

        public bool isGeneratePhysicMaterial = true;
        public float mass = 70.0f;
        public float drag = 3.0f;
        public float airDrag = 0.5f;
	    [Tooltip ("For Initialize()")] public float height = 1.6f;
        public float crouchHeight = 0.8f;
        [Range(0f, 1f)] public float crouchSpeedReduction = 0.5f;
        public CrouchMode crouchMode = CrouchMode.Hold;
        public float crouchTransitionSpeed = 10.0f;

        public LayerMask groundMask = 1;
        [Tooltip("Maximum walkable slope angle in degrees.")]
        public float slopeLimit = 45f;
        [Tooltip("Gravity slide force multiplier on steep slopes. 0 = disable sliding.")]
        public float slopeSlideForceMul = 1.0f;
        [Tooltip("Speed reduction going uphill on walkable slopes. 0 = none, 1 = full at slopeLimit.")]
        [Range(0f, 1f)] public float uphillSpeedReduction = 0.3f;
        [Tooltip("Speed boost going downhill on walkable slopes. 0 = none, 1 = full at slopeLimit.")]
        [Range(0f, 1f)] public float downhillSpeedBoost = 0.15f;
        public LayerMask ceilingMask = 1;
        private bool isGrounded;

        public Rigidbody rb;
        public CapsuleCollider cc;

        private float epsilon = 0.01f;

        // --- Push system for jointed dynamic objects (hinge doors, etc.) ---
        private Rigidbody _pushTarget;
        private Vector3 _pushDirection;
        private float _pushStrength;
        private Vector3 _pushTargetPrevPos;
        private float _pushTargetMoveDistance = float.MaxValue;
        private bool _pushTargetContactThisFrame;
        private const float PushStuckThreshold = 0.001f;
        private const float PushCapsDistance = 0.1f;

        /// <summary>
        /// Initialize the movement. Generate physic material if needed. Prepare the rigidbody.
        /// </summary>
        public virtual void Initialize () {
            if (!rb) {
                Debug.LogWarning("Movement: Rigidbody is not assigned.");
                return;
            }
            if (!cc) {
                Debug.LogWarning("Movement: CapsuleCollider is not assigned.");
                return;
            }
            rb.freezeRotation = true;
            rb.isKinematic = true;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rb.mass = mass;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            cc.height = height;
            initialColliderCenter = cc.center;
            crouchMultiplier = 1f;
            targetColliderHeight = height;
            currentColliderHeight = height;
            currentAcceleration = referenceAcceleration;
            endurance = referenceEndurance;
            if (isGeneratePhysicMaterial) {
                PhysicsMaterial physicMaterial = new PhysicsMaterial {
                    name = "Generated Material",
                    bounciness = 0.01f,
                    dynamicFriction = 0.0f,
                    staticFriction = 0.0f,
                    frictionCombine = PhysicsMaterialCombine.Minimum,
                    bounceCombine = PhysicsMaterialCombine.Minimum
                };
                cc.material = physicMaterial;
            }
        }

        /// <summary>
        /// Allow the user to move.
        /// </summary>
        public virtual void AllowMovement () {
            isMovementAvailable = true;
            if (isDebugLog && rb) Debug.Log (rb.gameObject.name + ": Allow Movement");
        }

        /// <summary>
        /// Ban the user to move. Optional, immediately stop the rigidbody.
        /// </summary>
        public virtual void BanMovement (bool isStopImmediately = false) {
            isMovementAvailable = false;
            if (isDebugLog && rb) Debug.Log (rb.gameObject.name + ": Ban Movement");
            if (isStopImmediately) {
                _moveVelocity = Vector3.zero;
                if (isDebugLog && rb) Debug.Log (rb.gameObject.name + ": Stop Movement");
            }
        }

        /// <summary>
        /// Allow the user to move faster.
        /// </summary>
        public virtual void AllowRunning () {
            isRunningAvailable = true;
            if (isDebugLog && rb) Debug.Log (rb.gameObject.name + ": Allow Running");
        }

        /// <summary>
        /// Ban the user to move faster.
        /// </summary>
        public virtual void BanRunning () {
            isRunningAvailable = false;
            if (isDebugLog && rb) Debug.Log (rb.gameObject.name + ": Ban Running");
        }

        /// <summary>
        /// Allow the user to jump up.
        /// </summary>
        public virtual void AllowJumping () {
            isJumpingAvailable = true;
            if (isDebugLog && rb) Debug.Log (rb.gameObject.name + ": Allow Jumping");
        }

        /// <summary>
        /// Ban the user from jumping up.
        /// </summary>
        public virtual void BanJumping () {
            isJumpingAvailable = false;
            if (isDebugLog && rb) Debug.Log (rb.gameObject.name + ": Ban Jumping");
        }

        /// <summary>
        /// Restore air control to the configured airControlFactor value.
        /// </summary>
        public virtual void AllowAirControl () {
            airControlFactor = savedAirControlFactor;
            if (isDebugLog && rb) Debug.Log (rb.gameObject.name + ": Allow Air Control");
        }

        /// <summary>
        /// Disable air control entirely (sets factor to 0). Call AllowAirControl() to restore.
        /// </summary>
        public virtual void BanAirControl () {
            savedAirControlFactor = airControlFactor;
            airControlFactor = 0f;
            if (isDebugLog && rb) Debug.Log (rb.gameObject.name + ": Ban Air Control");
        }

        /// <summary>
        /// Allow the user to crouch.
        /// </summary>
        public virtual void AllowCrouching () {
            isCrouchingAvailable = true;
            if (isDebugLog && rb) Debug.Log (rb.gameObject.name + ": Allow Crouching");
        }

        /// <summary>
        /// Ban the user from crouching. Stands the character up immediately if crouching.
        /// </summary>
        public virtual void BanCrouching () {
            isCrouchingAvailable = false;
            if (isCrouching) StandUp();
            if (isDebugLog && rb) Debug.Log (rb.gameObject.name + ": Ban Crouching");
        }

        /// <summary> Add external velocity (e.g. from moving platforms, step climb). </summary>
        public void AddVelocity (Vector3 velocity) => _moveVelocity += velocity;

        /// <summary> Set vertical velocity (overwrites y). Used by step climb. </summary>
        public void SetVerticalVelocity (float y) => _moveVelocity.y = y;

        /// <summary> Current accumulated velocity. </summary>
        public Vector3 GetVelocity () => _moveVelocity;

        /// <summary>
        /// Current endurance value.
        /// </summary>
        public float GetEnduranceValue () {
            return endurance;
        }

        /// <summary>
        /// Is this controller on the ground?
        /// </summary>
        public bool IsGrounded () {
            return isGrounded;
        }

        /// <summary>
        /// Is this controller currently crouching?
        /// </summary>
        public bool IsCrouching () {
            return isCrouching;
        }

        /// <summary>
        /// Is this controller airborne (not grounded)?
        /// </summary>
        public bool IsAirborne () {
            return !isGrounded;
        }

        /// <summary>
        /// Whether a ground hit was detected this frame.
        /// </summary>
        public bool HasGroundHit () {
            return hasGroundHit;
        }

        /// <summary>
        /// The RaycastHit from ground detection. Only valid when HasGroundHit() is true.
        /// </summary>
        public RaycastHit GetGroundHit () {
            return groundHit;
        }

        /// <summary>
        /// The collider the character is standing on. Null if airborne.
        /// </summary>
        public Collider GetGroundCollider () {
            return hasGroundHit ? groundHit.collider : null;
        }

        /// <summary>
        /// The transform the character is standing on. Null if airborne.
        /// </summary>
        public Transform GetGroundTransform () {
            return hasGroundHit ? groundHit.transform : null;
        }

        /// <summary>
        /// True when grounded on a surface steeper than slopeLimit.
        /// </summary>
        public bool IsOnSteepSlope () => isOnSteepSlope;

        /// <summary>
        /// True when grounded on a walkable surface (not steep).
        /// </summary>
        public bool IsOnStableGround () => isGrounded && !isOnSteepSlope;

        /// <summary>
        /// Slope angle in degrees of the surface under the character. 0 when airborne.
        /// </summary>
        public float GetGroundSlopeAngle () => groundSlopeAngle;

        /// <summary>
        /// Normal of the ground surface. Vector3.up when airborne.
        /// </summary>
        public Vector3 GetGroundNormal () => groundNormal;

        /// <summary>
        /// Crouch state. Call in Update.
        /// </summary>
        public virtual void Crouching () {
            if (!cc) return;
            if (!isCrouchingAvailable) return;
            if (crouchHeight >= height) return;
            if (crouchMode == CrouchMode.Hold) {
                if (crouchingInputValue && !isCrouching && isGrounded) {
                    EnterCrouch();
                }
                else if (!crouchingInputValue && isCrouching) {
                    if (!CanStandUp()) return;
                    ExitCrouch();
                }
            }
            else {
                if (crouchingInputPressed) {
                    if (isCrouching) {
                        if (!CanStandUp()) return;
                        ExitCrouch();
                    }
                    else if (isGrounded) {
                        EnterCrouch();
                    }
                }
            }
        }

        /// <summary>
        /// Immediately restores the character to standing collider. Safe to call externally (e.g. on death).
        /// </summary>
        public void StandUp () {
            if (!cc) return;
            bool wasCrouching = isCrouching;
            isCrouching = false;
            targetColliderHeight = height;
            currentColliderHeight = height;
            cc.height = height;
            cc.center = initialColliderCenter;
            crouchMultiplier = 1f;
            if (wasCrouching) onCrouchStop?.Invoke();
        }

        private void EnterCrouch () {
            isCrouching = true;
            targetColliderHeight = crouchHeight;
            crouchMultiplier = 1f - crouchSpeedReduction;
            onCrouchStart?.Invoke();
        }

        private void ExitCrouch () {
            isCrouching = false;
            targetColliderHeight = height;
            crouchMultiplier = 1f;
            onCrouchStop?.Invoke();
        }

        private bool CanStandUp () {
            Vector3 topCenter = cc.transform.position + initialColliderCenter + Vector3.up * (height / 2f - cc.radius);
            return !Physics.CheckSphere(topCenter, cc.radius, ceilingMask, QueryTriggerInteraction.Ignore);
        }

        /// <summary>
        /// Physical movement and jumping. Use in FixedUpdate.
        /// Kinematic body + MovePosition for proper collisions with other dynamic Rigidbodies.
        /// Manual CapsuleCast handles wall sliding.
        /// </summary>
        public virtual void Accelerate () {
            if (!rb || !cc) return;

            // --- Pre-move: check if last frame's push target (jointed object) actually moved ---
            if (_pushTarget != null) {
                _pushTargetMoveDistance = Vector3.Distance(_pushTarget.position, _pushTargetPrevPos);
            } else {
                _pushTargetMoveDistance = float.MaxValue;
            }
            _pushTargetContactThisFrame = false;

            LookingForGround ();
            UpdateCrouchCollider ();
            MoveTorwardsAcceleration ();

            // Gravity — manual accumulation for kinematic body
            if (!isGrounded) {
                _moveVelocity.y += Physics.gravity.y * Time.fixedDeltaTime;
            }

            // Slope sliding
            if (isOnSteepSlope && isGrounded) {
                Vector3 slopeForce = Vector3.ProjectOnPlane(Physics.gravity, groundNormal) * slopeSlideForceMul;
                _moveVelocity += slopeForce * Time.fixedDeltaTime;
            }

            // Horizontal movement
            if (isMovementAvailable) {
                bool hasInput = Mathf.Abs(movementInputValues.x) > epsilon || Mathf.Abs(movementInputValues.y) > epsilon;
                if (hasInput) {
                    float controlScale = isGrounded ? 1f : airControlFactor;
                    if (controlScale >= epsilon) {
                        Vector3 targetVel = new Vector3 (movementInputValues.x, 0, movementInputValues.y);
                        targetVel = Vector3.ClampMagnitude (targetVel, 1);
                        targetVel = rb.transform.TransformDirection (targetVel) * currentAcceleration * crouchMultiplier * controlScale;

                        if (isGrounded && hasGroundHit) {
                            if (isOnSteepSlope) {
                                Vector3 downhill = Vector3.ProjectOnPlane(Vector3.down, groundNormal).normalized;
                                float uphillComponent = Vector3.Dot(targetVel, -downhill);
                                if (uphillComponent > 0f)
                                    targetVel -= (-downhill) * uphillComponent;
                            } else if (groundSlopeAngle > epsilon) {
                                Vector3 downhill = Vector3.ProjectOnPlane(Vector3.down, groundNormal).normalized;
                                float alignment = Vector3.Dot(targetVel.normalized, downhill);
                                float slopeRatio = groundSlopeAngle / slopeLimit;
                                float speedMul = alignment < 0f
                                    ? 1f - (uphillSpeedReduction * slopeRatio * -alignment)
                                    : 1f + (downhillSpeedBoost * slopeRatio * alignment);
                                targetVel *= speedMul;
                            }
                        }

                        _moveVelocity.x = Mathf.Lerp (_moveVelocity.x, targetVel.x, startingSharpness * Time.fixedDeltaTime);
                        _moveVelocity.z = Mathf.Lerp (_moveVelocity.z, targetVel.z, startingSharpness * Time.fixedDeltaTime);
                    }
                } else {
                    // No input — decelerate to stop
                    if (isGrounded && !isOnSteepSlope) {
                        _moveVelocity.x = Mathf.Lerp (_moveVelocity.x, 0, stoppingSharpness * Time.fixedDeltaTime);
                        _moveVelocity.z = Mathf.Lerp (_moveVelocity.z, 0, stoppingSharpness * Time.fixedDeltaTime);
                    }
                }
            }

            // Jump — overrides vertical velocity
            ProcessJump ();

            // --- Manual collision handling ---
            Vector3 delta = _moveVelocity * Time.fixedDeltaTime;
            BuildCapsulePoints (out Vector3 capBottom, out Vector3 capTop);

            // Horizontal capsule sweep: static walls → slide along surface;
            // dynamic rigidbodies → let the kinematic body push them through physics.
            Vector3 hDelta = new Vector3 (delta.x, 0, delta.z);
            float hDist = hDelta.magnitude;

            if (hDist > epsilon) {
                Vector3 hDir = hDelta / hDist;
                if (Physics.CapsuleCast (
                        capBottom, capTop, cc.radius,
                        hDir, out RaycastHit wallHit, hDist,
                        groundMask, QueryTriggerInteraction.Ignore
                    )) {
                    Rigidbody hitRb = wallHit.rigidbody;
                    bool isStatic = hitRb == null || hitRb.isKinematic;

                    if (isStatic) {
                        // Static/kinematic wall — slide along surface
                        Vector3 slideDir = Vector3.ProjectOnPlane (hDir, wallHit.normal).normalized;
                        float slideDist = hDist * Mathf.Max (0f, Vector3.Dot (hDir, slideDir));
                        Vector3 remainingH = slideDir * slideDist;
                        delta = new Vector3 (remainingH.x, delta.y, remainingH.z);
                    } else {
                        // Dynamic rigidbody
                        bool isJointDynamic = hitRb.GetComponent<Joint> () != null;

                        if (isJointDynamic) {
                            // Jointed object (hinge door, etc.) — push via AddForce
                            bool firstContact = _pushTarget != hitRb;
                            if (firstContact) {
                                _pushTarget = hitRb;
                                _pushTargetMoveDistance = float.MaxValue;
                            }
                            _pushTargetPrevPos = hitRb.position;
                            _pushTargetContactThisFrame = true;
                            _pushDirection = hDir;
                            _pushStrength = Mathf.Clamp (_moveVelocity.magnitude * 0.5f, 3f, 15f);
                            hitRb.AddForce (hDir * _pushStrength, ForceMode.Impulse);

                            // Dampen player velocity if door is at joint limit (stuck)
                            bool stuck = _pushTargetMoveDistance < PushStuckThreshold;
                            bool veryClose = wallHit.distance < PushCapsDistance;
                            if (stuck || (firstContact && veryClose)) {
                                float capFactor = Mathf.Lerp (0.1f, 0.6f, Mathf.Min (wallHit.distance / PushCapsDistance, 1f));
                                delta *= capFactor;
                                _moveVelocity.x *= 0.5f;
                                _moveVelocity.z *= 0.5f;
                            }
                        }
                        // Jointless dynamic: pass through — MovePosition pushes naturally
                    }
                }
            }

            // Vertical capsule sweep: falling → stop on ground, jumping → stop on ceiling
            if (Mathf.Abs (delta.y) > epsilon) {
                Vector3 vDelta = new Vector3 (0, delta.y, 0);
                Vector3 vDir = vDelta.normalized;
                float vDist = Mathf.Abs (delta.y);

                // Shift sweep origin slightly in the movement direction so we don't
                // self-intersect with the ground we're already standing on.
                Vector3 sweepOriginBottom = capBottom + vDir * cc.radius * 0.5f;
                Vector3 sweepOriginTop    = capTop    + vDir * cc.radius * 0.5f;

                if (Physics.CapsuleCast (
                        sweepOriginBottom, sweepOriginTop, cc.radius,
                        vDir, out RaycastHit vertHit, vDist + cc.radius * 0.5f,
                        groundMask, QueryTriggerInteraction.Ignore
                    )) {
                    Rigidbody vertRb = vertHit.rigidbody;
                    bool vertStatic = vertRb == null || vertRb.isKinematic;

                    float safeDist = Mathf.Max (0f, vertHit.distance - cc.radius * 0.5f - 0.005f);
                    if (vDelta.y < 0f) {
                        // Falling → land on surface (any: static, dynamic, jointed)
                        delta.y = -safeDist;
                        if (!isGrounded) {
                            isGrounded = true;
                            jumpCount = 0;
                            _moveVelocity.y = 0;
                            onLanded?.Invoke ();
                        }
                    } else if (vertStatic) {
                        // Jumping → hit static/kinematic ceiling
                        if (safeDist < vDist) {
                            delta.y = safeDist;
                            _moveVelocity.y = 0;
                        }
                    }
                    // Dynamic above (jointed or jointless): pass through — MovePosition pushes upward
                }
            }

            // Apply movement
            rb.linearVelocity = _moveVelocity;   // for external readers (Hero speed FOV, editor)
            rb.MovePosition (rb.position + delta);

            // --- Cleanup: clear push target if no longer in contact ---
            if (!_pushTargetContactThisFrame) {
                _pushTarget = null;
                _pushTargetPrevPos = Vector3.zero;
                _pushTargetMoveDistance = float.MaxValue;
            }
        }

        /// <summary>
        /// Buffer jump input. Call in Update to avoid missing input between FixedUpdate ticks.
        /// </summary>
	    public virtual void Jumping () {
		    if (jumpingInputValue) {
                jumpBuffered = true;
            }
	    }

        /// <summary>
        /// Deduct endurance and fire depletion event if needed.
        /// </summary>
        public void ConsumeEndurance (float amount) {
            endurance = Mathf.Max(0f, endurance - amount);
            if (endurance <= 0.05f) {
                depletionTimer = depletionRecoveryDelay;
                onEnduranceDepleted?.Invoke();
            }
        }

        private void ProcessJump () {
            if (!isJumpingAvailable) return;
            if (isOnSteepSlope) { jumpBuffered = false; return; }
            if (!jumpBuffered) return;
            if (jumpCount < maxJumpCount && endurance > jumpEnduranceCost) {
                _moveVelocity.y = jumpForce;
                ConsumeEndurance(jumpEnduranceCost);
                jumpCount++;
                onJump?.Invoke(jumpCount);
            }
            jumpBuffered = false;
        }

        /// <summary>
        /// Running state. Better use it in Update.
        /// </summary>
	    public virtual void Running () {
		    suppressEnduranceRecovery = false;
		    if (!isRunningAvailable) return;
		    if (isGrounded && runningInputValue && endurance > 0.05f && !isCrouching) {
                releaseAcceleration = false;
			    endurance = Mathf.Max(0f, endurance - Time.deltaTime * enduranceDrainRate);
			    currentAcceleration = Mathf.MoveTowards (currentAcceleration, runningAcceleration, Time.deltaTime * 10);
                if (!wasRunning) {
                    wasRunning = true;
                    onRunStart?.Invoke();
                }
                if (endurance <= 0.05f) {
                    onEnduranceDepleted?.Invoke();
                }
		    }
		    else {
                releaseAcceleration = true;
                if (wasRunning) {
                    wasRunning = false;
                    onRunStop?.Invoke();
                }
			    if (depletionTimer > 0f) depletionTimer -= Time.deltaTime;
                if (!suppressEnduranceRecovery && depletionTimer <= 0f && Mathf.Abs(endurance - referenceEndurance) > epsilon) {
                    endurance = Mathf.MoveTowards (endurance, referenceEndurance, Time.deltaTime * enduranceRecoveryRate);
                }
		    }
	    }

        private void LookingForGround () {
            switch (groundDetectionMode) {
                case GroundDetectionMode.MultiRaycast:
                    LookingForGroundMultiRaycast();
                    break;
                case GroundDetectionMode.SphereCast:
                    LookingForGroundSphereCast();
                    break;
                default:
                    LookingForGroundRaycast();
                    break;
            }
        }

        private void LookingForGroundRaycast () {
            groundCheckPosition = cc.bounds.center;
            float rayDistance = Mathf.Max(cc.height * 0.5f, cc.radius) + groundCheckSkinWidth;
            if (Physics.Raycast (groundCheckPosition, Vector3.down, out groundHit, rayDistance, groundMask, QueryTriggerInteraction.Ignore)) {
                SetGrounded(true);
            }
            else {
                SetGrounded(false);
            }
        }

        private void LookingForGroundMultiRaycast () {
            groundCheckPosition = cc.bounds.center;
            float rayDistance = Mathf.Max(cc.height * 0.5f, cc.radius) + groundCheckSkinWidth;
            // Center ray.
            bool found = Physics.Raycast (groundCheckPosition, Vector3.down, out groundHit, rayDistance, groundMask, QueryTriggerInteraction.Ignore);
            // Ring rays.
            if (!found) {
                float angleStep = 360f / multiRaycastRayCount;
                for (int i = 0; i < multiRaycastRayCount; i++) {
                    float angle = i * angleStep * Mathf.Deg2Rad;
                    Vector3 offset = new Vector3 (Mathf.Cos(angle) * groundCheckRadius, 0f, Mathf.Sin(angle) * groundCheckRadius);
                    if (Physics.Raycast (groundCheckPosition + offset, Vector3.down, out groundHit, rayDistance, groundMask, QueryTriggerInteraction.Ignore)) {
                        found = true;
                        break;
                    }
                }
            }
            if (found) {
                SetGrounded(true);
            }
            else {
                SetGrounded(false);
            }
        }

        private void LookingForGroundSphereCast () {
            groundCheckPosition = cc.bounds.center;
            float castDistance = Mathf.Max(cc.height * 0.5f, cc.radius) - groundCheckRadius + groundCheckSkinWidth;
            if (Physics.SphereCast (groundCheckPosition, groundCheckRadius, Vector3.down, out groundHit, castDistance, groundMask, QueryTriggerInteraction.Ignore)) {
                SetGrounded(true);
            }
            else {
                SetGrounded(false);
            }
        }

        private void SetGrounded (bool detected) {
            if (detected) {
                hasGroundHit = true;
                groundNormal = groundHit.normal;
                groundSlopeAngle = Vector3.Angle(Vector3.up, groundNormal);
                isOnSteepSlope = groundSlopeAngle > slopeLimit;
                if (!isGrounded) {
                    isGrounded = true;
                    jumpCount = 0;
                    _moveVelocity.y = 0;
                    onLanded?.Invoke();
                }
            }
            else {
                hasGroundHit = false;
                groundNormal = Vector3.up;
                groundSlopeAngle = 0f;
                isOnSteepSlope = false;
                if (isGrounded) {
                    isGrounded = false;
                    onAirborne?.Invoke();
                }
            }
        }

        /// <summary> Build capsule bottom/top points matching the current collider shape. </summary>
        private void BuildCapsulePoints (out Vector3 bottom, out Vector3 top) {
            Vector3 center = rb.position + cc.center;
            float sphereOffset = Mathf.Max (cc.height * 0.5f, cc.radius) - cc.radius;
            bottom = center - Vector3.up * sphereOffset;
            top    = center + Vector3.up * sphereOffset;
        }

        private void UpdateCrouchCollider () {
            if (Mathf.Abs(currentColliderHeight - targetColliderHeight) <= epsilon) return;
            currentColliderHeight = Mathf.MoveTowards(currentColliderHeight, targetColliderHeight, crouchTransitionSpeed * Time.fixedDeltaTime);
            // Derive center from effective half-height to keep the physics capsule bottom pinned.
            // Unity clamps the physics shape to max(height/2, radius), so cc.height/2 alone is wrong
            // when crouchHeight <= 2*radius (capsule becomes a sphere).
            float standingHalf = Mathf.Max(height * 0.5f, cc.radius);
            float currentHalf = Mathf.Max(currentColliderHeight * 0.5f, cc.radius);
            float bottomY = initialColliderCenter.y - standingHalf;
            cc.height = currentColliderHeight;
            cc.center = new Vector3(initialColliderCenter.x, bottomY + currentHalf, initialColliderCenter.z);
        }

        /// <summary>
        /// Draw ground-check gizmos. Call from Hero's OnDrawGizmos. Green ray = grounded, red = airborne.
        /// Yellow sphere = hit point, cyan sphere = capsule bottom (origin - height/2).
        /// </summary>
        public void DrawGroundCheckGizmos () {
            if (!cc) return;
            Vector3 origin = cc.bounds.center;
            float effectiveHalf = Mathf.Max(cc.height * 0.5f, cc.radius);
            float rayDist = groundDetectionMode == GroundDetectionMode.SphereCast
                ? effectiveHalf - groundCheckRadius + groundCheckSkinWidth
                : effectiveHalf + groundCheckSkinWidth;
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawLine(origin, origin + Vector3.down * rayDist);
            Gizmos.DrawWireSphere(origin, 0.04f);
            if (groundDetectionMode == GroundDetectionMode.MultiRaycast) {
                float step = 360f / multiRaycastRayCount;
                for (int i = 0; i < multiRaycastRayCount; i++) {
                    float a = i * step * Mathf.Deg2Rad;
                    Vector3 off = new Vector3(Mathf.Cos(a) * groundCheckRadius, 0f, Mathf.Sin(a) * groundCheckRadius);
                    Gizmos.DrawLine(origin + off, origin + off + Vector3.down * rayDist);
                }
            }
            else if (groundDetectionMode == GroundDetectionMode.SphereCast) {
                Gizmos.DrawWireSphere(origin + Vector3.down * rayDist, groundCheckRadius);
            }
            Gizmos.color = Color.cyan;
            Vector3 capsuleBottom = origin + Vector3.down * effectiveHalf;
            Gizmos.DrawWireSphere(capsuleBottom, 0.04f);
            // Unbounded probe to find the real ground under the origin (ignores rayDistance limit).
            if (Physics.Raycast(origin, Vector3.down, out RaycastHit probe, 10f, groundMask, QueryTriggerInteraction.Ignore)) {
                Gizmos.color = Color.white;
                Gizmos.DrawWireSphere(probe.point, 0.05f);
                Gizmos.DrawLine(capsuleBottom, probe.point);
            }
            if (hasGroundHit) {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(groundHit.point, 0.06f);
                Gizmos.DrawRay(groundHit.point, groundHit.normal * 0.3f);
            }
        }

        private void MoveTorwardsAcceleration () {
            if (!releaseAcceleration) return;
            if (Mathf.Abs(currentAcceleration - referenceAcceleration) > epsilon) {
                currentAcceleration = Mathf.MoveTowards (currentAcceleration, referenceAcceleration, Time.deltaTime * 10);
            }
        }
    }
}
