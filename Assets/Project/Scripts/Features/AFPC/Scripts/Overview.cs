using UnityEngine;
using UnityEngine.Events;

namespace AFPC {

    /// <summary>
    /// This class allows the user to look around and perform some POV effects.
    /// </summary>
    [System.Serializable]
    public class Overview {

        /// <summary> Fired when aiming starts (right mouse down). </summary>
        public UnityEvent onAimStart;
        /// <summary> Fired when aiming stops (right mouse up). </summary>
        public UnityEvent onAimStop;
        /// <summary> Fired when the camera is shaken. Parameter: shake amount. </summary>
        public UnityEvent<float> onShake;

        public bool isDebugLog;

        [HideInInspector] public Vector2 lookingInputValues;
        [HideInInspector] public bool lookingIsMouseDelta;
        [HideInInspector] public bool aimingInputValue;

        public bool isFollowingInstant = true;
        public float damping = 10.0f;
        public Vector3 cameraOffset = new Vector3(0,0.8f,0);

        public bool isHorizontalInverted;
	    public bool isVerticalInverted = true;
	    private bool isLookingAvailable = true;
	    public float sensitivity = 4.0f;
	    public float horizontalRange = 0.0f;
	    public float verticalRange = 50.0f;
        private Vector3 targetRotation;

	    public LayerMask searchMask = 0;
        public float searchDistance = 1;

        public float defaultFOV = 80.0f;
	    public float aimingFOV = 40.0f;
        public float aimingFOVSmoothing = 5.0f;
	    private bool isAimingAvailable = true;
        private bool wasAiming;

        public bool isSpeedFOV;
        public float speedFOVMax = 100.0f;
        public float speedFOVSmoothing = 5.0f;
        [Tooltip ("Speed below this threshold won't affect FOV")] public float speedFOVThreshold = 0.8f;
        [Range(0f, 1f)] public float speedFOVGlobalWeight = 1f;
        [Range(0f, 1f)] public float speedFOVWeightForward = 1f;
        [Range(0f, 1f)] public float speedFOVWeightStrafe = 0f;
        [Range(0f, 1f)] public float speedFOVWeightVertical = 0f;
        [HideInInspector] public float speedInput;
        [HideInInspector] public bool isGrounded;

        public bool isHeadBobbing;
        public float bobbingSpeed = 10.0f;
        public float bobbingAmount = 0.05f;
        public float bobbingRunMultiplier = 1.5f;
        private float bobbingTimer;
        private float bobbingDefaultY;
        private bool bobbingInitialized;

        public float crouchCameraOffset = -0.4f;
        public float crouchTransitionSpeed = 10.0f;
        private float targetCrouchOffset;
        private float currentCrouchOffset;
        private float cameraVelocityY;

        private const float Epsilon = 0.001f;

        public float referenceShakingAmount;
        [Tooltip("Global multiplier applied to every Shake() call. 1 = normal, 0 = no shake, 2 = double intensity.")]
        public float shakeAmplitude = 1f;
        [Tooltip("How fast shakingAmount decays toward referenceShakingAmount per second. Higher = shorter shakes.")]
        public float shakeDecayRate = 1f;
	    private bool isShakingAvailable = true;
	    private float shakingAmount;

        public Camera camera;

        [Tooltip("Hide and lock the cursor when the game starts.")]
        public bool hideCursor = true;

        /// <summary>
        /// Initialize the overview. Sync target rotation and FOV with the current camera state.
        /// </summary>
        public virtual void Initialize () {
            if (!camera) {
                Debug.LogWarning("Overview: Camera is not assigned.");
                return;
            }
            targetRotation = camera.transform.eulerAngles;
            // eulerAngles returns X in [0,360). Normalize to signed range so
            // vertical clamping works correctly from the very first Looking() call.
            if (targetRotation.x > 180f) targetRotation.x -= 360f;
            camera.transform.rotation = Quaternion.Euler(targetRotation);
            camera.fieldOfView = defaultFOV;
        }

