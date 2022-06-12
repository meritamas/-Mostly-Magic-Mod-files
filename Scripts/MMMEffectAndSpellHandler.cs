// Project:         MeriTamas's (Mostly) Magic Mod for Daggerfall Unity (http://www.dfworkshop.net)
// Copyright:       Copyright (C) 2022 meritamas
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Author:          meritamas (meritamas@outlook.com)

#region using statements
using System;
using System.Collections.Generic;
using UnityEngine;
using DaggerfallConnect;
using DaggerfallConnect.Arena2;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.Formulas;
using DaggerfallWorkshop.Game.Guilds;
using DaggerfallWorkshop.Game.MagicAndEffects;
using Wenzil.Console;
#endregion

namespace MTMMM
{
    public static class MMMEffectAndSpellHandler
    {
        #region Confidentiality Levels Fields
        public static int DefaultConfidentialityLevel = 3;         // if there is no concrete confidentiality level defined for the given effect/guild, this value is used
        static Dictionary<string, int> defaultConfidentialityLevels = new Dictionary<string, int> {
            // NOTE: confidentiality = 0 means it can be provided to non-members (-1), conf of 1 or above means just members
            { "CreateItem", 3 },          { "Teleport-Effect", 0 },      { "FixItem", 1 },              { "MultiplyProvisions", 1 },        // without group, TODO: set up group
            { "Chameleon-Normal", 0 },    { "Chameleon-True", 6 },                                      { "Charm", 0 },                 { "Climbing", 0 },
            { "ComprehendLanguages", 0 },                                { "ContinuousDamage-Fatigue", 5 }, { "ContinuousDamage-SpellPoints", 5 }, { "ContinuousDamage-Health", 5 },
            { "Cure-Poison", 0 },         { "Cure-Paralyzation", 0 },    { "Cure-Disease", 4 },
            { "Damage-Fatigue", 1 },      { "Damage-Health", 0 },        { "Damage-SpellPoints", 3 },
            { "Detect-Enemy", 3 },        { "Detect-Magic", 3 },         { "Detect-Treasure", 3 },
            { "Disintegrate", 8 },                                       { "Dispel-Daedra", 8 },        { "Dispel-Magic", 0 },          { "Dispel-Undead", 6 },
            { "Drain-Agility", 4 },       { "Drain-Endurance", 4 },      { "Drain-Intelligence", 9 },   { "Drain-Luck", 4 },
            { "Drain-Personality", 6 },   { "Drain-Speed", 4 },          { "Drain-Strength", 4 },       { "Drain-Willpower", 9 },
            { "ElementalResistance-Frost", 1 }, { "ElementalResistance-Fire", 1 }, { "ElementalResistance-Poison", 1 }, { "ElementalResistance-Shock", 1 },
            { "Fortify-Agility", 3 },     { "Fortify-Endurance", 3 },    { "Fortify-Intelligence", 7 }, { "Fortify-Luck", 3 },
            { "Fortify-Personality", 5 }, { "Fortify-Speed", 3 },        { "Fortify-Strength", 3 },     { "Fortify-Willpower", 7 },
            { "FreeAction", 0 },          { "Heal-Health", 0 },          { "Heal-Fatigue", 0 },
            { "Heal-Agility", 1 },        { "Heal-Endurance", 1 },       { "Heal-Intelligence", 3 },    { "Heal-Luck", 1 },
            { "Heal-Personality", 2 },    { "Heal-Speed", 1 },           { "Heal-Strength", 1 },        { "Heal-Willpower", 3 },
            { "Identify", 4 },                                           { "Invisibility-Normal", 0 },  { "Invisibility-True", 6 },
            { "Jumping", 0 },             { "Levitate", 1 },             { "Light", 0 },                { "Lock", 1 },
            { "MageLight-Inferno", 1 },   { "MageLight-Rime", 1 },       { "MageLight-Venom", 1 },      { "MageLight-Storm", 1 },              { "MageLight-Arcane", 1 },
            { "Open", 1 },                { "Pacify-Animal", 0 },        { "Pacify-Undead", 4 },        { "Pacify-Humanoid", 3 },              { "Pacify-Daedra", 7 },
            { "Paralyze", 4 },            { "Regenerate", 5 },           { "Shadow-Normal", 0 },        { "Shadow-True", 6 },
            { "Shield", 0 },              { "Silence", 6 },              { "Slowfall", 0 },             { "SoulTrap", 5 },
            { "SpellAbsorption", 5 },     { "SpellReflection", 5 },      { "SpellResistance", 5 },
            { "Transfer-Agility", 6 },    { "Transfer-Endurance", 6 },   { "Transfer-Intelligence", 11 }, { "Transfer-Luck", 6 },
            { "Transfer-Personality", 8 }, { "Transfer-Speed", 6 },      { "Transfer-Strength", 6 },    { "Transfer-Willpower", 11 },
            { "Transfer-Health", 3 },     { "Transfer-Fatigue", 3 },
            { "WaterWalking", 1 },        { "WaterBreathing", 0 }
        };



