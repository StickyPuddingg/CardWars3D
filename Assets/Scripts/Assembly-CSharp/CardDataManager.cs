using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardDataManager : ILoadable
{
    private const string CreaturesFileName = "db_Creatures.json";
    private const string BuildingsFileName = "db_Buildings.json";
    private const string SpellsFileName = "db_Spells.json";
    private const string DweebsFileName = "db_Dweeb.json";
    private const string DefaultRewardCardID = "Creature_Pig";

    private static CardDataManager instance;
    
    // C# 4.0 Compatible Property Getter
    public static CardDataManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new CardDataManager();
            }
            return instance;
        }
    }

    private bool loaded;
    public bool Loaded 
    { 
        get { return loaded; } 
        private set { loaded = value; } 
    }

    private readonly Dictionary<string, CardForm> _allCards = new Dictionary<string, CardForm>();
    private readonly Dictionary<string, CardForm>[] _cardsByType = new Dictionary<string, CardForm>[4]
    {
        new Dictionary<string, CardForm>(), // 0: Creature
        new Dictionary<string, CardForm>(), // 1: Building
        new Dictionary<string, CardForm>(), // 2: Spell
        new Dictionary<string, CardForm>()  // 3: Dweeb
    };

    // Delegate definition since Action<T1, T2> is used for the loading callback
    private delegate void CustomDataAssignmentHandler<T>(Dictionary<string, object> dict, T card);

    public IEnumerator Load()
    {
        // 1. Load Creatures
        yield return LoadCardType<CreatureCard>(CreaturesFileName, 0, delegate(Dictionary<string, object> dict, CreatureCard card)
        {
            card.ObjectName = TFUtils.LoadString(dict, "ObjectName");
            card.BaseATK = TFUtils.LoadInt(dict, "ATK");
            card.BaseDEF = TFUtils.LoadInt(dict, "DEF");
            card.ShortHand = dict.ContainsKey("ShortHandName") 
                ? TFUtils.LoadLocalizedString(dict, "ShortHandName") 
                : card.Name;
        });

        // 2. Load Buildings
        yield return LoadCardType<BuildingCard>(BuildingsFileName, 1, delegate(Dictionary<string, object> dict, BuildingCard card)
        {
            card.ObjectName = TFUtils.LoadString(dict, "ObjectName");
        });

        // 3. Load Spells
        yield return LoadCardType<SpellCard>(SpellsFileName, 2, delegate(Dictionary<string, object> dict, SpellCard card)
        {
            card.ParticleName = TFUtils.LoadString(dict, "Particles");
        });

        // 4. Load Dweebs
        yield return LoadDweebCards(DweebsFileName);

        Loaded = true;
    }

    private IEnumerator LoadCardType<T>(string fileName, int typeIndex, CustomDataAssignmentHandler<T> customDataAssignment) where T : CardForm, new()
    {
        Dictionary<string, object>[] rawDataArray = SQUtils.ReadJSONData(fileName);
        if (rawDataArray == null) yield break;

        foreach (Dictionary<string, object> dict in rawDataArray)
        {
            T currentCard = new T();
            FillCardData(dict, currentCard);
            
            if (customDataAssignment != null)
            {
                customDataAssignment(dict, currentCard);
            }

            if (string.IsNullOrEmpty(currentCard.ID))
            {
                TFUtils.DebugLog("CardDataManager: skipping card in " + fileName + " due to missing ID.");
                continue;
            }

            if (_allCards.ContainsKey(currentCard.ID) || _cardsByType[typeIndex].ContainsKey(currentCard.ID))
            {
                TFUtils.DebugLog("CardDataManager: duplicate card skipped: " + currentCard.ID);
                continue;
            }

            _allCards.Add(currentCard.ID, currentCard);
            _cardsByType[typeIndex].Add(currentCard.ID, currentCard);

            if (LoadingManager.ShouldYield())
            {
                yield return null;
            }
        }
    }

    private IEnumerator LoadDweebCards(string fileName)
    {
        Dictionary<string, object>[] rawDataArray = SQUtils.ReadJSONData(fileName);
        if (rawDataArray == null) yield break;

        foreach (Dictionary<string, object> dict in rawDataArray)
        {
            DweebCard dweeb = new DweebCard();
            dweeb.ID = TFUtils.LoadString(dict, "ID");
            dweeb.Name = TFUtils.LoadLocalizedString(dict, "Name");
            dweeb.RawDescription = TFUtils.LoadLocalizedString(dict, "Desc");
            dweeb.SpriteName = TFUtils.LoadString(dict, "SpriteName");

            if (string.IsNullOrEmpty(dweeb.ID))
            {
                TFUtils.DebugLog("CardDataManager: skipping dweeb card with missing ID.");
                continue;
            }

            if (_cardsByType[3].ContainsKey(dweeb.ID))
            {
                TFUtils.DebugLog("CardDataManager: duplicate dweeb card skipped: " + dweeb.ID);
                continue;
            }

            _cardsByType[3].Add(dweeb.ID, dweeb);

            if (LoadingManager.ShouldYield())
            {
                yield return null;
            }
        }
    }

    private void FillCardData(Dictionary<string, object> dict, CardForm form)
    {
        form.ID = TFUtils.LoadString(dict, "ID");
        form.Name = TFUtils.LoadLocalizedString(dict, "Name");
        form.RawDescription = TFUtils.LoadLocalizedString(dict, "Desc");
        form.BaseVal1 = TFUtils.LoadInt(dict, "val1", 0);
        form.BaseVal2 = TFUtils.LoadInt(dict, "val2", 0);
        form.BaseSalePrice = TFUtils.LoadInt(dict, "BaseSalePrice", 0);
        form.IconAtlas = TFUtils.LoadString(dict, "IconAtlas");
        form.FrameAtlas = TFUtils.LoadString(dict, "FrameAtlas");
        form.SpriteName = TFUtils.LoadString(dict, "SpriteName");
        form.FrameSpriteName = TFUtils.LoadString(dict, "FrameSpriteName");
        form.ScriptName = TFUtils.LoadString(dict, "ScriptName");

        form.ScriptVizName = TFUtils.LoadString(dict, "VizOverride", string.Empty).Trim();
        if (string.IsNullOrEmpty(form.ScriptVizName))
        {
            form.ScriptVizName = form.ScriptName;
        }

        // Old-school Enum parsing compatible with legacy Unity/Mono profiles
        try
        {
            string abilityStr = TFUtils.LoadString(dict, "AbilityType");
            form.AbilityType = (AbilityType)Enum.Parse(typeof(AbilityType), abilityStr, true);
        }
        catch
        {
            form.AbilityType = AbilityType.None;
        }

        try
        {
            string factionStr = TFUtils.LoadString(dict, "Faction");
            form.Faction = (Faction)Enum.Parse(typeof(Faction), factionStr, true);
        }
        catch
        {
            form.Faction = default(Faction);
        }

        try
        {
            string qualityStr = TFUtils.LoadString(dict, "Quality");
            form.Quality = (Quality)Enum.Parse(typeof(Quality), qualityStr, true);
        }
        catch
        {
            form.Quality = Quality.Standard;
        }

        form.Rarity = TFUtils.LoadInt(dict, "Rarity");
        form.CanFuse = TFUtils.LoadBool(dict, "CanFuse", true);
        form.Cost = TFUtils.LoadInt(dict, "Cost");
        form.FloopCost = TFUtils.LoadInt(dict, "FloopCost", 0);
        form.CostDescription = KFFLocalization.Get("!!CARD_FLOOPCOST").Replace("<cost>", form.FloopCost.ToString());
    }

    public CardForm GetCard(CardType type, string id)
    {
        int index = (int)type;
        if (index >= 0 && index < _cardsByType.Length)
        {
            CardForm card;
            if (_cardsByType[index].TryGetValue(id, out card))
            {
                return card;
            }
        }
        return null;
    }

    public CardForm GetCard(string id, bool backwards_compatibility = true)
    {
        CardForm card;
        if (_allCards.TryGetValue(id, out card))
        {
            return card;
        }

        if (backwards_compatibility)
        {
            foreach (KeyValuePair<string, CardForm> kvp in _allCards)
            {
                if (kvp.Value.Name == id)
                {
                    return kvp.Value;
                }
            }

            CardForm defaultCard;
            if (_allCards.TryGetValue(DefaultRewardCardID, out defaultCard))
            {
                return defaultCard;
            }
        }
        return null;
    }

    public List<CardForm> GetCards(CardType type)
    {
        int index = (int)type;
        if (index >= 0 && index < _cardsByType.Length)
        {
            return new List<CardForm>(_cardsByType[index].Values);
        }
        return new List<CardForm>();
    }

    public List<CardForm> GetCards()
    {
        return new List<CardForm>(_allCards.Values);
    }

    public void Destroy()
    {
        instance = null;
    }

    private void VerifyCards()
    {
        var resourceManager = SLOTGameSingleton<SLOTResourceManager>.GetInstance();
        if (resourceManager == null) return;

        foreach (CardForm card in _allCards.Values)
        {
            switch (card.Type)
            {
                case CardType.Creature:
                    resourceManager.LoadResource("Summons/" + card.ObjectName);
                    break;
                case CardType.Building:
                    resourceManager.LoadResource("Building/" + card.ObjectName);
                    break;
            }
        }
    }
}