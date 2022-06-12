// Project:         MeriTamas's (Mostly) Magic Mod for Daggerfall Unity (http://www.dfworkshop.net)
// Copyright:       Copyright (C) 2022 meritamas
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Author:          meritamas (meritamas@outlook.com)
// Credits due to the DFU developers based on the work of whom this class could be created.
// 
// Notes:
//

using System;
using System.Collections.Generic;
using DaggerfallConnect;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallConnect.Arena2;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.Items;
using DaggerfallWorkshop.Game.UserInterface;
using DaggerfallWorkshop.Game.UserInterfaceWindows;
using DaggerfallWorkshop.Game.MagicAndEffects;

namespace MTMMM
{
    /// <summary>
    /// Repair Item
    /// </summary>
    public class MTRepairItem : BaseEntityEffect
    {
        public static readonly string EffectKey = "FixItem";
        public static TextFile.Token[] ourSpellMakerDescription = new TextFile.Token[] {
            new TextFile.Token(TextFile.Formatting.Text, "Fix Item"),
            new TextFile.Token(TextFile.Formatting.JustifyCenter, null),
            new TextFile.Token(TextFile.Formatting.Text, "Capable of fixing mundane items, will restore simpler (e.g. leather, iron, steel) items faster."),
            new TextFile.Token(TextFile.Formatting.JustifyCenter, null),
            new TextFile.Token(TextFile.Formatting.NewLine, null),
            new TextFile.Token(TextFile.Formatting.Text, "Target: an item already in the player's inventory"),
            new TextFile.Token(TextFile.Formatting.JustifyCenter, null),
            new TextFile.Token(TextFile.Formatting.Text, "Magnitude: has a linear effect on how much the target item is restored"),
            new TextFile.Token(TextFile.Formatting.JustifyCenter, null),
            new TextFile.Token(TextFile.Formatting.Text, "Chance: N/A, Duration: N/A"),
            new TextFile.Token(TextFile.Formatting.JustifyCenter, null)
        };
        public static TextFile.Token[] OurSpellMakerDescription() { return ourSpellMakerDescription; }


        public override string GroupName
        {
            get { return EffectKey; }
        }
        public override TextFile.Token[] SpellMakerDescription => OurSpellMakerDescription();
        public override TextFile.Token[] SpellBookDescription => OurSpellMakerDescription();

        static string messagePrefix = "MTRepairItem: ";

        DaggerfallListPickerWindow itemPicker;
        List<DaggerfallUnityItem> validRepairItems = new List<DaggerfallUnityItem>();

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

        public override void SetProperties()
        {
            properties.Key = EffectKey;
            properties.SupportDuration = false;
            properties.SupportMagnitude = true;
            properties.ShowSpellIcon = false;
            properties.AllowedTargets = EntityEffectBroker.TargetFlags_Self;
            properties.AllowedElements = EntityEffectBroker.ElementFlags_MagicOnly;
            properties.AllowedCraftingStations = MagicCraftingStations.SpellMaker;
            properties.MagicSkill = DFCareer.MagicSkills.Alteration;
            properties.MagnitudeCosts = MakeEffectCosts(14, 28);       
        }

        public bool IsValidForRepair(DaggerfallUnityItem item)      //
        {
            if (item.currentCondition == item.maxCondition) return false;

            if (item.IsEnchanted || item.IsArtifact) return false;

            if (item.ItemGroup == ItemGroups.Armor || item.ItemGroup == ItemGroups.MensClothing || item.ItemGroup == ItemGroups.WomensClothing ||
                item.TemplateIndex == 530) return true; // 530 being the camping equipment intruduced by Climates & Calories

            DFCareer.Skills skill = item.GetWeaponSkillID();
            if (skill == DFCareer.Skills.ShortBlade || skill == DFCareer.Skills.LongBlade || skill == DFCareer.Skills.Axe || skill == DFCareer.Skills.BluntWeapon || skill == DFCareer.Skills.Archery) return true;

            return false;
        }

        public override void Start(EntityEffectManager manager, DaggerfallEntityBehaviour caster = null)
        {
            base.Start(manager, caster);
            PromptPlayer();
        }

