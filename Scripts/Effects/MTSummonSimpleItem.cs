// Project:         MeriTamas's (Mostly) Magic Mod for Daggerfall Unity (http://www.dfworkshop.net)
// Copyright:       Copyright (C) 2022 meritamas
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Author:          meritamas (meritamas@outlook.com)

// Class is based on the Create Item class which is part of the Daggerfall Tools For Unity project - many parts retained unchanged
// Project:         Daggerfall Tools For Unity
// Copyright:       Copyright (C) 2009-2021 Daggerfall Workshop
// Web Site:        http://www.dfworkshop.net
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Source Code:     https://github.com/Interkarma/daggerfall-unity
// Original Author: Gavin Clayton (interkarma@dfworkshop.net)

using System;
using DaggerfallConnect;
using DaggerfallConnect.Arena2;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.Items;
using DaggerfallWorkshop.Game.UserInterface;
using DaggerfallWorkshop.Game.UserInterfaceWindows;
using DaggerfallWorkshop.Game.MagicAndEffects;

namespace MTMMM
{
    /// <summary>
    /// Create Item
    /// </summary>
    public class MTSummonSimpleItem : BaseEntityEffect
    {
        public static readonly string EffectKey = "CreateItem";

        public override string GroupName
        {
            get { return EffectKey; }
        }

        static string messagePrefix = "MTSummonSimpleItem: ";

        DaggerfallListPickerWindow itemPicker;
        static int lastSelectedIndex = 0;

        enum CreateItemSelection
        {
            LeatherCuirass,
            LeatherGauntlets,
            LeatherGreaves,
            LeatherLeftPauldron,
            LeatherRightPauldron,
            LeatherHelm,
            LeatherBoots,
            ChainCuirass,
            ChainGauntlets,
            ChainGreaves,
            ChainLeftPauldron,
            ChainRightPauldron,
            ChainHelm,
            ChainBoots,
            SteelCuirass,
            SteelGauntlets,
            SteelGreaves,
            SteelLeftPauldron,
            SteelRightPauldron,
            SteelHelm,
            SteelBoots,
            SteelBuckler,
            SteelDagger,
            SteelLongsword,
            SteelStaff,
            ShortBow,
            Arrows,
            SteelBattleAxe,
            Robes,
        }

        static void Message(string message)
        {
            MTMostlyMagicMod.Message(messagePrefix + message);
        }

        static void SilentMessage(string message)
        {
            MTMostlyMagicMod.SilentMessage(messagePrefix + message);
        }

        static void HUDMessage(string message)
        {
            MTMostlyMagicMod.Message(message, true, true, true, false, false);
        }


        public MTSummonSimpleItem()
        {
            // Setup item picker for effect selection
            UserInterfaceManager uiManager = DaggerfallUI.Instance.UserInterfaceManager;
            itemPicker = new DaggerfallListPickerWindow(uiManager);
            itemPicker.OnItemPicked += ItemPicker_OnItemPicked;
            itemPicker.AllowCancel = false;
            foreach (CreateItemSelection item in Enum.GetValues(typeof(CreateItemSelection)))
            {
                itemPicker.ListBox.AddItem(TextManager.Instance.GetLocalizedText(item.ToString()));
            }
            itemPicker.ListBox.SelectIndex(lastSelectedIndex);
            itemPicker.ListBox.ScrollToSelected();
        }


        public override void SetProperties()
        {
            properties.Key = EffectKey;
            properties.ClassicKey = MakeClassicKey(2, 255);
            properties.SupportDuration = true;            
            properties.ShowSpellIcon = false;
            properties.AllowedTargets = EntityEffectBroker.TargetFlags_Self;
            properties.AllowedElements = EntityEffectBroker.ElementFlags_MagicOnly;
            properties.AllowedCraftingStations = MagicCraftingStations.SpellMaker;
            properties.MagicSkill = DFCareer.MagicSkills.Mysticism;
            properties.DurationCosts = MakeEffectCosts(60, 120);
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

            // Prompt for item selection
            UserInterfaceManager uiManager = DaggerfallUI.Instance.UserInterfaceManager;
            itemPicker.PreviousWindow = uiManager.TopWindow;
            uiManager.PushWindow(itemPicker);
        }

