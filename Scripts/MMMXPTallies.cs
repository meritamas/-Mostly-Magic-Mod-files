// Project:         MeriTamas's (Mostly) Magic Mod for Daggerfall Unity (http://www.dfworkshop.net)
// Copyright:       Copyright (C) 2022 meritamas
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Author:          meritamas (meritamas@outlook.com)

using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using DaggerfallConnect;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.Formulas;
using DaggerfallWorkshop.Game.MagicAndEffects;
using DaggerfallWorkshop.Game.Serialization;
using DaggerfallWorkshop.Game.Utility;
using Wenzil.Console;
using FullSerializer;

namespace MTMMM
{
    
    public struct SpellXPTally
    {
        public string Key;
        public int Tally;
                
        public SpellXPTally(string key, int tally = 0)
        {
            Key = key;
            Tally = tally;      // should return Tally for SaveLoad purposes but Tally/100 for in-game purposes  
        }
    }

    [Flags]
    public enum MMMTargetTypes
    {
        None = 0,
        CasterOnly = 1,
        ByTouch = 2,
        SingleTargetAtRange = 4,
        AreaAroundCaster = 8,
        AreaAtRange = 16,
        ThingAlreadyInPosession = 32,
        ThingBeingConjuredCreated = 64,
        ConjuredCreature = 128
    }

    public static class MMMXPTallies 
    {
        public static bool diversifiedSpellExperienceRequiredForMagicSkillAdvancement;
        public static bool strongSpellsAdvanceMagicSkillsMore;
        public static string[] ItemEffects = { "FixItem" };     // list of spell effects that qualify as Item effects (TargetType)
        public static string[] ItemConjurationCreationEffects = { "CreateItem", "MultiplyProvisions" };
                        // list of spell effects that qualify as Conjured Item effects (TargetType)
        public static string[] CreatureConjurationEffects = { };
                        // list of spell effects that qualify as Conjured Creature effects (TargetType)

        public static int[] RequiredXPAmounts = { 3, 3, 4, 4, 5, 5,        6, 7, 7, 8, 9, 10,     12, 13, 15, 16, 18, 21,
                                                 23, 26, 29, 32, 36, 41,    46, 51, 57, 64, 72, 80  };
        public static int[] RequiredTallyAmounts = InitialRequiredTallyAmounts();        // i-th element needs to be calculated as sum of RequiredXPAmounts[0]..RequiredXPAmounts[i]

        static List<SpellXPTally> committedSpellXPTallies = new List<SpellXPTally>();
        static List<SpellXPTally> spellXPTallies = new List<SpellXPTally>();        
        public static int AlchemyXPTally;
        public static int EXPTally;
        public static bool CalculationDebugToPlayer = true;

        #region debug        
        static string messagePrefix = "MMMXPTallies: ";

        static void Message(string message)
        {
            MTMostlyMagicMod.Message(messagePrefix + message);
        }

        static void SilentMessage(string message)
        {
            MTMostlyMagicMod.SilentMessage(messagePrefix + message);
        }

        public static void SilentMessage(string message, params object[] args)
        {
            MTMostlyMagicMod.SilentMessage(messagePrefix + message, args);
        }
        #endregion

        #region InitMethod
        static int[] InitialRequiredTallyAmounts()
        {
            int[] workArray = new int[30];
            workArray[0] = RequiredXPAmounts[0];
            for (int i = 1; i < 30; i++)
                workArray[i] = workArray[i - 1] + RequiredXPAmounts[i];
            return workArray;
        }
        #endregion

        #region SaveLoad and Init methods

        public static SpellXPTally[] GetSpellTallyArray()
        {
            return spellXPTallies.ToArray();
        }

        public static void SetSpellTalliesFromArray(SpellXPTally[] newArray)
        {
            spellXPTallies = newArray.ToList();
        }
        #endregion

        #region Spell Tally Public Methods
        public static void ResetPreliminaries()
        {
            spellXPTallies = new List<SpellXPTally>();
        }        

