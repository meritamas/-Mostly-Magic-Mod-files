// Project:         MeriTamas's (Mostly) Magic Mod for Daggerfall Unity (http://www.dfworkshop.net)
// Copyright:       Copyright (C) 2023 meritamas
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

using System;
using System.Collections.Generic;
using DaggerfallConnect.Arena2;
using UnityEngine;
using DaggerfallWorkshop.Utility;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.MagicAndEffects;
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
    public class MTRecalcStats : MonoBehaviour
    {
        static string messagePrefix = "MTRecalcStats: ";
        static public bool extraStrongMonsters = false;
        static public int enemySpellMissileSpeeds = 0;

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
        EntityEffectManager entityEffectManager;
        EnemyEntity entity;
        PlayerEntity playerEntity;
        static System.Random globalNumberGenerator = new System.Random(System.DateTime.Now.Millisecond);
        System.Random ourNumberGenerator;

        static void Message(string message)
        {
            MTMostlyMagicMod.Message(messagePrefix + message);
        }

        static void SilentMessage(string message)
        {
            MTMostlyMagicMod.SilentMessage(messagePrefix + message);
        }

        public static int GetPlaceAndTimeDifficultyLevel()
        {            
            bool atLocation = (GameManager.Instance.PlayerGPS.CurrentLocationIndex == -1);      // get if e are at a location or not
            bool atNight = DaggerfallUnity.Instance.WorldTime.Now.IsNight;
            bool inWinter = (DaggerfallUnity.Instance.WorldTime.Now.SeasonValue == DaggerfallDateTime.Seasons.Winter);
            bool inSummer = (DaggerfallUnity.Instance.WorldTime.Now.SeasonValue == DaggerfallDateTime.Seasons.Summer);
                // TOADD: is dungeon present. If so, mob levels should be same as if in dungeon (these people were coming from or going to the DUNGEON, after all)
            
            // you need this: climate - see below, timeOFday, season, location
            // Set based on world climate
            switch (GameManager.Instance.PlayerGPS.CurrentClimateIndex)
            {      
                case (int)MapsFile.Climates.Mountain:
                    if (atLocation)
                    {
                        SilentMessage("GetPlaceAndTimeDifficultyLevel: Climates.Mountain, at location");
                        return 12;
                    }
                    else
                    {
                        if (atNight)
                        {
                            SilentMessage("GetPlaceAndTimeDifficultyLevel: Climates.Mountain, not in location, at night");
                            return 18;
                        }
                        else
                        {
                            SilentMessage("GetPlaceAndTimeDifficultyLevel: Climates.Mountain, not in location, during daytime");
                            return 15;
                        }
                    }
                    break;
                case (int)MapsFile.Climates.Swamp:
                case (int)MapsFile.Climates.Woodlands:      // Swamp and Woodlands are the same
                    if (atLocation)
                    {
                        SilentMessage("GetPlaceAndTimeDifficultyLevel: Climates.Swamp/Woodlands, at location");
                        return 15;
                    }
                    else
                    {
                        if (atNight)
                        {
                            SilentMessage("GetPlaceAndTimeDifficultyLevel: Climates.Swamp/Woodlands, not in location, at night");
                            return 18;
                        }
                        else
                        {
                            SilentMessage("GetPlaceAndTimeDifficultyLevel: Climates.Swamp/Woodlands, not in location, during daytime");
                            return 12;
                        }
                    }
                    break;
                case (int)MapsFile.Climates.MountainWoods:
                    if (atLocation)     // TODO: more difficult in winter
                                        // more difficult based on pixel height
                                        // more difficult if surronded by mountains
                    {
                        SilentMessage("GetPlaceAndTimeDifficultyLevel: Climates.MountainWoods, at location");
                        return 15;
                    }
                    else
                    {
                        if (atNight)
                        {
                            SilentMessage("GetPlaceAndTimeDifficultyLevel: Climates.MountainWoods, not in location, at night");
                            return 21;
                        }
                        else
                        {
                            SilentMessage("GetPlaceAndTimeDifficultyLevel: Climates.MountainWoods, not in location, during daytime");
                            return 12;
                        }
                    }
                    break;

                case (int)MapsFile.Climates.Rainforest:
                case (int)MapsFile.Climates.Subtropical:        // Rainforest and Subtropical are the same
                    if (atLocation)
                    {
                        SilentMessage("GetPlaceAndTimeDifficultyLevel: Climates.Rainforest/Subtropical, at location");
                        return 12;
                    }
                    else
                    {
                        if (atNight)
                        {
                            SilentMessage("GetPlaceAndTimeDifficultyLevel: Climates.Rainforest/Subtropical, not in location, at night");
                            return 18;
                        }
                        else
                        {
                            SilentMessage("GetPlaceAndTimeDifficultyLevel: Climates.Rainforest/Subtropical, not in location, during daytime");
                            return 12;
                        }
                    }
                    break;               
                                
                case (int)MapsFile.Climates.HauntedWoodlands:
                    if (atLocation)
                    {
                        SilentMessage("GetPlaceAndTimeDifficultyLevel: Climates.HauntedWoodlands, at location");
                        return 15;
                    }
                    else
                    {
                        if (atNight)
                        {
                            SilentMessage("GetPlaceAndTimeDifficultyLevel: Climates.HauntedWoodlands, not in location, at night");
                            return 21;
                        }
                        else
                        {
                            SilentMessage("GetPlaceAndTimeDifficultyLevel: Climates.HauntedWoodlands, not in location, during daytime");
                            return 12;
                        }
                    }
                    break;

                case (int)MapsFile.Climates.Ocean:          // Ocean                    
                case (int)MapsFile.Climates.Desert:
                case (int)MapsFile.Climates.Desert2:        // Ocean and Desert are the same, and also the default
                default:
                    if (atLocation)
                    {
                        SilentMessage("GetPlaceAndTimeDifficultyLevel: Climates.Ocean/Desert/Desert2/Default, at location");
                        return 12;
                    }
                    else
                    {
                        if (atNight)
                        {
                            SilentMessage("GetPlaceAndTimeDifficultyLevel: Climates.Ocean/Desert/Desert2/Default, not in location, at night");
                            return 15;
                        }
                        else
                        {
                            SilentMessage("GetPlaceAndTimeDifficultyLevel: Climates.Ocean/Desert/Desert2/Default, not in location, during daytime");
                            return 8;
                        }
                    }
                    break;
            }
        }


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
            // from 1 to 15 
        static int clampItemCategory (int categoryNumber)
        {
            if (categoryNumber < 1) return 1;
            if (categoryNumber > 15) return 15;
            return categoryNumber;
        }

        int getRandomItemMaterial(int entityLevel)
        {/*
            What these things should mean.
            If there is no deviation from the central level, then the chances of getting [a lesser material, exactly that material, a higher material] should be [33, 34, 33]
            If there is a -1 deviation form the central level, then these chances should be [47, 27, 26], with a +1 deviation: [26, 27, 47]
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
                case 1: 
                case 2: return WeaponMaterialTypes.Iron;        // LVL1-6
                case 3: 
                case 4: return WeaponMaterialTypes.Steel;       // LVL7-12
                case 5: return WeaponMaterialTypes.Silver;      // LVL13-15
                case 6: 
                case 7: return WeaponMaterialTypes.Elven;
                case 8: return WeaponMaterialTypes.Dwarven;                                        // LVL22-24
                case 9: return (MTMostlyMagicMod.region == (int)DaggerfallRegions.OrsiniumArea) ? WeaponMaterialTypes.Orcish : WeaponMaterialTypes.Mithril;
                case 10: return (MTMostlyMagicMod.region == (int)DaggerfallRegions.OrsiniumArea) ? WeaponMaterialTypes.Mithril : WeaponMaterialTypes.Adamantium;
                case 11: 
                case 12: return (MTMostlyMagicMod.region == (int)DaggerfallRegions.OrsiniumArea) ? WeaponMaterialTypes.Adamantium : WeaponMaterialTypes.Ebony;
                case 13: 
                case 14: return (MTMostlyMagicMod.region == (int)DaggerfallRegions.OrsiniumArea) ? WeaponMaterialTypes.Ebony : WeaponMaterialTypes.Orcish;
                case 15: return WeaponMaterialTypes.Daedric;                

                default: return WeaponMaterialTypes.Iron;   // possibly change in the future for debug purposes
            }
        }

        static ArmorMaterialTypes getArmorMaterialFromNumber(int number)
        {
            switch (number)
            {
                case 1: return ArmorMaterialTypes.Leather;      // LVL1-3
                case 2: return ArmorMaterialTypes.Chain;        // LVL4-6
                case 3: return ArmorMaterialTypes.Iron;         // LVL7-9
                case 4: return ArmorMaterialTypes.Steel;        // LVL10-12
                case 5: return ArmorMaterialTypes.Silver;       // LVL13-15
                case 6: 
                case 7: return ArmorMaterialTypes.Elven;        // LVL16-21
                case 8: return ArmorMaterialTypes.Dwarven;      // LVL22-24
                case 9: return (MTMostlyMagicMod.region == (int)DaggerfallRegions.OrsiniumArea) ? ArmorMaterialTypes.Orcish : ArmorMaterialTypes.Mithril;
                case 10: return (MTMostlyMagicMod.region == (int)DaggerfallRegions.OrsiniumArea) ? ArmorMaterialTypes.Mithril : ArmorMaterialTypes.Adamantium;
                case 11: 
                case 12: return (MTMostlyMagicMod.region == (int)DaggerfallRegions.OrsiniumArea) ? ArmorMaterialTypes.Adamantium : ArmorMaterialTypes.Ebony;
                case 13: 
                case 14: return (MTMostlyMagicMod.region == (int)DaggerfallRegions.OrsiniumArea) ? ArmorMaterialTypes.Ebony : ArmorMaterialTypes.Orcish;
                case 15: return ArmorMaterialTypes.Daedric;               

                default: return ArmorMaterialTypes.Leather;   // possibly change in the future for debug purposes
            }
        }   
        /*
         *  Here comes the comment to how extra strong monsters are generated 
             */
        void SetExtraStrongMonsterStats(bool isThisANewEnemy)
        {
            if (ourNumberGenerator.Next(0, 10) > 0)
            {
                SilentMessage("This is definitely not an extra strong " + entity.Career.Name + ".");
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
                case ((int)MonsterCareers.OrcShaman):
                case ((int)MonsterCareers.DaedraSeducer):
                case ((int)MonsterCareers.Daedroth):
                case ((int)MonsterCareers.FireDaedra):
                case ((int)MonsterCareers.FrostDaedra):
                case ((int)MonsterCareers.Wraith):
                case ((int)MonsterCareers.Imp):
                    entity.MaxMagicka = (entity.MaxMagicka*spellPointMultiplicator) / 2;
                    if (isThisANewEnemy)
                            entity.CurrentMagicka = entity.MaxMagicka;              // setting new increased spellpoint number - only if running for the first time

                    entity.Stats.SetPermanentStatValue(DFCareer.Stats.Intelligence, entity.Stats.PermanentIntelligence+ 20);               // increasing intelligence by 20
                    entity.Stats.SetPermanentStatValue(DFCareer.Stats.Willpower, entity.Stats.PermanentWillpower + 20);                 // increasing willpower by 20

                    /* TODO: here comes the code to boost certain skills (magic skills...) and possibly other characteristics for extra strong spellcasters */
                    goto case ((int)MonsterCareers.Werewolf);                

                case ((int)MonsterCareers.Werewolf):
                case ((int)MonsterCareers.Wereboar):
                case ((int)MonsterCareers.Giant):
                case ((int)MonsterCareers.Harpy):
                case ((int)MonsterCareers.Dragonling_Alternate):
                case ((int)MonsterCareers.SkeletalWarrior):
                case ((int)MonsterCareers.Zombie):
                case ((int)MonsterCareers.Mummy):
                case ((int)MonsterCareers.OrcWarlord):
                    entity.Level += healthMultiplicator + spellPointMultiplicator; // should be just right... increases the entity's level (important for equipment matrial setting) by a number between 9 and 17
                    entity.MaxHealth = (entity.MaxHealth*healthMultiplicator) / 2;
                    if (isThisANewEnemy)
                        entity.CurrentHealth = entity.MaxHealth;                // setting new increased health number - but only if we are running for the first time

                    entity.Stats.SetPermanentStatValue(DFCareer.Stats.Speed, entity.Stats.PermanentSpeed + 10);
                    entity.Stats.SetPermanentStatValue(DFCareer.Stats.Endurance, entity.Stats.PermanentEndurance + 10);
                    entity.Stats.SetPermanentStatValue(DFCareer.Stats.Agility, entity.Stats.PermanentAgility + 20);
                    entity.Stats.SetPermanentStatValue(DFCareer.Stats.Strength, entity.Stats.PermanentStrength + 20);
                    entity.Stats.SetPermanentStatValue(DFCareer.Stats.Luck, entity.Stats.PermanentLuck + 10);                      // increasing stats for all extra strong monsters                    

                    /* TODO: here comes the code to boost certain skills (melee skills) and possibly other characteristics for all extra strong monsters */
                    // increase the armor
                    // minDmg multiplicator 2x to 4x
                    // maxDmg

                    SilentMessage("An extra strong "+ entity.Career.Name+".");
                    break;
                default:
                    SilentMessage("Not an extra strong " + entity.Career.Name + ".");
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

           SilentMessage(title + " Career.Name: " + entity.Career.Name + System.Environment.NewLine +
                "\tLevel: " + entity.Level + "  Health: " + entity.CurrentHealth + "/" + entity.MaxHealth + "  SpellPoints: " + entity.CurrentMagicka + "/" + entity.MaxMagicka +
                "  Spell Missile Speed: " + entityEffectManager.FireMissilePrefab.MovementSpeed +           // only prints the Fire Missile speed but this should do as the others should be equal
                System.Environment.NewLine +
                "\tAttributes.  Willpower=" + entity.Stats.PermanentWillpower + "  Intelligence=" + entity.Stats.PermanentIntelligence + "  Speed=" + entity.Stats.PermanentSpeed +
                "  Luck=" + entity.Stats.PermanentLuck + "  Agility=" + entity.Stats.PermanentAgility + "  Strength=" + entity.Stats.PermanentStrength +
                "  Endurance=" + entity.Stats.PermanentEndurance + "  Personality=" + entity.Stats.PermanentPersonality + System.Environment.NewLine +
                skillString + System.Environment.NewLine +  
                "\tResistances. Fire=" + entity.Resistances.PermanentFire + " Frost=" + entity.Resistances.PermanentFrost + " DiseaseOrPoison=" + entity.Resistances.PermanentDiseaseOrPoison +
                " Shock=" + entity.Resistances.PermanentShock + " Magic =" + entity.Resistances.PermanentMagic + System.Environment.NewLine +
                itemString + System.Environment.NewLine + 
                armorString ); 
        }
        /*  The random seed is saved/loaded via a Parchment object that has its name edited,  Name of this object is 'MMM????'
         *  Base Level is stored in another such object that has its name edited to 'BLMMM??'  */
        int GetEntityRandomSeed()
        {
            if (entity.Items != null)
            {
                int numberOfItems = entity.Items.Count;
                if (numberOfItems > 0)
                {
                    for (int i = 0; i < numberOfItems; i++)
                    {
                        DaggerfallUnityItem item = entity.Items.GetItem(i);
                        if (item.shortName.Substring(0, 3) == "MMM")                        
                            return Int32.Parse(item.shortName.Substring(3));                              
                    }
                }
            }
            return -1;
        }        

        int GetEntityBaseLevel()
        {
            if (entity.Items != null)
            {
                int numberOfItems = entity.Items.Count;
                if (numberOfItems > 0)
                {
                    for (int i = 0; i < numberOfItems; i++)
                    {
                        DaggerfallUnityItem item = entity.Items.GetItem(i);
                        if (item.shortName.Substring(0, 5) == "BLMMM")
                        {
                            return Int32.Parse(item.shortName.Substring(5)); 
                        }
                    }
                }
            }
            return -1;
        }

        void SetEntityRandomSeed(int ourSeed)    
        {
            if (entity.Items != null)
            {
                DaggerfallUnityItem newItem = ItemBuilder.CreateItem(ItemGroups.UselessItems2, (int)UselessItems2.Parchment);
                newItem.shortName = "MMM" + ourSeed.ToString("D4");
                entity.Items.AddItem(newItem);
            }
        }

        void SetEntityBaseLevel (int baseLevel)
        {
            if (entity.Items != null)
            {
                DaggerfallUnityItem newItem = ItemBuilder.CreateItem(ItemGroups.UselessItems2, (int)UselessItems2.Parchment);
                newItem.shortName = "BLMMM" + baseLevel.ToString("D2");
                entity.Items.AddItem(newItem);
            }
        }

        void SetEquipmentMaterials()
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

                    SilentMessage("Instead of " + oldItemString + " added and equipped " + tokens2[0].text + " (TI=" + item2.TemplateIndex + ")");

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

                    SilentMessage("Instead of " + oldItemString + " added and equipped " + tokens2[0].text + " (TI=" + item2.TemplateIndex + ") -- poisontype=" + item2.poisonType);
                }
            }
        }
                
        // LVL1  - LVL10 -> MissileSpeed = 11.0f + LVL (at LVL2, it is 13.0f)
        // LVL10 - LVL19 -> MissileSpeed = 21.0f + (LVL-10)*2 
        // LVL20 - LVL29 -> MissileSpeed = 41.0f + (LVL-20)*2.5
        // LVL30 -       -> MissileSpeed = 67.0f  
        float CalculateDesirableEnemyMissileSpeeds (int level)
        {
            if (level < 1) return 11.0f;
            if (level <= 10)
                return (float)(11 + level);
            if (level <= 20)
                return (float)(21 + (level - 10) * 2);
            if (level <= 30)
                return (float)(41 + (level - 20) * 5 / 2);  // INT *2.5
            return 67.0f;
        }              

        void setMissileSpeedInPrefabs()
        {
            float desirableMissileSpeeds = CalculateDesirableEnemyMissileSpeeds(entity.Level);
            SilentMessage("setMissileSpeedInPrefabs called: "+ desirableMissileSpeeds+" level calculated as appropriate, applying to prefabs...");

            entityEffectManager.FireMissilePrefab.MovementSpeed = desirableMissileSpeeds;
            entityEffectManager.ColdMissilePrefab.MovementSpeed = desirableMissileSpeeds;
            entityEffectManager.PoisonMissilePrefab.MovementSpeed = desirableMissileSpeeds;
            entityEffectManager.ShockMissilePrefab.MovementSpeed = desirableMissileSpeeds;
            entityEffectManager.MagicMissilePrefab.MovementSpeed = desirableMissileSpeeds;            
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
                SilentMessage("RecalcStats.Start() has been called. Entitybehaviour is null. exiting");
                return;
            }


            entityEffectManager = GetComponent<EntityEffectManager>();
            if (entityEffectManager == null)
            {
                SilentMessage("RecalcStats.Start() has been called. entityEffectManager is null. exiting");
                return;
            }

            entity = entityBehaviour.Entity as EnemyEntity; // what if it is not Enemy?? answer: we would not be here. We are only part of the enemy prefab.
            if (entity == null)
            {
                SilentMessage("RecalcStats.Start() has been called. Entity is null. exiting");
                return;
            }            

            MobileEnemy mobileEnemy = entity.MobileEnemy;
            EntityTypes entityType = entity.EntityType;
            int careerIndex = entity.CareerIndex;            
            
            SilentMessage("==========================================================================================" + System.Environment.NewLine +
                "RecalcStats.Start() has been called. MobileEnemy ID: " + mobileEnemy.ID + ", entityType: " + entityType + ", career name: " + entity.Career.Name);
            
            EntityCharacteristicsToLog("Enemy characteristics BEFORE any changes made. ");

            bool isThisANewEnemy = false;
            int ourSeed = GetEntityRandomSeed();
            if (ourSeed == -1)
            {
                isThisANewEnemy = true;
                ourSeed = globalNumberGenerator.Next(0, 10000);
                SetEntityRandomSeed(ourSeed);
                SilentMessage("No MMM signal artifact found on entity. Generated and set a random seed of " + ourSeed.ToString("D4"));
            }
            else
                SilentMessage("Found an MMM signal artifact on entity, random seed: " + ourSeed.ToString("D4"));

            ourNumberGenerator = new System.Random(ourSeed);

            /* here comes the code that does the work - but first, an insight into what we are up to
             *
             * what determines an enemy?
             * - its looks, gender, height etc (this mod is not concerned with any of these)
             * 
             * what determines the stength of an enemy?
             * - its max health, fatigue, magicka (this mod IS concerned with max health and magicka - max fatigue seems not to level)
             * - its stats (this mod was not overly concerned with this - because stats are set in game from Career and do not level, but we ARE changing this, making stats higher for higher-level enemies, not just skills
             *                  will also explore the possibility of increase stats for 'super strong enemies')
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
             *              we will consider Town Guards separately
             *
             * (1) As for dungeon mobs, Ralzar's basic idea that he applied to loot is a good one: invent a 'Dungeon Quality Level' and make the opponents' level contingent on that.
             * Here I would add that some difference should be allowed (like from DQL-2 to DQL+2). Ralzar has already invented DQL in Unleveled Loot, we will go with his method for now, with some additions.
             * First, HumanStronghold, RuinedCastle, Prison should also have a DQL assigned. 
             * Also, there should be some flexibility when it comes to dungeon levels. E.g. having all DragonsDen dungeons at level 18 is a good start, but perhaps having all concrete such dungeons having a level from
             * 18-2 to 18+2 might be even better. 
             * These dungeon levels should not be random. They should be deterministic. Could be calculated from the dungeon class and a hash of the dungeon oordinates and the actual quarter.
             * This way any given dungeon can gain or loose some quality as time passes.
             * An example: any concrete VampireHaunt dungeon should have a level calculated as a base (15) + modifier (from -2 to 2 calculated based on hash)
             * 
             * (2) Aa for random encounters, let's invent a Local Difficulty Level (LDL) and use it like DQL.
             *      For now, using a fixed figure of 10. LDL could in turn be contingent on
             *      - proximity to roads, settlements, dungeons or other locations
             *      - seasonal variance    
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
                    SilentMessage("   Not a class enemy. Checking if it is an extra strong monster, then setting equipment materials according to new potentially higher level.");
                    SetExtraStrongMonsterStats(isThisANewEnemy);                    
                }
                else
                    SilentMessage("   Not a class enemy. Setting equipment materials according to its level.");
                    // if it is a monster, we set it to be an extra strong one or not, and then process its items

                SetEquipmentMaterials();
                EntityCharacteristicsToLog("Enemy monster characteristics AFTER changes made. ");

                if (enemySpellMissileSpeeds == 1)
                    setMissileSpeedInPrefabs();                

                return; 
            }

            // careerindex and stats already set - they are okay that way for now - will be setting them based on level
            // TODO: if super strong enemies will have increased stats, the code could come here

            // I. So now, setting the level.

            /* If random encounter, then
             *  10%-10%-10% chance of enemy at local difficulty level-4 .. .. LDL +3
             *  10% chance of an enemy level lower than LDL-4 (down to LDL-10)
             *  10% chance of an enemy level higher than LDL+3  (up to LDL+9)
             *   ------ all this capped at playerlevel=23; above player level 23, the level of class enemies will not be getting higher */
            if (!GameManager.Instance.PlayerEnterExit.IsPlayerInsideDungeon)
            {
                int localDifficultyLevel = GetEntityBaseLevel();
                if (localDifficultyLevel == -1)                
                    SilentMessage("Player is not inside a dungeon. No BLMMM signal artifact found on entity.");                
                else
                    SilentMessage("Player is not inside a dungeon. Found a BLMMM signal artifact on entity, base level: " + localDifficultyLevel.ToString("D2"));


                if (careerIndex != (int)MobileTypes.Knight_CityWatch)
                {
                    SilentMessage("The entity is NOT a townguard: level-setting by random encounters method.");

                    if (localDifficultyLevel==-1)
                        localDifficultyLevel = GetPlaceAndTimeDifficultyLevel();
                                // will now take into account map pixel climate and time of day (maybe later season)

                    SetEntityBaseLevel(localDifficultyLevel); // save BLMMM object

                    tempRandomVar = ourNumberGenerator.Next(0, 10);
                    int tempRandomVar2 = ourNumberGenerator.Next(0, 6);

                    switch (tempRandomVar)
                    {
                        case 0: entity.Level = localDifficultyLevel - 10 + tempRandomVar2;
                            break;
                        case 9: entity.Level = localDifficultyLevel + 4 + tempRandomVar2;
                            break;
                        default:
                            entity.Level = localDifficultyLevel - 4 + tempRandomVar - 1;
                            break;
                    }
                    SilentMessage("localDifficultyLevel=" + localDifficultyLevel + ", RND1 = "+ tempRandomVar + ", RND2=" + tempRandomVar2 + ":: Level set to " + entity.Level);
                }
                else  // for townguards
                {
                    SilentMessage("The entity IS a townguard: setting level accordingly");
                    //      If townguard, we implement a method like we have for dungeons :: Town Guard Quality Level
                    //  This quality level is contingent on the size and prominance of the town:
                    //      Daggerfall, Wayrest and Sentinel: base level of 21, like the hardest of dungeons as per Ralzar
                    //      other regional capitals LVL18
                    //      other cities LVL15
                    //      smallest of towns LVL8
                    //      general: LVL12
                    //              there is also a small (-2..+2) correction based on time to account for the fluctuation of the quality of town guards available from time to time 
                    //          The individual guards can deviate form their guard (average) level (-2..+2)                    

                    if (localDifficultyLevel == -1)
                    {
                        localDifficultyLevel = 12;     // if nothing else triggers, this should give a 12

                        // here comes the code than determines what kind of town the player is in and changes the townGuardLevel accordingly
                        switch (GameManager.Instance.PlayerGPS.CurrentLocationType)
                        {
                            case DFRegion.LocationTypes.TownCity:
                                localDifficultyLevel = 15;
                                break;
                            case DFRegion.LocationTypes.TownHamlet:
                                localDifficultyLevel = 12;
                                break;
                            case DFRegion.LocationTypes.TownVillage:
                                localDifficultyLevel = 8;
                                break;
                        }
                        // here to adjust for other capitals -- TODO: check if all capitals name is the same as the region (I think there is one or two exceptions)
                        if (GameManager.Instance.PlayerGPS.CurrentLocation.Name == GameManager.Instance.PlayerGPS.CurrentLocation.RegionName)
                            localDifficultyLevel = 18;

                        // now to adjust for Daggerfall, Wayrest and Sentinel
                        if (GameManager.Instance.PlayerGPS.CurrentLocation.Name == "Daggerfall" || GameManager.Instance.PlayerGPS.CurrentLocation.Name == "Wayrest" ||
                             GameManager.Instance.PlayerGPS.CurrentLocation.Name == "Sentinel")
                            localDifficultyLevel = 21;                                                       // TODO: test these in-game to see if it triggers, extensive debug messaging


                        //  here comes the code that applies the correction for fluctuation
                        int[] modifierTable = { 0, 1, 2, 1, 0, -1, -2, -1 };

                        int modifierBasedOnHash = 0;       // this should be a modifier from -2 to +2 that is contingent on a hash from the coordinates of the place and the time (like which quarter it is)

                        int currentYear = DaggerfallUnity.Instance.WorldTime.Now.Year;
                        int currentMonth = DaggerfallUnity.Instance.WorldTime.Now.Month;
                        // X and Y already defined at the beginning of the method

                        int sequence = (X + Y + currentYear * 4 + currentMonth / 3) % 8;
                        modifierBasedOnHash = modifierTable[sequence];

                        localDifficultyLevel += modifierBasedOnHash;

                        SetEntityBaseLevel(localDifficultyLevel); // save BLMMM object
                    }

                    int townGuardLevel = localDifficultyLevel;

                    int[] individualModifierTable = { -2, -1, -1, 0, 0, 0, 0, +1, +1, +2 };
                    townGuardLevel += individualModifierTable[ourNumberGenerator.Next(0, 10)];     // apply the individual fluctuation
                                                                                                   // TODO: double check all this and add debug messages
                    entity.Level = townGuardLevel;  
                }                            
            }
            else
                /* If we are inside a dungeon, then the enemy level should depend on dungeon difficulty level.
                 * 10% DQL-2, 20% DQL-1, 40% DQL, 20% DQL+1, 10% DQL+2                 */   
            {
                tempRandomVar = ourNumberGenerator.Next(0, 10);                                                     

                int dungeonQualityLevel = MTMostlyMagicMod.dungeonDifficulty();        // MT check: getDungeonQuality ???
                SilentMessage("Player is inside a dungeon type "+ MTMostlyMagicMod.dungeon+" of DL: "+ dungeonQualityLevel + " random number picked is "+ tempRandomVar);
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
                SilentMessage("This is an exceptionally weak enemy - e.g. a trainee at this place.");
            }
            if (excellenceRandomVar == 19)
            {
                entity.Level += 5;
                SilentMessage("This is an exceptionally strong enemy - e.g. an elite or trainer at this place.");
            }

            if (entity.Level < 1) entity.Level = 1;
            /* Here I try to explain how I think these should work. What the results of all these calculations should be.
             * For dungeons, the base is the dungeon quality level. The basic idea was borrowed from Ralzar's mod. This can range from 3 to 23.
             *              Most enemies' level will fall within the DQL-2 to DQL+2 interval. So, the least quality dungeons can have as low as LVL1 foes while the highest quality dungeons can have as high as level 25 ones.
             *              The exception that you can meet are exceptionally talented foes applies in dungeons as well, so the overall maximum for dungeons is 30 level foes.
             * For random encounters // TODO: this part is no longer valid. Need to re-write
             *              most enemies will be in the playerlevel-4 to playerlevel+3 range, with a max level of 26 for a playerlevel of 23.
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

            entity.MaxHealth = MMMFormulaHelper.RollEnemyClassMaxHealth(entity.Level, entity.Career.HitPointsPerLevel, ourNumberGenerator);                              

            if (isThisANewEnemy)
            {
                entity.CurrentHealth = entity.MaxHealth;    
                SilentMessage("\tEntity level +" + entity.Level + ", Max Health set to " + entity.MaxHealth + ", since running for the first time, also set Current Health to " + entity.CurrentHealth);
            }
            else
                SilentMessage("\tEntity level +" + entity.Level + ", Max Health set to " + entity.MaxHealth+ ", since not running for the first time in the given enemy, leaving Current Health alone");

                // III. now, re-setting stats and skills :: a max of 150 for skills to make the strongest ones more difficult
            short skillsLevel = (short)((entity.Level * 4) + 34);
            if (skillsLevel > 150)
            {
                skillsLevel = 150;
            }

            for (int i = 0; i <= DaggerfallSkills.Count; i++)
            {
                entity.Skills.SetPermanentSkillValue(i, skillsLevel);
            }

            int levelIncreaseInEnemyStats = (entity.Level - 1) * 2 / 3;
            for (int i = 0; i < 8; i++)
                entity.Stats.SetPermanentStatValue(i, entity.Stats.GetPermanentStatValue(i) + levelIncreaseInEnemyStats);
                        // Stats now set based on not only career, but also level
                        // This is a simplistic solution, TODO: sepparate spellcasters from non-spellcasters and set attributes individually

            SilentMessage("   Skills set to " + skillsLevel+", Attributes increased by "+ levelIncreaseInEnemyStats);

                

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

                if (enemySpellMissileSpeeds == 1)
                    setMissileSpeedInPrefabs();             
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
                SetEquipmentMaterials();
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

            EntityCharacteristicsToLog("Class Enemy characteristics AFTER changes made. ");

        }        
    }
}
