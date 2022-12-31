// Project:         MeriTamas's (Mostly) Magic Mod for Daggerfall Unity (http://www.dfworkshop.net)
// Copyright:       Copyright (C) 2022 meritamas
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Author:          meritamas (meritamas@outlook.com)
// Big thanks to Gavin Clayton (interkarma@dfworkshop.net) for his suggestion to create this class. This will make things easier and be an asset as the mod becomes larger.

/*
 * The code is sloppy at various places. Some things are redundant, comments are missing, some comments are not useful anymore etc.
 * I have the intention of cleaning it up in the future.
 * For now, it seems to work as intended... or - let's rather say - reasonably well.
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using DaggerfallConnect;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.Formulas;
using DaggerfallWorkshop.Game.Items;
using DaggerfallWorkshop.Game.MagicAndEffects;

namespace MTMMM
{    
    public delegate int GetSpellLevelMethod(IEntityEffect effect);

    public static class MMMFormulaHelper
    {
        static string messagePrefix = "MMMFormulaHelper: ";
        public static bool SendInfoMessagesOnPCSpellLevel = false;
        public static bool SendInfoMessagesOnNonPCSpellLevel = false;
        public static bool ApplyExperienceTallies;
        public static bool PotionMode = false;
        public static int PotionStrength = 0;
        public static float spellMakerCoefficientFromSettings = 4.0f;

        public static GetSpellLevelMethod GetSpellLevel = GetSpellLevel1;
        public static int spellOfferInstructionCostPerHour_NonMembers = 1000;      // for non-members 
        public static int spellOfferInstructionCostPerHour_Rank0Members = 500;
        public static int spellOfferInstructionCostPerHour_Rank10Members = 50;      // TODO: consider setting up arrays and defaults and putting them into save 
        public static int spellCreationInstructionCostPerHour_Rank0Members = 1000;
            // these could involve option (for each hour began / for each hour finished), and a value for each rank 0 up to 10
        public static int spellCreationInstructionCostPerHour_Rank10Members = 100;

        public static int castCostFloor;

        public static MTSpellMakerWindow activeSpellMakerWindow = null;

        #region Debug Methods

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

        public static void SetPotionMode (DaggerfallUnityItem item)
        {
            PotionMode = true;
            PotionStrength = UnityEngine.Random.Range(10, 16);
            SilentMessage("Entering Potion Mode, potion strength (RND between 10 and 15, inclusive): "+PotionStrength);
        }

        public static void ExitPotionMode()
        {
            PotionMode = false;
            SilentMessage("Exiting Potion Mode.");
        }

        /// <summary>
        /// Calculate the Chance of Success of an effect using the active Spell Level Calculation method. 
        /// </summary>
        public static int GetSpellChance(BaseEntityEffect effect)
        {
            int spellLevel = GetSpellLevel(effect);
            DaggerfallEntityBehaviour caster = effect.Caster;
                        
            int chanceToReturn = effect.Settings.ChanceBase + effect.Settings.ChancePlus * (int)Mathf.Floor(spellLevel / effect.Settings.ChancePerLevel);

            Message(effect.Properties.Key+". ChanceBase =" + effect.Settings.ChanceBase + ", ChancePlus=" + effect.Settings.ChancePlus + " :: " +
                effect.Settings.ChanceBase + "+" + effect.Settings.ChancePlus + "*" + (int)Mathf.Floor(spellLevel / effect.Settings.ChancePerLevel) + " = " + chanceToReturn);      // TODO: re-eval if silent would be better

            //Debug.LogFormat("{5} ChanceValue {0} = base + plus * (level/chancePerLevel) = {1} + {2} * ({3}/{4})", settings.ChanceBase + settings.ChancePlus * (int)Mathf.Floor(casterLevel / settings.ChancePerLevel), settings.ChanceBase, settings.ChancePlus, casterLevel, settings.ChancePerLevel, Key);
            return chanceToReturn;
        }

        /// <summary>
        /// Calculate the Magnitude of an effect using the active Spell Level Calculation method. 
        /// </summary>
        public static int GetSpellMagnitude (BaseEntityEffect effect, EntityEffectManager manager)
        {
            if (effect.Caster == null)
                UnityEngine.Debug.LogWarningFormat("GetMagnitude() for {0} has no caster. Using caster level 1 for magnitude.", effect.Properties.Key);

            if (manager == null)
                UnityEngine.Debug.LogWarningFormat("GetMagnitude() for {0} has no parent manager.", effect.Properties.Key);

            int magnitude = 0;
            string messagePart = "";
            EffectSettings settings = effect.Settings;

            if (effect.Properties.SupportMagnitude)
            {
                int spellLevel = GetSpellLevel(effect);
                int baseMagnitude = UnityEngine.Random.Range(settings.MagnitudeBaseMin, settings.MagnitudeBaseMax + 1);
                int plusMagnitude = UnityEngine.Random.Range(settings.MagnitudePlusMin, settings.MagnitudePlusMax + 1);
                int multiplier = (int)Mathf.Floor(spellLevel / settings.MagnitudePerLevel);
                magnitude = baseMagnitude + plusMagnitude * multiplier;

                messagePart = effect.Properties.Key+". MagBaseMin=" + settings.MagnitudeBaseMin + ", MagBaseMax=" + settings.MagnitudeBaseMax +
                    ", MagPlusMin=" + settings.MagnitudePlusMin + ", MagPlusMax=" + settings.MagnitudePlusMax + " :: " +
                    baseMagnitude + "+" + plusMagnitude + "*" + multiplier + " = " + magnitude;
            }

            if (effect.ParentBundle.targetType != TargetTypes.CasterOnly)
                magnitude = FormulaHelper.ModifyEffectAmount(effect, manager.EntityBehaviour.Entity, magnitude);

            Message(messagePart + " (FINAL-MAG: "+ magnitude+")"); // TODO: re-eval if silent would be better

            return magnitude;
        }

        /// <summary>
        /// Calculate the Duration of an effect using the active Spell Level Calculation method. 
        /// </summary>
        public static int GetSpellDuration (BaseEntityEffect effect)
        {
            int spellLevel = GetSpellLevel(effect);
            if (effect.Properties.SupportDuration)
            {
                int durationToReturn = effect.Settings.DurationBase + effect.Settings.DurationPlus * (int)Mathf.Floor(spellLevel / effect.Settings.DurationPerLevel);

                Message(effect.Properties.Key + ". DurationBase =" + effect.Settings.DurationBase + ", DurationPlus=" + effect.Settings.DurationPlus + " :: " +
                    effect.Settings.DurationBase + "+" + effect.Settings.DurationPlus + "*" + (int)Mathf.Floor(spellLevel / effect.Settings.DurationPerLevel) + " = " + durationToReturn);

                return durationToReturn;
            }
            else
            {
                Message(effect.Properties.Key + ". Duration = 0");
                return 0;
            }

            //Debug.LogFormat("Effect '{0}' will run for {1} magic rounds", Key, roundsRemaining);
        }

        /// <summary>
        /// Get a short 3-4 capital letter abbreviation for the magic skill in question (REST, THAU, ILL, DEST, ALT, MYST). 
        /// </summary>
        public static string GetMagicSkillAbbrev (DFCareer.MagicSkills magicSkill)
        {
            switch (magicSkill)
            {
                case DFCareer.MagicSkills.Restoration:
                    return "REST";                    
                case DFCareer.MagicSkills.Destruction:
                    return "DEST";                   
                case DFCareer.MagicSkills.Alteration:
                    return "ALT";
                case DFCareer.MagicSkills.Mysticism:
                    return "MYST";
                case DFCareer.MagicSkills.Thaumaturgy:
                    return "THAU";                   
                case DFCareer.MagicSkills.Illusion:
                    return "ILL";
                case DFCareer.MagicSkills.None:
                    return "NONE";
                default:
                    return null;
            }
        }

        /// <summary>
        /// To use a set number of points to equalize two values. E.g. the distribution of luck points to adjust SkillLevel and WillPowerLevel when calculating SpellLevel 
        /// </summary>
        /// <returns>
        /// Returns the two values (sum of which is the number of points) that need to be added to the individual parameters that result in an adjustment of the original values
        /// as equal as possible.
        /// </returns>
        public static void DistributePointsToEqualize(int level1, int level2, int pointsToDistribute, out int additionToLevel1, out int additionToLevel2)
        {
            int pointsLeft = pointsToDistribute;
            additionToLevel1 = 0;
            additionToLevel2 = 0;

            while (pointsLeft > 0)  // reported bug potentially solved - for reporting the bug and finding the problematic piece of code, thanks to chantling  
            {
                if (level1 + additionToLevel1 < level2 + additionToLevel2)
                    additionToLevel1++;
                else
                    additionToLevel2++;
                pointsLeft--;
            }
        }

        /// <summary>
        /// The method for Spell Level Calculation supplied to the game's FormulaHelper 
        /// </summary>
        /// <returns>
        /// Uses the Mod's selected Spell Level calculator routine to return a Spell Level. By default, it is calculated via GetSpellLevel1 as follows:
        ///     -   Skill Spell Level = (Skill-9)/3  + possible modifier based on Luck
        ///     -   Willpower Spell Level = 10 + Willpower/5 + possible modifier based on Luck
        /// The modifiers based on Luck are calculated based on a certain amount of Luck Points; Number of Luck Points = (Luck - 50) / 10
        ///     Then,
        ///     -   if the Number of Luck Points is negative, it is subtracted from the Lesser of the two Spell levels (negative modifier)
        ///     -   if the Number of Luck Points is positive, the points are used one after the other to increment the lesser of the Spell levels
        /// The spell level returned is the lesser of the spell levels after applying the luck modifiers.    
        /// </returns>
        public static int GetSpellLevelForGame(DaggerfallEntity casterDummy, IEntityEffect effect)
        {
            //MMMAutomatons.SpellCastAutomaton_PlayerSpellLevelBeingCalculated();
            return GetSpellLevel (effect);
        }

        /// <summary>
        /// The default method for Spell Level Calculation 
        /// </summary>
        /// <returns>
        /// Returns a Spell Level calculated as follows:
        ///     -   Skill Spell Level = (Skill-9)/3  + possible modifier based on Luck
        ///     -   Willpower Spell Level = 10 + Willpower/5 + possible modifier based on Luck
        /// The modifiers based on Luck are calculated based on a certain amount of Luck Points; Number of Luck Points = (Luck - 50) / 10
        ///     Then,
        ///     -   if the Number of Luck Points is negative, it is subtracted from the Lesser of the two Spell levels (negative modifier)
        ///     -   if the Number of Luck Points is positive, the points are used one after the other to increment the lesser of the Spell levels
        /// The spell level returned is the lesser of the spell levels after applying the luck modifiers.    
        /// </returns>
        public static int GetSpellLevel1 (IEntityEffect effect)
        {
            if (PotionMode)
            {                
                SilentMessage("GetSpellLevel1: in Potion Mode. Returning Potion Strength: " + PotionStrength);                
                return PotionStrength;
            }

            DaggerfallEntityBehaviour caster = effect.Caster;

            int casterLevel = 1;
            string relevantSkill = GetMagicSkillAbbrev(effect.Properties.MagicSkill);
            if (relevantSkill == null)  relevantSkill = "";

            if (caster)
            {
                if (caster == GameManager.Instance.PlayerEntityBehaviour)
                {
                    PlayerEntity playerEntity = GameManager.Instance.PlayerEntity;

                    int skillLevel = caster.Entity.Level;       // base case, overriden every time the effect in question has a valid Magic Skill associated with it 

                    if (effect.Properties.MagicSkill == DFCareer.MagicSkills.None)
                        SilentMessage("Level being calculated for Player-cast spell effect with key '" + effect.Properties.Key + "', associated Magic Skill: '" + effect.Properties.MagicSkill + "'");
                    // this is to adapt to a change in DFU core - it seems that in dungeons (at least in Privateer's Hold) the game seeks to determine the level for a PC-cast effect with key Passive-Specials;
                    // for this Passive-Specials effect, effect.Properties.MagicSkill is equal to DFCareer.MagicSkills.None, in which case GetLiveSkillValue would throw an array index out of bounds exception
                    // TODO: if all goes well, consider removing this debug line
                    else
                    {
                        int baseSkill = playerEntity.Skills.GetLiveSkillValue((DFCareer.Skills)effect.Properties.MagicSkill);

                        if (ApplyExperienceTallies)
                        {
                            EffectBundleSettings guessedSpell = GuessWhichSpellWeAreCasting();
                            float coeff = CalculateCombinedXPTalliesCoefficient(effect.Properties.Key, guessedSpell.TargetType, guessedSpell.ElementType);
                            int skillAfterCoeff = (int)Math.Floor((float)baseSkill * coeff);

                            skillLevel = (skillAfterCoeff - 9) / 3;          // (Effective Skill - 9 ) / 3
                        }
                        else
                            skillLevel = (baseSkill - 9) / 3;               // (Skill-9)/3
                    }

                    int willpowerLevel = 10 + (playerEntity.Stats.LiveWillpower / 5);      // 10 + Willpower/5

                    int luckPointsToDistribute = (playerEntity.Stats.LiveLuck - 50) / 10; // (Luck - 50) / 10 
                    int luckPointsToSkillLevel = 0;
                    int luckPointsToWillpowerLevel = 0;
                    if (luckPointsToDistribute > 0)
                        DistributePointsToEqualize(skillLevel, willpowerLevel, luckPointsToDistribute, out luckPointsToSkillLevel, out luckPointsToWillpowerLevel);
                    else
                    {
                        if (skillLevel < willpowerLevel)
                            luckPointsToSkillLevel = luckPointsToDistribute;
                        else
                            luckPointsToWillpowerLevel = luckPointsToDistribute;
                    }                
                                       
                    casterLevel = Mathf.Max(1, Mathf.Min(skillLevel + luckPointsToSkillLevel, willpowerLevel+luckPointsToWillpowerLevel));

                    if (SendInfoMessagesOnPCSpellLevel)
                        SilentMessage("Player-cast " + effect.Properties.Key + " effect (" + relevantSkill + ") level calculated: " + " OVERALL = " + casterLevel +
                        ", SKILL: " + skillLevel+ ((luckPointsToSkillLevel>=0) ? "+" : "") +luckPointsToSkillLevel +
                        ", WILLPOWER = " + willpowerLevel+ ((luckPointsToWillpowerLevel >= 0) ? "+" : "") + luckPointsToWillpowerLevel+"");
                }               // made player-cast effect info messages silent (found them to be quite disturbing during gameplay) - enemy spells will still be visible on the HUd too if relevant option ticked
                else
                {
                    casterLevel = caster.Entity.Level;
                    if (SendInfoMessagesOnNonPCSpellLevel)                    
                        Message("Non-player-cast " + effect.Properties.Key + " effect (" + relevantSkill + ") level: " + casterLevel);                    
                }
            }        

            return casterLevel;
        }

        /// <summary>
        /// This is safe to call even if we are not enforcing XP Tallies, but there is no point in doing it.
        /// </summary>
        /// <param name="effectKey"></param>
        /// <param name="targetType"></param>
        /// <param name="elementType"></param>
        /// <returns></returns>
        public static float CalculateCombinedXPTalliesCoefficient(string effectKey, TargetTypes targetType, ElementTypes elementType)
        {
            // the easiest way we can get XP Tallies not to have a meaningful effect in spell casting is to have the code return 1.0 here, so:
            if (!ApplyExperienceTallies)
                return 1.0f;
            else
            {
                int effectCoefficient = MMMXPTallies.GetEffectCoefficient(effectKey);
                int elementCoefficient = MMMXPTallies.GetElementCoefficient(elementType);
                int targetTypeCoefficient = MMMXPTallies.GetTargetTypeCoefficient(MMMXPTallies.GetMMMTargetType(targetType, effectKey));
                float combinedXPTalliesCoefficient = (float)effectCoefficient * elementCoefficient * targetTypeCoefficient;
                return (float)combinedXPTalliesCoefficient / 1000000f;
            }
        }

        public static int CalculateLearningTimeCostFromMinutes (int rankInGuild, int minutes, bool spellMaking = false)
        {
            int valueToReturn;
            if (rankInGuild < 0)
                valueToReturn = 250 * ((minutes + 14) / 15);
            else
                valueToReturn = 125 * ((minutes + 8) / 15) * ((11-rankInGuild) / 10);

            if (spellMaking)
                valueToReturn *= 2;

            SilentMessage("CalculateLearningTimeCostFromMinutes({0},{1},{2})={3}", rankInGuild, minutes, spellMaking, valueToReturn);
            return valueToReturn;
        }


        /// <summary>
        /// The version to use in SpellBook, SpellMaker etc - where we know it is the player entity that is involved
        /// If XP tallies are not enforced, passes arguments to routine from core game
        /// </summary>
        /// <param name="effectEntries"></param>
        /// <param name="targetType"></param>
        /// <param name="casterEntity"></param>
        /// <param name="minimumCastingCost"></param>
        /// <returns></returns>
        public static FormulaHelper.SpellCost CalculateTotalEffectCosts_XP(EffectEntry[] effectEntries, TargetTypes targetType, ElementTypes elementType, DaggerfallEntity casterEntity = null, bool minimumCastingCost = false)
        {
            if (!ApplyExperienceTallies)
            {
                return CalculateTotalEffectCosts(effectEntries, targetType, casterEntity, minimumCastingCost);
            }

            if (elementType == ElementTypes.None)
            {
                SilentMessage("StackTrace:" + Environment.StackTrace);
            }
            FormulaHelper.SpellCost totalCost;
            totalCost.goldCost = 0;
            totalCost.spellPointCost = 0;
            int PlayerIntelligence = GameManager.Instance.PlayerEntity.Stats.GetLiveStatValue(DFCareer.Stats.Intelligence);

            // Must have effect entries
            if (effectEntries == null || effectEntries.Length == 0)
                return totalCost;            

            // Add costs for each active effect slot
            for (int i = 0; i < effectEntries.Length; i++)
            {
                if (string.IsNullOrEmpty(effectEntries[i].Key))
                    continue;

                IEntityEffect effectTemplate = GameManager.Instance.EntityEffectBroker.GetEffectTemplate(effectEntries[i].Key);

                if (effectTemplate == null)
                    continue;

                DFCareer.Skills magicSkill = (DFCareer.Skills)effectTemplate.Properties.MagicSkill;

                FormulaHelper.SpellCost partialCost = CalculateEffectCosts_XP(GameManager.Instance.EntityEffectBroker.GetEffectTemplate(effectEntries[i].Key),
                    effectEntries[i].Settings, targetType, elementType, casterEntity);
                totalCost.goldCost += partialCost.goldCost;
                totalCost.spellPointCost += partialCost.spellPointCost;
            }           

            // Multipliers for target type
            totalCost.goldCost = FormulaHelper.ApplyTargetCostMultiplier(totalCost.goldCost, targetType);
            totalCost.spellPointCost = FormulaHelper.ApplyTargetCostMultiplier(totalCost.spellPointCost, targetType);

            // Set vampire spell cost
            if (minimumCastingCost)
                totalCost.spellPointCost = castCostFloor;
            

            // Enforce minimum
            if (totalCost.spellPointCost < castCostFloor)
                totalCost.spellPointCost = castCostFloor;

            return totalCost;
        }

        /// <summary>
        /// The version to use for SpellBook, SpellMaker etc where we know it is the player who is involved.
        /// If XP Tallies are not enforced, passes arguments to routine from core game
        /// </summary>
        public static FormulaHelper.SpellCost CalculateEffectCosts_XP(IEntityEffect effect, EffectSettings settings, TargetTypes targetType, ElementTypes elementType,
            DaggerfallEntity casterEntity = null)
        {
            if (!ApplyExperienceTallies)
                return OriginalCalculateEffectCosts(effect, settings, casterEntity);
                    // if XP Tallies are not enforced, fall back to original routines
            
            bool activeComponents = false;

            if (elementType == ElementTypes.None)
            {
                SilentMessage("StackTrace:" + Environment.StackTrace);
            }

            // Get related skill
            int skillValue = 0;
            if (casterEntity == null)
            {
                // From player
                int basicSkillValue = GameManager.Instance.PlayerEntity.Skills.GetLiveSkillValue((DFCareer.Skills)effect.Properties.MagicSkill);
                float coeff = CalculateCombinedXPTalliesCoefficient(effect.Properties.Key, targetType, elementType);
                            // CalculateEffectCosts_XP should not be called if Skill Tallies are not enforced
                skillValue = (int)Math.Floor((float)basicSkillValue * coeff);
                if (MTSpellBookWindow.logSkillCalculationsDuringWindowInit)
                    SilentMessage(string.Format("CalculateEffectCosts for Player: {0} effect, {1} TargetType, {2} element: basicskill={3}, coefficient={4}, resultingskill={5}",
                        effect.Key, targetType, elementType, basicSkillValue, coeff, skillValue));                   

            }
            else
            {
                // From another entity
                skillValue = casterEntity.Skills.GetLiveSkillValue((DFCareer.Skills)effect.Properties.MagicSkill);
            }

            // Duration costs
            int durationGoldCost = 0;
            if (effect.Properties.SupportDuration)
            {
                activeComponents = true;
                durationGoldCost = GetEffectComponentCosts(
                    effect.Properties.DurationCosts,
                    settings.DurationBase,
                    settings.DurationPlus,
                    settings.DurationPerLevel,
                    skillValue);

                //Debug.LogFormat("Duration: gold {0} spellpoints {1}", durationGoldCost, durationSpellPointCost);
            }

            // Chance costs
            int chanceGoldCost = 0;
            if (effect.Properties.SupportChance)
            {
                activeComponents = true;
                chanceGoldCost = GetEffectComponentCosts(
                    effect.Properties.ChanceCosts,
                    settings.ChanceBase,
                    settings.ChancePlus,
                    settings.ChancePerLevel,
                    skillValue);

                //Debug.LogFormat("Chance: gold {0} spellpoints {1}", chanceGoldCost, chanceSpellPointCost);
            }

            // Magnitude costs
            int magnitudeGoldCost = 0;
            if (effect.Properties.SupportMagnitude)
            {
                activeComponents = true;
                int magnitudeBase = (settings.MagnitudeBaseMax + settings.MagnitudeBaseMin) / 2;
                int magnitudePlus = (settings.MagnitudePlusMax + settings.MagnitudePlusMin) / 2;
                magnitudeGoldCost = GetEffectComponentCosts(
                    effect.Properties.MagnitudeCosts,
                    magnitudeBase,
                    magnitudePlus,
                    settings.MagnitudePerLevel,
                    skillValue);

                //Debug.LogFormat("Magnitude: gold {0} spellpoints {1}", magnitudeGoldCost, magnitudeSpellPointCost);
            }

            // If there are no active components (e.g. Teleport) then fudge some costs
            // This gives the same casting cost outcome as classic and supplies a reasonable gold cost
            // Note: Classic does not assign a gold cost when a zero-component effect is the only effect present, which seems like a bug
            int fudgeGoldCost = 0;
            if (!activeComponents)
                fudgeGoldCost = GetEffectComponentCosts(BaseEntityEffect.MakeEffectCosts(60, 100, 160), 1, 1, 1, skillValue);
                        // TODO: optional Legacy Advanced Teleportation cost increase could be enforced here

            // Add gold costs together and calculate spellpoint cost from the result
            FormulaHelper.SpellCost effectCost;
            effectCost.goldCost = durationGoldCost + chanceGoldCost + magnitudeGoldCost + fudgeGoldCost;

            int effectiveSkillValue = 0;

            if (skillValue <= 95) effectiveSkillValue = skillValue;                     // if up to 95, no change
            if (skillValue > 95) effectiveSkillValue = 95 + ((skillValue - 95) / 3);      // if over 95, spell effect costs should not go down as rapidly,
            if (effectiveSkillValue > 109) effectiveSkillValue = 109;                   //  instead, skill=137 and over should give a (110 - effectiveSkillValue) = 1

            // SilentMessage("Effective Skill Value (effect key="+ effect.Key+") calculated = " + effectiveSkillValue); // turning off FOR ANOTHER TEST

            effectCost.spellPointCost = effectCost.goldCost * (110 - effectiveSkillValue) / 400;

            //Debug.LogFormat("Costs: gold {0} spellpoints {1}", finalGoldCost, finalSpellPointCost);
            return effectCost;
        }

        /// <summary>
        /// The original routine from FormulaHelper
        /// </summary>
        public static FormulaHelper.SpellCost OriginalCalculateEffectCosts(IEntityEffect effect, EffectSettings settings, DaggerfallEntity casterEntity = null)
        {       
            bool activeComponents = false;

            // Get related skill
            int skillValue = 0;
            if (casterEntity == null)
            {
                // From player
                skillValue = GameManager.Instance.PlayerEntity.Skills.GetLiveSkillValue((DFCareer.Skills)effect.Properties.MagicSkill);
            }
            else
            {
                // From another entity
                skillValue = casterEntity.Skills.GetLiveSkillValue((DFCareer.Skills)effect.Properties.MagicSkill);
            }

            // Duration costs
            int durationGoldCost = 0;
            if (effect.Properties.SupportDuration)
            {
                activeComponents = true;
                durationGoldCost = GetEffectComponentCosts(
                    effect.Properties.DurationCosts,
                    settings.DurationBase,
                    settings.DurationPlus,
                    settings.DurationPerLevel,
                    skillValue);

                //Debug.LogFormat("Duration: gold {0} spellpoints {1}", durationGoldCost, durationSpellPointCost);
            }

            // Chance costs
            int chanceGoldCost = 0;
            if (effect.Properties.SupportChance)
            {
                activeComponents = true;
                chanceGoldCost = GetEffectComponentCosts(
                    effect.Properties.ChanceCosts,
                    settings.ChanceBase,
                    settings.ChancePlus,
                    settings.ChancePerLevel,
                    skillValue);

                //Debug.LogFormat("Chance: gold {0} spellpoints {1}", chanceGoldCost, chanceSpellPointCost);
            }

            // Magnitude costs
            int magnitudeGoldCost = 0;
            if (effect.Properties.SupportMagnitude)
            {
                activeComponents = true;
                int magnitudeBase = (settings.MagnitudeBaseMax + settings.MagnitudeBaseMin) / 2;
                int magnitudePlus = (settings.MagnitudePlusMax + settings.MagnitudePlusMin) / 2;
                magnitudeGoldCost = GetEffectComponentCosts(
                    effect.Properties.MagnitudeCosts,
                    magnitudeBase,
                    magnitudePlus,
                    settings.MagnitudePerLevel,
                    skillValue);

                //Debug.LogFormat("Magnitude: gold {0} spellpoints {1}", magnitudeGoldCost, magnitudeSpellPointCost);
            }

            // If there are no active components (e.g. Teleport) then fudge some costs
            // This gives the same casting cost outcome as classic and supplies a reasonable gold cost
            // Note: Classic does not assign a gold cost when a zero-component effect is the only effect present, which seems like a bug
            int fudgeGoldCost = 0;
            if (!activeComponents)
                fudgeGoldCost = GetEffectComponentCosts(BaseEntityEffect.MakeEffectCosts(60, 100, 160), 1, 1, 1, skillValue);

            // Add gold costs together and calculate spellpoint cost from the result
            FormulaHelper.SpellCost effectCost;
            effectCost.goldCost = durationGoldCost + chanceGoldCost + magnitudeGoldCost + fudgeGoldCost;
            effectCost.spellPointCost = effectCost.goldCost * (110 - skillValue) / 400;

            //Debug.LogFormat("Costs: gold {0} spellpoints {1}", finalGoldCost, finalSpellPointCost);
            return effectCost;
        }

        /// <summary>
        /// An override for Spell Casting Cost Calculation - the only two differenes there should be is that (1) the floor will not be 5, but a user-set value and (2) our effect-cost calculator will be used
        /// </summary>
        public static FormulaHelper.SpellCost CalculateTotalEffectCosts(EffectEntry[] effectEntries, TargetTypes targetType, DaggerfallEntity casterEntity = null, bool minimumCastingCost = false)
        {
            FormulaHelper.SpellCost totalCost;
            totalCost.goldCost = 0;
            totalCost.spellPointCost = 0;
            int PlayerIntelligence = GameManager.Instance.PlayerEntity.Stats.GetLiveStatValue(DFCareer.Stats.Intelligence);

            // Must have effect entries
            if (effectEntries == null || effectEntries.Length == 0)
                return totalCost;            

            // Add costs for each active effect slot
            for (int i = 0; i < effectEntries.Length; i++)
            {
                if (string.IsNullOrEmpty(effectEntries[i].Key))
                    continue;

                IEntityEffect effectTemplate = GameManager.Instance.EntityEffectBroker.GetEffectTemplate(effectEntries[i].Key);

                if (effectTemplate == null)
                    continue;

                DFCareer.Skills magicSkill = (DFCareer.Skills) effectTemplate.Properties.MagicSkill;

                FormulaHelper.SpellCost partialCost = FormulaHelper.CalculateEffectCosts(effectEntries[i], casterEntity);                 
                totalCost.goldCost += partialCost.goldCost;
                totalCost.spellPointCost += partialCost.spellPointCost;               

            }           

            // Multipliers for target type
            totalCost.goldCost = FormulaHelper.ApplyTargetCostMultiplier(totalCost.goldCost, targetType);
            totalCost.spellPointCost = FormulaHelper.ApplyTargetCostMultiplier(totalCost.spellPointCost, targetType);

            // Set vampire spell cost
            if (minimumCastingCost)
                totalCost.spellPointCost = castCostFloor;            

            // Enforce minimum
            if (totalCost.spellPointCost < castCostFloor)
                totalCost.spellPointCost = castCostFloor;            

            return totalCost;
        }

        static int trunc(double value) { return (int)Math.Truncate(value); }

        static int GetEffectComponentCosts(
            EffectCosts costs,
            int starting,
            int increase,
            int perLevel,
            int skillValue)
        {
            //Calculate effect gold cost, spellpoint cost is calculated from gold cost after adding up for duration, chance and magnitude
            return trunc(costs.OffsetGold + costs.CostA * starting + costs.CostB * trunc(increase / perLevel));
        }

        /// <summary>
        /// Should not be called if we are not enforcing XP Tallies
        /// </summary>
        /// <returns></returns>
        public static EffectBundleSettings GuessWhichSpellWeAreCasting()
        {
            SilentMessage("Trying to guess which spell is concerned.");

            if (activeSpellMakerWindow!=null)
            {
                EffectBundleSettings bundleToReturn = activeSpellMakerWindow.ActualStateOfTheSpellCreated();
                string message = "Since activeSpellMakerWindow is true, we will go with the spell from the active MTSpellMakerWindow that has the following effects: ";
                for (int i = 0; i < bundleToReturn.Effects.Length; i++)
                    message += " "+bundleToReturn.Effects[i].Key;
                SilentMessage(message);
                return bundleToReturn;
            }

            if (MTSpellBookWindow.spellInTheProcess)
            {
                SilentMessage("Since MTSpellBookWindow.spellInTheProcess is true, we are apparently in the process of starting a spellcast via SpellBookWindow. Spell is: "
                    + MTSpellBookWindow.chosenSpell.Name);
                return MTSpellBookWindow.chosenSpell;
            }

            if (GameManager.Instance.PlayerEffectManager.ReadySpell != null)
            {
                SilentMessage("Since GameManager.Instance.PlayerEffectManager.ReadySpell is not null, it appears we are in the process of casting a spell. Returning ReadySpell: " + GameManager.Instance.PlayerEffectManager.ReadySpell.Settings.Name);
                return GameManager.Instance.PlayerEffectManager.ReadySpell.Settings;
            }

            SilentMessage("Our last guess is that we are in the process of casting a spell, but ReadySpell is already null for some reason. Trying LastSpell: "
                + GameManager.Instance.PlayerEffectManager.LastSpell.Settings.Name);
            return GameManager.Instance.PlayerEffectManager.LastSpell.Settings;     // NOTE: if this guess is bad, the game might hang
        }

        /// <summary>
        /// Calculates effect costs from an IEntityEffect and custom settings.
        /// </summary>
        public static FormulaHelper.SpellCost CalculateEffectCosts(IEntityEffect effect, EffectSettings settings, DaggerfallEntity casterEntity = null)
        {
            if (ApplyExperienceTallies)
            {
                //SilentMessage("CalculateEffectCosts called.");
                EffectBundleSettings guessedSpell = GuessWhichSpellWeAreCasting();
                TargetTypes targetType = guessedSpell.TargetType;
                ElementTypes elementType = guessedSpell.ElementType;       // a thought: maybe screen here for player-cast spells and pass on to XP only those where caster=player            

                return CalculateEffectCosts_XP(effect, settings, targetType, elementType, casterEntity);
            }
            else
                return OriginalCalculateEffectCosts(effect, settings, casterEntity);    // if there are no tallies being enforced, we should use the original routine
        }

        /// <summary>
        /// Generates health for enemy classes based on level and class, using a given random number generator
        /// </summary>
        public static int RollEnemyClassMaxHealth(int level, int hitPointsPerLevel, System.Random ourNumberGenerator)
        { 
            const int baseHealth = 10;
            int maxHealth = baseHealth;

            for (int i = 0; i < level; i++)
            {
                maxHealth += ourNumberGenerator.Next(1, hitPointsPerLevel + 1);
            }
            return maxHealth;
        }       

        public static int CalculateSpellCreationTimeCost(EffectBundleSettings spell, bool spellMaker=false)
        {

            SilentMessage("CalculateSpellCreationTimeCost called with spellMaker=" + spellMaker);
            FormulaHelper.SpellCost tmpSpellCost = MMMFormulaHelper.CalculateTotalEffectCosts_XP(spell.Effects, spell.TargetType, spell.ElementType);
            double magickaCost = tmpSpellCost.spellPointCost;
            
            float spellMakerCoefficient = 1.0f;
            if (spellMaker)
                spellMakerCoefficient = spellMakerCoefficientFromSettings;      

            SilentMessage("magickaCost for the spell calculated: " + magickaCost+ ", spellMakerCoefficient: "+spellMakerCoefficient);

            // If SpellpointCost == Intelligence, TargetType and Element maximally familiar and effect unknown, the returned time period should be 10 hours = 600 minutes
            // If SpellpointCost < INT / 5, the returned time period should be 24 minutes                   (1)   f(1/5) = 24
            // the transition should not be linear, rather parabolic or hyperbolic                              f(x) = 600 * x^2
            // the above serves only as hints as to how I designed the function - the number have been tweaked since then

            int ourLiveIntelligence = GameManager.Instance.PlayerEntity.Stats.LiveIntelligence;
            double intelligenceRatio = magickaCost / (double)ourLiveIntelligence;                 

            int spellLearningTimeCost = trunc(Math.Max(600 * intelligenceRatio * intelligenceRatio, 12)*spellMakerCoefficient);            

            SilentMessage("LiveINT=" + ourLiveIntelligence + ", magickaCost=" + magickaCost +
                ", IntelligenceRatio=" + intelligenceRatio + ", TimeCost=" + spellLearningTimeCost);
            return spellLearningTimeCost;
        }        
        
    }
}
