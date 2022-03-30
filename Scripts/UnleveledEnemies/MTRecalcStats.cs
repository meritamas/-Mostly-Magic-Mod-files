// Project:         MeriTamas's (Mostly) Magic Mod for Daggerfall Unity (http://www.dfworkshop.net)
// Copyright:       Copyright (C) 2022 meritamas
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Author:          meritamas (meritamas@outlook.com)
// Credits due to LypyL for the modding tutorial based on which this class was created.
// Also, some parts were taken from or inspired by other people's work. DFU developers, other mod authors etc. Will try to give credits in the code where due. 

/*
 * The code is sloppy at various places - this is partly due to the fact that this mod was created by merging three smaller mods.
 * Some things are redundant, comments are missing, some comments are not useful anymore etc.
 * I have the intention of cleaning it up in the future.
 * For now, it seems to work as intended.
*/

using System.Collections.Generic;
using DaggerfallConnect.Arena2;
using UnityEngine;
using DaggerfallWorkshop.Utility;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Utility;
using DaggerfallWorkshop.Game.Utility.ModSupport;   //required for modding features
using DaggerfallWorkshop.Game.Utility.ModSupport.ModSettings;
using System.Collections;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.Formulas;
using DaggerfallWorkshop.Game.Items;
using DaggerfallWorkshop;
using DaggerfallConnect;

namespace MTMMM
{
    public class RecalcStats : MonoBehaviour
    {
        static public bool extraStrongMonsters = false;

        // From EnemyEntity.cs -- originally from FALL.EXE offset 0x1C0F14
        static byte[] ImpSpells = { 0x07, 0x0A, 0x1D, 0x2C };
        static byte[] GhostSpells = { 0x22 };
        static byte[] OrcShamanSpells = { 0x06, 0x07, 0x16, 0x19, 0x1F };
        static byte[] WraithSpells = { 0x1C, 0x1F };
        static byte[] FrostDaedraSpells = { 0x10, 0x14 };
        static byte[] FireDaedraSpells = { 0x0E, 0x19 };
        static byte[] DaedrothSpells = { 0x16, 0x17, 0x1F };
        static byte[] VampireSpells = { 0x33 };
        static byte[] SeducerSpells = { 0x34, 0x43 };
        static byte[] VampireAncientSpells = { 0x08, 0x32 };
        static byte[] DaedraLordSpells = { 0x08, 0x0A, 0x0E, 0x3C, 0x43 };
        static byte[] LichSpells = { 0x08, 0x0A, 0x0E, 0x22, 0x3C };
        static byte[] AncientLichSpells = { 0x08, 0x0A, 0x0E, 0x1D, 0x1F, 0x22, 0x3C };
        static byte[][] EnemyClassSpells = { FrostDaedraSpells, DaedrothSpells, OrcShamanSpells, VampireAncientSpells, DaedraLordSpells, LichSpells, AncientLichSpells };
                
        DaggerfallEntityBehaviour entityBehaviour;
        EnemyEntity entity;
        PlayerEntity playerEntity;
        System.Random ourNumberGenerator;

        int getItemMaterialRandomDeviation()
        {
            int randomNumber = ourNumberGenerator.Next(0, 1024);

            if (randomNumber < 512) return 1;
            if (randomNumber < 768) return 2;
            if (randomNumber < 896) return 3;
            if (randomNumber < 960) return 4;
            if (randomNumber < 992) return 5;
            if (randomNumber < 1008) return 6;
            if (randomNumber < 1016) return 7;
            if (randomNumber < 1020) return 8;
            if (randomNumber < 1022) return 9;
            if (randomNumber < 1023) return 10;
                return 11;
        }
            // from 1 to 13 
        static int clampItemCategory (int categoryNumber)
        {
            if (categoryNumber < 1) return 1;
            if (categoryNumber > 13) return 13;
            return categoryNumber;
        }

        int getRandomItemMaterial(int entityLevel)
        {/*
            What these things should mean.
            If there is no deviation from the central level, then the chances of getting [a lesser material, exactly that material, a higher material] should be [33, 34, 33]
            If there is a -1 deviation form the cenral level, then these chances should be [47, 27, 26], with a +1 deviation: [26, 27, 47]
            So, If say LVL11 means the dominant material should be steel, than
                    for a LVL10 opponent, you should get 47% chance to get a lesser material, 27% to get steel and 26% to get a higher material
                    for a LVL11 opponent, you should get 33% chance to get a lesser material, 34% to get steel and 33% to get a higher material
                    for a LLV12 opponent, you should get 26% chance to get a lesser material, 27% to get steel and 47% to get a higher material
            */
            int[] maximumLows = {   47,    33,    26 };
            int[] minimumHighs = {  47+27, 33+34, 26+27};      

            int smallerEqualOrLarger = ourNumberGenerator.Next(0, 100);        
            int closestLevelCategory = (entityLevel -1) / 3 + 1;               // LVL 2 is the center level of the first category (LVL 1-3), LVL 5 of the 2nd category (LVL 4-6) etc
            int deviationFromCategoryCenter = ((entityLevel-1) % 3)-1;      // e.g. LVL1 is -1 from the category center (LVL2), LVL6 is +1 from the category center (LVL 5) etc
                                        // there is a simpler way to do the latter task: subtract from center level that can be calculated from the result of the first calc. 

            if (smallerEqualOrLarger >= maximumLows[deviationFromCategoryCenter + 1] && smallerEqualOrLarger < minimumHighs[deviationFromCategoryCenter + 1])
                return clampItemCategory(closestLevelCategory);

            if (smallerEqualOrLarger < maximumLows[deviationFromCategoryCenter + 1])            
                return clampItemCategory(closestLevelCategory - getItemMaterialRandomDeviation());

            if (smallerEqualOrLarger >= minimumHighs[deviationFromCategoryCenter + 1])
                return clampItemCategory(closestLevelCategory + getItemMaterialRandomDeviation());

            return 0;   // should never reach this point, so a return value of 0 would indicate an error
        }

