// Project:         MeriTamas's (Mostly) Magic Mod for Daggerfall Unity (http://www.dfworkshop.net)
// Copyright:       Copyright (C) 2021 meritamas
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Author:          meritamas (meritamas@outlook.com)
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
    public class MTMultiplyProvisions : BaseEntityEffect
    {
        public static readonly string EffectKey = "MultiplyProvisions";

        public static TextFile.Token[] ourSpellMakerDescription = new TextFile.Token[] {
            new TextFile.Token(TextFile.Formatting.Text, "Multiply Provisions"),
            new TextFile.Token(TextFile.Formatting.JustifyCenter, null),
            new TextFile.Token(TextFile.Formatting.Text, "If you have some of a provision (water, rations, arrows), this effect can add to the provision through conjuration."),
            new TextFile.Token(TextFile.Formatting.JustifyCenter, null),
            new TextFile.Token(TextFile.Formatting.Text, "Will work better when physically near to things that is being conjured up. (E.g. settlements, homes, temples)"),
            new TextFile.Token(TextFile.Formatting.JustifyCenter, null),
            new TextFile.Token(TextFile.Formatting.NewLine, null),
            new TextFile.Token(TextFile.Formatting.Text, "Target: an item that the player is conjuring up"),
            new TextFile.Token(TextFile.Formatting.JustifyCenter, null),
            new TextFile.Token(TextFile.Formatting.Text, "Magnitude: has an exponential effect on how much new provisions are added"),
            new TextFile.Token(TextFile.Formatting.JustifyCenter, null),
            new TextFile.Token(TextFile.Formatting.Text, "(strong spells will generate increasingly bigger increases)"),
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

        static string messagePrefix = "MTMultiplyProvisions: ";
        public static float minimumModifier = 0.75F;
        public static float maximumModifier = 1.33F;

        DaggerfallListPickerWindow itemPicker;
        static List<DaggerfallUnityItem> validProvisionItems = new List<DaggerfallUnityItem>();

        static void Message(string message)
        {
            MTMostlyMagicMod.Message(messagePrefix + message);
        }

        static void SilentMessage(string message)
        {
            MTMostlyMagicMod.SilentMessage(messagePrefix + message);
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
            properties.MagicSkill = DFCareer.MagicSkills.Mysticism;
            properties.MagnitudeCosts = MakeEffectCosts(25, 50);       // these numbers will do for now
        }

        public bool IsAValidProvisionItem(DaggerfallUnityItem item)      
        {
            if ((item.TemplateIndex == 539) && (item.weightInKg < 2.0)) return true;    // waterskin
            if ((item.TemplateIndex == 531) && (item.weightInKg < 4.0)) return true;    // rations
            if (String.Equals(item.shortName, "Arrow")) return true;                    // arrow(s)
                
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

            validProvisionItems.Clear(); // Clears the valid provision item list before every multiply provisions spell use
            PlayerEntity playerEntity = GameManager.Instance.PlayerEntity;
            int itemCount = playerEntity.Items.Count;

            for (int i = 0; i < itemCount; i++)
            {
                DaggerfallUnityItem item = playerEntity.Items.GetItem(i);                
                if (IsAValidProvisionItem(item))
                {
                    validProvisionItems.Add(item);
                    string validItemName = item.LongName +" (" + item.weightInKg+"kg, " + item.stackCount + " pcs)";
                    itemPicker.ListBox.AddItem(validItemName);
                }
            }                       

           /* if (itemPicker.ListBox.Count == 1)          // this should simplify things if there is only one provision item
            {
                MultiplyProvision(validProvisionItems[0]);
                EndItemPicking();
            }*/

            if (itemPicker.ListBox.Count == 0)
            {
                DaggerfallUI.MessageBox("You have no provisions that could be multiplied.");
                    // End effect - nothing to multiply
                End();
                return;
            }

            // Prompt for item selection            
            itemPicker.PreviousWindow = uiManager.TopWindow;
            uiManager.PushWindow(itemPicker);
        }

        private void MultiplyProvision (DaggerfallUnityItem provisionItem)
        { 
            int ourMagnitude = MMMFormulaHelper.GetSpellMagnitude(this, manager);
            float ourMultiplyCoefficient = 1.1F * (float)Math.Pow(1.025, ourMagnitude);
            switch (GameManager.Instance.PlayerGPS.CurrentLocationType)     // location modifier here
            {
                case DFRegion.LocationTypes.TownCity:
                case DFRegion.LocationTypes.TownVillage:
                case DFRegion.LocationTypes.TownHamlet:
                case DFRegion.LocationTypes.HomePoor:
                case DFRegion.LocationTypes.HomeWealthy:
                case DFRegion.LocationTypes.HomeFarms:
                case DFRegion.LocationTypes.ReligionTemple:
                    ourMultiplyCoefficient *= 2; 
                    break;
            }

            if (provisionItem.TemplateIndex == 539)     // the code for WaterSkins
            {
                int minResultingQuantityX10 = (int) Math.Truncate(provisionItem.weightInKg * 10 * ourMultiplyCoefficient * minimumModifier);
                int maxResultingQuantityX10 = (int) Math.Truncate(provisionItem.weightInKg * 10 * ourMultiplyCoefficient * maximumModifier);
                int resultingQuantityX10 = UnityEngine.Random.Range(minResultingQuantityX10, maxResultingQuantityX10);
                provisionItem.weightInKg = Math.Min(((float)resultingQuantityX10) / 10, 2.0F);
                SilentMessage("WaterSkin, Magnitude = " + ourMagnitude+ " ourMultiplyCoefficient="+ ourMultiplyCoefficient+ " minResultingQuantityX10="+ minResultingQuantityX10+
                    " maxResultingQuantityX10="+maxResultingQuantityX10+ " resultingQuantityX10="+ resultingQuantityX10+" newweight="+ provisionItem.weightInKg);                
            }

            if (provisionItem.TemplateIndex == 531)     // the code for Rations
            {
                int minResultingQuantity = (int) Math.Truncate(provisionItem.weightInKg * ourMultiplyCoefficient * minimumModifier);
                int maxResultingQuantity = (int) Math.Truncate(provisionItem.weightInKg * ourMultiplyCoefficient * maximumModifier);
                int resultingQuantity = UnityEngine.Random.Range(minResultingQuantity, maxResultingQuantity);
                provisionItem.weightInKg = Math.Min((float)resultingQuantity, 4.0F);
                SilentMessage("MTMultiplyProvisions : Rations, Magnitude = " + ourMagnitude + " ourMultiplyCoefficient=" + ourMultiplyCoefficient + " minResultingQuantity=" + minResultingQuantity +
                    " maxResultingQuantity=" + maxResultingQuantity + " resultingQuantity=" + resultingQuantity + " newweight=" + provisionItem.weightInKg);
            }

            if (String.Equals(provisionItem.shortName, "Arrow"))      // the code for Arrows
            {
                int minResultingQuantity = (int)Math.Truncate(provisionItem.stackCount * ourMultiplyCoefficient * minimumModifier);
                int maxResultingQuantity = (int)Math.Truncate(provisionItem.stackCount * ourMultiplyCoefficient * maximumModifier);
                int resultingQuantity = UnityEngine.Random.Range(minResultingQuantity, maxResultingQuantity);
                provisionItem.stackCount = Math.Max(resultingQuantity, 4);
                SilentMessage("MTMultiplyProvisions : Arrows, Magnitude = " + ourMagnitude + " ourMultiplyCoefficient=" + ourMultiplyCoefficient + " minResultingQuantity=" + minResultingQuantity +
                    " maxResultingQuantity=" + maxResultingQuantity + " resultingQuantity=" + resultingQuantity + " newstackCount=" + provisionItem.stackCount);
            }                      
        }

        private void ItemPicker_OnItemPicked(int index, string itemString)      
        {
            DaggerfallUnityItem provisionItemToMultiply = validProvisionItems[index];

            MultiplyProvision(provisionItemToMultiply);

            EndItemPicking();            
        }

        private void EndItemPicking()
        {
            // Close picker and unsubscribe event
            UserInterfaceManager uiManager = DaggerfallUI.Instance.UserInterfaceManager;
            itemPicker.OnItemPicked -= ItemPicker_OnItemPicked;
            itemPicker.CloseWindow();

            // End effect - multiply done
            End();
        }
    }
}