        private void ItemPicker_OnItemPicked(int index, string itemString)
        {
            lastSelectedIndex = index;

            //Add selected item to inventory 
            DaggerfallUnityItem item = CreateNewItem((CreateItemSelection)index);
            if (item != null)
                GameManager.Instance.PlayerEntity.Items.AddItem(item);
            else
                HUDMessage("Conjuration spell effect failed.");

            // Close picker and unsubscribe event
            UserInterfaceManager uiManager = DaggerfallUI.Instance.UserInterfaceManager;
            itemPicker.OnItemPicked -= ItemPicker_OnItemPicked;
            itemPicker.CloseWindow();
            
            End();
        }

        DaggerfallUnityItem CreateNewItem(CreateItemSelection selection)
        {
            Genders gender = GameManager.Instance.PlayerEntity.Gender;
            Races race = GameManager.Instance.PlayerEntity.Race;
            DaggerfallUnityItem item = null;

            int randomNumber = UnityEngine.Random.Range(0, 100);

            int baseChanceOfSuccess = MMMFormulaHelper.GetSpellDuration(this);

            int locationCoefficient = 1;
            switch (GameManager.Instance.PlayerGPS.CurrentLocationType)
            {
                case DFRegion.LocationTypes.TownCity:
                    locationCoefficient = 8;
                    break;
                case DFRegion.LocationTypes.TownHamlet:
                    locationCoefficient = 4;
                    break;
                case DFRegion.LocationTypes.TownVillage:
                    locationCoefficient = 2;
                    break;
            }
            Message("MTSummonSimpleItem : locationCoefficient generated = "+ locationCoefficient+", base chance of success is "+ baseChanceOfSuccess);
            
            if (randomNumber >= baseChanceOfSuccess * locationCoefficient)
                return item;

            switch (selection)
            {
                case CreateItemSelection.LeatherCuirass:
                    item = ItemBuilder.CreateArmor(gender, race, Armor.Cuirass, ArmorMaterialTypes.Leather);
                    break;
                case CreateItemSelection.LeatherGauntlets:
                    item = ItemBuilder.CreateArmor(gender, race, Armor.Gauntlets, ArmorMaterialTypes.Leather);
                    break;
                case CreateItemSelection.LeatherGreaves:
                    item = ItemBuilder.CreateArmor(gender, race, Armor.Greaves, ArmorMaterialTypes.Leather);
                    break;
                case CreateItemSelection.LeatherLeftPauldron:
                    item = ItemBuilder.CreateArmor(gender, race, Armor.Left_Pauldron, ArmorMaterialTypes.Leather);
                    break;
                case CreateItemSelection.LeatherRightPauldron:
                    item = ItemBuilder.CreateArmor(gender, race, Armor.Right_Pauldron, ArmorMaterialTypes.Leather);
                    break;
                case CreateItemSelection.LeatherHelm:
                    item = ItemBuilder.CreateArmor(gender, race, Armor.Helm, ArmorMaterialTypes.Leather);
                    break;
                case CreateItemSelection.LeatherBoots:
                    item = ItemBuilder.CreateArmor(gender, race, Armor.Boots, ArmorMaterialTypes.Leather);
                    break;
                case CreateItemSelection.ChainCuirass:
                    item = ItemBuilder.CreateArmor(gender, race, Armor.Cuirass, ArmorMaterialTypes.Chain);
                    break;
                case CreateItemSelection.ChainGauntlets:
                    item = ItemBuilder.CreateArmor(gender, race, Armor.Gauntlets, ArmorMaterialTypes.Chain);
                    break;
                case CreateItemSelection.ChainGreaves:
                    item = ItemBuilder.CreateArmor(gender, race, Armor.Greaves, ArmorMaterialTypes.Chain);
                    break;
                case CreateItemSelection.ChainLeftPauldron:
                    item = ItemBuilder.CreateArmor(gender, race, Armor.Left_Pauldron, ArmorMaterialTypes.Chain);
                    break;
                case CreateItemSelection.ChainRightPauldron:
                    item = ItemBuilder.CreateArmor(gender, race, Armor.Right_Pauldron, ArmorMaterialTypes.Chain);
                    break;
                case CreateItemSelection.ChainHelm:
                    item = ItemBuilder.CreateArmor(gender, race, Armor.Helm, ArmorMaterialTypes.Chain);
                    break;
                case CreateItemSelection.ChainBoots:
                    item = ItemBuilder.CreateArmor(gender, race, Armor.Boots, ArmorMaterialTypes.Chain);
                    break;
                case CreateItemSelection.SteelCuirass:
                    item = ItemBuilder.CreateArmor(gender, race, Armor.Cuirass, ArmorMaterialTypes.Steel);
                    break;
                case CreateItemSelection.SteelGauntlets:
                    item = ItemBuilder.CreateArmor(gender, race, Armor.Gauntlets, ArmorMaterialTypes.Steel);
                    break;
                case CreateItemSelection.SteelGreaves:
                    item = ItemBuilder.CreateArmor(gender, race, Armor.Greaves, ArmorMaterialTypes.Steel);
                    break;
                case CreateItemSelection.SteelLeftPauldron:
                    item = ItemBuilder.CreateArmor(gender, race, Armor.Left_Pauldron, ArmorMaterialTypes.Steel);
                    break;
                case CreateItemSelection.SteelRightPauldron:
                    item = ItemBuilder.CreateArmor(gender, race, Armor.Right_Pauldron, ArmorMaterialTypes.Steel);
                    break;
                case CreateItemSelection.SteelHelm:
                    item = ItemBuilder.CreateArmor(gender, race, Armor.Helm, ArmorMaterialTypes.Steel);
                    break;
                case CreateItemSelection.SteelBoots:
                    item = ItemBuilder.CreateArmor(gender, race, Armor.Boots, ArmorMaterialTypes.Steel);
                    break;
                case CreateItemSelection.SteelBuckler:
                    item = ItemBuilder.CreateArmor(gender, race, Armor.Buckler, ArmorMaterialTypes.Steel);
                    break;
                case CreateItemSelection.SteelDagger:
                    item = ItemBuilder.CreateWeapon(Weapons.Dagger, WeaponMaterialTypes.Steel);
                    break;
                case CreateItemSelection.SteelLongsword:
                    item = ItemBuilder.CreateWeapon(Weapons.Longsword, WeaponMaterialTypes.Steel);
                    break;
                case CreateItemSelection.SteelStaff:
                    item = ItemBuilder.CreateWeapon(Weapons.Staff, WeaponMaterialTypes.Steel);
                    break;
                case CreateItemSelection.ShortBow:
                    item = ItemBuilder.CreateWeapon(Weapons.Short_Bow, WeaponMaterialTypes.Steel);
                    break;
                case CreateItemSelection.Arrows:
                    item = ItemBuilder.CreateWeapon(Weapons.Arrow, WeaponMaterialTypes.Steel);
                    break;
                case CreateItemSelection.SteelBattleAxe:
                    item = ItemBuilder.CreateWeapon(Weapons.Battle_Axe, WeaponMaterialTypes.Steel);
                    break;
                case CreateItemSelection.Robes:
                    if (gender == Genders.Male)
                        item = ItemBuilder.CreateMensClothing(MensClothing.Plain_robes, race);
                    else
                        item = ItemBuilder.CreateWomensClothing(WomensClothing.Plain_robes, race);
                    break;
            }

            int conditionPercentage = UnityEngine.Random.Range(45, 100);                    // conjuration is about getting an already existing item to go to you. these items will not always be brand new
            item.currentCondition = item.maxCondition * item.ConditionPercentage / 100;

            return item;
        }
    }
}