        static WeaponMaterialTypes getWeaponMaterialFromNumber(int number)
        {
            switch (number)
            {
                case 1: return WeaponMaterialTypes.Iron;
                case 2: 
                case 3: return WeaponMaterialTypes.Steel;
                case 4: return WeaponMaterialTypes.Silver;
                case 5: return WeaponMaterialTypes.Elven;
                case 6: return WeaponMaterialTypes.Dwarven;
                case 7: return (MTMostlyMagicMod.region == (int)DaggerfallRegions.OrsiniumArea) ? WeaponMaterialTypes.Orcish : WeaponMaterialTypes.Mithril;
                case 8: return (MTMostlyMagicMod.region == (int)DaggerfallRegions.OrsiniumArea) ? WeaponMaterialTypes.Mithril : WeaponMaterialTypes.Adamantium;
                case 9: return (MTMostlyMagicMod.region == (int)DaggerfallRegions.OrsiniumArea) ? WeaponMaterialTypes.Adamantium : WeaponMaterialTypes.Ebony;
                case 10:                
                case 11: return (MTMostlyMagicMod.region == (int)DaggerfallRegions.OrsiniumArea) ? WeaponMaterialTypes.Ebony : WeaponMaterialTypes.Orcish;
                case 12:
                case 13: return WeaponMaterialTypes.Daedric;

                default: return WeaponMaterialTypes.Iron;   // possibly change in the future for debug purposes
            }
        }

        static ArmorMaterialTypes getArmorMaterialFromNumber(int number)
        {
            switch (number)
            {
                case 1: return ArmorMaterialTypes.Leather;
                case 2: return ArmorMaterialTypes.Chain;
                case 3: return ArmorMaterialTypes.Iron;
                case 4: return ArmorMaterialTypes.Steel;
                case 5: return ArmorMaterialTypes.Silver;
                case 6: return ArmorMaterialTypes.Elven;
                case 7: return ArmorMaterialTypes.Dwarven;
                case 8: return (MTMostlyMagicMod.region == (int)DaggerfallRegions.OrsiniumArea) ? ArmorMaterialTypes.Orcish : ArmorMaterialTypes.Mithril;
                case 9: return (MTMostlyMagicMod.region == (int)DaggerfallRegions.OrsiniumArea) ? ArmorMaterialTypes.Mithril : ArmorMaterialTypes.Adamantium;
                case 10: return (MTMostlyMagicMod.region == (int)DaggerfallRegions.OrsiniumArea) ? ArmorMaterialTypes.Adamantium : ArmorMaterialTypes.Ebony;
                case 11:
                case 12: return (MTMostlyMagicMod.region == (int)DaggerfallRegions.OrsiniumArea) ? ArmorMaterialTypes.Ebony : ArmorMaterialTypes.Orcish;                
                case 13: return ArmorMaterialTypes.Daedric;

                default: return ArmorMaterialTypes.Leather;   // possibly change in the future for debug purposes
            }
        }   
        /*
         *  Here comes the comment to how extra strong enemies are generated 
             */
        void SetExtraStrongMonsterStats(bool isThisANewEnemy)
        {
            if (ourNumberGenerator.Next(0, 10) > 0)
            {
                MMMFormulaHelper.MMMFormulaHelperSilentInfoMessage("This is definitely not an extra strong " + entity.Career.Name + ".");
                return;
            }           // the code that follows set the distinc characteristics for extra strong monsters

            int careerIndex = entity.CareerIndex;
            int healthMultiplicator = ourNumberGenerator.Next(5, 11);      // should be halved (so, from 2.5x to 5.5x)       
            int spellPointMultiplicator = ourNumberGenerator.Next(4, 8);   // should be halved (so, from 2x to 4x)            

            switch (careerIndex)
            {
                case ((int)MonsterCareers.VampireAncient):
                case ((int)MonsterCareers.AncientLich):
                case ((int)MonsterCareers.DaedraLord):
                    entity.MaxMagicka = (entity.MaxMagicka*spellPointMultiplicator) / 2;
                    if (isThisANewEnemy)
                            entity.CurrentMagicka = entity.MaxMagicka;              // setting new increased spellpoint number - only if running for the first time

                    entity.Stats.SetPermanentStatValue(DFCareer.Stats.Intelligence, entity.Stats.PermanentIntelligence+ 20);               // increasing intelligence by 20
                    entity.Stats.SetPermanentStatValue(DFCareer.Stats.Willpower, entity.Stats.PermanentWillpower + 20);                 // increasing willpower by 20
                    /* TODO: here comes the code to boost certain skills (magic skills...) and possibly other characteristics for extra strong spellcasters */
                    goto case ((int)MonsterCareers.Werewolf);                

                case ((int)MonsterCareers.Werewolf):
                case ((int)MonsterCareers.OrcWarlord):                    
                    entity.MaxHealth = (entity.MaxHealth*healthMultiplicator) / 2;
                    if (isThisANewEnemy)
                        entity.CurrentHealth = entity.MaxHealth;                // setting new increased health number - but only if we are running for the first time

                    entity.Stats.SetPermanentStatValue(DFCareer.Stats.Speed, entity.Stats.PermanentSpeed + 10);
                    entity.Stats.SetPermanentStatValue(DFCareer.Stats.Endurance, entity.Stats.PermanentEndurance + 10);
                    entity.Stats.SetPermanentStatValue(DFCareer.Stats.Agility, entity.Stats.PermanentAgility + 20);
                    entity.Stats.SetPermanentStatValue(DFCareer.Stats.Strength, entity.Stats.PermanentStrength + 20);
                    entity.Stats.SetPermanentStatValue(DFCareer.Stats.Luck, entity.Stats.PermanentLuck + 10);                      // increasing stats for all extra strong monsters

                    /* TODO: here comes the code to boost certain skills (melee skills) and possibly other characteristics for all extra strong monsters */

                    MMMFormulaHelper.MMMFormulaHelperSilentInfoMessage("An extra strong "+ entity.Career.Name+".");
                    break;
                default:
                    MMMFormulaHelper.MMMFormulaHelperSilentInfoMessage("Not an extra strong " + entity.Career.Name + ".");
                    break;
            }
        }

