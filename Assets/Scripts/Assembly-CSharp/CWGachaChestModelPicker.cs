using System;
using System.Collections.Generic;
using UnityEngine;

public class CWGachaChestModelPicker : MonoBehaviour
{
    public enum DebugSeasonalOverride
    {
        Off,
        Halloween,
        Christmas
    }

    [Serializable]
    public class ModelVariation
    {
        public string ID;
        public GameObject Model; // The main chest model
        public string Label;

        // Drag all other secondary scene objects/decorations for this season here!
        public List<GameObject> ExtraSeasonalObjects;
    }

    public UILabel chestLabel;
    public List<ModelVariation> Variations;

    // -s Debug-only override to force a seasonal chest for testing in the scene.
    public DebugSeasonalOverride debugSeasonalOverride = DebugSeasonalOverride.Off;

    private const string DefaultVariation = "Default";

    private void OnEnable()
    {
        ApplySelection();
    }

    private void ApplySelection()
    {
        if (Variations == null || Variations.Count == 0)
        {
            Logger.Error("[Gacha Picker] Variations list is empty or null!");
            Destroy(this);
            return;
        }

        string selectedVariationId = ResolveVariantId();
        Logger.Log("[Gacha Picker] Selected Variant ID determined as: " + selectedVariationId);

        // Find the chosen chest config
        ModelVariation selectedVariation = Variations.Find(delegate (ModelVariation elem)
        {
            return elem.ID == selectedVariationId;
        });

        // Fallback to default if no matching variation is found in the list setup
        if (selectedVariation == null)
        {
            Logger.Warn(string.Format("[Gacha Picker] Match failed for ID '{0}'. Defaulting to element 0.", selectedVariationId));
            selectedVariation = Variations[0];
        }

        // Loop through and handle RAM cleanup: Keep chosen config, Destroy everything else
        foreach (ModelVariation variation in Variations)
        {
            if (variation == null) continue;

            if (variation == selectedVariation)
            {
                // 1. Keep and activate the chosen chest model
                if (variation.Model != null)
                {
                    variation.Model.SetActive(true);
                }

                if (chestLabel != null && !string.IsNullOrEmpty(variation.Label))
                {
                    chestLabel.text = KFFLocalization.Get(variation.Label);
                }

                // 2. Keep and activate all extra scene assets for this specific season
                if (variation.ExtraSeasonalObjects != null)
                {
                    foreach (GameObject extraObj in variation.ExtraSeasonalObjects)
                    {
                        if (extraObj != null)
                        {
                            extraObj.SetActive(true);
                        }
                    }
                }
            }
            else
            {
                // 3. RAM Cleanup: Destroy unchosen main chest model
                if (variation.Model != null)
                {
                    Destroy(variation.Model);
                    variation.Model = null;
                }

                // 4. RAM Cleanup: Destroy all extra unchosen environment objects instantly
                if (variation.ExtraSeasonalObjects != null)
                {
                    foreach (GameObject extraObj in variation.ExtraSeasonalObjects)
                    {
                        if (extraObj != null)
                        {
                            Destroy(extraObj);
                        }
                    }
                    variation.ExtraSeasonalObjects.Clear();
                }
            }
        }

        // Clear label text if defaulting down to a generic setup with no active label
        if (selectedVariation == Variations[0] && chestLabel != null && string.IsNullOrEmpty(selectedVariation.Label))
        {
            chestLabel.text = string.Empty;
        }

        // Finish up: Self-destruct this selector component script entirely
        Destroy(this);
    }

    private string ResolveVariantId()
    {
        if (debugSeasonalOverride == DebugSeasonalOverride.Halloween)
        {
            return "party_halloween";
        }
        if (debugSeasonalOverride == DebugSeasonalOverride.Christmas)
        {
            return "party_holiday";
        }

        PartyInfo currentPartyInfo = GachaManager.Instance.GetCurrentPartyInfo();
        if (currentPartyInfo != null)
        {
            return currentPartyInfo.id;
        }

        return DefaultVariation;
    }
}