        static Dictionary<uint, Dictionary<string, int>> ConfidentialityLevels = new Dictionary<uint, Dictionary<string, int>>();
        #endregion
        #region Spell Instructor Themes Fields
        public static bool applyThemes;
        public static bool applyConfidentialityLevels;

        static List<string> reservedEffects = new List<string>();

        public struct EffectAvailabilityPrescription
        {
            public string Region;
            public string Location;
            public uint Guild;
            public string EffectKey;
        };

        static List<EffectAvailabilityPrescription> effectAvailabilityPrescriptions = new List<EffectAvailabilityPrescription>();

        // TODO: Double-check deity arrays 

        static DFCareer.MagicSkills[] AkatoshArray = { DFCareer.MagicSkills.Restoration,
        DFCareer.MagicSkills.Destruction, DFCareer.MagicSkills.Destruction, DFCareer.MagicSkills.Destruction, DFCareer.MagicSkills.Destruction, DFCareer.MagicSkills.Destruction, DFCareer.MagicSkills.Destruction,
                        DFCareer.MagicSkills.Illusion,
                        DFCareer.MagicSkills.Thaumaturgy,
        DFCareer.MagicSkills.Alteration, DFCareer.MagicSkills.Alteration, DFCareer.MagicSkills.Alteration, DFCareer.MagicSkills.Alteration, DFCareer.MagicSkills.Alteration, DFCareer.MagicSkills.Alteration,
                        DFCareer.MagicSkills.Mysticism };    // For Akatosh: 1 Rest, 6 Dest, 1 Ill, 1 Thau, 6 Alt, 1 Myst
        static DFCareer.MagicSkills[] ArkayArray = {
        DFCareer.MagicSkills.Restoration, DFCareer.MagicSkills.Restoration, DFCareer.MagicSkills.Restoration, DFCareer.MagicSkills.Restoration, DFCareer.MagicSkills.Restoration, DFCareer.MagicSkills.Restoration,
        DFCareer.MagicSkills.Destruction, DFCareer.MagicSkills.Destruction, DFCareer.MagicSkills.Destruction, DFCareer.MagicSkills.Destruction, DFCareer.MagicSkills.Destruction, DFCareer.MagicSkills.Destruction,
                        DFCareer.MagicSkills.Illusion,
                        DFCareer.MagicSkills.Thaumaturgy,
                        DFCareer.MagicSkills.Alteration,
                        DFCareer.MagicSkills.Mysticism };           // For Arkay: 6 Rest, 6 Dest, 1 Ill, 1 Thau, 1 Alt, 1 Myst
        static DFCareer.MagicSkills[] DibellaArray = {
        DFCareer.MagicSkills.Restoration, DFCareer.MagicSkills.Restoration, DFCareer.MagicSkills.Restoration, DFCareer.MagicSkills.Restoration, DFCareer.MagicSkills.Restoration, DFCareer.MagicSkills.Restoration,
                        DFCareer.MagicSkills.Destruction,
        DFCareer.MagicSkills.Illusion, DFCareer.MagicSkills.Illusion, DFCareer.MagicSkills.Illusion, DFCareer.MagicSkills.Illusion, DFCareer.MagicSkills.Illusion, DFCareer.MagicSkills.Illusion,
                        DFCareer.MagicSkills.Thaumaturgy,
                        DFCareer.MagicSkills.Alteration,
                        DFCareer.MagicSkills.Mysticism };           // For Dibella: 6 Rest, 1 Dest, 6 Ill, 1 Thau, 1 Alt, 1 Myst
        static DFCareer.MagicSkills[] JulianosArray = { DFCareer.MagicSkills.Restoration,
                        DFCareer.MagicSkills.Destruction,
                        DFCareer.MagicSkills.Illusion,
                        DFCareer.MagicSkills.Thaumaturgy, DFCareer.MagicSkills.Thaumaturgy, DFCareer.MagicSkills.Thaumaturgy, DFCareer.MagicSkills.Thaumaturgy,
                        DFCareer.MagicSkills.Alteration, DFCareer.MagicSkills.Alteration, DFCareer.MagicSkills.Alteration, DFCareer.MagicSkills.Alteration,
                        DFCareer.MagicSkills.Mysticism, DFCareer.MagicSkills.Mysticism, DFCareer.MagicSkills.Mysticism, DFCareer.MagicSkills.Mysticism, DFCareer.MagicSkills.Mysticism
                        };                                          // For Julianos: 1 Rest, 1 Dest, 1 Ill, 4 Thau, 4 Alt, 5 Myst
        static DFCareer.MagicSkills[] StendarrArray = {
        DFCareer.MagicSkills.Restoration, DFCareer.MagicSkills.Restoration, DFCareer.MagicSkills.Restoration, DFCareer.MagicSkills.Restoration, DFCareer.MagicSkills.Restoration, DFCareer.MagicSkills.Restoration,
        DFCareer.MagicSkills.Restoration, DFCareer.MagicSkills.Restoration, DFCareer.MagicSkills.Restoration, DFCareer.MagicSkills.Restoration, DFCareer.MagicSkills.Restoration,
                        DFCareer.MagicSkills.Destruction,
                        DFCareer.MagicSkills.Illusion,
                        DFCareer.MagicSkills.Thaumaturgy,
                        DFCareer.MagicSkills.Alteration,
                        DFCareer.MagicSkills.Mysticism        };        // For Stendarr: 11 Rest, 1 Dest, 1 Ill, 1 Thau, 1 Alt, 1 Myst
        static DFCareer.MagicSkills[] KynarethArray = { DFCareer.MagicSkills.Restoration,
        DFCareer.MagicSkills.Destruction, DFCareer.MagicSkills.Destruction, DFCareer.MagicSkills.Destruction, DFCareer.MagicSkills.Destruction, DFCareer.MagicSkills.Destruction, DFCareer.MagicSkills.Destruction,
        DFCareer.MagicSkills.Illusion, DFCareer.MagicSkills.Illusion, DFCareer.MagicSkills.Illusion, DFCareer.MagicSkills.Illusion, DFCareer.MagicSkills.Illusion, DFCareer.MagicSkills.Illusion,
                        DFCareer.MagicSkills.Thaumaturgy,
                        DFCareer.MagicSkills.Alteration,
                        DFCareer.MagicSkills.Mysticism };               // For Kynareth: 1 Rest, 6 Dest, 6 Ill, 1 Thau, 1 Alt, 1 Myst
        static DFCareer.MagicSkills[] MaraArray = {
        DFCareer.MagicSkills.Restoration, DFCareer.MagicSkills.Restoration, DFCareer.MagicSkills.Restoration, DFCareer.MagicSkills.Restoration, DFCareer.MagicSkills.Restoration, DFCareer.MagicSkills.Restoration,
                        DFCareer.MagicSkills.Destruction,
        DFCareer.MagicSkills.Illusion, DFCareer.MagicSkills.Illusion, DFCareer.MagicSkills.Illusion, DFCareer.MagicSkills.Illusion, DFCareer.MagicSkills.Illusion, DFCareer.MagicSkills.Illusion,
                        DFCareer.MagicSkills.Thaumaturgy,
                        DFCareer.MagicSkills.Alteration,
                        DFCareer.MagicSkills.Mysticism };                // For Mara: 6 Rest, 1 Dest, 6 Ill, 1 Thau, 1 Alt, 1 Myst
        static DFCareer.MagicSkills[] ZenitharArray ={ DFCareer.MagicSkills.Restoration,       
                        DFCareer.MagicSkills.Destruction,
                        DFCareer.MagicSkills.Illusion,
        DFCareer.MagicSkills.Thaumaturgy, DFCareer.MagicSkills.Thaumaturgy, DFCareer.MagicSkills.Thaumaturgy, DFCareer.MagicSkills.Thaumaturgy, DFCareer.MagicSkills.Thaumaturgy, DFCareer.MagicSkills.Thaumaturgy,
        DFCareer.MagicSkills.Thaumaturgy, DFCareer.MagicSkills.Thaumaturgy, DFCareer.MagicSkills.Thaumaturgy, DFCareer.MagicSkills.Thaumaturgy, DFCareer.MagicSkills.Thaumaturgy,
                        DFCareer.MagicSkills.Alteration,
                        DFCareer.MagicSkills.Mysticism };               // For Zenithar: 1 Rest, 1 Dest, 1 Ill, 11 Thau, 1 Alt, 1 Myst
        static DFCareer.MagicSkills[] KynarethTempleArray = { DFCareer.MagicSkills.Restoration,
        DFCareer.MagicSkills.Restoration, DFCareer.MagicSkills.Restoration, DFCareer.MagicSkills.Restoration, DFCareer.MagicSkills.Restoration, DFCareer.MagicSkills.Restoration, DFCareer.MagicSkills.Restoration,
                        DFCareer.MagicSkills.Destruction, DFCareer.MagicSkills.Destruction, DFCareer.MagicSkills.Destruction,
                        DFCareer.MagicSkills.Illusion, DFCareer.MagicSkills.Illusion, DFCareer.MagicSkills.Illusion,
                        DFCareer.MagicSkills.Thaumaturgy,
                        DFCareer.MagicSkills.Alteration,
                        DFCareer.MagicSkills.Mysticism };           // For Kynareth-Temples: 7 Rest, 3 Dest, 3 Ill, 1 Thau, 1 Alt, 1 Myst 

