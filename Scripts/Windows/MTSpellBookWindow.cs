// Project:         MeriTamas's (Mostly) Magic Mod for Daggerfall Unity (http://www.dfworkshop.net)
// Copyright:       Copyright (C) 2023 meritamas
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Author:          meritamas (meritamas@outlook.com)

// This class was based on the DaggerfallSpellBookWindow class which is part of the Daggerfall Tools For Unity project - many parts retained unchanged
// Copyright:       Copyright (C) 2009-2020 Daggerfall Workshop
// Web Site:        http://www.dfworkshop.net
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Source Code:     https://github.com/Interkarma/daggerfall-unity
// Original Author: Lypyl (lypyldf@gmail.com), Gavin Clayton (interkarma@dfworkshop.net)
// Contributors:    Allofich, Hazelnut

using DaggerfallConnect;
using DaggerfallConnect.Arena2;
using DaggerfallConnect.Save;

using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.Formulas;
using DaggerfallWorkshop.Game.Guilds;
using DaggerfallWorkshop.Game.Items;
using DaggerfallWorkshop.Game.MagicAndEffects;
using DaggerfallWorkshop.Game.UserInterface;
using DaggerfallWorkshop.Game.UserInterfaceWindows;
using DaggerfallWorkshop.Game.Utility.ModSupport;   //required for modding features
using DaggerfallWorkshop.Game.Utility.ModSupport.ModSettings;
using DaggerfallWorkshop.Utility;
using DaggerfallWorkshop.Utility.AssetInjection;