        string[] skillNameStrings = { "Medical", "Etiquette", "Streetwise", "Jumping", "Orcish", "Harpy", "Giantish", "Dragonish", "Nymph", "Daedric", "Spriggan", "Centaurian", "Impish", "Lockpicking",
            "Mercantile", "Pickpocket", "Stealth", "Swimming", "Climbing", "Backstabbing", "Dodging", "Running", "Destruction", "Restoration", "Illusion", "Alteration", "Thaumaturgy", "Mysticism",
            "ShortBlade", "LongBlade", "HandToHand", "Axe", "BluntWeapon", "Archery", "CriticalStrike"};

        string[] equipSlotStrings = { "Amulet0", "Amulet1", "Bracelet0", "Bracelet1", "Ring0", "Ring1", "Bracer0", "Bracer1", "Mark0", "Mark1", "Crystal0", "Crystal1", "Head",
            "RightArm", "Cloak1", "LeftArm", "Cloak2", "ChestClothes", "ChestArmor", "RightHand", "Gloves", "LeftHand", "Unknown1", "LegsArmor", "LegsClothes", "Unknown2", "Feet" };

        void EntityCharacteristicsToLog (string title)
        {
            string armorString = "\tArmor values: ";
            for (int i = 0; i < entity.ArmorValues.Length; i++)
                armorString += entity.ArmorValues[i] + "  ";

            string skillString = "\tSkill values.";
            for (int i = 0; i < (int)DFCareer.Skills.Count; i++)         // potential problem if number of skills increased (array-out-of-bounds exception)
            {
                skillString += skillNameStrings[i] + "=" + entity.Skills.GetPermanentSkillValue(i) + "  ";
                if (i % 7 == 6)
                    skillString += System.Environment.NewLine + "\t\t";
            }

            
            string itemString = "\tItems in inventory."+ System.Environment.NewLine;
            if (entity.Items != null)
            {
                int numberOfItems = entity.Items.Count;
                if (numberOfItems == 0)
                    itemString += "\t\tNone" + System.Environment.NewLine;
                else
                {
                    itemString += "\t\tnumberOfItems=" + numberOfItems+System.Environment.NewLine;
                    for (int i = 0; i < numberOfItems; i++)
                    {
                        DaggerfallUnityItem item = entity.Items.GetItem(i);
                        if (item != null)
                            itemString += "\t\tTemplateIndex=" + item.TemplateIndex + ", shortName: '"+ item.shortName+"', ItemName = '" + item.ItemName + "', LongName='" + item.LongName + "', isIngredient=" + item.IsIngredient + System.Environment.NewLine;
                        else
                            itemString += "\t\tItem #" + i + " is NULL." + System.Environment.NewLine;

                        //  itemString += "\t\tItem #" + i + " is NOT null" + System.Environment.NewLine;
                    }
                }
            }
            else
                itemString += "\t\t entity.Items is null." + System.Environment.NewLine;
            

            itemString += "\tItems equipped." + System.Environment.NewLine;
            for (int i = (int)EquipSlots.Amulet0; i <= (int)EquipSlots.Feet; i++)   
            {                
                DaggerfallUnityItem item = entity.ItemEquipTable.GetItem((EquipSlots)i);
                if (!(item is null))
                {
                    TextFile.Token[] tokens = ItemHelper.GetItemInfo(item, DaggerfallUnity.Instance.TextProvider);
                    MacroHelper.ExpandMacros(ref tokens, item);
                    string currentItemString = tokens[0].text;
                    itemString += "\t\t" + equipSlotStrings[i] + ": " + currentItemString + ", TemplateIndex="+ item.TemplateIndex+ System.Environment.NewLine; ;
                }                    
            }

            MMMFormulaHelper.MMMFormulaHelperSilentInfoMessage(title + " Career.Name: " + entity.Career.Name + System.Environment.NewLine +
                "\tLevel: " + entity.Level + "  Health: " + entity.CurrentHealth + "/" + entity.MaxHealth + "  SpellPoints: " + entity.CurrentMagicka + "/" + entity.MaxMagicka + System.Environment.NewLine +
                "\tAttributes.  Willpower=" + entity.Stats.PermanentWillpower + "  Intelligence=" + entity.Stats.PermanentIntelligence + "  Speed=" + entity.Stats.PermanentSpeed +
                "  Luck=" + entity.Stats.PermanentLuck + "  Agility=" + entity.Stats.PermanentAgility + "  Strength=" + entity.Stats.PermanentStrength +
                "  Endurance=" + entity.Stats.PermanentEndurance + "  Personality=" + entity.Stats.PermanentPersonality + System.Environment.NewLine +
                skillString + System.Environment.NewLine +
                "\tResistances. Fire=" + entity.Resistances.PermanentFire + " Frost=" + entity.Resistances.PermanentFrost + " DiseaseOrPoison=" + entity.Resistances.PermanentDiseaseOrPoison +
                " Shock=" + entity.Resistances.PermanentShock + " Magic =" + entity.Resistances.PermanentMagic + System.Environment.NewLine +
                itemString + System.Environment.NewLine + 
                armorString ); 
        }        