        /// <summary>
        /// Will search the tally list and if a tally exist for this key, will increment it by the passed increment.
        /// If no such tally exists, it creates one and sets it to be equal to the passed increment.        /// 
        /// </summary>
        /// <param name="key">The key of the spell xp tally to be incremented.</param>
        /// <param name="increment">The value by which the tally is to be incremented.</param>
        /// <returns>the new value of the Tally, as Saved/Loaded - (In-Game Value * 100)</returns>
        public static int SpellXPTally(string key, float increment=1.0f)
        {           

            string debugMessage;
            for (int i = 0; i < spellXPTallies.Count; i++)
                if (spellXPTallies[i].Key == key)
                {
                    SpellXPTally tallyToBeModified = spellXPTallies[i];
                    tallyToBeModified.Tally += (int)(increment*100);
                    spellXPTallies[i] = tallyToBeModified;
                    debugMessage = string.Format("Tally '{0}' found. Incremented by {1} to the new value of {2}.", key, increment, tallyToBeModified.Tally);
                    if (increment > 0.005f)
                        SilentMessage(debugMessage);
                    return tallyToBeModified.Tally;
                }

            int initialTallyValue = (int)(increment * 100);
            spellXPTallies.Add(new SpellXPTally(key, initialTallyValue));
            debugMessage = string.Format("Tally '{0}' not found. Created a new tally which after an initial tally of {1} stands at {2}.", key, increment, initialTallyValue);
            if (increment > 0.005f)
                SilentMessage(debugMessage);
            return initialTallyValue;
        }

        /// <summary>
        /// The method that deals with Tallies from an in-game perspective.
        /// TODO: Consider tallying experience taking into account player reflexes (similar to skills tallying)
        /// </summary>
        /// <param name="key">The key of the tally.</param>
        /// <param name="increment"></param>
        /// <returns></returns>
        public static int SpellXPTally(string key, int increment=0)
        {
            return SpellXPTally(key, (float)increment) / 100;     // for in-game purposes, we are interested in already confirmed tallies
        }

        static int GetEffectXP(string key)
        {
            return SpellXPTally(key, 0);       // should use the one that does divide by 100 (the int version)
        }

        public static int GetEffectCoefficient(string key)
        {
            int effectXP = GetEffectXP(key);
            if (CalculationDebugToPlayer)
                SilentMessage(string.Format("GetEffectCoefficient: XP for effect '{0}' is {1}", key, effectXP));
            return GetCoefficientFromXP(effectXP);
        }

        static void SingleTargetTypeTally (MMMTargetTypes targetType, float tallyAmount, int maxTally=100000)
        {
            string tallyName = targetType.ToString() + "TT";
            if (SpellXPTally(tallyName, 0.0f)<maxTally)
                SpellXPTally(tallyName, tallyAmount);
        }

