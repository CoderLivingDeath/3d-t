using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace AFPC {

/// <summary>
/// UI Toolkit HUD for health, shield, endurance and damage effects.
/// Requires a UIDocument component on the same GameObject.
/// </summary>
public class AFPCUI : MonoBehaviour {

    [HideInInspector] public Hero hero;

    private VisualElement damageFX;
    private VisualElement healthFill;
    private VisualElement shieldFill;
    private VisualElement enduranceFill;
    private Label healthPctLabel;
    private Label shieldPctLabel;
    private Label endurancePctLabel;

    private Label interactionLabel;
    private AFPCInteraction interaction;

    private VisualElement extensionsPanel;
    private readonly List<(VisualElement row, AFPCExtension ext)> extensionRows = new List<(VisualElement, AFPCExtension)>();
    private bool extensionsBuilt;

    private float damageFXAlpha;

    private void OnEnable () {
        var doc = GetComponent<UIDocument>();
        if (!doc) {
            Debug.LogWarning("AFPCUI: UIDocument component not found.", this);
            return;
        }
        var root = doc.rootVisualElement;
        if (root == null) {
            Debug.LogWarning("AFPCUI: UIDocument has no root visual element.", this);
            return;
        }
        damageFX = root.Q("damage-fx");
        healthFill = root.Q("health-fill");
        shieldFill = root.Q("shield-fill");
        enduranceFill = root.Q("endurance-fill");
        healthPctLabel = root.Q<Label>("health-pct");
        shieldPctLabel = root.Q<Label>("shield-pct");
        endurancePctLabel = root.Q<Label>("endurance-pct");
        if (healthFill == null || shieldFill == null || enduranceFill == null)
            Debug.LogWarning("AFPCUI: One or more bar fill elements not found in UXML.", this);
        interactionLabel = root.Q<Label>("interaction-label");
        extensionsPanel = root.Q("extensions-panel");
        extensionsBuilt = false;
    }

    private void Update () {
        if (hero && healthFill != null) {
            float healthPct    = hero.lifecycle.referenceHealth    > 0 ? hero.lifecycle.GetHealthValue()    / hero.lifecycle.referenceHealth    : 0f;
            float shieldPct    = hero.lifecycle.referenceShield    > 0 ? hero.lifecycle.GetShieldValue()    / hero.lifecycle.referenceShield    : 0f;
            float endurancePct = hero.movement.referenceEndurance  > 0 ? hero.movement.GetEnduranceValue()  / hero.movement.referenceEndurance  : 0f;

            healthFill.style.width = Length.Percent(healthPct * 100);
            shieldFill.style.width = Length.Percent(shieldPct * 100);
            enduranceFill.style.width = Length.Percent(endurancePct * 100);

            if (healthPctLabel != null) healthPctLabel.text = Mathf.RoundToInt(healthPct * 100) + "%";
            if (shieldPctLabel != null) shieldPctLabel.text = Mathf.RoundToInt(shieldPct * 100) + "%";
            if (endurancePctLabel != null) endurancePctLabel.text = Mathf.RoundToInt(endurancePct * 100) + "%";
        }

        if (damageFXAlpha > 0 && damageFX != null) {
            damageFXAlpha = Mathf.MoveTowards(damageFXAlpha, 0, Time.deltaTime * 2);
            damageFX.style.backgroundColor = new Color(0.8f, 0.12f, 0.12f, damageFXAlpha * 0.35f);
        }

        if (interactionLabel != null) {
            if (interaction == null && hero != null) interaction = hero.GetExtension<AFPCInteraction>();
            interactionLabel.text = interaction != null ? interaction.GetFocusedPrompt() : string.Empty;
        }

        if (!extensionsBuilt && hero != null && hero.extensions != null && hero.extensions.Count > 0)
            BuildExtensionsPanel();

        foreach (var (row, ext) in extensionRows) {
            bool active = ext != null && ext.IsActive();
            if (active) row.AddToClassList("ext-row--active");
            else        row.RemoveFromClassList("ext-row--active");
        }
    }

    private void BuildExtensionsPanel () {
        if (extensionsPanel == null) return;
        extensionsPanel.Clear();
        extensionRows.Clear();
        foreach (var ext in hero.extensions) {
            if (ext == null) continue;
            string extName = ext.GetType().Name;
            if (extName.StartsWith("AFPC")) extName = extName.Substring(4);
            var row = new VisualElement();
            row.AddToClassList("ext-row");
            var label = new Label(extName.ToUpper());
            label.AddToClassList("ext-label");
            row.Add(label);
            extensionsPanel.Add(row);
            extensionRows.Add((row, ext));
        }
        extensionsBuilt = true;
    }

    public void DamageFX () {
        damageFXAlpha = 1;
    }
}
}
