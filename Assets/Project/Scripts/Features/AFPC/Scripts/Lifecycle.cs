using UnityEngine;
using UnityEngine.Events;

namespace AFPC {

    /// <summary>
    /// This class contain health-damage-death cycle.
    /// </summary>
    [System.Serializable]
    public class Lifecycle {

        public bool isDebugLog;

        public string ID = "AFPC";
        private bool isAvailable = true;

        private bool isHealthRecovery = true;
	    public float referenceHealth = 100.0f;
	    private float health = 1;
	    public float healthRecoveryInterval = 1.0f;
	    private float healthRecoveryTimer;

        private bool isShieldRecovery = true;
	    public float referenceShield = 100.0f;
	    private float shield = 1;
	    public float shieldRecoveryInterval = 1.0f;
	    private float shieldRecoveryTimer;

        private bool isFrenzy;
        [SerializeField] private float frenzyThreshold = 20.0f;

        private float epsilon = 0.01f;

        /// <summary> Fired when the character takes damage. Parameter: damage amount. </summary>
        public UnityEvent<float> onDamage;
        /// <summary> Fired when the character is healed. Parameter: heal amount. </summary>
        public UnityEvent<float> onHeal;
        /// <summary> Fired when the character dies. </summary>
        public UnityEvent onDeath;
        /// <summary> Fired when the character respawns. </summary>
        public UnityEvent onRespawn;
        /// <summary> Fired when the character is activated. </summary>
        public UnityEvent onActivate;
        /// <summary> Fired when the character is deactivated. </summary>
        public UnityEvent onDeactivate;
        /// <summary> Fired when frenzy state changes. Parameter: is frenzy active. </summary>
        public UnityEvent<bool> onFrenzyChanged;
        /// <summary> Fired when health changes. Parameter: current health. </summary>
        public UnityEvent<float> onHealthChanged;
        /// <summary> Fired when shield changes. Parameter: current shield. </summary>
        public UnityEvent<float> onShieldChanged;

        /// <summary>
        /// Set maximum health and shield in the start.
        /// </summary>
        public virtual void Initialize () {
	        SetMaximumHealthAndShield();
        }

        /// <summary>
        /// Check the availability of this character.
        /// </summary>
        /// <returns></returns>
        public bool Availability() {
            return isAvailable;
        }

        /// <summary>
        /// Activate the character.
        /// </summary>
        public virtual void Activate () {
            isAvailable = true;
            onActivate?.Invoke();
            if (isDebugLog) Debug.Log (ID + ": Active.");
        }

        /// <summary>
        /// Deactivate the character.
        /// </summary>
        public virtual void Deactivate () {
            isAvailable = false;
            onDeactivate?.Invoke();
            if (isDebugLog) Debug.Log (ID + ": Not active.");
        }

        /// <summary>
        /// Restore the health and shield to the maximum.
        /// </summary>
        public virtual void SetMaximumHealthAndShield () {
            health = referenceHealth;
            shield = referenceShield;
            if (isDebugLog) Debug.Log (ID + ": Set Maximum Health and Shield.");
        }

        /// <summary>
        /// Drive the health and shield values to the 1.
        /// </summary>
        public virtual void SetMinimumHealthAndShield () {
            health = 1;
            shield = 1;
            if (isDebugLog) Debug.Log (ID + ": Set Minimum Health and Shield.");
        }

        /// <summary>
        /// Current health of the character.
        /// </summary>
        /// <returns></returns>
        public float GetHealthValue () {
            return health;
        }

        /// <summary>
        /// Set the time interval (in seconds) between each health recovery tick.
        /// </summary>
        /// <param name="value"></param>
        public void SetHealthRecoveryInterval (float value) {
            healthRecoveryInterval = value;
        }

        /// <summary>
        /// Allow this character to recover health.
        /// </summary>
        public virtual void AllowHealthRecovery () {
            isHealthRecovery = true;
            if (isDebugLog) Debug.Log (ID + ": Allow Health Recovery.");
        }

        /// <summary>
        /// Ban this character to recover health.
        /// </summary>
        public virtual void BanHealthRecovery () {
            isHealthRecovery = false;
            if (isDebugLog) Debug.Log (ID + ": Ban Health Recovery.");
        }

        /// <summary>
        /// Current shield of the character.
        /// </summary>
        /// <returns></returns>
        public float GetShieldValue () {
            return shield;
        }

        /// <summary>
        /// Set the time interval (in seconds) between each shield recovery tick.
        /// </summary>
        /// <param name="value"></param>
        public void SetShieldRecoveryInterval (float value) {
            shieldRecoveryInterval = value;
        }

        /// <summary>
        /// Allow this character to recover health.
        /// </summary>
        public virtual void AllowShieldRecovery () {
            isShieldRecovery = true;
            if (isDebugLog) Debug.Log (ID + ": Allow Shield Recovery.");
        }