        void PromptPlayer()
        {
            // Get peered entity gameobject
            DaggerfallEntityBehaviour entityBehaviour = GetPeeredEntityBehaviour(manager);
            if (!entityBehaviour)
                return;

            // Target must be player - no effect on other entities
            if (entityBehaviour != GameManager.Instance.PlayerEntityBehaviour)
                return;


            // Setup item picker
            UserInterfaceManager uiManager = DaggerfallUI.Instance.UserInterfaceManager;
            itemPicker = new DaggerfallListPickerWindow(uiManager);
            itemPicker.OnItemPicked += ItemPicker_OnItemPicked;
            itemPicker.AllowCancel = false;

            validRepairItems.Clear(); // Clears the valid item list before every repair item spell use
            PlayerEntity playerEntity = GameManager.Instance.PlayerEntity;
            int itemCount = playerEntity.Items.Count;

            for (int i = 0; i < playerEntity.Items.Count; i++)
            {
                DaggerfallUnityItem item = playerEntity.Items.GetItem(i);
                //int percentReduce = (int)Mathf.Floor(item.maxCondition * 0.15f); // For Testing Purposes right now.
                //item.LowerCondition(percentReduce); // For Testing Purposes right now.
                if (IsValidForRepair(item))
                {
                    validRepairItems.Add(item);
                    string validItemName = item.currentCondition + "/" + item.maxCondition + " (" + item.ConditionPercentage + "%)" + "      " + item.LongName;
                    itemPicker.ListBox.AddItem(validItemName);
                }
            }

            if (itemPicker.ListBox.Count == 0)
            {
                DaggerfallUI.MessageBox("You have no valid items in need of repair.");
                // End effect - nothing to repair
                End();
                return;
            }

            // Prompt for item selection            
            itemPicker.PreviousWindow = uiManager.TopWindow;
            uiManager.PushWindow(itemPicker);
        }

        /*  about the coefficient :: The basic idea is as follows - some basic calculations and then reverse-engineer from that
             *  with the same parameters, it should replenish the condition of a Steel Cuirass like the Stamina spell replenishes Fatigue for an "average character"
             *  concretely: at LVL 15, Stamina (1-8 + 2/LVL) recharges about 35, an "average character" for our purposes could be one with END 70 and STR 70, so max fatigue = 140
             *  so, at LVL 15, it takes about 4 Stamina spells to fully recharge fatigue
             *  so, at LVL 15, it should take a similar (1-8 + 2/LVL) RepairItem spell about four times to recharge the condition of a Steel Cuirass             *  
             *  so, a RepairItem spell with a magnitude of 35 should take four iterations to completely refill 6300
             *  from this, it follows that one magnitude of the spell should bring about 45 points of condition restored to an item (45*35*4 = 6300) 
                45 rounded to the nearest ten, that is 50

                One more twist (Or two).
                1. It should be more and more difficult to repair an item, the more damaged it is.
                So, the range of 0->10% should require energy equal to:
                    - 10%->30%
                    - 30%->60%
                    - 60%->100%
                2. This being a light spell designed for mundane, everyday items, it should be increasingly difficult to repair items of higher materials. */

            /// <summary>
            /// Gets the effective material difference between the Item's material and Steel
            /// </summary>
            /// <param name="item">Needs to be a weapon or armor piece.</param>
            /// <returns></returns>
        private int GetMaterialModifier(DaggerfallUnityItem item)
        {
            int material = item.NativeMaterialValue;

            if (item.ItemGroup == ItemGroups.Armor)     // if armor
            {
                switch (material)
                {
                    case (int) ArmorMaterialTypes.Leather:
                    case (int) ArmorMaterialTypes.Chain:
                    case (int)ArmorMaterialTypes.Chain2:
                    case (int)ArmorMaterialTypes.Iron:
                    case (int)ArmorMaterialTypes.Steel:
                        return 0;
                        break;
                    default:
                        return material % 256;
                        /*if (material < 256)
                            return material - (int)WeaponMaterialTypes.Steel;
                        else
                            return material - (int)ArmorMaterialTypes.Steel;*/
                }
            }
            else            // if weapon            
            {
                switch (material)
                {
                    case (int)WeaponMaterialTypes.Iron:
                    case (int)WeaponMaterialTypes.Steel:
                        return 0;
                        break;
                    default:
                        return material - (int)WeaponMaterialTypes.Steel;
                }
            }
        }