        static Dictionary<int, DFCareer.MagicSkills[]> ThemeMagicSkills = new Dictionary<int, DFCareer.MagicSkills[]> {
            { 92, AkatoshArray },           // Akatosh                 
            { 82, ArkayArray },             // Arkay
            { 98, DibellaArray },           // Dibella
            { 94, JulianosArray },          // Julianos
            {106, StendarrArray },          // Stendarr
            { 36, KynarethArray },          // Kynareth
            { 88, MaraArray },              // Mara
            { 84, ZenitharArray }           // Zenithar
        };      // Deity codes used in MapsFile class: 36 Kynareth      82 Arkay        84 Zenithar     88 Mara     92 Akatosh      94 Julianos     98 Dibella      106 Stendarr

        #endregion
        // here come the Effect And Spell Offer Registration fields


        #region Confidentiality Levels methods
        /// <summary>
        /// Gets the appropriate effect confidentiality level at the given place based on the effect key and the ID of the faction/guild. 
        /// </summary>
        /// <param name="effectKey">The key of the effect.</param>
        /// <param name="teacherFactionID">The faction the teacher (the structure where the teacher is) belongs to.</param>
        /// <returns>If confidentiality levels are turned on, the confidentiality level. Else it returns 0. </returns>
        public static int GetEffectConfidentialityLevel (string effectKey, uint teacherFactionID)
        {
            if (!applyConfidentialityLevels)
            {
                SilentMessage("GetEffectConfidentialityLevel called. Since confidentiality levels are turned off, returning 0.");
                return 0;
            }

            Dictionary<string, int> FactionConfidentialityLevels;
            if (!ConfidentialityLevels.TryGetValue(teacherFactionID, out FactionConfidentialityLevels))
            {
                FactionConfidentialityLevels = new Dictionary<string, int>();
                foreach (KeyValuePair<string, int> kvp in defaultConfidentialityLevels)
                {
                    FactionConfidentialityLevels.Add(kvp.Key, kvp.Value);
                }   // This code creates a new dictionary for the given guild if no such dictionary existed and adds the values from the generic dictionary (a default on hard coded into the mod)  
                ConfidentialityLevels.Add(teacherFactionID, FactionConfidentialityLevels);
            }
                // at this point, FactionConfidentialityLevels points to the appropriate guild dictionary

            int confidentialityLevelToReturn = DefaultConfidentialityLevel;
            if (!FactionConfidentialityLevels.TryGetValue(effectKey, out confidentialityLevelToReturn))
            {
                FactionConfidentialityLevels.Add(effectKey, DefaultConfidentialityLevel);           // insert an entry with the default confidentiality level                
            }

            return confidentialityLevelToReturn;
        }