        public static void TargetTypeTally(MMMTargetTypes targetType, float tallyAmount)
        {
            switch (targetType)
            {
                case MMMTargetTypes.CasterOnly:     
                    SingleTargetTypeTally(MMMTargetTypes.CasterOnly, tallyAmount);  
                    break;
                case MMMTargetTypes.ByTouch:        
                    SingleTargetTypeTally(MMMTargetTypes.ByTouch, tallyAmount);
                    SingleTargetTypeTally(MMMTargetTypes.ThingAlreadyInPosession, tallyAmount / 2, 7500);
                    SingleTargetTypeTally(MMMTargetTypes.AreaAroundCaster, tallyAmount / 4, 7500);
                    SingleTargetTypeTally(MMMTargetTypes.SingleTargetAtRange, tallyAmount / 8, 7500);
                    break;
                case MMMTargetTypes.ThingAlreadyInPosession:
                    SingleTargetTypeTally(MMMTargetTypes.ThingAlreadyInPosession, tallyAmount);
                    SingleTargetTypeTally(MMMTargetTypes.ByTouch, tallyAmount / 4, 15000);
                    break;
                case MMMTargetTypes.AreaAroundCaster:
                    SingleTargetTypeTally(MMMTargetTypes.AreaAroundCaster, tallyAmount);
                    SingleTargetTypeTally(MMMTargetTypes.ByTouch, tallyAmount / 4, 7500);
                    SingleTargetTypeTally(MMMTargetTypes.ThingAlreadyInPosession, tallyAmount / 4, 7500);
                    SingleTargetTypeTally(MMMTargetTypes.SingleTargetAtRange, tallyAmount / 8, 7500);
                    break;

                case MMMTargetTypes.SingleTargetAtRange:        
                    SingleTargetTypeTally(MMMTargetTypes.SingleTargetAtRange, tallyAmount);
                    SingleTargetTypeTally(MMMTargetTypes.ByTouch, tallyAmount / 4, 7500);
                    SingleTargetTypeTally(MMMTargetTypes.ThingAlreadyInPosession, tallyAmount / 4, 7500);
                    SingleTargetTypeTally(MMMTargetTypes.AreaAtRange, tallyAmount / 8, 7500);
                    SingleTargetTypeTally(MMMTargetTypes.AreaAroundCaster, tallyAmount / 8, 7500);                    
                    break;                
                case MMMTargetTypes.AreaAtRange:       
                    SingleTargetTypeTally(MMMTargetTypes.AreaAtRange, tallyAmount);
                    SingleTargetTypeTally(MMMTargetTypes.SingleTargetAtRange, tallyAmount / 2, 30000);
                    SingleTargetTypeTally(MMMTargetTypes.AreaAroundCaster, tallyAmount / 4, 7500);                       
                    break;
                    
                case MMMTargetTypes.ThingBeingConjuredCreated:
                    SingleTargetTypeTally(MMMTargetTypes.ThingBeingConjuredCreated, tallyAmount);
                    SingleTargetTypeTally(MMMTargetTypes.ConjuredCreature, tallyAmount / 8, 7500);
                    break;
                case MMMTargetTypes.ConjuredCreature:
                    SingleTargetTypeTally(MMMTargetTypes.ConjuredCreature, tallyAmount);
                    SingleTargetTypeTally(MMMTargetTypes.ThingBeingConjuredCreated, tallyAmount / 2, 30000);
                    break;
                default:
                    break;
            }            
        }

        static int GetTargetTypeXP(MMMTargetTypes targetType)
        {
            return SpellXPTally(targetType.ToString() + "TT", 0);       // should use the one that does divide by 100
        }

        public static MMMTargetTypes GetMMMTargetType (TargetTypes targetType, string effectKey)
        {
            if (ItemEffects.Contains(effectKey))
                return MMMTargetTypes.ThingAlreadyInPosession;

            if (ItemConjurationCreationEffects.Contains(effectKey))
                return MMMTargetTypes.ThingBeingConjuredCreated;

            return (MMMTargetTypes)targetType;
        }

        public static int GetTargetTypeCoefficient(MMMTargetTypes targetType)
        {
            int targetTypeXP = GetTargetTypeXP(targetType);
            if (CalculationDebugToPlayer)
                SilentMessage(string.Format("GetTargetTypeCoefficient: XP for Target Type {0} is {1}", targetType, targetTypeXP));
            return GetCoefficientFromXP(targetTypeXP);
        }
                
        public static int ElementTypeTally(ElementTypes elementType, float tallyAmount)
        {
            return SpellXPTally(elementType.ToString() + "E", tallyAmount);
        }

        static int GetElementXP(ElementTypes elementType)
        {
            return SpellXPTally(elementType.ToString() + "E", 0);       // should use the one that does divide by 100
        }

        public static int GetElementCoefficient(ElementTypes elementType)
        {
            int elementXP = GetElementXP(elementType);
            if (CalculationDebugToPlayer)
                SilentMessage(string.Format("GetElementCoefficient: XP for Element {0} is {1}", elementType, elementXP));            
            return GetCoefficientFromXP(elementXP);
        }