using UnityEngine;

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace MTMMM
{
    public class MTSpellBookWindow : DaggerfallSpellBookWindow
    {
        public static bool confidentialityLevelsApplied;
        public static bool spellInstructorThemesApplied;

        uint factionID;
        int playerRankInGuild;

        public static float baseGuildFeeMultiplier = 4.0f;
        public static int maxLengthOfDisplayedSpellNameString=22;
        public static bool displayEffectiveMagicSkill = false;

        public static bool spellInTheProcess = false;
        public static EffectBundleSettings chosenSpell;

        public static bool logSkillCalculationsDuringWindowInit = false;
        static bool calculationDebugToPlayerBefore; // TODO: evaluate if this should be static
        static string messagePrefix = "MTSpellBookWindow: ";
        public static bool spellLearning;


        public static string[] NeedToFindOtherTeacherMessage = new string[] {             // the guild hall in not qualified to teach the relevant effect
            "Unfortunately, I cannot help you with this task.",
            "You should seek the help of someone who has studied ",
            "this field of magic extensively.",
            "Such a person would be able to help you."};
        // No parameter needed

        public static string[] WouldTakeTooMuchTimeMessage = new string[] {             // would take more than 10 hours to teach spell
            "It would take me too long to teach this spell to you.",
            "It could help if you developed your intelligence, gained some ",
            "experience in the school of magic or with similar spells.",
            "Or, you could try some other way to master the spell."    };
                // No parameter needed

        public static string[] NotEnoughGoldForBareEssentialsMessage = new string[] {       // the player cannot affor the bare essentials, so is dismissed
            "I am afraid you don't have enough gold to cover the guild fee of {0} ",
            "and the instruction fee of {1}."    };     // guild fee, instruction fee}


        public static string[] OnlyBareEssentialsMessage = new string[] {       // when the player can only afford the bare essentials, so no further choice needs to be given
                                                                                        // or, when already made the choice to decline refreshments
            "Teaching this spell to you will take some time ({1} minutes, ",
            "{3} fatigue points) and {4} magicka points.",
            "Our institution charges {0} gold pieces for the spell.",
            "You will also pay a personal fee of {2} gold pieces for my time.",
            "",
            "Shall we begin now?"
            };           // what needs to be passed: (0) guild fee, (1) time cost, (2) instruction cost, (3) fatigue cost, (4) spell point cost

        public static string[] TooTiredAndNotEnoughGoldForPotionsMessage = new string[] { // the player can afford the bare essentials, but is too tired and can't afford the refreshment potions
            "The guild fee will be {0} gold pieces and you will also",
            "pay a personal fee of {2} gold pieces for my time.",
            "",
            "Teaching this spell to you would take some time ({1} minutes, ",
            "{3} fatigue points) and {4} magicka points.",
            "",
            "You are too tired at the moment. I would offer refreshments ",
            "but you cannot afford them, so I am afraid you will need to ",
            "come back later."
            };      // what needs to be passed: (0) guild fee, (1) time cost, (2) instruction cost, (3) fatigue cost, (4) spell point cost

        public static string[] HasToTakeRefreshmentsMessage = new string[] {       // when player is tired, needs refreshments, CAN affort these, but no choice needs to be given
            "Teaching this spell to you will take some time ({1} minutes, ",
            "{3} fatigue points) and {4} magicka points.",
            "",
            "Our institution charges {0} gold pieces for the spell.",
            "You will also pay a personal fee of {2} gold pieces for my time.",
            "",
            "I can see that you are quite tired. I will also offer a restoration ",
            "of your expended fatigue for {5} gold pieces and magicka for",
            "{6} gold pieces.",
            "",
            "Altogether that will be {7} gold pieces and {1} minutes of time.",
            "",
            "Shall we begin now?"
            };          // what needs to be passed: (0) guild fee, (1) time cost, (2) instruction cost, (3) fatigue cost, (4) spell point cost, (5) actual fatigue potion cost,
                        // (6) actual spellpoint potion cost, (7) total gold cost including refreshments

        public static string[] HasChoiceAboutBuyingRefreshmentsMessage = new string[] { // the player has enough fatigue+magicka, but also enough gold for refreshments
            "The guild fee will be {0} gold pieces and you will also",
            "pay a personal fee of {2} gold pieces for my time.",
            "",
            "Teaching this spell to you would take some time ({1} minutes, ",
            "{3} fatigue points) and {4} magicka points.",
            "",
            "In addition, I can offer you to restore your expended fatigue ",
            "for {5} gold pieces and magicka for {6} gold pieces.",
            "",
            "Would you like to have this fatigue and magicka restored?"
            };      // what needs to be passed: (0) guild fee, (1) time cost, (2) instruction cost, (3) fatigue cost, (4) spell point cost
                    // (5) actual fatigue potion cost, (6) actual spellpoint potion cost

        public static string[] HasAcceptedTakingRefreshmentsMessage = new string[]    // the player has made the choice to take the refreshments
        {
            "Including the refreshments, as discussed, teaching this ",
            "spell to you will take {1} minutes and involve a total",
            "expense of {0} gold pieces.",
            "",
            "Shall we begin now?"
        };          // (0) total gold cost including refreshments, (1) time cost


        public string[] onSpellSale1 = { "An interesting choice. Of course, there will be some costs involved.",
            "",
            "First, the guild fee will be {0} gold pieces.",    // index=2
            "It will also take {0} minutes and {1} of your spell points for me to teach you the new spell.",    // index=3
            "If we are finished withing an hour, I can do this as a favor to you, but otherwise this fee needs to be payed as well...",
            "... that will be {0} gold pieces.", // index=5            
            " ",
            "To sum it up, the new spell will cost you {0} gold pieces, {1} minutes, {2} spell points and {3} fatigue points.", // index=7
            " ",
            "Shall we begin now?"};
        int actualSpellPointCost;

        // here come the parameters of the spell that the player has chosen - for the purposes of negotiating the options and the endprice
        int baseGuildFee;       // spell-point casting cost *4
        int actualGuildFee;     // apply FormulaHelper method (Mercantile) to baseguildfee
        int actualSpellLearningTimeCost;
        int baseInstructionFee;     // a function of instruction time and guild rank
        int actualInstructionFee;     // apply FormulaHelper method (Mercantile) to baseInstructionfee
        int actualSpellPointCostOfLearning;
        int baseSpellPointPotionCost;   // a function of spell point cost of learning and guild rank
        int actualSpellPointPotionCost;     // apply FormulaHelper method (Mercantile) to baseSpellPointPotionCost
        int actualFatigueCostOfLearning;
        int actualFatigueCostOfLearningToPlayer;
        int baseFatiguePotionCost; // a function of fatigue cost of learning and guild rank
        int actualFatiguePotionCost;        // apply FormulaHelper method (Mercantile) to baseFatiguePotionCost

        bool buyingPotionsToo;

        public MTSpellBookWindow(IUserInterfaceManager uiManager, DaggerfallBaseWindow previous = null, bool buyMode = false) : base(uiManager, previous, buyMode)
        {
            // MMMFormulaHelper.ReturnSpellAttributeIncidencesString(); // shutting off FOR OTHER TEST
            //MMMAutomatons.pleaseIgnoreInput = true;
            calculationDebugToPlayerBefore = MMMXPTallies.CalculationDebugToPlayer;
            MMMXPTallies.CalculationDebugToPlayer = false;
            OnClose += SendClosingMessage;
            if (buyMode)
            {
                factionID = GameManager.Instance.PlayerEnterExit.FactionID;
                SilentMessage("MTSpellBookWindow being created in buy mode, in object with a factionID of "+ factionID);
                IGuild ownerGuild = GameManager.Instance.GuildManager.GetGuild((int)factionID);
                playerRankInGuild = ownerGuild.Rank;
                SilentMessage("Guild object successfully obtained for " + factionID+", player rank in guild: "+playerRankInGuild);
            }
        }

        protected override void SetupMain()
        {
            base.SetupMain();
            spellsListBox.MaxCharacters = maxLengthOfDisplayedSpellNameString;          
        }

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

        protected override void PopulateSpellsList(List<EffectBundleSettings> spells, int? availableSpellPoints = null)
        {
            foreach (EffectBundleSettings spell in spells)
            {
                var effectBroker = GameManager.Instance.EntityEffectBroker;

                // Get spell costs
                // Costs can change based on player skills and stats so must be calculated each time
                FormulaHelper.SpellCost tmpSpellCost = MMMFormulaHelper.CalculateTotalEffectCosts_XP(spell.Effects, spell.TargetType, spell.ElementType, null, spell.MinimumCastingCost);              

                // Lycanthropy is a free spell, even though it shows a cost in classic
                // Setting cost to 0 so it displays correctly in spellbook
                if (spell.Tag == PlayerEntity.lycanthropySpellTag)
                    tmpSpellCost.spellPointCost = 0;   

                // Display spell name and cost - confidentiality hidden because its role will be in the exclusion from the list of spells that are too confidential for the PC 
                ListBox.ListItem listItem;

                string spellListString;

                if (!IsAvailable(spell.Name))
                {
                    spellListString = "??-" + spell.Name.Substring(2);
                }
                else
                {
                    if (displayEffectiveMagicSkill && spell.Effects.Length == 1)
                    {
                        DFCareer.MagicSkills skillID = GameManager.Instance.EntityEffectBroker.GetEffectTemplate(spell.Effects[0].Key).Properties.MagicSkill;
                        int skill = GameManager.Instance.PlayerEntity.Skills.GetLiveSkillValue((DFCareer.Skills)skillID);
                        if (MMMFormulaHelper.ApplyExperienceTallies)
                        {
                            float coeff = MMMFormulaHelper.CalculateCombinedXPTalliesCoefficient(spell.Effects[0].Key, spell.TargetType, spell.ElementType);
                            skill = (int)Math.Floor((float)skill * coeff);
                        }
                        spellListString = string.Format("{0}-{1} ({2}:{3})", tmpSpellCost.spellPointCost, spell.Name, MMMFormulaHelper.GetMagicSkillAbbrev(skillID), skill);
                    }
                    else
                    {
                        spellListString = string.Format("{0}-{1}", tmpSpellCost.spellPointCost, spell.Name);
                    }
                }
                spellsListBox.AddItem(spellListString, out listItem);                

                if ((availableSpellPoints != null && availableSpellPoints < tmpSpellCost.spellPointCost) || (spellListString.StartsWith("??-")))
                    // TODO: think of other aspects based on which some spells could be shown differently
                {
                    // Desaturate unavailable spells
                    float desaturation = 0.75f;
                    listItem.textColor = Color.Lerp(listItem.textColor, Color.grey, desaturation);
                    listItem.selectedTextColor = Color.Lerp(listItem.selectedTextColor, Color.grey, desaturation);
                    listItem.highlightedTextColor = Color.Lerp(listItem.highlightedTextColor, Color.grey, desaturation);
                    listItem.highlightedSelectedTextColor = Color.Lerp(listItem.highlightedSelectedTextColor, Color.grey, desaturation);
                }
            }
        }

        private string ToUnavailable(string spellName)
        {
            return "  " + spellName;
        }

        private bool IsAvailable(string spellName)
        {
            return !spellName.StartsWith("  ");
        }           // TODO: consider merging these with SpellMakerWindow and moving it to a common helper

        protected override void LoadSpellsForSale()
        {
            // Load spells for sale
            offeredSpells.Clear();

            var effectBroker = GameManager.Instance.EntityEffectBroker;

            IEnumerable<SpellRecord.SpellRecordData> standardSpells = effectBroker.StandardSpells;
            if (standardSpells == null || standardSpells.Count() == 0)
            {
                Debug.LogError("Failed to load SPELLS.STD for spellbook in buy mode.");
                return;
            }

            if (!confidentialityLevelsApplied)
                SilentMessage("As per user preference (mod settings), confidentiality levels not applied.");

            // Add standard spell bundles to offer
            foreach (SpellRecord.SpellRecordData standardSpell in standardSpells)
            {
                // Filter internal spells starting with exclamation point '!'
                if (standardSpell.spellName.StartsWith("!"))
                    continue;

                // NOTE: Classic allows purchase of duplicate spells
                // If ever changing this, must ensure spell is an *exact* duplicate (i.e. not a custom spell with same name)
                // Just allowing duplicates for now as per classic and let user manage preference

                // Get effect bundle settings from classic spell
                EffectBundleSettings bundle;
                if (!effectBroker.ClassicSpellRecordDataToEffectBundleSettings(standardSpell, BundleTypes.Spell, out bundle))
                    continue;

                if (confidentialityLevelsApplied)
                {
                    int confLevel = MMMEffectAndSpellHandler.GetSpellConfidentialityLevel(bundle, factionID);
                    SilentMessage("Standard spell '{0}' has confidentiality level of {1}. Player rank in guild is {2}", bundle.Name, confLevel, playerRankInGuild);

                    // Store offered spell and add to list box
                    if (confLevel > playerRankInGuild)
                    {
                        SilentMessage("Spell NOT on offer.");
                    }
                    else
                    {
                        if (!MMMEffectAndSpellHandler.CanTheyTeachThisSpellHere(bundle))  // should return true in every case if Themes not applied so no further checks needed for this condition 
                        {
                            bundle.Name = ToUnavailable(bundle.Name);
                            SilentMessage("Changing spell name to signal its unavailability.");
                        }

                        offeredSpells.Add(bundle);
                        SilentMessage("Spell on offer.");
                    }
                }
                else
                {
                    offeredSpells.Add(bundle);
                }
            }

            EffectBundleSettings[] customBundles = effectBroker.GetCustomSpellBundles(EntityEffectBroker.CustomSpellBundleOfferUsage.SpellsForSale);

            if (!confidentialityLevelsApplied)
                offeredSpells.AddRange(customBundles);
            else
            {
                List<EffectBundleSettings> customBundlesToAdd = new List<EffectBundleSettings>();

                for (int i = 0; i < customBundles.Length; i++)
                {
                    int confLevel = MMMEffectAndSpellHandler.GetSpellConfidentialityLevel(customBundles[i], factionID);
                    SilentMessage("Spell '{0}' has confidentiality level of {1}. Player rank in guild is {2}", customBundles[i].Name, confLevel, playerRankInGuild);

                    // Store offered spell and add to list box
                    if (confLevel > playerRankInGuild)
                    {
                        SilentMessage("Spell NOT on offer.");
                    }
                    else
                    {
                        if (!MMMEffectAndSpellHandler.CanTheyTeachThisSpellHere(customBundles[i]))
                        {                       // should return true in every case if Themes not applied so no further checks needed for this condition
                            customBundles[i].Name = ToUnavailable(customBundles[i].Name);
                            SilentMessage("Changing spell name to signal its unavailability.");
                        }

                        customBundlesToAdd.Add(customBundles[i]);
                        SilentMessage("Spell on offer."); 
                    }
                }

                // Add custom spells for sale bundles to list of offered spells
                offeredSpells.AddRange(customBundlesToAdd);
            }
            // Sort spells for easier finding
            offeredSpells = offeredSpells.OrderBy(x => x.Name).ToList();
        }

        protected virtual void CalculateSpellCosts()
        {
            EffectBundleSettings spell = offeredSpells[spellsListBox.SelectedIndex];
            
            FormulaHelper.SpellCost spellCost = MMMFormulaHelper.CalculateTotalEffectCosts_XP(spell.Effects, spell.TargetType, spell.ElementType, null, spell.MinimumCastingCost);
            actualSpellPointCost = spellCost.spellPointCost;
            baseGuildFee = (int)Math.Round(((float)actualSpellPointCost) * baseGuildFeeMultiplier);       // spell-point casting cost *4
            actualGuildFee = FormulaHelper.CalculateTradePrice(baseGuildFee, buildingDiscoveryData.quality, false);

            actualSpellLearningTimeCost = MMMFormulaHelper.CalculateSpellCreationTimeCost(spell);

            baseInstructionFee = MMMFormulaHelper.CalculateLearningTimeCostFromMinutes(playerRankInGuild, actualSpellLearningTimeCost);

            /*if (playerRankInGuild < 0)
                baseInstructionFee = 1000 * ((actualSpellLearningTimeCost + 59) / 60);
            else
                baseInstructionFee = 500 * (actualSpellLearningTimeCost / 60) * (11 - playerRankInGuild) / 10;           // a function of instruction time and guild rank  */

            actualInstructionFee = FormulaHelper.CalculateTradePrice(baseInstructionFee, buildingDiscoveryData.quality, false);

            actualSpellPointCostOfLearning = actualSpellPointCost * 3;
            baseSpellPointPotionCost = actualSpellPointCostOfLearning * (11 - playerRankInGuild);   // a function of spell point cost of learning and guild rank
            actualSpellPointPotionCost = FormulaHelper.CalculateTradePrice(baseSpellPointPotionCost, buildingDiscoveryData.quality, false);
            actualFatigueCostOfLearning = PlayerEntity.DefaultFatigueLoss * actualSpellLearningTimeCost;
            actualFatigueCostOfLearningToPlayer = (actualFatigueCostOfLearning + 30) / 60;        // rounded number
            baseFatiguePotionCost = actualFatigueCostOfLearningToPlayer * (11 - playerRankInGuild) / 2;       // a function of fatigue cost of learning and guild rank
            actualFatiguePotionCost = FormulaHelper.CalculateTradePrice(baseFatiguePotionCost, buildingDiscoveryData.quality, false);

                    // Costs are halved on Witches Festival holiday
            uint gameMinutes = DaggerfallUnity.Instance.WorldTime.DaggerfallDateTime.ToClassicDaggerfallTime();
            int holidayID = FormulaHelper.GetHolidayId(gameMinutes, 0);
            if (holidayID == (int)DaggerfallConnect.DFLocation.Holidays.Witches_Festival)
            {
                actualGuildFee = Math.Max(actualGuildFee/2, 1);
                actualInstructionFee = Math.Max(actualInstructionFee / 2, 1);
                actualSpellPointPotionCost = Math.Max(actualSpellPointPotionCost / 2, 1);
                actualFatiguePotionCost = Math.Max(actualFatiguePotionCost / 2, 1);                
            }
        }

        protected override void UpdateSelection()
        {
            //  MMMFormulaHelper.MMMFormulaHelperSilentInfoMessage("UpdateSelection called.");
            // SilentMessage(MMMFormulaHelper.ReturnSpellAttributeIncidencesString()); // turning off for ANOTHER TEST
            if (spellLearning)
            {                
                // Update spell list scroller
                spellsListScrollBar.Reset(spellsListBox.RowsDisplayed, spellsListBox.Count, spellsListBox.ScrollIndex);
                spellsListScrollBar.TotalUnits = spellsListBox.Count;
                spellsListScrollBar.ScrollIndex = spellsListBox.ScrollIndex;

                // Get spell settings selected from player spellbook or offered spells
                EffectBundleSettings spellSettings;
                if (buyMode)
                {
                    spellSettings = offeredSpells[spellsListBox.SelectedIndex];
                    
                    // In classic, the price shown in buy mode is the player casting cost * 4                    
                    //(int _, int spellPointCost) = MMMFormulaHelper.CalculateTotalEffectCosts_XP(spellSettings.Effects, spellSettings.TargetType, spellSettings.ElementType);
                    //presentedCost = spellPointCost * 4;
                    /*actualSpellPointCost = spellPointCost;
                    actualSpellPointCostOfLearning = actualSpellPointCost * 3;      // TODO: consider moving this constant to mod options
                    actualSpellLearningTimeCost = MMMFormulaHelper.CalculateSpellCreationTimeCost(spellSettings);*/
                    CalculateSpellCosts();

                    if ((actualSpellLearningTimeCost > 600) || (spellsListBox.SelectedItem.StartsWith("??-")))
                        spellCostLabel.Text = "N/A";
                    else
                    {
                        if ((GameManager.Instance.PlayerEntity.CurrentFatigue < PlayerEntity.DefaultFatigueLoss * (actualSpellLearningTimeCost + 60)) || // should provide for fatigue to end training with 1 hours reserve left
                        (GameManager.Instance.PlayerEntity.CurrentMagicka < actualSpellPointCostOfLearning))
                            presentedCost = actualGuildFee + actualInstructionFee + actualFatiguePotionCost + actualSpellPointPotionCost;
                        else
                            presentedCost = actualGuildFee + actualInstructionFee;

                        spellCostLabel.Text = presentedCost.ToString();
                    }
                }
                else
                {
                    // Get spell and exit if spell index not found
                    if (!GameManager.Instance.PlayerEntity.GetSpell(spellsListBox.SelectedIndex, out spellSettings))
                    {
                        spellNameLabel.Text = string.Empty;
                        ClearEffectLabels();
                        ShowIcons(false);
                        return;
                    }
                }

                // Update spell name label
                spellNameLabel.Text = spellSettings.Name;

                // Update effect labels
                if (spellSettings.Effects != null && spellSettings.Effects.Length > 0)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        if (i < spellSettings.Effects.Length)
                            SetEffectLabels(spellSettings.Effects[i].Key, i);
                        else
                            SetEffectLabels(string.Empty, i);
                    }
                }
                else
                {
                    SetEffectLabels(string.Empty, 0);
                    SetEffectLabels(string.Empty, 1);
                    SetEffectLabels(string.Empty, 2);
                }

                // Update spell icons
                spellIconPanel.BackgroundTexture = GetSpellIcon(spellSettings.Icon);
                spellTargetIconPanel.BackgroundTexture = GetSpellTargetIcon(spellSettings.TargetType);
                spellTargetIconPanel.ToolTipText = GetTargetTypeDescription(spellSettings.TargetType);
                spellElementIconPanel.BackgroundTexture = GetSpellElementIcon(spellSettings.ElementType);
                spellElementIconPanel.ToolTipText = GetElementDescription(spellSettings.ElementType);
                ShowIcons(true);
            }
            else
                base.UpdateSelection();
        }


        
        protected override void BuyButton_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            EffectBundleSettings spell = offeredSpells[spellsListBox.SelectedIndex];

            if (!IsAvailable(spell.Name))
            {
                DaggerfallMessageBox messageBox = new DaggerfallMessageBox(uiManager, this);
                messageBox.SetText(NeedToFindOtherTeacherMessage, this);      
                messageBox.ClickAnywhereToClose = true;
                messageBox.Show();
                return;
            }               // if unavailable, send appropriate messagebox 

            if (spellLearning)
            {
                DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
                const int tradeMessageBaseId = 260;
                const int notEnoughGoldId = 454;                
                int msgOffset = 0;
                DaggerfallMessageBox messageBox;

                CalculateSpellCosts();                
                
                SilentMessage("Attempting to learn spell '{12}'. First stage. Printing calculation results. Guild Fee {0}->{1}. Learning time: {2} minutes, instruction fee: {3}->{4}. " +
                    "SpellPointCost: {5}, Fatigue Cost: {6} (cca. {7}), SpellPoint Potion Cost: {8}->{9}, Fatigue Potion Cost: {10}->{11}.", baseGuildFee,
                    actualGuildFee, actualSpellLearningTimeCost, baseInstructionFee, actualInstructionFee, actualSpellPointCostOfLearning, actualFatigueCostOfLearning,
                    actualFatigueCostOfLearningToPlayer, baseSpellPointPotionCost, actualSpellPointPotionCost, baseFatiguePotionCost, actualFatiguePotionCost, spell.Name);
                        

                if (!GameManager.Instance.PlayerEntity.Items.Contains(ItemGroups.MiscItems, (int)MiscItems.Spellbook))
                {
                    DaggerfallUI.MessageBox(noSpellBook);
                    return;
                }

                messageBox = new DaggerfallMessageBox(uiManager, this);

                if (actualSpellLearningTimeCost > 600)
                {
                    //DaggerfallUI.MessageBox("You seem unable to master such a spell. (More intelligence, experience in the school or with similar spells could help.");
                    messageBox.SetText(WouldTakeTooMuchTimeMessage, this);
                    messageBox.ClickAnywhereToClose = true;
                    messageBox.Show();
                    return;
                }

                if (GameManager.Instance.PlayerEntity.GetGoldAmount() < actualGuildFee+actualInstructionFee)        // bare essentials
                {
                    //DaggerfallUI.MessageBox(notEnoughGoldId);
                    messageBox.SetText(MMMMessages.GetMessage(NotEnoughGoldForBareEssentialsMessage, actualGuildFee, actualInstructionFee), this);
                    messageBox.ClickAnywhereToClose = true;
                    messageBox.Show();
                    return;
                }               

                if ((GameManager.Instance.PlayerEntity.CurrentFatigue < PlayerEntity.DefaultFatigueLoss * (actualSpellLearningTimeCost + 60)) || // should provide for fatigue to end training with 1 hours reserve left
                    (GameManager.Instance.PlayerEntity.CurrentMagicka < actualSpellPointCostOfLearning))
                {       // if the player is low on fatigue or magicka (if player has enough gold, then offered refreshments, otherwise dismissed)
                    if (GameManager.Instance.PlayerEntity.GetGoldAmount() < actualGuildFee + actualInstructionFee + actualSpellPointPotionCost + actualFatiguePotionCost)
                    {           // dismissed to return when rested
                        messageBox.SetText(MMMMessages.GetMessage(TooTiredAndNotEnoughGoldForPotionsMessage, actualGuildFee, actualSpellLearningTimeCost,
                            actualInstructionFee, actualFatigueCostOfLearningToPlayer, actualSpellPointCostOfLearning), this);
                                // what needs to be passed: (0) guild fee, (1) time cost, (2) instruction cost, (3) fatigue cost, (4) spell point cost
                        messageBox.ClickAnywhereToClose = true;
                        messageBox.Show();
                        return;
                    }
                    else
                    {           // offered training + refreshments
                        SilentMessage("Player is offered training + refreshments.");
                        messageBox.SetText(MMMMessages.GetMessage(HasToTakeRefreshmentsMessage, actualGuildFee, actualSpellLearningTimeCost, actualInstructionFee,
                            actualFatigueCostOfLearningToPlayer, actualSpellPointCostOfLearning, actualFatiguePotionCost, actualSpellPointPotionCost,
                            actualGuildFee + actualInstructionFee + actualFatiguePotionCost + actualSpellPointPotionCost), this);
                            // what needs to be passed: (0) guild fee, (1) time cost, (2) instruction cost, (3) fatigue cost, (4) spell point cost, (5) actual fatigue potion cost,
                            // (6) actual spellpoint potion cost, (7) total gold cost including refreshments

                        buyingPotionsToo = true;
                        messageBox.AddButton(DaggerfallMessageBox.MessageBoxButtons.Yes);
                        messageBox.AddButton(DaggerfallMessageBox.MessageBoxButtons.No);
                        messageBox.OnButtonClick += ConfirmTrade_OnButtonClick;
                        messageBox.Show();
                        //uiManager.PushWindow(messageBox);
                        return;
                    }
                    //DaggerfallUI.MessageBox("You seem too tired to learn this spell right now. Return when you are well rested.");
                    
                }

                if (GameManager.Instance.PlayerEntity.GetGoldAmount() < actualGuildFee + actualInstructionFee + actualSpellPointPotionCost + actualFatiguePotionCost)
                {           // if gold not sufficient to cover refreshments too, then offer just bare essentials
                    SilentMessage("Player is offered training without refreshments.");
                    messageBox.SetText(MMMMessages.GetMessage(OnlyBareEssentialsMessage, actualGuildFee, actualSpellLearningTimeCost,
                        actualInstructionFee, actualFatigueCostOfLearningToPlayer, actualSpellPointCostOfLearning), this);
                            // what needs to be passed: (0) guild fee, (1) time cost, (2) instruction cost, (3) fatigue cost, (4) spell point cost
                    buyingPotionsToo = false;
                    messageBox.AddButton(DaggerfallMessageBox.MessageBoxButtons.Yes);
                    messageBox.AddButton(DaggerfallMessageBox.MessageBoxButtons.No);
                    messageBox.OnButtonClick += ConfirmTrade_OnButtonClick;
                    messageBox.Show();
                    //uiManager.PushWindow(messageBox);
                    return;
                }
                else
                {           // if everything goes for the player, he is prompted if he'd prefer with refreshments or without
                    SilentMessage("Player is prompted if he'd prefer with refreshments or without.");
                    messageBox.SetText(MMMMessages.GetMessage(HasChoiceAboutBuyingRefreshmentsMessage, actualGuildFee, actualSpellLearningTimeCost,
                        actualInstructionFee, actualFatigueCostOfLearningToPlayer, actualSpellPointCostOfLearning, actualFatiguePotionCost, actualSpellPointPotionCost), this);
                                // what needs to be passed: (0) guild fee, (1) time cost, (2) instruction cost, (3) fatigue cost, (4) spell point cost
                                // (5) actual fatigue potion cost, (6) actual spellpoint potion cost
                    buyingPotionsToo = false;
                    messageBox.AddButton(DaggerfallMessageBox.MessageBoxButtons.Yes);
                    messageBox.AddButton(DaggerfallMessageBox.MessageBoxButtons.No);
                    messageBox.OnButtonClick += ConfirmTrade_PlayerPromptedWhetherHeWantsRefreshments;
                    messageBox.Show();
                    //uiManager.PushWindow(messageBox);
                    return;
                }

                /* string[] actualSpellTeachBoxText = { onSpellSale1[0],
                    onSpellSale1[1],
                string.Format(onSpellSale1[2], classicTradePrice),    // guild fee gold cost
                string.Format(onSpellSale1[3], actualSpellLearningTimeCost, actualSpellPointCostOfLearning),    // time cost, spell point cost
                onSpellSale1[4],
                string.Format(onSpellSale1[5], tradePrice-classicTradePrice),      // training gold cost // TODO: consider moving 500 to mod options
                onSpellSale1[6],
                string.Format(onSpellSale1[7], tradePrice, actualSpellLearningTimeCost, actualSpellPointCostOfLearning, PlayerEntity.DefaultFatigueLoss * actualSpellLearningTimeCost / 60),
                onSpellSale1[8],
                onSpellSale1[9]
                };  */


                /*
                messageBox.SetText(actualSpellTeachBoxText, this);
                messageBox.AddButton(DaggerfallMessageBox.MessageBoxButtons.Yes);
                messageBox.AddButton(DaggerfallMessageBox.MessageBoxButtons.No);
                messageBox.OnButtonClick += ConfirmTrade_OnButtonClick;
                uiManager.PushWindow(messageBox);*/

            }
            else
            {
                base.BuyButton_OnMouseClick(sender, position);
            }
        }

        protected virtual void ConfirmTrade_PlayerPromptedWhetherHeWantsRefreshments(DaggerfallMessageBox sender, DaggerfallMessageBox.MessageBoxButtons messageBoxButton)
        {
            uiManager.PopWindow();
            DaggerfallMessageBox messageBox = new DaggerfallMessageBox(uiManager, this);

            if (messageBoxButton == DaggerfallMessageBox.MessageBoxButtons.Yes)
            {
                messageBox.SetText(MMMMessages.GetMessage(HasAcceptedTakingRefreshmentsMessage, actualGuildFee + actualInstructionFee + actualSpellPointPotionCost +
                    actualFatigueCostOfLearningToPlayer, actualSpellLearningTimeCost), this);
                                // (0) total gold cost including refreshments, (1) time cost
                buyingPotionsToo = true;    // the player has opted to take refreshments
            }
            else
            {
                messageBox.SetText(MMMMessages.GetMessage(OnlyBareEssentialsMessage, actualGuildFee, actualSpellLearningTimeCost, actualInstructionFee,
                    actualFatigueCostOfLearningToPlayer, actualSpellPointCostOfLearning), this);
                            // what needs to be passed: (0) guild fee, (1) time cost, (2) instruction cost, (3) fatigue cost, (4) spell point cost
                buyingPotionsToo = false;   // the player has opted to decline refreshments
            }
            
            messageBox.AddButton(DaggerfallMessageBox.MessageBoxButtons.Yes);
            messageBox.AddButton(DaggerfallMessageBox.MessageBoxButtons.No);
            messageBox.OnButtonClick += ConfirmTrade_OnButtonClick;
            messageBox.Show();
            //uiManager.PushWindow(messageBox);
        }

        protected virtual int GetClassicTradePrice()
        {
            return FormulaHelper.CalculateTradePrice(presentedCost, buildingDiscoveryData.quality, false);
        }

        protected virtual int GetClassicFinalTradePrice()
        {
            return FormulaHelper.CalculateTradePrice(GetClassicTradePrice(), buildingDiscoveryData.quality, false);
        }

        protected override int GetTradePrice()          // TODO: can wait - add calculation for magicka/stamina/health potions sold
                                                        // TODO: can wait - add calculation for food provided if instruction takes longer than 4 hours - check how Ralzar's mod handles things                                                        
        {
            int classicTradePrice = GetClassicTradePrice();

            if (!spellLearning)
                return classicTradePrice;           // if the relevant mod preference is not ticked, return as classic would, else continue and calculate our own price

            int hoursOfInstructionCompleted = (actualSpellLearningTimeCost / 60);

            int ourPresentedCost = classicTradePrice + 500 * hoursOfInstructionCompleted;      // just add a flat 500 gold fee for each hour of instruction completed
                                                // TODO: consider changing it so presented cost included the extra charges

            int tradePriceToReturn = FormulaHelper.CalculateTradePrice(ourPresentedCost, buildingDiscoveryData.quality, false);
            SilentMessage(string.Format("GetTradePrice: Classic Trade Price={0}, Hours Of Instruction Completed={1}, Our Presented Cost={2}, Trade Price To Return={3}.",
                classicTradePrice, hoursOfInstructionCompleted, ourPresentedCost, tradePriceToReturn));

            return tradePriceToReturn;
        }

        protected override void ConfirmTrade_OnButtonClick(DaggerfallMessageBox sender, DaggerfallMessageBox.MessageBoxButtons messageBoxButton)
        {
            if (!spellLearning)
                base.ConfirmTrade_OnButtonClick(sender, messageBoxButton);
            else
            {                
                if (messageBoxButton == DaggerfallMessageBox.MessageBoxButtons.Yes)
                {
                    // Deduct gold - adding gold sound for additional feedback
                    SilentMessage("ConfirmTrade_OnButtonClickPlayer: player fatigue before change: "+ GameManager.Instance.PlayerEntity.CurrentFatigue);

                    int goldCost = actualGuildFee+actualInstructionFee;
                    if (buyingPotionsToo)
                        goldCost += actualFatiguePotionCost + actualSpellPointPotionCost;

                    GameManager.Instance.PlayerEntity.DeductGoldAmount(goldCost);
                    DaggerfallUI.Instance.PlayOneShot(SoundClips.GoldPieces);

                    int numberOfFatiguePointsToSubtract = 0;
                    int numberOfSpellointsToSubtract = 0;

                    if (!buyingPotionsToo)
                    {
                        numberOfFatiguePointsToSubtract = actualFatigueCostOfLearning;
                        numberOfSpellointsToSubtract = actualSpellPointCostOfLearning;

                        GameManager.Instance.PlayerEntity.DecreaseFatigue(numberOfFatiguePointsToSubtract);
                        GameManager.Instance.PlayerEntity.DecreaseMagicka(numberOfSpellointsToSubtract);
                    }

                    MTMostlyMagicMod.ElapseMinutes (actualSpellLearningTimeCost); 

                    SilentMessage(string.Format("ConfirmTrade_OnButtonClick: Spell learning costs incurred. Gold={0}, SpellPoints={1}, FatiguePoints={2}, Time={3} minutes.",
                    goldCost, numberOfSpellointsToSubtract, numberOfFatiguePointsToSubtract, actualSpellLearningTimeCost));
                    SilentMessage("ConfirmTrade_OnButtonClickPlayer: player fatigue after change: " + GameManager.Instance.PlayerEntity.CurrentFatigue);                  

                    // Add to player entity spellbook
                    GameManager.Instance.PlayerEntity.AddSpell(offeredSpells[spellsListBox.SelectedIndex]);

                    /* have spell learning train the relevant magic skills too   */

                    int hoursOfInstructionCompleted = (actualSpellLearningTimeCost / 60);
                    int totalTrainingPoints = hoursOfInstructionCompleted * UnityEngine.Random.Range(10, 20 + 1) / 3 + 3;

                    EffectBundleSettings spell = offeredSpells[spellsListBox.SelectedIndex];
                    IEntityEffect effectTemplate;    

                    for (int i = 0; i < spell.Effects.Length; i++)
                    {
                        effectTemplate = GameManager.Instance.EntityEffectBroker.GetEffectTemplate(spell.Effects[i].Key);
                        DFCareer.Skills skillToTrain = (DFCareer.Skills)effectTemplate.Properties.MagicSkill;

                        int skillAdvancementMultiplier = DaggerfallSkills.GetAdvancementMultiplier(skillToTrain);
                        short tallyAmount = (short)((totalTrainingPoints / spell.Effects.Length) * skillAdvancementMultiplier);
                        GameManager.Instance.PlayerEntity.TallySkill(skillToTrain, tallyAmount);
                        SilentMessage(string.Format("ConfirmTrade_OnButtonClickPlayer. {0}/{1}: totalTrainingPoints={2}, key={3}, skillToTrain={4}, skillAdvancementMultiplier={5}, tallyAmount={6}",
                            i, spell.Effects.Length, totalTrainingPoints, spell.Effects[i].Key, (int)skillToTrain, skillAdvancementMultiplier, tallyAmount));
                    }
                    
                    UpdateGold();
                }
                MMMXPTallies.CalculationDebugToPlayer = calculationDebugToPlayerBefore;
                CloseWindow();      // TODO: revise if this is correct here
            }            
        }

        #region OnClose
        protected virtual void SendClosingMessage()
        {
            MMMXPTallies.CalculationDebugToPlayer = calculationDebugToPlayerBefore;
            spellInTheProcess = false;                  // TODO: revise, this is wrong here
            SilentMessage("OnClose triggered, SendClosingMessage() called.");
        }
        #endregion

        #region Spell Casting

        protected override void SpellsListBox_OnUseSelectedItem()
        {
            // Get spell settings and exit if spell index not found
            EffectBundleSettings spellSettings;
            if (!GameManager.Instance.PlayerEntity.GetSpell(spellsListBox.SelectedIndex, out chosenSpell))
                return;

            // Lycanthropes cast for free
            bool noSpellPointCost = chosenSpell.Tag == PlayerEntity.lycanthropySpellTag;

            // Assign to player effect manager as ready spell
            EntityEffectManager playerEffectManager = GameManager.Instance.PlayerEffectManager;
            if (playerEffectManager)
            {
                spellInTheProcess = true;
                playerEffectManager.SetReadySpell(new EntityEffectBundle(chosenSpell, GameManager.Instance.PlayerEntityBehaviour), noSpellPointCost);                

                DaggerfallUI.Instance.PopToHUD();
            }
            //MMMAutomatons.pleaseIgnoreInput = false;
            //MMMAutomatons.SpellCastAutomaton_SpellBookWindowSuccessfulSpellChoice(spellSettings);
            // SilentMessage("SpellsListBox_OnUseSelectedItem: Automaton informed that spellcasting attempted.");
        }
        #endregion

    }
}