        public static int GetEffectConfidentialityLevelInLocalGuild(string effectKey)
        {
            return GetEffectConfidentialityLevel(effectKey, GameManager.Instance.PlayerEnterExit.FactionID);
        }

        public static bool IsEffectKnowableToPlayerHere (string effectKey)
        {
            uint factionID = GameManager.Instance.PlayerEnterExit.FactionID;            
            IGuild ownerGuild = GameManager.Instance.GuildManager.GetGuild((int)factionID);
            int playerRankInGuild = ownerGuild.Rank;
            return (playerRankInGuild >= GetEffectConfidentialityLevel(effectKey, factionID)-1);
                    // -1 so exactly those effects are available here whose one-effect spells are available in SpellBook
        }

        public static int GetSpellConfidentialityLevel(EffectBundleSettings spell, uint teacherFactionID)
        {
            int spellConfidentialityLevel = -1;

            if (applyConfidentialityLevels)
            {

                if (spell.Effects != null)
                    for (int i = 0; i < spell.Effects.Length; i++)
                    {
                        string effectKey = spell.Effects[i].Key;

                        int effectConfidentialityLevel = GetEffectConfidentialityLevel(effectKey, teacherFactionID);
                        SilentMessage(string.Format("Confidentiality Level {0} found for effect '{1}' at faction ID {2}", effectConfidentialityLevel, effectKey, teacherFactionID));
                        spellConfidentialityLevel += effectConfidentialityLevel;
                    }

                if (spell.LegacyEffects != null)
                    spellConfidentialityLevel += spell.LegacyEffects.Length;
            }
            else
            {
                SilentMessage("GetSpellConfidentialityLevel called. Since confidentiality levels are turned off, skipped the material part of the code and will be returning -1.");                
            }

            return spellConfidentialityLevel;
        }