        /// <summary>
        /// Ban this character to recover health.
        /// </summary>
        public virtual void BanShieldRecovery () {
            isShieldRecovery = false;
            if (isDebugLog) Debug.Log (ID + ": Ban Shield Recovery.");
        }

        /// <summary>
        /// Check the Frenzy state.
        /// The Frenzy state is used to give your users a special state when his health level is low.
        /// </summary>
        /// <returns></returns>
        public bool IsFrenzy () {
            return isFrenzy;
        }

        /// <summary>
        /// Set a minimum health threshold for the frenzy state.
        /// </summary>
        /// <param name="value"></param>
        public void SetFrenzyThreshold (float value) {
            frenzyThreshold = value;
            if (isDebugLog) Debug.Log (ID + ": Frenzy threshold is: " + value);
        }

        /// <summary>
        /// Recovering health and shield.
        /// </summary>
	    public virtual void Runtime () {
		    HealthRecovery ();
		    ShieldRecovery ();
	    }

	    private void HealthRecovery () {
		    if (!isHealthRecovery) return;
		    if (healthRecoveryInterval <= 0) return;
		    if (Mathf.Abs(health - referenceHealth) < epsilon) return;
		    healthRecoveryTimer += Time.deltaTime;
		    if (healthRecoveryTimer >= healthRecoveryInterval) {
			    healthRecoveryTimer = 0;
			    if (health < referenceHealth) {
				    health += 1;
				    CheckFrenzy ();
			    }
			    else {
				    health = referenceHealth;
			    }
			    onHealthChanged?.Invoke(health);
		    }
	    }

	    private void ShieldRecovery () {
		    if (!isShieldRecovery) return;
		    if (shieldRecoveryInterval <= 0) return;
		    if (Mathf.Abs(shield - referenceShield) < epsilon) return;
		    shieldRecoveryTimer += Time.deltaTime;
		    if (shieldRecoveryTimer >= shieldRecoveryInterval) {
			    shieldRecoveryTimer = 0;
			    if (shield < referenceShield) {
				    shield += 1;
			    }
			    else {
				    shield = referenceShield;
			    }
			    onShieldChanged?.Invoke(shield);
		    }
	    }

        /// <summary>
        /// Damage the character. The shield will be damaged first.
        /// </summary>
        /// <param name="value"></param>
        public virtual void Damage (float value) {
            if (!isAvailable) return;
            float shieldDamage = Mathf.Min (shield, value);
            float healthDamage = Mathf.Min (health, value - shieldDamage);
            shield -= shieldDamage;
            health -= healthDamage;
            if (shieldDamage > 0) onShieldChanged?.Invoke(shield);
            if (healthDamage > 0) onHealthChanged?.Invoke(health);
            onDamage?.Invoke(value);
            if (Mathf.Abs(health) < epsilon) {
                Death ();
            }
            if (isDebugLog) Debug.Log (ID + ": Damaged: " + value);
        }

        /// <summary>
        /// Heal the character.
        /// </summary>
        /// <param name="value"></param>
        public virtual void Heal (float value) {
            if (!isAvailable) return;
            float healthHeal = Mathf.Min (referenceHealth - health, value);
            float shieldHeal = Mathf.Min (referenceShield - shield, value - healthHeal);
            health += healthHeal;
            shield += shieldHeal;
            if (healthHeal > 0) onHealthChanged?.Invoke(health);
            if (shieldHeal > 0) onShieldChanged?.Invoke(shield);
            onHeal?.Invoke(value);
            if (isDebugLog) Debug.Log (ID + ": Healed: " + value);
        }

	    private bool CheckFrenzy () {
            bool wasFrenzy = isFrenzy;
		    isFrenzy = health < (referenceHealth / 100) * frenzyThreshold;
            if (isFrenzy != wasFrenzy) {
                onFrenzyChanged?.Invoke(isFrenzy);
            }
		    return isFrenzy;
	    }

        /// <summary>
        /// Activate the character and restore health and shield.
        /// </summary>
        public virtual void Respawn() {
            if (isAvailable) return;
            Activate();
            AllowHealthRecovery();
            AllowShieldRecovery();
            SetMaximumHealthAndShield();
            CheckFrenzy ();
            onRespawn?.Invoke();
            onHealthChanged?.Invoke(health);
            onShieldChanged?.Invoke(shield);
            if (isDebugLog) Debug.Log (ID + ": Respawn");
        }

        /// <summary>
        /// Deactivate the character and set health and shield to the minimum.
        /// </summary>
        public virtual void Death () {
            if (!isAvailable) return;
            Deactivate();
		    isFrenzy = false;
		    BanHealthRecovery();
            BanShieldRecovery();
		    SetMinimumHealthAndShield();
            onDeath?.Invoke();
            if (isDebugLog) Debug.Log (ID + ": Death");
        }
    }
}
