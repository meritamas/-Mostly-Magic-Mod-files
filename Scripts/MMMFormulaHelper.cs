// Project:         MeriTamas's (Mostly) Magic Mod for Daggerfall Unity (http://www.dfworkshop.net)
// Copyright:       Copyright (C) 2021 meritamas
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Author:          meritamas (meritamas@outlook.com)
// Big thanks to Gavin Clayton (interkarma@dfworkshop.net) for his suggestion to create this class. This will make things easier and be an asset as the mod is developed furter.

/*
 * The code is sloppy at various places - this is partly due to the fact that this mod was created by merging three smaller mods.
 * Some things are redundant, comments are missing, some comments are not useful anymore etc.
 * I have the intention of cleaning it up in the future.
 * For now, it seems to work as intended or - let's rather say - reasonably well.
*/

using UnityEngine;
using DaggerfallConnect;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.Formulas;
using DaggerfallWorkshop.Game.MagicAndEffects;

namespace MTMMM
{
    public delegate void MTDebugMethod (string s);
    public delegate int GetSpellLevelMethod(IEntityEffect effect);    

    public static class MMMFormulaHelper
    {
        public static MTDebugMethod MMMFormulaHelperInfoMessage;
        public static bool SendInfoMessagesOnPCSpellLevel = false;
        public static bool SendInfoMessagesOnNonPCSpellLevel = false;        

        public static GetSpellLevelMethod GetSpellLevel = GetSpellLevel1;

        public static int castCostFloor;

        /// <summary>
        /// Calculate the Chance of Success of an effect using the active Spell Level Calculation method. 
        /// </summary>
        public static int GetSpellChance(BaseEntityEffect effect)
        {
            int spellLevel = GetSpellLevel(effect);
            DaggerfallEntityBehaviour caster = effect.Caster;
                        
            int chanceToReturn = effect.Settings.ChanceBase + effect.Settings.ChancePlus * (int)Mathf.Floor(spellLevel / effect.Settings.ChancePerLevel);

            if (MMMFormulaHelperInfoMessage != null)
                MMMFormulaHelperInfoMessage(effect.Properties.Key+". ChanceBase =" + effect.Settings.ChanceBase + ", ChancePlus=" + effect.Settings.ChancePlus + " :: " +
                    effect.Settings.ChanceBase + "+" + effect.Settings.ChancePlus + "*" + (int)Mathf.Floor(spellLevel / effect.Settings.ChancePerLevel) + " = " + chanceToReturn);

            //Debug.LogFormat("{5} ChanceValue {0} = base + plus * (level/chancePerLevel) = {1} + {2} * ({3}/{4})", settings.ChanceBase + settings.ChancePlus * (int)Mathf.Floor(casterLevel / settings.ChancePerLevel), settings.ChanceBase, settings.ChancePlus, casterLevel, settings.ChancePerLevel, Key);
            return chanceToReturn;
        }

        /// <summary>
        /// Calculate the Magnitude of an effect using the active Spell Level Calculation method. 
        /// </summary>
        public static int GetSpellMagnitude (BaseEntityEffect effect, EntityEffectManager manager)
        {
            if (effect.Caster == null)
                Debug.LogWarningFormat("GetMagnitude() for {0} has no caster. Using caster level 1 for magnitude.", effect.Properties.Key);

            if (manager == null)
                Debug.LogWarningFormat("GetMagnitude() for {0} has no parent manager.", effect.Properties.Key);

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

            if (MMMFormulaHelperInfoMessage != null)
                MMMFormulaHelperInfoMessage(messagePart + " (FINAL-MAG: "+ magnitude+")");

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

                if (MMMFormulaHelperInfoMessage != null)
                    MMMFormulaHelperInfoMessage(effect.Properties.Key + ". DurationBase =" + effect.Settings.DurationBase + ", DurationPlus=" + effect.Settings.DurationPlus + " :: " +
                        effect.Settings.DurationBase + "+" + effect.Settings.DurationPlus + "*" + (int)Mathf.Floor(spellLevel / effect.Settings.DurationPerLevel) + " = " + durationToReturn);

                return durationToReturn;
            }
            else
            {
                if (MMMFormulaHelperInfoMessage != null)
                    MMMFormulaHelperInfoMessage(effect.Properties.Key + ". Duration = 0");
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
            DaggerfallEntityBehaviour caster = effect.Caster;

            int casterLevel = 1;
            string relevantSkill = GetMagicSkillAbbrev(effect.Properties.MagicSkill);
            if (relevantSkill == null)  relevantSkill = "";

            if (caster)
            {
                if (caster == GameManager.Instance.PlayerEntityBehaviour)
                {
                    PlayerEntity playerEntity = GameManager.Instance.PlayerEntity;                                   
                      
                    int skillLevel = (playerEntity.Skills.GetLiveSkillValue((DFCareer.Skills)effect.Properties.MagicSkill) - 9) / 3;          // (Skill-9)/3
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

                    if ((SendInfoMessagesOnPCSpellLevel) && (MMMFormulaHelperInfoMessage != null))
                        MMMFormulaHelperInfoMessage("Player-cast " + effect.Properties.Key + " effect (" + relevantSkill + ") level calculated: " + " OVERALL = " + casterLevel +
                        ", SKILL: " + skillLevel+ ((luckPointsToSkillLevel>=0) ? "+" : "") +luckPointsToSkillLevel +
                        ", WILLPOWER = " + willpowerLevel+ ((luckPointsToWillpowerLevel >= 0) ? "+" : "") + luckPointsToWillpowerLevel+"");
                }
                else
                {
                    casterLevel = caster.Entity.Level;
                    if ((SendInfoMessagesOnNonPCSpellLevel) && (MMMFormulaHelperInfoMessage != null))                    
                        MMMFormulaHelperInfoMessage("Non-player-cast " + effect.Properties.Key + " effect (" + relevantSkill + ") level: " + casterLevel);                    
                }
            }        

            return casterLevel;
        }

        /// <summary>
        /// An override for Spell Casting Cost Calculation - the only differene there should be is that the floor will not 5, but a user-set value 
        /// </summary>
        public static FormulaHelper.SpellCost CalculateTotalEffectCosts(EffectEntry[] effectEntries, TargetTypes targetType, DaggerfallEntity casterEntity = null, bool minimumCastingCost = false)
        {
            FormulaHelper.SpellCost totalCost;
            totalCost.goldCost = 0;
            totalCost.spellPointCost = 0;

            // Must have effect entries
            if (effectEntries == null || effectEntries.Length == 0)
                return totalCost;

            // Add costs for each active effect slot
            for (int i = 0; i < effectEntries.Length; i++)
            {
                if (string.IsNullOrEmpty(effectEntries[i].Key))
                    continue;

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
    }
}