            /// <summary>
            /// Returns the superdictionary object for the purposes of saving the game
            /// </summary>
            /// <returns>The superdictionary object the values in which are to be saved</returns>
            public static Dictionary<uint, Dictionary<string, int>> GetDictionaries()
        {
            return ConfidentialityLevels;
        }

        /// <summary>
        /// Initializes the superdictionary object in the process of Loading the game
        /// </summary>
        /// <param name="dictionaries">The loaded superdictionary object</param>
        public static void SetDictionaries(Dictionary<uint, Dictionary<string, int>> dictionaries)
        {
            if (dictionaries!=null)
                ConfidentialityLevels = dictionaries;
        }

        #endregion

        #region Spell Instructor Themes methods

        public static void AddEffectAvailabilityPrescription (EffectAvailabilityPrescription prescription)
        {
            effectAvailabilityPrescriptions.Add(prescription);
        }

        public static bool CanTheyTeachThisEffectHere(string effectKey)
        {
            string currentRegionName = GameManager.Instance.PlayerGPS.CurrentRegionName;
            string locationName = GameManager.Instance.PlayerGPS.CurrentLocation.Name;

            int currentRegionIndex = GameManager.Instance.PlayerGPS.CurrentRegionIndex;
            int locationIndex = GameManager.Instance.PlayerGPS.CurrentLocation.LocationIndex;

            uint ownerFactionID = GameManager.Instance.PlayerEnterExit.FactionID;
                            // first, we gather and process the data pertaining to the given location, then we continue with the effect-related data          

            IEntityEffect effectTemplate = GameManager.Instance.EntityEffectBroker.GetEffectTemplate(effectKey);
            DFCareer.MagicSkills relevantMagicSkill = effectTemplate.Properties.MagicSkill;
            int effectConfidentialityLevel = GetEffectConfidentialityLevel(effectKey, ownerFactionID);
                            // second, we got the magic skill and confidentiality level relevant to the given effect

            SilentMessage("Region: {0}-{1}, Location: {2}-{3}, Faction: {4}, Effect: {5} ({6}), Confidentiality Level: {7}.", currentRegionIndex, currentRegionName,
                locationIndex, locationName, ownerFactionID, effectKey, relevantMagicSkill, effectConfidentialityLevel);

            EffectAvailabilityPrescription prescription = new EffectAvailabilityPrescription { Region = currentRegionName, Location = locationName, Guild = ownerFactionID, EffectKey = effectKey };
            if (effectAvailabilityPrescriptions.Contains(prescription))
            {
                SilentMessage("Prescription found. Returning true.");
                return true;                    // if effect explicitely prescribed for this provider, return true
            }
            else
            {
                SilentMessage("Prescription NOT found.");
                if (reservedEffects.Contains(effectKey))        // otherwise if effect on reserved list, return false
                {
                    SilentMessage("This is a reserved effect. Returning false.");
                    return false;
                }
            }                                   // if neither prescribed, nor reserved, then continue

            if (effectConfidentialityLevel < 2)
            {
                SilentMessage("This is a low-confidentiality 'available at each hall' effect. Returning true.");
                return true;                // if it is an 'available everywhere' effect, return true here, otherwise continue            
            }

            int randomSeed = (int)(currentRegionIndex + locationIndex + ownerFactionID);           // as good a 'random seed' as any
            int magicSchoolDeterminer = randomSeed % 16;
            int availableHighestConfidentialityLevel = (randomSeed / 16) % 10 +2;

            int regionalDeityCode = MapsFile.RegionTemples[currentRegionIndex];
            DFCareer.MagicSkills themeMagicSkill = ThemeMagicSkills[regionalDeityCode][magicSchoolDeterminer];      // here we determine the 'theme' school of the place

            SilentMessage("Random seed: {0}, Magic School determiner: {1}, Available Highest Conf Level: {2}, regional deity: {3}, Theme: {4}", randomSeed, magicSchoolDeterminer,
                availableHighestConfidentialityLevel, regionalDeityCode, themeMagicSkill);

            if (relevantMagicSkill != themeMagicSkill)
            {
                SilentMessage("Relevant Magic Skill different than Theme, returning false.");
                return false;                      // if effect outside theme school, return false
            }

            SilentMessage("Returning {0}", (effectConfidentialityLevel <= availableHighestConfidentialityLevel));
            return (effectConfidentialityLevel <= availableHighestConfidentialityLevel);
                    // if effect inside theme school, return (effect conf level <= available highes conf level)           
        }

        public static bool CanTheyTeachThisSpellHere(EffectBundleSettings spell)
        {
            if (spell.Effects != null)
                for (int i = 0; i < spell.Effects.Length; i++)
                {         
                    if (CanTheyTeachThisEffectHere(spell.Effects[i].Key))
                    {
                        SilentMessage("CanTheyTeachThisSpellHere: Since effect '{0}' can be taught here, returning true for spell '{1}'.", spell.Effects[i].Key, spell.Name);
                        return true;
                    }                    
                }

            if (spell.LegacyEffects != null)
            {
                SilentMessage("CanTheyTeachThisSpellHere: Since spell '{0}' contains a legacy effect, returning true.", spell.Name);
                return true;
            }

            SilentMessage("CanTheyTeachThisSpellHere: Returning the default answer, which is false.");
            return false;
        }


        #endregion

        #region Spell Effect And Spell Offer Registration

        #endregion

        #region debug        
        static string messagePrefix = "MMMEffectAndSpellHandler: ";

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



    }

}
