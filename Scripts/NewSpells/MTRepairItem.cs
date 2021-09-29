// Project:         MeriTamas's (Mostly) Magic Mod for Daggerfall Unity (http://www.dfworkshop.net)
// Copyright:       Copyright (C) 2021 meritamas
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Author:          meritamas (meritamas@outlook.com)
// Credits due to the DFU developers based on the work of whom this class could be created.
// 
// Notes:
//

using System;
using System.Collections.Generic;
using DaggerfallConnect;
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

        DaggerfallListPickerWindow itemPicker;
        List<DaggerfallUnityItem> validRepairItems = new List<DaggerfallUnityItem>();        

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
            properties.MagnitudeCosts = MakeEffectCosts(14, 28);       // TODO: check out what this means exactly
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

        private void ItemPicker_OnItemPicked(int index, string itemString)      // TODO: put the repair mechanic here
        {
            DaggerfallUnityItem itemToRepair = validRepairItems[index];
            int maximumRepairAmount = 50 * MMMFormulaHelper.GetSpellMagnitude(this, manager);
            /*  about the coefficient :: The basic idea is as follows - some basic calculations and then reverse-engineer from that
             *  with the same parameters, it should replenish the condition of a Steel Cuirass like the Stamina spell replenishes Fatigue for an "average character"
             *  concretely: at LVL 15, Stamina (1-8 + 2/LVL) recharges about 35, an "average character" for our purposes could be one with END 70 and STR 70, so max fatigue = 140
             *  so, at LVL 15, it takes about 4 Stamina spells to fully recharge fatigue
             *  so, at LVL 15, it should take a similar (1-8 + 2/LVL) RepairItem spell about four times to recharge the condition of a Steel Cuirass 
             *  so, a RepairItem spell with a magnitude of 35 should take four iterations to completely refill 6300
             *  from this, it follows that one magnitude of the spell should bring about 45 points of condition restored to an item (45*35*4 = 6300) 
                45 rounded to the nearest ten, that is 50 */

            int repairAmount = itemToRepair.maxCondition - itemToRepair.currentCondition;
            if (maximumRepairAmount < repairAmount) repairAmount = maximumRepairAmount;
            itemToRepair.currentCondition += repairAmount;

            // Close picker and unsubscribe event
            UserInterfaceManager uiManager = DaggerfallUI.Instance.UserInterfaceManager;
            itemPicker.OnItemPicked -= ItemPicker_OnItemPicked;
            itemPicker.CloseWindow();

            // End effect - repair done
            End();
        }
    }
}