        private int CalculatePointsToReturn(DaggerfallUnityItem item, int magnitude)
        {
            double baseRepairPerMagnitude = UnityEngine.Random.Range(77, 82);      // this ensures that the spell does not restore the exact same amount of condition points each time            
            int magnitudePointsLeft = magnitude;

            double currentCondition = item.currentCondition;
            int maxCondition = item.maxCondition;
            double threshold10 = maxCondition / 10;
            double threshold30 = maxCondition * 3 / 10;
            double threshold60 = maxCondition * 6 / 10;
            double repairPerMagnitude = 0;      // should be subsequently overridden each time

            DFCareer.Skills skill = item.GetWeaponSkillID();
            if (item.ItemGroup == ItemGroups.Armor || skill == DFCareer.Skills.ShortBlade || skill == DFCareer.Skills.LongBlade ||
                skill == DFCareer.Skills.Axe || skill == DFCareer.Skills.BluntWeapon || skill == DFCareer.Skills.Archery)   // if weapon or armor
            {
                int materialModifier = GetMaterialModifier(item);
                repairPerMagnitude = baseRepairPerMagnitude * Math.Pow(0.5, materialModifier / 2);
                SilentMessage("Armor/Weapon: material {0}, calculated repairPerMagnitude {1}", item.NativeMaterialValue, repairPerMagnitude);
            }                
            
            if (item.ItemGroup == ItemGroups.MensClothing || item.ItemGroup == ItemGroups.WomensClothing || item.TemplateIndex == 530)
                        // 530 being the camping equipment intruduced by Climates & Calories
            {
                repairPerMagnitude = 4;
                SilentMessage("NOT Armor/Weapon: repairPerMagnitude set at {0}", repairPerMagnitude);
            }

            do
            {
                SilentMessage("WHILE: MagPointsLeft = {0}, CurrentCondition = {1}", magnitudePointsLeft, currentCondition);
                magnitudePointsLeft--;

                if (currentCondition < threshold10)
                {
                    currentCondition += repairPerMagnitude * 0.25;
                    continue;
                }

                if (currentCondition < threshold30)
                {
                    currentCondition += repairPerMagnitude * 0.5;
                    continue;
                }

                if (currentCondition < threshold60)
                {
                    currentCondition += repairPerMagnitude * 0.75;
                    continue;
                }

                currentCondition += repairPerMagnitude;
            } while (currentCondition < maxCondition && magnitudePointsLeft > 0);

            SilentMessage("Exiting while loop with {0} magnitude points left and CurrentCondition at {1}", magnitudePointsLeft, currentCondition);
            int currentConditionInt = Convert.ToInt32(currentCondition);
            
            return Math.Min(currentConditionInt, maxCondition) - item.currentCondition;
        }

        private void ItemPicker_OnItemPicked(int index, string itemString)      
        {
            DaggerfallUnityItem itemToRepair = validRepairItems[index];
            
            //int maximumRepairAmount = repairPerMagnitude * ;


            int repairAmount = CalculatePointsToReturn(itemToRepair, MMMFormulaHelper.GetSpellMagnitude(this, manager));

                        // code to prevent 'overcharging' the item  -- there in CalculatePointsToReturn - TODO: consider moving it here

            itemToRepair.currentCondition += repairAmount;

            SilentMessage("MTRepairItem : " + repairAmount + " condition points returned to item " + itemToRepair.LongName);

            // Close picker and unsubscribe event
            UserInterfaceManager uiManager = DaggerfallUI.Instance.UserInterfaceManager;
            itemPicker.OnItemPicked -= ItemPicker_OnItemPicked;
            itemPicker.CloseWindow();

            // End effect - repair done
            End();
        }
    }
}