        void Start()
        {
            int X = GameManager.Instance.PlayerGPS.CurrentMapPixel.X;
            int Y = GameManager.Instance.PlayerGPS.CurrentMapPixel.Y;
            int tempRandomVar;
            int tempValue;
            playerEntity = GameManager.Instance.PlayerEntity;
            int playerLevel = playerEntity.Level;
            string debugString;                                                                                                 // TODO: use this to streamline debug messaging (less messages, more details/message)

            entityBehaviour = GetComponent<DaggerfallEntityBehaviour>();

            if (entityBehaviour==null)
            {
                MMMFormulaHelper.MMMFormulaHelperSilentInfoMessage("RecalcStats.Start() has been called. Entitybehaviour is null. exiting");
                return;
            }

            entity = entityBehaviour.Entity as EnemyEntity; // what if it is not Enemy?? answer: we would not be here. We are only part of the enemy prefab.
            if (entity == null)
            {
                MMMFormulaHelper.MMMFormulaHelperSilentInfoMessage("RecalcStats.Start() has been called. Entity is null. exiting");
                return;
            }            

            MobileEnemy mobileEnemy = entity.MobileEnemy;
            EntityTypes entityType = entity.EntityType;
            int careerIndex = entity.CareerIndex;            
            
            MMMFormulaHelper.MMMFormulaHelperSilentInfoMessage("==========================================================================================" + System.Environment.NewLine +
                "RecalcStats.Start() has been called. MobileEnemy ID: " + mobileEnemy.ID + ", entityType: " + entityType + ", career name: " + entity.Career.Name);
            
            EntityCharacteristicsToLog("Enemy characteristics BEFORE any changes made. "); 

            bool isThisANewEnemy = true;            // will be able to judge only after setting MaximumHealth - then, if CurrentHealth is lower, then it is not a new enemy             
            int randomSeed = 0;

            if (entity.Items != null)
            {
                int numberOfItems = entity.Items.Count;
                if (numberOfItems > 0)
                {
                    for (int i = 0; i < numberOfItems; i++)
                    {
                        DaggerfallUnityItem item = entity.Items.GetItem(i);
                        if (item != null)
                            randomSeed += item.TemplateIndex;
                    }
                }
            }                       

            randomSeed += mobileEnemy.ID + X * Y;                   // for now,  mobileEnemy.ID + map pixel coordinates + item templateindexes should do the job                        
            ourNumberGenerator = new System.Random(randomSeed);     // eventually planning a more versatile way of doing things - currently each monster in the same map pixel of the same kind will have the exact same stats

            MMMFormulaHelper.MMMFormulaHelperSilentInfoMessage("X="+X+", Y="+Y+ ", random seed="+randomSeed);            

            /* here comes the code that does the work - but first, an insight into what we are up to
             *
             * what determines an enemy?
             * - its looks, gender, height etc (this mod is not concerned with any of these)
             * 
             * what determines the stength of an enemy?
             * - its max health, fatigue, magicka (this mod IS concerned with max health and magicka - max fatigue seems not to level)
             * - its stats (this mod IS NOT overly concerned with this - because stats are set in game from Career and do not level anyway;; will explore the possibility of increase stats for 'super strong enemies')
             * - its skills (this mod IS concerned with this)
             * - its spells (this mod IS concerned with this -  for class enemies whose spells are contingent on level)
             * - its equipment (this mod IS concerned with this)
             *
             * our task here is to recalculate the things we ARE concerned with, while leaving the stuff we ARE NOT concerned with alone (in order not to break these later aspects)
             * - When it comes to monsters, there isn't much that needs to be done and it seems Unleveled Mobs by Jay_H and Unleveled Loot by Ralzar do most of that.
             * - But, when it comes to class enemies, there do remain some important changes that need to be made. Unleveled Mobs does not address these, while Unleveled Loot addresses solely the equipment part.
             * (I am also working under the assumption that Armor is not to be considered an extra chance to miss, but rather a factor that reduces the damage done (as introduced by ??? ??? mod)
             *
             * The idea that entity level should determine most properties of a class enemy (health, fatigue, magicka, stats, skills) seems to be a viable one.
             * The first question is this: what should determine this 'entity level' that then determines most other properties?
             * To the unmodded game, the answer for the most part is: player level. Here, we try to develop a different idea.
             *
             * Let's separate the class enemies into two groups: (1) dungeon mobs, (2) random encounters (to distinguish is easy - like Ralzar did - just need to know if the player is in a dungeon)
             *              perhaps we will separately consider Town Guards
             * 
             * (2) With random encounters, it is hard to leave player level out of the equation if we don't want complete randomness.
             * This might not be a good idea - low-level characters might get crushed too often and high-level characters would get too many outclassed enemies.
             * The best option seems to be a compromise:
             * - most (like 80%) random encounters with a level close (but not necessarily equal) to player level
             * - and a smaller number (like 10-10%) of enemies with a much lower and much higher level compared to the player
             *             * 
             * (1) As for dungeon mobs, Ralzar's basic idea that he applied to loot is a good one: invent a 'Dungeon Quality Level' and make the opponents' level contingent on that.
             * Here I would add that some difference should be allowed (like from DQL-2 to DQL+2). Ralzar has already invented DQL in Unleveled Loot, we will go with his method for now, with some additions.
             * First, HumanStronghold, RuinedCastle, Prison should also have a DQL assigned. 
             * Also, there should be some flexibility when it comes to dungeon levels. E.g. having all DragonsDen dungeons at level 18 is a good start, but perhaps having all concrete such dungeons having a level from
             * 18-2 to 18+2 might be even better. 
             * These dungeon levels should not be random. They should be deterministic. Could be calculated from the dungeon class and a hash of the dungeon oordinates and the actual quarter.
             * This way any given dungeon can gain or loose some quality as time passes.
             * An example: any concrete VampireHaunt dungeon should have a level calculated as a base (15) + modifier (from -2 to 2 calculated based on hash)
             *
             * So, now we go and set the enemy entity's:
             * - level
             * - stats ??
             * - health
             * - skills
             * - magicka and spells
             * - equipment
             */

            if (entityType != EntityTypes.EnemyClass)
            {
                /* Printing important stats about the enemy for experimental and debug purposes*/                

                if (extraStrongMonsters)
                {
                    MMMFormulaHelper.MMMFormulaHelperSilentInfoMessage("   Not a class enemy. Checking if it is an extra strong monster.");
                    SetExtraStrongMonsterStats(isThisANewEnemy);                    
                }
                else
                    MMMFormulaHelper.MMMFormulaHelperSilentInfoMessage("   Not a class enemy. Leaving it alone.");
                            // if it is a monster, we either set it to be an extra strong one or we leave it alone                      

                if (entity.CurrentHealth == entity.MaxHealth)
                    entity.MaxHealth += 1;        // increasing MaxHealth in order to signal that this enemy's items have already been processed (entity.CurrentHealth<entity.MaxHealth check)
                                              // nearly every particular of enemies seems to be thrown away during save/load - MaxHealth and CurrentHealth seem to be exceptions, these are loaded back unchanged                      
                EntityCharacteristicsToLog("Enemy monster characteristics AFTER changes made. ");

                return; 
            }              

            // careerindex and stats already set - they are okay that way for now
                    // TODO: if super strong enemies will have increased stats, the code could come here

            // I. So now, setting the level.

            /* If random encounter, then
             *  10%-10%-10% chance of enemy at playerlevel-4 .. .. playerlevel +3
             *  10% chance of an enemy level lower than playerlevel-4 (down to playerlevel-10)
             *  10% chance of an enemy level higher than playerlevel+3  (up to playerlevel+9)
             *  all this capped at playerlevel=23; above player level 23, the level of class enemies will not be getting higher */            
            if (!GameManager.Instance.PlayerEnterExit.IsPlayerInsideDungeon)
            {
                if (careerIndex != (int)MobileTypes.Knight_CityWatch)
                {
                    MMMFormulaHelper.MMMFormulaHelperSilentInfoMessage("Player is not inside a dungeon and the entity is NOT a townguard: level-setting by random encounters method.");
                    int effectivePlayerLevel = playerLevel;
                    if (effectivePlayerLevel > 23) effectivePlayerLevel = 23; // cap
                    tempRandomVar = ourNumberGenerator.Next(0, 10);
                    if (tempRandomVar == 0)
                        entity.Level = playerLevel - 10 + ourNumberGenerator.Next(0, 6);
                    else if (tempRandomVar == 9)
                        entity.Level = playerLevel + 4 + ourNumberGenerator.Next(0, 6);
                    else entity.Level = playerLevel - 4 + tempRandomVar - 1;
                }
                else  // for townguards
                {
                    MMMFormulaHelper.MMMFormulaHelperSilentInfoMessage("Player is not inside a dungeon and the entity IS a townguard: setting level accordingly");
                    //      If townguard, we implement a method like we have for dungeons :: Town Guard Quality Level
                    //  This quality level is contingent on the size and prominance of the town:
                    //      Daggerfall, Wayrest and Sentinel: base level of 21, like the hardest of dungeons as per Ralzar
                    //      other regional capitals LVL18
                    //      other cities LVL15
                    //      smallest of towns LVL7
                    //      general: LVL11
                    //              there is also a small (-2..+2) correction based on time to account for the fluctuation of the quality of town guards available from time to time 
                    //          The individual guards can deviate form their guard (average) level (-2..+2)
                    
                    int townGuardLevel = 11;     // if nothing else triggers, this should give an 11

                            // here comes the code than determines what kind of town the player is in and changes the townGuardLevel accordingly
                    switch (GameManager.Instance.PlayerGPS.CurrentLocationType)
                    {
                        case DFRegion.LocationTypes.TownCity:
                            townGuardLevel = 15;
                            break;
                        case DFRegion.LocationTypes.TownHamlet:
                            townGuardLevel = 11;
                            break;
                        case DFRegion.LocationTypes.TownVillage:
                            townGuardLevel = 7;
                            break;
                    }
                            // here to adjust for other capitals
                    if (GameManager.Instance.PlayerGPS.CurrentLocation.Name == GameManager.Instance.PlayerGPS.CurrentLocation.RegionName)
                        townGuardLevel = 18;

                            // now to adjust for Daggerfall, Wayrest and Sentinel
                    if (GameManager.Instance.PlayerGPS.CurrentLocation.Name == "Daggerfall" || GameManager.Instance.PlayerGPS.CurrentLocation.Name == "Wayrest" ||
                         GameManager.Instance.PlayerGPS.CurrentLocation.Name == "Sentinel")
                        townGuardLevel = 21;                                                                                            // TODO: test these in-game to see if it triggers, extensive debug messaging

                    //  here comes the code that applies the correction for fluctuation
                    int[] modifierTable = { 0, 1, 2, 1, 0, -1, -2, -1 };

                    int modifierBasedOnHash = 0;       // this should be a modifier from -2 to +2 that is contingent on a hash from the coordinates of the place and the time (like which quarter it is)
                    
                    int currentYear = DaggerfallUnity.Instance.WorldTime.Now.Year;
                    int currentMonth = DaggerfallUnity.Instance.WorldTime.Now.Month;                
                            // X and Y already defined at the beginning of the method

                    int sequence = (X + Y + currentYear * 4 + currentMonth / 3) % 8;
                    modifierBasedOnHash = modifierTable[sequence];

                    townGuardLevel += modifierBasedOnHash;
                                                                                        
                    int[] individualModifierTable = { -2, -1, -1, 0, 0, 0, 0, +1, +1, +2 };
                    townGuardLevel += individualModifierTable[ourNumberGenerator.Next(0, 10)];     // apply the individual fluctuation
                                                                                                                    // TODO: double check all this and add debug messages

                    entity.Level = townGuardLevel;                                                                                                      

                }
            }
            else
                /* If we are inside a dungeon, then the enemy level should depend on dungeon quality level.
                 * 10% DQL-2, 20% DQL-1, 40% DQL, 20% DQL+1, 10% DQL+2                 */   
            {
                tempRandomVar = ourNumberGenerator.Next(0, 10);                                        // TODO: revise code like above                

                int dungeonQualityLevel = MTMostlyMagicMod.dungeonQuality();
                MMMFormulaHelper.MMMFormulaHelperSilentInfoMessage("Player is inside a dungeon type "+ MTMostlyMagicMod.dungeon+" of QL: "+ dungeonQualityLevel + " random number picked is "+ tempRandomVar);
                switch (tempRandomVar)
                {
                    case 0:
                        entity.Level = dungeonQualityLevel - 2;
                        break;
                    case 1:
                    case 2:
                        entity.Level = dungeonQualityLevel - 1;
                        break;
                    case 7:
                    case 8:
                        entity.Level = dungeonQualityLevel + 1;
                        break;
                    case 9:
                        entity.Level = dungeonQualityLevel + 2;
                        break;
                    default:
                        entity.Level = dungeonQualityLevel;
                        break;
                }
            }
                    // adding/subtracting levels for exceptionally strong/weak enemies - you have a small change to encounter such foes anywhere
            int excellenceRandomVar = ourNumberGenerator.Next(0, 20);
            if (excellenceRandomVar == 0)
            {
                entity.Level -= 5;
                MMMFormulaHelper.MMMFormulaHelperSilentInfoMessage("This is an exceptionally weak enemy - e.g. a trainee at this place.");
            }
            if (excellenceRandomVar == 19)
            {
                entity.Level += 5;
                MMMFormulaHelper.MMMFormulaHelperSilentInfoMessage("This is an exceptionally strong enemy - e.g. an elite or trainer at this place.");
            }

            if (entity.Level < 1) entity.Level = 1;
            /* Here I try to explain how I think these should work. What the results of all these calculations should be.
             * For dungeons, the base is the dungeon quality level. The basic idea was borrowed from Ralzar's mod. This can range from 3 to 23.
             *              Most enemies' level will fall within the DQL-2 to DQL+2 interval. So, the least quality dungeons can have as low as LVL1 foes while the highest quality dungeons can have as high as 25 level ones.
             *              The exception that you can meet are exceptionally talented foes applies in dungeons as well, so the overall maximum for dungeons is 30 level foes.
             * For random encounters, most enemies will be in the playerlevel-4 to playerlevel+3 range, with a max level of 26 for a playerlevel of 23.
             *              One in ten enemies will be (perhaps significantly) weeker and one in ten will be (perhaps significantly) stronger. The strongest ones should be level 31.
             *              The exception that you can meet are exceptionally talented foes applies here as well, so the overall maximum for random encounters is level 36 foes.
             *                  These should pose a challenge to all but the strongest characters in the game.
             *                      Skills at 150. Magic skills around 120. Orcish-Daedric armor and weapons. Perhaps even magical equipment and increased attributes.(last item to be added to the mod eventually).
             * One special case. Town guards.
             *              The base range of levels should be dependent on the town.
             *              Top category: Daggerfall, Wayrest, Sentinel ;; other capitals ;; cities ;; towns ;; smaller places ----- perhaps like dungeons :: "town quality level"
             *              As always: exceptionally talented foes applies for city guards as well. So, maximum guard level should be LVL30=21+2(TQL dev)+2 (individ. dev.) + 5 (exceptional talent). 
                */

            // II. re-setting max health now

            isThisANewEnemy = (entity.CurrentHealth == entity.MaxHealth);       // setting a flag so we know later that we are running for the first time on the given enemy
            
            entity.MaxHealth = MMMFormulaHelper.RollEnemyClassMaxHealth(entity.Level, entity.Career.HitPointsPerLevel, ourNumberGenerator);                              

            if (isThisANewEnemy)
            {
                entity.CurrentHealth = entity.MaxHealth;    // TODO: only do this if running for the first time
                MMMFormulaHelper.MMMFormulaHelperSilentInfoMessage("\tEntity level +" + entity.Level + ", Max Health set to " + entity.MaxHealth + ", since running for the first time, also set Current Health to " + entity.CurrentHealth);
            }
            else
                MMMFormulaHelper.MMMFormulaHelperSilentInfoMessage("\tEntity level +" + entity.Level + ", Max Health set to " + entity.MaxHealth+ ", since not running for the first time in the given enemy, leaving Current Health alone");

                // III. now, re-setting skills :: a max of 150 to make the strongest ones more difficult
            short skillsLevel = (short)((entity.Level * 4) + 34);
            if (skillsLevel > 150)
            {
                skillsLevel = 150;
            }

            for (int i = 0; i <= DaggerfallSkills.Count; i++)
            {
                entity.Skills.SetPermanentSkillValue(i, skillsLevel);
            }

            MMMFormulaHelper.MMMFormulaHelperSilentInfoMessage("   Skills set to " + skillsLevel);            

                // IV. now, re-setting spells and max. magicka
            if (mobileEnemy.CastsMagic)
            {
                int spellListLevel = entity.Level / 3;
                if (spellListLevel > 6)
                    spellListLevel = 6;
                entity.SetEnemySpells(EnemyClassSpells[spellListLevel]);    // involves setting magic skills to 80     
                                                                            // also involves setting the max. magicka to level*10+100, but with the new level
                                                                            // currently does not appear to involve random number generation
                if (isThisANewEnemy)
                    entity.CurrentMagicka = entity.MaxMagicka;          // setting currentmagicka to equal maxmagicka - only if running on a given enemy for the first time
                                                                    // TODO: see if enemy spell points is actually saved by game..., if not, this would better run each time
                
                short spellSkillLevel = 80;                                                               // TODO: could reevaluate in the future('extra strong enemies')                                                  
                entity.Skills.SetPermanentSkillValue(DFCareer.Skills.Destruction, spellSkillLevel);
                entity.Skills.SetPermanentSkillValue(DFCareer.Skills.Restoration, spellSkillLevel);
                entity.Skills.SetPermanentSkillValue(DFCareer.Skills.Illusion, spellSkillLevel);
                entity.Skills.SetPermanentSkillValue(DFCareer.Skills.Alteration, spellSkillLevel);
                entity.Skills.SetPermanentSkillValue(DFCareer.Skills.Thaumaturgy, spellSkillLevel);
                entity.Skills.SetPermanentSkillValue(DFCareer.Skills.Mysticism, spellSkillLevel);
            }
            /* V. now, re-setting the equipment
             * Equipment should reflect enemy entity level (not player level and not dungeon level),
             * with some modifications like boosts to Daedric for Daedric foes and Orcish for Orsinium and Orc strongholds (like Ralzar)
             * A difference to Ralzar's would be that the extra Orcish items (Orsinium, Orcish Stronghold) are added at the time of enemy spawning, not death.
             * (Ideally also for any affected monsters (Orcs and Daedra), but I am leaving those to Ralzar's code to be done at death
             *
             * What we are trying to do here:
             * First, unequip and get rid of the equipment that was previously generated. (including the armor-effects)
             * Then, re-generate the same equipment, only of different materials.
             * In the future, I plan to expand this code to add further potent, partly magic equipment to 'super strong enemies'                                        // TODO       */

            if (isThisANewEnemy)
                // resetting equipemnt only needed if we are running on the given enemy for the first time
            {
                for (int i = (int)EquipSlots.Head; i <= (int)EquipSlots.Feet; i++)       // does it include boots? ::  added equal, should be correct now ::                   TODO: check if it is correct this way.
                {
                    DaggerfallUnityItem item = entity.ItemEquipTable.GetItem((EquipSlots)i);
                    if (item != null && item.ItemGroup == ItemGroups.Armor)
                    {
                        TextFile.Token[] tokens = ItemHelper.GetItemInfo(item, DaggerfallUnity.Instance.TextProvider);
                        MacroHelper.ExpandMacros(ref tokens, item);
                        string oldItemString = tokens[0].text + " (TI=" + item.TemplateIndex + ")";

                        entity.ItemEquipTable.UnequipItem(item);

                        int groupIndex = item.GroupIndex; // not sure this is necessary
                        DaggerfallUnityItem item2 = ItemBuilder.CreateItem(ItemGroups.Armor, item.TemplateIndex);

                        ItemBuilder.ApplyArmorSettings(item2, playerEntity.Gender, playerEntity.Race, getArmorMaterialFromNumber(getRandomItemMaterial(entity.Level)), item.CurrentVariant);
                        // town guards can have better than steel        // TODO: re-evaluate if any penalty is appropriate for town guards;; probably so (classic has a cap: no better than steel)
                        item2.currentCondition = item2.maxCondition * item.ConditionPercentage / 100;

                        entity.Items.RemoveItem(item);
                        entity.Items.AddItem(item2);
                        entity.ItemEquipTable.EquipItem(item2, true, false);

                        TextFile.Token[] tokens2 = ItemHelper.GetItemInfo(item2, DaggerfallUnity.Instance.TextProvider);
                        MacroHelper.ExpandMacros(ref tokens2, item2);

                        MMMFormulaHelper.MMMFormulaHelperSilentInfoMessage("Instead of " + oldItemString + " added and equipped " + tokens2[0].text + " (TI=" + item2.TemplateIndex + ")");

                    }

                    if (item != null && item.ItemGroup == ItemGroups.Weapons)
                    {
                        TextFile.Token[] tokens = ItemHelper.GetItemInfo(item, DaggerfallUnity.Instance.TextProvider);
                        MacroHelper.ExpandMacros(ref tokens, item);
                        string oldItemString = tokens[0].text + " (TI=" + item.TemplateIndex + ")";

                        entity.ItemEquipTable.UnequipItem(item);

                        int groupIndex = item.GroupIndex;       // not sure this is necessary
                        DaggerfallUnityItem item2 = ItemBuilder.CreateItem(ItemGroups.Weapons, item.TemplateIndex);

                        ItemBuilder.ApplyWeaponMaterial(item2, getWeaponMaterialFromNumber(getRandomItemMaterial(entity.Level)));
                        // town guards can have better than steel        // TODO: re-evaluate if any penalty is appropriate for town guards;; probably so (classic has a cap: no better than steel)
                        item2.currentCondition = item2.maxCondition * item.ConditionPercentage / 100;

                        item2.poisonType = item.poisonType;     // retain poison

                        entity.Items.RemoveItem(item);
                        entity.Items.AddItem(item2);
                        entity.ItemEquipTable.EquipItem(item2, true, false);

                        TextFile.Token[] tokens2 = ItemHelper.GetItemInfo(item2, DaggerfallUnity.Instance.TextProvider);
                        MacroHelper.ExpandMacros(ref tokens2, item2);

                        MMMFormulaHelper.MMMFormulaHelperSilentInfoMessage("Instead of " + oldItemString + " added and equipped " + tokens2[0].text + " (TI=" + item2.TemplateIndex + ") -- poisontype=" + item2.poisonType);
                    }
                }
                //Debug.Log("   Unequipped and removed " + listOfItemsToDisposeOf.Count+" items");      // TODO: reevaluate if such line is needed + disposing of unneeded items     

                // recalculating armor stats
                // Initialize armor values to 100 (no armor)
                for (int i = 0; i < entity.ArmorValues.Length; i++)
                {
                    entity.ArmorValues[i] = 100;
                }
                // Calculate armor values from equipment
                for (int i = (int)EquipSlots.Head; i < (int)EquipSlots.Feet; i++)
                {
                    DaggerfallUnityItem item = entity.ItemEquipTable.GetItem((EquipSlots)i);
                    if (item != null && item.ItemGroup == ItemGroups.Armor)
                    {
                        entity.UpdateEquippedArmorValues(item, true);
                    }
                }

                // Comments to the code originally from Interkarma:
                // Clamp to maximum armor value of 60. In classic this also applies for monsters.
                // Note: Classic sets the value to 60 if it is > 50, which seems like an oversight.
                for (int i = 0; i < entity.ArmorValues.Length; i++)
                {
                    if (entity.ArmorValues[i] > 60)
                        entity.ArmorValues[i] = 60;
                }                
            }

            entity.MaxHealth += 1;        // increasing MaxHealth in order to signal that this enemy's items have already been processed (fail the entity.CurrentHealth<entity.MaxHealth check next time)
                                          // nearly every particular of enemies seems to be thrown away during save/load - MaxHealth and CurrentHealth seem to be exceptions, these are loaded back unchanged
                                          // so these CAN be used to signal that we have processed the given entity 

            EntityCharacteristicsToLog("Class Enemy characteristics AFTER changes made. ");

        }        
    }
}