        /// <summary>
        /// Hide and lock the cursor (e.g. during gameplay).
        /// </summary>
        public void HideCursor () {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        /// <summary>
        /// Show and unlock the cursor (e.g. for menus or pause screens).
        /// </summary>
        public void ShowCursor () {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        /// <summary>
        /// Allow the controller to read looking input values and rotate the camera.
        /// </summary>
        public virtual void AllowLooking () {
            isLookingAvailable = true;
            if (isDebugLog && camera) Debug.Log (camera.gameObject.name + ": Allow Looking");
        }

        /// <summary>
        /// Ban controller to read looking input values and rotate camera.
        /// </summary>
        public virtual void BanLooking () {
            isLookingAvailable = false;
            if (isDebugLog && camera) Debug.Log (camera.gameObject.name + ": Ban Looking");
        }

        /// <summary>
        /// Allow the user to change camera FOV to view far objects.
        /// </summary>
        public virtual void AllowAiming () {
            isAimingAvailable = true;
            if (isDebugLog && camera) Debug.Log (camera.gameObject.name + ": Allow Aiming");
        }

        /// <summary>
        /// Ban the user to change camera FOV to view far objects.
        /// </summary>
        public virtual void BanAiming () {
            isAimingAvailable = false;
            if (isDebugLog && camera) Debug.Log (camera.gameObject.name + ": Ban Aiming");
        }

        /// <summary>
        /// Allow camera shaking by lens shifting. Required "Physical camera" mode on.
        /// </summary>
        public virtual void AllowShaking () {
            isShakingAvailable = true;
            if (isDebugLog && camera) Debug.Log (camera.gameObject.name + ": Allow Shaking");
        }

        /// <summary>
        /// Ban camera shaking by lens shifting.
        /// </summary>
        public virtual void BanShaking () {
            isShakingAvailable = false;
            if (isDebugLog && camera) Debug.Log (camera.gameObject.name + ": Ban Shaking");
        }

        /// <summary>
        /// Notify the camera to shift down or restore to standing eye height.
        /// </summary>
        public void SetCrouching (bool crouching) {
            targetCrouchOffset = crouching ? crouchCameraOffset : 0f;
        }

        /// <summary>
        /// Follow the camera to the controller with offset.
        /// </summary>
        public void Follow (Vector3 target) {
            if (!camera) return;
            if (!bobbingInitialized) {
                bobbingDefaultY = cameraOffset.y;
                bobbingInitialized = true;
            }
            Vector3 position = target + cameraOffset;
            if (isHeadBobbing && isGrounded && speedInput > 0.1f) {
                float multiplier = speedInput > 0.7f ? bobbingRunMultiplier : 1.0f;
                bobbingTimer += Time.deltaTime * bobbingSpeed * multiplier;
                position.y += Mathf.Sin (bobbingTimer) * bobbingAmount * multiplier;
            }
            else {
                bobbingTimer = 0;
                position.y = target.y + bobbingDefaultY;
            }
            float smoothTime = crouchTransitionSpeed > 0f ? 1f / crouchTransitionSpeed : 0.001f;
            currentCrouchOffset = Mathf.SmoothDamp (currentCrouchOffset, targetCrouchOffset, ref cameraVelocityY, smoothTime);
            position.y += currentCrouchOffset;
            if (isFollowingInstant) {
                camera.transform.position = position;
            }
            else {
                camera.transform.position = Vector3.Lerp (camera.transform.position, position, damping * Time.deltaTime);
            }
        }

        /// <summary>
        /// Rotate the camera with looking input values.
        /// </summary>
        public virtual void Looking () {
		    if (!camera) return;
		    if (!isLookingAvailable) return;
            if (Mathf.Abs(lookingInputValues.x) < Epsilon && Mathf.Abs(lookingInputValues.y) < Epsilon) return;
		    if (isHorizontalInverted) lookingInputValues.x *= -1;
		    if (isVerticalInverted) lookingInputValues.y *= -1;

            float scale = lookingIsMouseDelta ? 0.1f : Time.deltaTime;
            targetRotation.x += lookingInputValues.y * sensitivity * scale;
            targetRotation.y += lookingInputValues.x * sensitivity * scale;

            if (Mathf.Abs(horizontalRange) > Epsilon) {
			    targetRotation.y = Mathf.Clamp (targetRotation.y, -horizontalRange, horizontalRange);
		    } else {
                targetRotation.y = Mathf.Repeat (targetRotation.y, 360);
            }
		    if (Mathf.Abs(verticalRange) > Epsilon) {
			    targetRotation.x = Mathf.Clamp (targetRotation.x, -verticalRange, verticalRange);
		    }
		    camera.transform.rotation = Quaternion.Euler (targetRotation);
	    }

        /// <summary>
        /// Changing the camera FOV value or return to the default FOV value.
        /// </summary>
        public virtual void Aiming () {
            if (!isAimingAvailable) return;
            if (!camera) return;
            if (aimingInputValue && !wasAiming) {
                wasAiming = true;
                onAimStart?.Invoke();
            }
            else if (!aimingInputValue && wasAiming) {
                wasAiming = false;
                onAimStop?.Invoke();
            }
            float targetFOV = defaultFOV;
		    if (aimingInputValue) {
                targetFOV = aimingFOV;
		    }
		    else if (isSpeedFOV && speedInput > speedFOVThreshold) {
                float t = (speedInput - speedFOVThreshold) / (1.0f - speedFOVThreshold);
                targetFOV = Mathf.Lerp (defaultFOV, speedFOVMax, t);
            }
            if (Mathf.Abs(camera.fieldOfView - targetFOV) > Epsilon) {
                float smoothing = camera.fieldOfView < defaultFOV || aimingInputValue ? aimingFOVSmoothing : speedFOVSmoothing;
                camera.fieldOfView = Mathf.MoveTowards (camera.fieldOfView, targetFOV, smoothing * Time.deltaTime * 60);
            }
	    }

        /// <summary>
        /// Raycast in the forward direction to search some objects.
        /// </summary>
        public GameObject Search () {
            if (!camera) return null;
            if (Physics.Raycast(camera.transform.position + (camera.transform.forward * 0.5f), camera.transform.forward, out RaycastHit hit, searchDistance, searchMask)) {
                if (isDebugLog) Debug.Log ("GameObject found: " + hit.collider.gameObject.name);
                return hit.collider.gameObject;
            }
            return null;
	    }

        /// <summary>
        /// Control the camera lens shift values.
        /// </summary>
	    public virtual void Shaking () {
		    if (!isShakingAvailable) return;
		    if (!camera) return;
		    if (Mathf.Abs(shakingAmount - referenceShakingAmount) > Epsilon) {
			    shakingAmount = Mathf.MoveTowards (shakingAmount, referenceShakingAmount, shakeDecayRate * Time.deltaTime);
		    }
		    if (Mathf.Abs(shakingAmount) > Epsilon) {
			    camera.lensShift = Vector2.Lerp (Vector2.zero, AddRandomSphereVector (camera.lensShift, shakingAmount), Time.deltaTime);
		    }
	    }

        /// <summary>
        /// Shake the camera lens with value.
        /// </summary>
        public virtual void Shake (float value) {
            shakingAmount = value * shakeAmplitude;
            onShake?.Invoke(value);
            if (isDebugLog && camera) Debug.Log (camera.gameObject.name + ": Shake camera with: " + value + " value.");
        }

        private Vector3 AddRandomSphereVector (Vector3 position, float amount) {
	        return position += UnityEngine.Random.insideUnitSphere * amount;
	    }

        /// <summary>
        /// Add yaw rotation in degrees. Used by moving platforms to rotate the look direction.
        /// Applies immediately so the camera updates even without mouse input.
        /// </summary>
        public void RotateYaw (float degrees) {
            targetRotation.y += degrees;
            if (camera) camera.transform.rotation = Quaternion.Euler(targetRotation);
        }

        /// <summary>
        /// Rotate rigidbody to looking direction.
        /// </summary>
        public void RotateRigidbodyToLookDirection (Rigidbody rb) {
            if (!camera || !rb) return;
            rb.rotation = Quaternion.Euler (0, camera.transform.rotation.eulerAngles.y, 0);
        }
    }
}