        public static int GetCoefficientFromXP(int xP)
        {
            int i;
            for (i = 0; i < 30; i++)
                if (xP < RequiredTallyAmounts[i])
                    break;
            if (CalculationDebugToPlayer)
                SilentMessage(string.Format("GetCoefficientFromXP: For an XP of {0} the i we should use in 70+i is equal to {1}", xP, i));
            return 70 + i;
        }         

        public static void PlayerSpellCasting_OnReleaseFrame()
        {
            int spellPointsOnItem = 0;
            int spellPointsOnConjuredItem = 0;
            int spellPointsOnConjuredCreature = 0;
            int totalSpellPointCost = 0;
            EntityEffectBundle spellBundle;
            int playerIntelligence = GameManager.Instance.PlayerEntity.Stats.LiveIntelligence;

            if (GameManager.Instance.PlayerEffectManager.ReadySpell == null)
                spellBundle = GameManager.Instance.PlayerEffectManager.LastSpell;
            else
                spellBundle = GameManager.Instance.PlayerEffectManager.ReadySpell;

            EffectBundleSettings spell = spellBundle.Settings;

            for (int i = 0; i < spell.Effects.Length; i++) {
                string effectKey = spell.Effects[i].Key;
                IEntityEffect effectTemplate = GameManager.Instance.EntityEffectBroker.GetEffectTemplate(effectKey);
                DFCareer.Skills magicSkill = (DFCareer.Skills)effectTemplate.Properties.MagicSkill;

                FormulaHelper.SpellCost effectCost = MMMFormulaHelper.CalculateEffectCosts_XP(effectTemplate, spell.Effects[i].Settings, spell.TargetType, spell.ElementType);
                int spellPointCost = effectCost.spellPointCost;

                float effectTallyAmount = Math.Max(Math.Min(((float)spellPointCost) / playerIntelligence * 7.0f, 7.01f), 1.01f);

                int resultingEffectXPTally = SpellXPTally(effectKey, effectTallyAmount);                                                  // increment the effect-tally by an appropriate amount
                               
                short skillExtraTallyAmount = 0;
                int minimumSkillTallyAmount = 1;
                int maximumSkillTallyAmount = 1;

                        // here comes the part to calculate if and how many (extra) points to tally toward the given magic skill 
                
                if (diversifiedSpellExperienceRequiredForMagicSkillAdvancement)
                {
                    float diversifiedSpellExperienceCoefficient= 1.0f;      // if resulting effect XP tally is at most 50000 (500.00), have coefficient of 1.0
                  
                    if (resultingEffectXPTally > 95000)                     // if resulting effect XP tally is more than 95000 (950.00), have coefficient of 0.1
                        diversifiedSpellExperienceCoefficient = 0.1f;
                    if (resultingEffectXPTally > 50000 && resultingEffectXPTally <= 95000)
                        diversifiedSpellExperienceCoefficient = (float)(1.0 - 0.002 * (resultingEffectXPTally/100 - 500));
                                    // if resulting effect XP tally is somewhere in between, have the coefficient be something in between

                    SilentMessage("PlayerSpellCasting_OnReleaseFrame: Diversified...Advancement on, so after a {0} coefficient is applied to original tally amount of {1}, {2} should be tallied to the skill.",
                        diversifiedSpellExperienceCoefficient, effectTallyAmount, effectTallyAmount * diversifiedSpellExperienceCoefficient);
                    effectTallyAmount *= diversifiedSpellExperienceCoefficient;

                    if (effectTallyAmount<1.01)
                    {
                        int successRoll = Dice100.Roll();
                        int chance = (int)effectTallyAmount;
                        if (successRoll < chance)
                            effectTallyAmount = 0.01f;
                        else
                            effectTallyAmount = 1.01f;
                        SilentMessage("PlayerSpellCasting_OnReleaseFrame: since the value was between 0 and 1, we took chance. Successroll={0}, Ourchance={1}, Result={2}",
                            successRoll, chance, effectTallyAmount);
                    }
                    minimumSkillTallyAmount = 0;                    
                }

                if (strongSpellsAdvanceMagicSkillsMore)
                    maximumSkillTallyAmount = 7;

                skillExtraTallyAmount = (short)((Math.Max(Math.Min(Math.Floor(effectTallyAmount), maximumSkillTallyAmount), minimumSkillTallyAmount))-1);
                                
                GameManager.Instance.PlayerEntity.TallySkill(magicSkill, skillExtraTallyAmount);
                            // add the calculated value (from -1 up to 6) to the tally for the relevant magic skill
                SilentMessage("Extra points added to (in case of -1, deducted from) {0} skill tally: {1} ", magicSkill, skillExtraTallyAmount);

                if (MMMXPTallies.ItemEffects.Contains(effectKey))
                {
                    spellPointsOnItem += spellPointCost;
                    SilentMessage(string.Format("PlayerSpellCasting_OnReleaseFrame: {0} identified as an Item effect.", effectKey));
                }

                if (MMMXPTallies.ItemConjurationCreationEffects.Contains(effectKey))
                {
                    spellPointsOnConjuredItem += spellPointCost;
                    SilentMessage(string.Format("PlayerSpellCasting_OnReleaseFrame: {0} identified as a Conjured/created Item effect.", effectKey));
                }

                if (MMMXPTallies.CreatureConjurationEffects.Contains(effectKey))
                {
                    spellPointsOnConjuredCreature += spellPointCost;
                    SilentMessage(string.Format("PlayerSpellCasting_OnReleaseFrame: {0} identified as a Creature Conjuration effect.", effectKey));
                }

                totalSpellPointCost += spellPointCost;

                SilentMessage(string.Format("PlayerSpellCasting_OnReleaseFrame: {0} tallied for '{1}', {2} added to '{3}' tally.",
                    effectTallyAmount, effectKey, skillExtraTallyAmount, MMMFormulaHelper.GetMagicSkillAbbrev((DFCareer.MagicSkills)magicSkill)));
            }

            float itemTallyAmount = 0f;
            float conjuredItemTallyAmount = 0f;
            float conjuredCreatureTallyAmount = 0f;
            float tallyAmount = 0f;

            if (spellPointsOnItem > 0)
            {                   // first, we tackle if there were some item effects
                itemTallyAmount = Math.Max(Math.Min(((float)spellPointsOnItem) / playerIntelligence * 7.0f, 7.0f), 1.0f);
                TargetTypeTally(MMMTargetTypes.ThingAlreadyInPosession, itemTallyAmount);
            }

            if (spellPointsOnConjuredItem > 0)
            {                   // second, we tackle if there were some conjuration or creation effects
                conjuredItemTallyAmount = Math.Max(Math.Min(((float)spellPointsOnConjuredItem) / playerIntelligence * 7.0f, 7.0f), 1.0f);
                TargetTypeTally(MMMTargetTypes.ThingBeingConjuredCreated, conjuredItemTallyAmount);
            }

            if (spellPointsOnConjuredCreature > 0)
            {                   // second, we tackle if there were some conjuration or creation effects
                conjuredCreatureTallyAmount = Math.Max(Math.Min(((float)spellPointsOnConjuredCreature) / playerIntelligence * 7.0f, 7.0f), 1.0f);
                TargetTypeTally(MMMTargetTypes.ConjuredCreature, conjuredCreatureTallyAmount);
            }

            // now, we tackle the remaning spell points
            int remainingSpellPointCost = totalSpellPointCost - spellPointsOnItem - spellPointsOnConjuredItem - spellPointsOnConjuredCreature;
            if (remainingSpellPointCost > 0)
            {
                tallyAmount = Math.Max(Math.Min(((float)remainingSpellPointCost) / playerIntelligence * 7.0f, 7.0f), 1.0f);
                TargetTypeTally((MMMTargetTypes)spell.TargetType, tallyAmount);
            }

            ElementTypeTally(spell.ElementType, tallyAmount + itemTallyAmount + conjuredItemTallyAmount + conjuredCreatureTallyAmount);  // the element gets it all            

        }        

        #endregion

    }

}
