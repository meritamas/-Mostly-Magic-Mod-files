// Project:         MeriTamas's (Mostly) Magic Mod for Daggerfall Unity (http://www.dfworkshop.net)
// Copyright:       Copyright (C) 2023 meritamas
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Author:          meritamas (meritamas@outlook.com)

// This class was based on the DaggerfallSpellMakerWindow class which is part of the Daggerfall Tools For Unity project - many parts retained unchanged
// Project:         Daggerfall Tools For Unity
// Copyright:       Copyright (C) 2009-2021 Daggerfall Workshop
// Web Site:        http://www.dfworkshop.net
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Source Code:     https://github.com/Interkarma/daggerfall-unity
// Original Author: Gavin Clayton (interkarma@dfworkshop.net)
// Contributors:    
//
// Notes:
//

using DaggerfallConnect;
using UnityEngine;
using System;
using System.Collections.Generic;
using DaggerfallConnect.Utility;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.Formulas;
using DaggerfallWorkshop.Game.Guilds;
using DaggerfallWorkshop.Game.Items;
using DaggerfallWorkshop.Game.MagicAndEffects;
using DaggerfallWorkshop.Game.UserInterface;
using DaggerfallWorkshop.Game.UserInterfaceWindows;
using DaggerfallWorkshop.Utility;

namespace MTMMM
{
    /// <summary>
    /// Spellmaker UI.
    /// </summary>
    public class MTSpellMakerWindow : DaggerfallSpellMakerWindow
    {
        static string messagePrefix = "MTSpellMakerWindow: ";
        public static float baseGuildFeeMultiplier = 4.0f;
        public static float spellCreationTimeCoefficient = 4.0f;
        bool calculationDebugToPlayerBefore;

        uint factionID;
        int playerRankInGuild;

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






        public string[] onSpellMake1 = {
            "An interesting choice. Of course, there will be some costs involved.",
            "First, the guild fee will be {0} gold pieces.",    // index=1
            "You will also need to spend {0} minutes and {1} spell points practicing the new spell and the process of creating",    // index=2
            "the new spell will take my time and effort too.",
            "If we are finished withing an hour, I can let that go, but otherwise you need to pay my fee as well ({0} gold pcs).", // index=4            
            " ",
            "To sum it up, the new spell will cost you {0} gold pieces, {1} minutes, {2} spell points and {3} fatigue points.", // index=6
            " ",
            "Shall we begin now?" };

        public List<string> knowableEffectGroups;
        public Dictionary<string, string[]> knowableEffects;

        #region UI Rects

        protected DFSize selectedIconsBaseSize = new DFSize(40, 80);

        protected Vector2 tipLabelPos = new Vector2(5, 22);
        protected Vector2 nameLabelPos = new Vector2(60, 185);
        protected Rect effect1NameRect = new Rect(3, 30, 230, 9);
        protected Rect effect2NameRect = new Rect(3, 62, 230, 9);
        protected Rect effect3NameRect = new Rect(3, 94, 230, 9);
        protected Rect addEffectButtonRect = new Rect(244, 114, 28, 28);
        protected Rect buyButtonRect = new Rect(244, 147, 24, 16);
        protected Rect newButtonRect = new Rect(244, 163, 24, 16);
        protected Rect exitButtonRect = new Rect(244, 179, 24, 16);
        protected Rect casterOnlyButtonRect = new Rect(275, 114, 24, 16);
        protected Rect byTouchButtonRect = new Rect(275, 130, 24, 16);
        protected Rect singleTargetAtRangeButtonRect = new Rect(275, 146, 24, 16);
        protected Rect areaAroundCasterButtonRect = new Rect(275, 162, 24, 16);
        protected Rect areaAtRangeButtonRect = new Rect(275, 178, 24, 16);
        protected Rect fireBasedButtonRect = new Rect(299, 114, 16, 16);
        protected Rect coldBasedButtonRect = new Rect(299, 130, 16, 16);
        protected Rect poisonBasedButtonRect = new Rect(299, 146, 16, 16);
        protected Rect shockBasedButtonRect = new Rect(299, 162, 16, 16);
        protected Rect magicBasedButtonRect = new Rect(299, 178, 16, 16);
        protected Rect nextIconButtonRect = new Rect(275, 80, 9, 16);
        protected Rect previousIconButtonRect = new Rect(275, 96, 9, 16);
        protected Rect selectIconButtonRect = new Rect(288, 94, 16, 16);
        protected Rect nameSpellButtonRect = new Rect(59, 184, 142, 7);

        protected Rect casterOnlySubRect = new Rect(0, 0, 24, 16);
        protected Rect byTouchSubRect = new Rect(0, 16, 24, 16);
        protected Rect singleTargetAtRangeSubRect = new Rect(0, 32, 24, 16);
        protected Rect areaAroundCasterSubRect = new Rect(0, 48, 24, 16);
        protected Rect areaAtRangeSubRect = new Rect(0, 64, 24, 16);

        protected Rect fireBasedSubRect = new Rect(24, 0, 16, 16);
        protected Rect coldBasedSubRect = new Rect(24, 16, 16, 16);
        protected Rect poisonBasedSubRect = new Rect(24, 32, 16, 16);
        protected Rect shockBasedSubRect = new Rect(24, 48, 16, 16);
        protected Rect magicBasedSubRect = new Rect(24, 64, 16, 16);

        #endregion

        #region UI Controls

        protected TextLabel tipLabel;
        protected TextLabel effect1NameLabel;
        protected TextLabel effect2NameLabel;
        protected TextLabel effect3NameLabel;
        protected TextLabel maxSpellPointsLabel;
        protected TextLabel moneyLabel;
        protected TextLabel goldCostLabel;
        protected TextLabel spellPointCostLabel;
        //protected TextLabel spellNameLabel;

        protected DaggerfallListPickerWindow effectGroupPicker;
        protected DaggerfallListPickerWindow effectSubGroupPicker;
        protected DaggerfallEffectSettingsEditorWindow effectEditor;
        protected SpellIconPickerWindow iconPicker;

        protected Button selectIconButton;

        protected Button casterOnlyButton;
        protected Button byTouchButton;
        protected Button singleTargetAtRangeButton;
        protected Button areaAroundCasterButton;
        protected Button areaAtRangeButton;

        protected Button fireBasedButton;
        protected Button coldBasedButton;
        protected Button poisonBasedButton;
        protected Button shockBasedButton;
        protected Button magicBasedButton;

        #endregion

        #region UI Textures

        protected Texture2D baseTexture;
        protected Texture2D selectedIconsTexture;

        protected Texture2D casterOnlySelectedTexture;
        protected Texture2D byTouchSelectedTexture;
        protected Texture2D singleTargetAtRangeSelectedTexture;
        protected Texture2D areaAroundCasterSelectedTexture;
        protected Texture2D areaAtRangeSelectedTexture;

        protected Texture2D fireBasedSelectedTexture;
        protected Texture2D coldBasedSelectedTexture;
        protected Texture2D poisonBasedSelectedTexture;
        protected Texture2D shockBasedSelectedTexture;
        protected Texture2D magicBasedSelectedTexture;

        #endregion

        #region Fields

        protected const MagicCraftingStations thisMagicStation = MagicCraftingStations.SpellMaker;

        protected const string baseTextureFilename = "INFO01I0.IMG";
        protected const string goldSelectIconsFilename = "MASK01I0.IMG";
        protected const string colorSelectIconsFilename = "MASK04I0.IMG";

        protected const int alternateAlphaIndex = 12;
        protected const int maxEffectsPerSpell = 3;
        protected const int defaultSpellIcon = 1;
        protected const TargetTypes defaultTargetFlags = EntityEffectBroker.TargetFlags_All;
        protected const ElementTypes defaultElementFlags = EntityEffectBroker.ElementFlags_MagicOnly;

        protected const SoundClips inscribeGrimoire = SoundClips.ParchmentScratching;

        protected PlayerGPS.DiscoveredBuilding buildingDiscoveryData;

        List<IEntityEffect> enumeratedEffectTemplates = new List<IEntityEffect>();

        EffectEntry[] effectEntries = new EffectEntry[maxEffectsPerSpell];
        EffectBundleSettings spell = new EffectBundleSettings();

        protected int editOrDeleteSlot = -1;
        protected TargetTypes allowedTargets = defaultTargetFlags;
        protected ElementTypes allowedElements = defaultElementFlags;
        protected TargetTypes selectedTarget = TargetTypes.CasterOnly;
        protected ElementTypes selectedElement = ElementTypes.Magic;
        protected SpellIcon selectedIcon;

        int totalGoldCost = 0;
        int totalSpellPointCost = 0;       

        int actualSpellPointCost;

        // here come the parameters of the spell that the player has chosen - for the purposes of negotiating the options and the endprice
        int baseGuildFee = 0;       // spell-point casting cost *4
        int actualGuildFee = 0;     // apply FormulaHelper method (Mercantile) to baseguildfee
        int actualSpellMakingTimeCost = 0;
        int baseInstructionFee = 0;     // a function of instruction time and guild rank
        int actualInstructionFee = 0;     // apply FormulaHelper method (Mercantile) to baseInstructionfee
        int actualSpellPointCostOfMaking = 0;
        int baseSpellPointPotionCost = 0;   // a function of spell point cost of learning and guild rank
        int actualSpellPointPotionCost = 0;     // apply FormulaHelper method (Mercantile) to baseSpellPointPotionCost
        int actualFatigueCostOfMaking = 0;
        int actualFatigueCostOfMakingToPlayer = 0;
        int baseFatiguePotionCost = 0; // a function of fatigue cost of learning and guild rank
        int actualFatiguePotionCost = 0;        // apply FormulaHelper method (Mercantile) to baseFatiguePotionCost

        bool buyingPotionsToo;


        protected EffectEntry[] EffectEntries { get { return effectEntries; } }
        protected int TotalGoldCost { get { return totalGoldCost; } }
        protected int TotalSpellPointCost { get { return totalSpellPointCost; } }

        
        #endregion

        #region Constructors

        public MTSpellMakerWindow(IUserInterfaceManager uiManager, DaggerfallBaseWindow previous = null)
            : base(uiManager, previous)
        {
            
        }

        public override void OnPop()
        {
            MMMFormulaHelper.activeSpellMakerWindow = null;
            SilentMessage("Set active spellmaker in MMMFormulaHelper to null.");
        }

        #endregion

        #region Debug Message Methods
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

        #region Setup Methods

        protected override void Setup()
        {
            // Load all the textures used by spell maker window
            LoadTextures();

            // Always dim background
            ParentPanel.BackgroundColor = ScreenDimColor;

            // Setup native panel background
            NativePanel.BackgroundColor = new Color(0, 0, 0, 0.75f);
            NativePanel.BackgroundTexture = baseTexture;

            // Setup controls
            SetupLabels();
            SetupButtons();
            SetupPickers();
            SetIcon(selectedIcon);
            SetStatusLabels();

            // Setup effect editor window
            effectEditor = (DaggerfallEffectSettingsEditorWindow)UIWindowFactory.GetInstanceWithArgs(UIWindowType.EffectSettingsEditor, new object[] { uiManager, this });
            effectEditor.OnSettingsChanged += EffectEditor_OnSettingsChanged;
            effectEditor.OnClose += EffectEditor_OnClose;

            // Setup icon picker
            iconPicker = (SpellIconPickerWindow)UIWindowFactory.GetInstance(UIWindowType.SpellIconPicker, uiManager, this);
            iconPicker.OnClose += IconPicker_OnClose;
        }

        public override void OnPush()
        {
            factionID = GameManager.Instance.PlayerEnterExit.FactionID;
            SilentMessage("MTSpellMakerWindow being created, in object with a factionID of " + factionID);
            IGuild ownerGuild = GameManager.Instance.GuildManager.GetGuild((int)factionID);
            playerRankInGuild = ownerGuild.Rank;
            SilentMessage("Guild object successfully obtained for " + factionID + ", player rank in guild: " + playerRankInGuild);

            calculationDebugToPlayerBefore = MMMXPTallies.CalculationDebugToPlayer;
            MMMXPTallies.CalculationDebugToPlayer = false;            

            MMMFormulaHelper.activeSpellMakerWindow = this;
            SilentMessage("Set self up to be the active spellmaker in MMMFormulaHelper.");

            if (GameManager.Instance.PlayerEnterExit.IsPlayerInside)
                buildingDiscoveryData = GameManager.Instance.PlayerEnterExit.BuildingDiscoveryData;
            InitEffectSlots();

            SetDefaults();
        }

        protected virtual void SetDefaults()
        {
            allowedTargets = defaultTargetFlags;
            allowedElements = defaultElementFlags;
            selectedIcon = new SpellIcon()
            {
                index = defaultSpellIcon,
            };
            editOrDeleteSlot = -1;

            for (int i = 0; i < maxEffectsPerSpell; i++)
            {
                effectEntries[i] = new EffectEntry();
            }

            if (IsSetup)
            {
                effect1NameLabel.Text = string.Empty;
                effect2NameLabel.Text = string.Empty;
                effect3NameLabel.Text = string.Empty;
                spellNameLabel.Text = string.Empty;
                UpdateAllowedButtons();
                SetIcon(selectedIcon);
                SetStatusLabels();
            }
        }

        #endregion

        #region Protected Methods

        protected virtual void LoadTextures()
        {
            // Load source textures
            baseTexture = ImageReader.GetTexture(baseTextureFilename, 0, 0, true, alternateAlphaIndex);
            selectedIconsTexture = ImageReader.GetTexture(goldSelectIconsFilename);

            // Load target texture
            casterOnlySelectedTexture = ImageReader.GetSubTexture(selectedIconsTexture, casterOnlySubRect, selectedIconsBaseSize);
            byTouchSelectedTexture = ImageReader.GetSubTexture(selectedIconsTexture, byTouchSubRect, selectedIconsBaseSize);
            singleTargetAtRangeSelectedTexture = ImageReader.GetSubTexture(selectedIconsTexture, singleTargetAtRangeSubRect, selectedIconsBaseSize);
            areaAroundCasterSelectedTexture = ImageReader.GetSubTexture(selectedIconsTexture, areaAroundCasterSubRect, selectedIconsBaseSize);
            areaAtRangeSelectedTexture = ImageReader.GetSubTexture(selectedIconsTexture, areaAtRangeSubRect, selectedIconsBaseSize);

            fireBasedSelectedTexture = ImageReader.GetSubTexture(selectedIconsTexture, fireBasedSubRect, selectedIconsBaseSize);
            coldBasedSelectedTexture = ImageReader.GetSubTexture(selectedIconsTexture, coldBasedSubRect, selectedIconsBaseSize);
            poisonBasedSelectedTexture = ImageReader.GetSubTexture(selectedIconsTexture, poisonBasedSubRect, selectedIconsBaseSize);
            shockBasedSelectedTexture = ImageReader.GetSubTexture(selectedIconsTexture, shockBasedSubRect, selectedIconsBaseSize);
            magicBasedSelectedTexture = ImageReader.GetSubTexture(selectedIconsTexture, magicBasedSubRect, selectedIconsBaseSize);
        }

        protected virtual void SetupLabels()
        {
            // Tip label
            tipLabel = DaggerfallUI.AddTextLabel(DaggerfallUI.DefaultFont, tipLabelPos, string.Empty, NativePanel);

            // Status labels
            maxSpellPointsLabel = DaggerfallUI.AddTextLabel(DaggerfallUI.DefaultFont, new Vector2(43, 149), string.Empty, NativePanel);
            moneyLabel = DaggerfallUI.AddTextLabel(DaggerfallUI.DefaultFont, new Vector2(39, 158), string.Empty, NativePanel);
            goldCostLabel = DaggerfallUI.AddTextLabel(DaggerfallUI.DefaultFont, new Vector2(59, 167), string.Empty, NativePanel);
            spellPointCostLabel = DaggerfallUI.AddTextLabel(DaggerfallUI.DefaultFont, new Vector2(70, 176), string.Empty, NativePanel);

            // Name label
            spellNameLabel = DaggerfallUI.AddTextLabel(DaggerfallUI.DefaultFont, nameLabelPos, string.Empty, NativePanel);
            spellNameLabel.ShadowPosition = Vector2.zero;

            // Effect1
            Panel effect1NamePanel = DaggerfallUI.AddPanel(effect1NameRect, NativePanel);
            effect1NamePanel.HorizontalAlignment = HorizontalAlignment.Center;
            effect1NamePanel.OnMouseClick += Effect1NamePanel_OnMouseClick;
            effect1NameLabel = DaggerfallUI.AddTextLabel(DaggerfallUI.LargeFont, Vector2.zero, string.Empty, effect1NamePanel);
            effect1NameLabel.HorizontalAlignment = HorizontalAlignment.Center;
            effect1NameLabel.ShadowPosition = Vector2.zero;

            // Effect2
            Panel effect2NamePanel = DaggerfallUI.AddPanel(effect2NameRect, NativePanel);
            effect2NamePanel.HorizontalAlignment = HorizontalAlignment.Center;
            effect2NamePanel.OnMouseClick += Effect2NamePanel_OnMouseClick;
            effect2NameLabel = DaggerfallUI.AddTextLabel(DaggerfallUI.LargeFont, Vector2.zero, string.Empty, effect2NamePanel);
            effect2NameLabel.HorizontalAlignment = HorizontalAlignment.Center;
            effect2NameLabel.ShadowPosition = Vector2.zero;

            // Effect3
            Panel effect3NamePanel = DaggerfallUI.AddPanel(effect3NameRect, NativePanel);
            effect3NamePanel.HorizontalAlignment = HorizontalAlignment.Center;
            effect3NamePanel.OnMouseClick += Effect3NamePanel_OnMouseClick;
            effect3NameLabel = DaggerfallUI.AddTextLabel(DaggerfallUI.LargeFont, Vector2.zero, string.Empty, effect3NamePanel);
            effect3NameLabel.HorizontalAlignment = HorizontalAlignment.Center;
            effect3NameLabel.ShadowPosition = Vector2.zero;
        }

        protected virtual void SetupPickers()
        {
            // Use a picker for effect group
            effectGroupPicker = new DaggerfallListPickerWindow(uiManager, this);
            effectGroupPicker.ListBox.OnUseSelectedItem += AddEffectGroupListBox_OnUseSelectedItem;

            // Use another picker for effect subgroup
            // This allows user to hit escape and return to effect group list, unlike classic which dumps whole spellmaker UI
            effectSubGroupPicker = new DaggerfallListPickerWindow(uiManager, this);
            effectSubGroupPicker.ListBox.OnUseSelectedItem += AddEffectSubGroup_OnUseSelectedItem;
        }

        protected virtual void SetupButtons()
        {
            // Control
            AddTipButton(addEffectButtonRect, "addEffect", AddEffectButton_OnMouseClick, DaggerfallShortcut.Buttons.SpellMakerAddEffect);
            AddTipButton(buyButtonRect, "buySpell", BuyButton_OnMouseClick, DaggerfallShortcut.Buttons.SpellMakerBuySpell);
            AddTipButton(newButtonRect, "newSpell", NewSpellButton_OnMouseClick, DaggerfallShortcut.Buttons.SpellMakerNewSpell);
            AddTipButton(exitButtonRect, "exit", ExitButton_OnMouseClick, DaggerfallShortcut.Buttons.SpellMakerExit);
            AddTipButton(nameSpellButtonRect, "nameSpell", NameSpellButton_OnMouseClick, DaggerfallShortcut.Buttons.SpellMakerNameSpell);

            // Target
            casterOnlyButton = AddTipButton(casterOnlyButtonRect, "casterOnly", CasterOnlyButton_OnMouseClick, DaggerfallShortcut.Buttons.SpellMakerTargetCaster);
            byTouchButton = AddTipButton(byTouchButtonRect, "byTouch", ByTouchButton_OnMouseClick, DaggerfallShortcut.Buttons.SpellMakerTargetTouch);
            singleTargetAtRangeButton = AddTipButton(singleTargetAtRangeButtonRect, "singleTargetAtRange", SingleTargetAtRangeButton_OnMouseClick, DaggerfallShortcut.Buttons.SpellMakerTargetSingleAtRange);
            areaAroundCasterButton = AddTipButton(areaAroundCasterButtonRect, "areaAroundCaster", AreaAroundCasterButton_OnMouseClick, DaggerfallShortcut.Buttons.SpellMakerTargetAroundCaster);
            areaAtRangeButton = AddTipButton(areaAtRangeButtonRect, "areaAtRange", AreaAtRangeButton_OnMouseClick, DaggerfallShortcut.Buttons.SpellMakerTargetAreaAtRange);

            // Element
            fireBasedButton = AddTipButton(fireBasedButtonRect, "fireBased", FireBasedButton_OnMouseClick, DaggerfallShortcut.Buttons.SpellMakerElementFire);
            coldBasedButton = AddTipButton(coldBasedButtonRect, "coldBased", ColdBasedButton_OnMouseClick, DaggerfallShortcut.Buttons.SpellMakerElementCold);
            poisonBasedButton = AddTipButton(poisonBasedButtonRect, "poisonBased", PoisonBasedButton_OnMouseClick, DaggerfallShortcut.Buttons.SpellMakerElementPoison);
            shockBasedButton = AddTipButton(shockBasedButtonRect, "shockBased", ShockBasedButton_OnMouseClick, DaggerfallShortcut.Buttons.SpellMakerElementShock);
            magicBasedButton = AddTipButton(magicBasedButtonRect, "magicBased", MagicBasedButton_OnMouseClick, DaggerfallShortcut.Buttons.SpellMakerElementMagic);

            // Icons
            AddTipButton(nextIconButtonRect, "nextIcon", NextIconButton_OnMouseClick, DaggerfallShortcut.Buttons.SpellMakerNextIcon);
            AddTipButton(previousIconButtonRect, "previousIcon", PreviousIconButton_OnMouseClick, DaggerfallShortcut.Buttons.SpellMakerPrevIcon);
            selectIconButton = AddTipButton(selectIconButtonRect, "selectIcon", SelectIconButton_OnMouseClick, DaggerfallShortcut.Buttons.SpellMakerSelectIcon);
            //selectIconButton.OnRightMouseClick += PreviousIconButton_OnMouseClick;

            // Select default buttons
            UpdateAllowedButtons();
        }

        protected virtual void SetIcon(SpellIcon icon)
        {
            // Fallback to classic index if no valid icon pack key
            if (string.IsNullOrEmpty(icon.key) || !DaggerfallUI.Instance.SpellIconCollection.HasPack(icon.key))
            {
                icon.key = string.Empty;
                icon.index = icon.index % DaggerfallUI.Instance.SpellIconCollection.SpellIconCount;
            }

            // Set icon
            selectedIcon = icon;
            selectIconButton.BackgroundTexture = DaggerfallUI.Instance.SpellIconCollection.GetSpellIcon(selectedIcon);
        }

        protected virtual void SetStatusLabels()
        {
            int presentedCost;
            maxSpellPointsLabel.Text = GameManager.Instance.PlayerEntity.MaxMagicka.ToString();
            moneyLabel.Text = GameManager.Instance.PlayerEntity.GoldPieces.ToString();

            if ((GameManager.Instance.PlayerEntity.CurrentFatigue < PlayerEntity.DefaultFatigueLoss * (actualSpellMakingTimeCost + 60)) || // should provide for fatigue to end training with 1 hours reserve left
                        (GameManager.Instance.PlayerEntity.CurrentMagicka < actualSpellPointCostOfMaking))
                presentedCost = actualGuildFee + actualInstructionFee + actualFatiguePotionCost + actualSpellPointPotionCost;
            else
                presentedCost = actualGuildFee + actualInstructionFee;
            goldCostLabel.Text = presentedCost.ToString();

            spellPointCostLabel.Text = actualSpellPointCost.ToString();
        }

        #endregion

        #region Private Methods

        protected virtual Button AddTipButton(Rect rect, string tipID, BaseScreenComponent.OnMouseClickHandler handler, DaggerfallShortcut.Buttons button)
        {
            Button tipButton = DaggerfallUI.AddButton(rect, NativePanel);
            tipButton.OnMouseEnter += TipButton_OnMouseEnter;
            tipButton.OnMouseLeave += TipButton_OnMouseLeave;
            tipButton.OnMouseClick += handler;
            tipButton.Hotkey = DaggerfallShortcut.GetBinding(button);
            tipButton.Tag = tipID;

            return tipButton;
        }

        protected virtual void InitEffectSlots()
        {
            effectEntries = new EffectEntry[maxEffectsPerSpell];
            for (int i = 0; i < maxEffectsPerSpell; i++)
            {
                effectEntries[i] = new EffectEntry();
            }
        }

        protected virtual void ClearPendingDeleteEffectSlot()
        {
            if (editOrDeleteSlot == -1)
                return;

            effectEntries[editOrDeleteSlot] = new EffectEntry();
            UpdateSlotText(editOrDeleteSlot, string.Empty);
            editOrDeleteSlot = -1;
            UpdateAllowedButtons();
        }

        protected int GetFirstFreeEffectSlotIndex()
        {
            for (int i = 0; i < maxEffectsPerSpell; i++)
            {
                if (string.IsNullOrEmpty(effectEntries[i].Key))
                    return i;
            }

            return -1;
        }

        protected int GetFirstUsedEffectSlotIndex()
        {
            for (int i = 0; i < maxEffectsPerSpell; i++)
            {
                if (!string.IsNullOrEmpty(effectEntries[i].Key))
                    return i;
            }

            return -1;
        }

        protected int CountUsedEffectSlots()
        {
            int total = 0;
            for (int i = 0; i < maxEffectsPerSpell; i++)
            {
                if (!string.IsNullOrEmpty(effectEntries[i].Key))
                    total++;
            }

            return total;
        }

        protected virtual void UpdateSlotText(int slot, string text)
        {
            // Get text label to update
            TextLabel label = null;
            switch (slot)
            {
                case 0:
                    label = effect1NameLabel;
                    break;
                case 1:
                    label = effect2NameLabel;
                    break;
                case 2:
                    label = effect3NameLabel;
                    break;
                default:
                    return;
            }

            // Set label text
            label.Text = text;
        }

        protected virtual void AddAndEditSlot(IEntityEffect effectTemplate)
        {
            SilentMessage("AddAndEditSlot called with key={0}", effectTemplate.Key);
            effectEditor.EffectTemplate = effectTemplate;
            int slot = GetFirstFreeEffectSlotIndex();
            effectEntries[slot] = effectEditor.EffectEntry;
            UpdateSlotText(slot, effectEditor.EffectTemplate.DisplayName);
            UpdateAllowedButtons();
            editOrDeleteSlot = slot;
            uiManager.PushWindow(effectEditor);
        }

        protected virtual void EditOrDeleteSlot(int slot)
        {
            const int howToAlterSpell = 1708;

            // Do nothing if slot not set
            if (string.IsNullOrEmpty(effectEntries[slot].Key))
                return;

            // Offer to edit or delete effect
            editOrDeleteSlot = slot;
            DaggerfallMessageBox mb = new DaggerfallMessageBox(uiManager, this);
            mb.SetTextTokens(howToAlterSpell);
            Button editButton = mb.AddButton(DaggerfallMessageBox.MessageBoxButtons.Edit);
            editButton.OnMouseClick += EditButton_OnMouseClick;
            Button deleteButton = mb.AddButton(DaggerfallMessageBox.MessageBoxButtons.Delete);
            deleteButton.OnMouseClick += DeleteButton_OnMouseClick;
            mb.OnButtonClick += EditOrDeleteSpell_OnButtonClick;
            mb.OnCancel += EditOrDeleteSpell_OnCancel;
            mb.Show();
        }

        protected virtual void SetSpellTarget(TargetTypes targetType)
        {
            // Exclude target types based on effects added
            if ((allowedTargets & targetType) == TargetTypes.None)
                return;

            // Clear buttons
            casterOnlyButton.BackgroundTexture = null;
            byTouchButton.BackgroundTexture = null;
            singleTargetAtRangeButton.BackgroundTexture = null;
            areaAroundCasterButton.BackgroundTexture = null;
            areaAtRangeButton.BackgroundTexture = null;

            // Set selected icon
            switch (targetType)
            {
                case TargetTypes.CasterOnly:
                    casterOnlyButton.BackgroundTexture = casterOnlySelectedTexture;
                    break;
                case TargetTypes.ByTouch:
                    byTouchButton.BackgroundTexture = byTouchSelectedTexture;
                    break;
                case TargetTypes.SingleTargetAtRange:
                    singleTargetAtRangeButton.BackgroundTexture = singleTargetAtRangeSelectedTexture;
                    break;
                case TargetTypes.AreaAroundCaster:
                    areaAroundCasterButton.BackgroundTexture = areaAroundCasterSelectedTexture;
                    break;
                case TargetTypes.AreaAtRange:
                    areaAtRangeButton.BackgroundTexture = areaAtRangeSelectedTexture;
                    break;
            }

            selectedTarget = targetType;
            UpdateSpellCosts();
        }

        protected virtual void SetSpellElement(ElementTypes elementType)
        {
            // Exclude element types based on effects added
            if ((allowedElements & elementType) == ElementTypes.None)
                return;

            // Clear buttons
            fireBasedButton.BackgroundTexture = null;
            coldBasedButton.BackgroundTexture = null;
            poisonBasedButton.BackgroundTexture = null;
            shockBasedButton.BackgroundTexture = null;
            magicBasedButton.BackgroundTexture = null;

            // Set selected icon
            switch (elementType)
            {
                case ElementTypes.Fire:
                    fireBasedButton.BackgroundTexture = fireBasedSelectedTexture;
                    break;
                case ElementTypes.Cold:
                    coldBasedButton.BackgroundTexture = coldBasedSelectedTexture;
                    break;
                case ElementTypes.Poison:
                    poisonBasedButton.BackgroundTexture = poisonBasedSelectedTexture;
                    break;
                case ElementTypes.Shock:
                    shockBasedButton.BackgroundTexture = shockBasedSelectedTexture;
                    break;
                case ElementTypes.Magic:
                    magicBasedButton.BackgroundTexture = magicBasedSelectedTexture;
                    break;
            }

            selectedElement = elementType;
            UpdateSpellCosts();
        }

        protected virtual void UpdateAllowedButtons()
        {
            // Set defaults when no effects added
            if (GetFirstUsedEffectSlotIndex() == -1)
            {
                allowedTargets = defaultTargetFlags;
                allowedElements = defaultElementFlags;
                SetSpellTarget(TargetTypes.CasterOnly);
                SetSpellElement(ElementTypes.Magic);
                EnforceSelectedButtons();
                return;
            }

            // Combine flags
            allowedTargets = EntityEffectBroker.TargetFlags_All;
            allowedElements = EntityEffectBroker.ElementFlags_MagicOnly;
            for (int i = 0; i < maxEffectsPerSpell; i++)
            {
                // Must be a valid entry
                if (!string.IsNullOrEmpty(effectEntries[i].Key))
                {
                    // Get effect template
                    IEntityEffect effectTemplate = GameManager.Instance.EntityEffectBroker.GetEffectTemplate(effectEntries[i].Key);

                    // Allowed targets are least permissive result set from combined target flags
                    allowedTargets = allowedTargets & effectTemplate.Properties.AllowedTargets;

                    // Allowed elements are most permissive result set from combined element flags (magic always allowed)
                    allowedElements = allowedElements | effectTemplate.Properties.AllowedElements;
                }
            }

            // Ensure a valid button is selected
            EnforceSelectedButtons();
        }

        protected void EnforceSelectedButtons()
        {
            if ((allowedTargets & selectedTarget) == TargetTypes.None)
                SelectFirstAllowedTargetType();

            if ((allowedElements & selectedElement) == ElementTypes.None)
                SelectFirstAllowedElementType();
        }

        protected virtual void SelectFirstAllowedTargetType()
        {
            if ((allowedTargets & TargetTypes.CasterOnly) == TargetTypes.CasterOnly)
            {
                SetSpellTarget(TargetTypes.CasterOnly);
                return;
            }
            else if ((allowedTargets & TargetTypes.ByTouch) == TargetTypes.ByTouch)
            {
                SetSpellTarget(TargetTypes.ByTouch);
                return;
            }
            else if ((allowedTargets & TargetTypes.SingleTargetAtRange) == TargetTypes.SingleTargetAtRange)
            {
                SetSpellTarget(TargetTypes.SingleTargetAtRange);
                return;
            }
            else if ((allowedTargets & TargetTypes.AreaAroundCaster) == TargetTypes.AreaAroundCaster)
            {
                SetSpellTarget(TargetTypes.AreaAroundCaster);
                return;
            }
            else if ((allowedTargets & TargetTypes.AreaAtRange) == TargetTypes.AreaAtRange)
            {
                SetSpellTarget(TargetTypes.AreaAtRange);
                return;
            }
        }

        protected virtual void SelectFirstAllowedElementType()
        {
            if ((allowedElements & ElementTypes.Fire) == ElementTypes.Fire)
            {
                SetSpellElement(ElementTypes.Fire);
                return;
            }
            else if ((allowedElements & ElementTypes.Cold) == ElementTypes.Cold)
            {
                SetSpellElement(ElementTypes.Cold);
                return;
            }
            else if ((allowedElements & ElementTypes.Poison) == ElementTypes.Poison)
            {
                SetSpellElement(ElementTypes.Poison);
                return;
            }
            else if ((allowedElements & ElementTypes.Shock) == ElementTypes.Shock)
            {
                SetSpellElement(ElementTypes.Shock);
                return;
            }
            else if ((allowedElements & ElementTypes.Magic) == ElementTypes.Magic)
            {
                SetSpellElement(ElementTypes.Magic);
                return;
            }
        }

        protected List<EffectEntry> GetEffectEntries()
        {
            // Get a list of actual effect entries and ignore empty slots
            List<EffectEntry> effects = new List<EffectEntry>();
            for (int i = 0; i < maxEffectsPerSpell; i++)
            {
                if (!string.IsNullOrEmpty(effectEntries[i].Key))
                    effects.Add(effectEntries[i]);
            }

            return effects;
        }

        protected virtual void CalculateSpellCosts()
        {
            List<EffectEntry> effects = GetEffectEntries();
            spell = new EffectBundleSettings();
            spell.Version = EntityEffectBroker.CurrentSpellVersion;
            spell.BundleType = BundleTypes.Spell;
            spell.TargetType = selectedTarget;
            spell.ElementType = selectedElement;
            spell.Name = spellNameLabel.Text;
            spell.IconIndex = selectedIcon.index;
            spell.Icon = selectedIcon;
            spell.Effects = effects.ToArray();

            
            FormulaHelper.SpellCost spellCost = MMMFormulaHelper.CalculateTotalEffectCosts_XP(spell.Effects, spell.TargetType, spell.ElementType, null, spell.MinimumCastingCost);
            actualSpellPointCost = spellCost.spellPointCost;
            baseGuildFee = (int)Math.Round(((float)actualSpellPointCost) * baseGuildFeeMultiplier);       // spell-point casting cost *4
            actualGuildFee = FormulaHelper.CalculateTradePrice(baseGuildFee, buildingDiscoveryData.quality, false);

            actualSpellMakingTimeCost = MMMFormulaHelper.CalculateSpellCreationTimeCost(spell, true);

            baseInstructionFee = MMMFormulaHelper.CalculateLearningTimeCostFromMinutes(playerRankInGuild, actualSpellMakingTimeCost, true);

            /*baseInstructionFee = 1000 * ((actualSpellMakingTimeCost + 30) / 60); // there was another error here: rank risregarded*/
            
            actualInstructionFee = FormulaHelper.CalculateTradePrice(baseInstructionFee, buildingDiscoveryData.quality, false);

            actualSpellPointCostOfMaking = actualSpellPointCost * 3;
            baseSpellPointPotionCost = actualSpellPointCostOfMaking * (11 - playerRankInGuild);   // a function of spell point cost of learning and guild rank
            actualSpellPointPotionCost = FormulaHelper.CalculateTradePrice(baseSpellPointPotionCost, buildingDiscoveryData.quality, false);
            actualFatigueCostOfMaking = PlayerEntity.DefaultFatigueLoss * actualSpellMakingTimeCost;
            actualFatigueCostOfMakingToPlayer = (actualFatigueCostOfMaking + 30) / 60;        // rounded number
            baseFatiguePotionCost = actualFatigueCostOfMakingToPlayer * (11 - playerRankInGuild) / 2;       // a function of fatigue cost of learning and guild rank
            actualFatiguePotionCost = FormulaHelper.CalculateTradePrice(baseFatiguePotionCost, buildingDiscoveryData.quality, false);

            // Costs are halved on Witches Festival holiday
            uint gameMinutes = DaggerfallUnity.Instance.WorldTime.DaggerfallDateTime.ToClassicDaggerfallTime();
            int holidayID = FormulaHelper.GetHolidayId(gameMinutes, 0);
            if (holidayID == (int)DaggerfallConnect.DFLocation.Holidays.Witches_Festival)
            {
                actualGuildFee = Math.Max(actualGuildFee / 2, 1);
                actualInstructionFee = Math.Max(actualInstructionFee / 2, 1);
                actualSpellPointPotionCost = Math.Max(actualSpellPointPotionCost / 2, 1);
                actualFatiguePotionCost = Math.Max(actualFatiguePotionCost / 2, 1);
            }
        }

        public EffectBundleSettings ActualStateOfTheSpellCreated()
        {
            List<EffectEntry> effects = GetEffectEntries();
            spell = new EffectBundleSettings();
            spell.Version = EntityEffectBroker.CurrentSpellVersion;
            spell.BundleType = BundleTypes.Spell;
            spell.TargetType = selectedTarget;
            spell.ElementType = selectedElement;
            spell.Name = spellNameLabel.Text;
            spell.IconIndex = selectedIcon.index;
            spell.Icon = selectedIcon;
            spell.Effects = effects.ToArray();
            return spell;
        }

        protected void UpdateSpellCosts()
        {
            SilentMessage("UpdateSpellCosts called");
            // Note: Daggerfall shows gold cost 0 and spellpoint cost 5 with no effects added
            // Not copying this behaviour at this time intentionally as it seems unclear for an invalid
            // spell to have any casting cost at all - may change later
            totalGoldCost = 0;
            totalSpellPointCost = 0;

            // Do nothing when effect editor not setup or not used effect slots
            // This means there is nothing to calculate
            if (effectEditor == null || !effectEditor.IsSetup)
            {
                SetStatusLabels();
                return;
            }            

            // Update slot being edited with current effect editor settings
            if (editOrDeleteSlot != -1)
                effectEntries[editOrDeleteSlot] = effectEditor.EffectEntry;

            /*
            // Get total costs
            (totalGoldCost, totalSpellPointCost) = FormulaHelper.CalculateTotalEffectCosts(effectEntries, selectedTarget);

            int originalTotalGoldCost = totalGoldCost;

            List<EffectEntry> effects = GetEffectEntries();
            spell = new EffectBundleSettings();
            spell.Version = EntityEffectBroker.CurrentSpellVersion;
            spell.BundleType = BundleTypes.Spell;
            spell.TargetType = selectedTarget;
            spell.ElementType = selectedElement;
            spell.Name = spellNameLabel.Text;
            spell.IconIndex = selectedIcon.index;
            spell.Icon = selectedIcon;
            spell.Effects = effects.ToArray();

            actualSpellMakingTimeCost = MMMFormulaHelper.CalculateSpellCreationTimeCost(spell, true);
            actualSpellPointCostOfMaking = totalSpellPointCost * 3;
            int hoursOfInstructionCompleted = (actualSpellMakingTimeCost / 60);
            totalGoldCost += 1000 * hoursOfInstructionCompleted;     // flat fee of 1000 for each hour finished
            */
            CalculateSpellCosts();

                    // TODO: handle this CountUsedEffectSlots() == 0

            SilentMessage("Update spell costs. Printing calculation results. Guild Fee {0}->{1}. Learning time: {2} minutes, instruction fee: {3}->{4}. " +
                    "SpellPointCost: {5}, Fatigue Cost: {6} (cca. {7}), SpellPoint Potion Cost: {8}->{9}, Fatigue Potion Cost: {10}->{11}.", baseGuildFee,
                    actualGuildFee, actualSpellMakingTimeCost, baseInstructionFee, actualInstructionFee, actualSpellPointCostOfMaking, actualFatigueCostOfMaking,
                    actualFatigueCostOfMakingToPlayer, baseSpellPointPotionCost, actualSpellPointPotionCost, baseFatiguePotionCost, actualFatiguePotionCost);

            SetStatusLabels();
        }

        #endregion

        private string ToUnavailable(string effectEntry)
        {
            return "  " + effectEntry;
        }

        private bool IsAvailable (string effectEntry)
        {
            return !effectEntry.StartsWith("  ");
        }       // TODO: consider merging these with SpellBookWindow and moving it to a common helper

        #region Button Events        

        protected virtual void AddEffectButton_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            const int noMoreThan3Effects = 1707;

            if (knowableEffects == null || knowableEffectGroups == null)
            {
                knowableEffects = new Dictionary<string, string[]>();
                knowableEffectGroups = new List<string>();

                string[] groupNames0 = GameManager.Instance.EntityEffectBroker.GetGroupNames(true, thisMagicStation);

                for (int i = 0; i < groupNames0.Length; i++)
                {
                    int usableSubGroupCount = 0;

                    List<IEntityEffect> enumeratedEffectTemplates0 = GameManager.Instance.EntityEffectBroker.GetEffectTemplates(groupNames0[i], thisMagicStation);
                    List<string> knowableSubGroups = new List<string>();

                    string toPrint = string.Format("Group #{0}: '{1}'", i, groupNames0[i]) + Environment.NewLine;

                    for (int j = 0; j < enumeratedEffectTemplates0.Count; j++)
                    {
                        toPrint += string.Format("Key='{0}', GroupName='{1}', SubGroupName='{2}', ",
                            enumeratedEffectTemplates0[j].Key, enumeratedEffectTemplates0[j].GroupName, enumeratedEffectTemplates0[j].SubGroupName) + " ";

                        if (!MMMEffectAndSpellHandler.IsEffectKnowableToPlayerHere(enumeratedEffectTemplates0[j].Key))
                        {
                            toPrint += "not knowable here." + Environment.NewLine;
                            continue;
                        }

                        if (MMMEffectAndSpellHandler.CanTheyTeachThisEffectHere(enumeratedEffectTemplates0[j].Key))
                        {
                            toPrint += "knowable and usable here." + Environment.NewLine;
                            knowableSubGroups.Add(enumeratedEffectTemplates0[j].Key);
                            usableSubGroupCount += 1;
                        }
                        else
                        {
                            toPrint += "knowable but not usable here." + Environment.NewLine;
                            knowableSubGroups.Add(ToUnavailable(enumeratedEffectTemplates0[j].Key));
                        }
                    }

                    if (knowableSubGroups.Count == 0)
                    {
                        toPrint+="Effect Group not knowable here.";
                    }
                    else
                    {                        
                        if (usableSubGroupCount > 0)
                        {
                            toPrint+= "Effect group knowable and usable here.";
                            knowableEffectGroups.Add(groupNames0[i]);
                            knowableEffects.Add(groupNames0[i], knowableSubGroups.ToArray());
                        }
                        else
                        {
                            toPrint+= "Effect group knowable but not usable here.";
                            knowableEffectGroups.Add(ToUnavailable(groupNames0[i]));
                        }                        
                    }
                    SilentMessage(toPrint);
                }
            }
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);

            // TODO: here, we put a check on whether effect availability has already been checked
            //              if not, call a method that goes through, evaluates each effect and saves them in a transparent data-structure
            //              the data structure should be a Dictionary (string, entry[]) where entry is a struct consisting of a Key and an availability signal
            //              availability could be either
            //                      Nonvisible - conf level higher than player standing in guild - player shoud see no mention of this effect
            //                      Nonavailable - conf level equal or lower than player standing in guild, but the effect falls outside the current Hall's Theme
                                                    //      player should see this but should be signalled that it is not available in some way and when clicked, a MessageBox should clarify
            //                      Available - player standing sufficient and spell falls into the guild hall's Theme - this should be normal, player can pick these ones
            //              if check is made, then check against this data-structure
            //                      if all elements of effect group are NonVisible, then entire subgroup should be NonVisible
            //                      if all elements of effect group are NonAvailable, then entire subgroup should be NonAvailable
            //                      otherwise, subgroup should be Avaiable

            // Must have a free effect slot
            if (GetFirstFreeEffectSlotIndex() == -1)
            {
                DaggerfallMessageBox mb = new DaggerfallMessageBox(
                    uiManager,
                    DaggerfallMessageBox.CommonMessageBoxButtons.Nothing,
                    DaggerfallUnity.Instance.TextProvider.GetRSCTokens(noMoreThan3Effects),
                    this);
                mb.ClickAnywhereToClose = true;
                mb.Show();
                return;
            }

            // Clear existing
            effectGroupPicker.ListBox.ClearItems();
            tipLabel.Text = string.Empty;

            // TODO: Filter out effects incompatible with any effects already added (e.g. incompatible target types)                      
                    
            effectGroupPicker.ListBox.AddItems(knowableEffectGroups);
            effectGroupPicker.ListBox.SelectedIndex = 0;

            // Show effect group picker
            uiManager.PushWindow(effectGroupPicker);
        }

        protected virtual void BuyButton_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            const int notEnoughGold = 1702;
            const int noSpellBook = 1703;
            const int youMustChooseAName = 1704;

            DaggerfallMessageBox messageBox;

            CalculateSpellCosts();

            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);

            SilentMessage("Attempting to make a spell. 1st stage. Printing calculation results. Guild Fee {0}->{1}. Learning time: {2} minutes, instruction fee: {3}->{4}. " +
                    "SpellPointCost: {5}, Fatigue Cost: {6} (cca. {7}), SpellPoint Potion Cost: {8}->{9}, Fatigue Potion Cost: {10}->{11}.", baseGuildFee,
                    actualGuildFee, actualSpellMakingTimeCost, baseInstructionFee, actualInstructionFee, actualSpellPointCostOfMaking, actualFatigueCostOfMaking,
                    actualFatigueCostOfMakingToPlayer, baseSpellPointPotionCost, actualSpellPointPotionCost, baseFatiguePotionCost, actualFatiguePotionCost);
            
            // Presence of spellbook is also checked earlier
            if (!GameManager.Instance.PlayerEntity.Items.Contains(ItemGroups.MiscItems, (int)MiscItems.Spellbook))
            {
                DaggerfallUI.MessageBox(noSpellBook);
                return;
            }            

            // Spell must have at least one effect - adding custom message
            List<EffectEntry> effects = GetEffectEntries();
            if (effects.Count == 0)
            {
                DaggerfallUI.MessageBox(TextManager.Instance.GetLocalizedText("noEffectsError"));
                return;
            }

            // Spell must have a name; Only bother the player if everything else is correct
            if (string.IsNullOrEmpty(spellNameLabel.Text))
            {
                DaggerfallUI.MessageBox(youMustChooseAName);
                return;
            }
            else
                SilentMessage("BuyButton_OnMouseClick called. Text in spellNameLabel: "+ spellNameLabel.Text);

            messageBox = new DaggerfallMessageBox(uiManager, this);

            // Create effect bundle settings
            spell = new EffectBundleSettings();
            spell.Version = EntityEffectBroker.CurrentSpellVersion;
            spell.BundleType = BundleTypes.Spell;
            spell.TargetType = selectedTarget;
            spell.ElementType = selectedElement;
            spell.Name = spellNameLabel.Text;           // TODO: dump data to player.log
            SilentMessage("BuyButton_OnMouseClick: Spell name set='" + spell.Name+"'");

            spell.IconIndex = selectedIcon.index;
            spell.Icon = selectedIcon;
            spell.Effects = effects.ToArray();
            
            if (actualSpellMakingTimeCost > 600)
            {
                //DaggerfallUI.MessageBox("You seem unable to master such a spell. (More intelligence, experience in the school or with similar spells could help.");
                messageBox.SetText(WouldTakeTooMuchTimeMessage);
                messageBox.ClickAnywhereToClose = true;
                messageBox.Show();
                return;
            }

            if ((GameManager.Instance.PlayerEntity.CurrentFatigue < PlayerEntity.DefaultFatigueLoss * (actualSpellMakingTimeCost + 60)) || // should provide for fatigue to end training with 1 hours reserve left
                    (GameManager.Instance.PlayerEntity.CurrentMagicka < actualSpellPointCostOfMaking))
            {       // if the player is low on fatigue or magicka (if player has enough gold, then offered refreshments, otherwise dismissed)
                if (GameManager.Instance.PlayerEntity.GetGoldAmount() < actualGuildFee + actualInstructionFee + actualSpellPointPotionCost + actualFatiguePotionCost)
                {           // dismissed to return when rested
                    messageBox.SetText(MMMMessages.GetMessage(TooTiredAndNotEnoughGoldForPotionsMessage, actualGuildFee, actualSpellMakingTimeCost,
                        actualInstructionFee, actualFatigueCostOfMakingToPlayer, actualSpellPointCostOfMaking));
                    // what needs to be passed: (0) guild fee, (1) time cost, (2) instruction cost, (3) fatigue cost, (4) spell point cost
                    messageBox.ClickAnywhereToClose = true;
                    messageBox.Show();
                    return;
                }
                else
                {           // offered training + refreshments
                    SilentMessage("Player is offered training + refreshments.");
                    messageBox.SetText(MMMMessages.GetMessage(HasToTakeRefreshmentsMessage, actualGuildFee, actualSpellMakingTimeCost, actualInstructionFee,
                        actualFatigueCostOfMakingToPlayer, actualSpellPointCostOfMaking, actualFatiguePotionCost, actualSpellPointPotionCost,
                        actualGuildFee + actualInstructionFee + actualFatiguePotionCost + actualSpellPointPotionCost));
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
                messageBox.SetText(MMMMessages.GetMessage(OnlyBareEssentialsMessage, actualGuildFee, actualSpellMakingTimeCost,
                    actualInstructionFee, actualFatigueCostOfMakingToPlayer, actualSpellPointCostOfMaking));
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
                messageBox.SetText(MMMMessages.GetMessage(HasChoiceAboutBuyingRefreshmentsMessage, actualGuildFee, actualSpellMakingTimeCost,
                    actualInstructionFee, actualFatigueCostOfMakingToPlayer, actualSpellPointCostOfMaking, actualFatiguePotionCost, actualSpellPointPotionCost));
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
        }

        protected virtual void ConfirmTrade_PlayerPromptedWhetherHeWantsRefreshments(DaggerfallMessageBox sender, DaggerfallMessageBox.MessageBoxButtons messageBoxButton)
        {
            uiManager.PopWindow();
            DaggerfallMessageBox messageBox = new DaggerfallMessageBox(uiManager, this);

            if (messageBoxButton == DaggerfallMessageBox.MessageBoxButtons.Yes)
            {
                messageBox.SetText(MMMMessages.GetMessage(HasAcceptedTakingRefreshmentsMessage, actualGuildFee + actualInstructionFee + actualSpellPointPotionCost +
                    actualFatiguePotionCost, actualSpellMakingTimeCost));
                // (0) total gold cost including refreshments, (1) time cost
                buyingPotionsToo = true;    // the player has opted to take refreshments
            }
            else
            {
                messageBox.SetText(MMMMessages.GetMessage(OnlyBareEssentialsMessage, actualGuildFee, actualSpellMakingTimeCost, actualInstructionFee,
                    actualFatigueCostOfMakingToPlayer, actualSpellPointCostOfMaking));
                // what needs to be passed: (0) guild fee, (1) time cost, (2) instruction cost, (3) fatigue cost, (4) spell point cost
                buyingPotionsToo = false;   // the player has opted to decline refreshments
            }

            messageBox.AddButton(DaggerfallMessageBox.MessageBoxButtons.Yes);
            messageBox.AddButton(DaggerfallMessageBox.MessageBoxButtons.No);
            messageBox.OnButtonClick += ConfirmTrade_OnButtonClick;
            messageBox.Show();
            //uiManager.PushWindow(messageBox);
        }


        protected virtual void ConfirmTrade_OnButtonClick(DaggerfallMessageBox sender, DaggerfallMessageBox.MessageBoxButtons messageBoxButton)
        {
                    // TODO: invent and apply condition when base method needs to be executed
            const int spellHasBeenInscribed = 1705;
            if (messageBoxButton == DaggerfallMessageBox.MessageBoxButtons.Yes)
            {
                SilentMessage("ConfirmTrade_OnButtonClickPlayer: player fatigue before change: " + GameManager.Instance.PlayerEntity.CurrentFatigue);

                int goldCost = actualGuildFee + actualInstructionFee;
                if (buyingPotionsToo)
                    goldCost += actualFatiguePotionCost + actualSpellPointPotionCost;
                GameManager.Instance.PlayerEntity.DeductGoldAmount(goldCost);     
                
                SilentMessage("ConfirmTrade_OnButtonClickPlayer: spell name before save: "+spell.Name);
                    // Add to player entity spellbook
                GameManager.Instance.PlayerEntity.AddSpell(spell);  
                DaggerfallUI.Instance.PlayOneShot(inscribeGrimoire);

                int numberOfFatiguePointsToSubtract = 0;
                int numberOfSpellointsToSubtract = 0;

                if (!buyingPotionsToo)
                {
                    numberOfFatiguePointsToSubtract = actualFatigueCostOfMaking;
                    numberOfSpellointsToSubtract = actualSpellPointCostOfMaking;

                    GameManager.Instance.PlayerEntity.DecreaseFatigue(numberOfFatiguePointsToSubtract);
                    GameManager.Instance.PlayerEntity.DecreaseMagicka(numberOfSpellointsToSubtract);
                }

                MTMostlyMagicMod.ElapseMinutes(actualSpellMakingTimeCost);

                SilentMessage(string.Format("ConfirmTrade_OnButtonClick: Spell making costs incurred. Gold={0}, SpellPoints={1}, FatiguePoints={2}, Time={3} minutes.",
                    goldCost, numberOfSpellointsToSubtract, numberOfFatiguePointsToSubtract, actualSpellMakingTimeCost));
                SilentMessage("ConfirmTrade_OnButtonClickPlayer: player fatigue after change: " + GameManager.Instance.PlayerEntity.CurrentFatigue);

                // now, have spell engineering train the relevant magic skills too
                int hoursOfInstructionCompleted = actualSpellMakingTimeCost / 60;

                IEntityEffect effectTemplate;
                int totalTrainingPoints = hoursOfInstructionCompleted * UnityEngine.Random.Range(10, 20 + 1) / 3 + 3;   // +3 for a minimum training - equivalent of casting the spell 3 times
                                // TODO: consider merging this coefficient 3 with the other 3 and moving to mod settings

                for (int i = 0; i < spell.Effects.Length; i++)
                {
                    effectTemplate = GameManager.Instance.EntityEffectBroker.GetEffectTemplate(spell.Effects[i].Key);
                    DFCareer.Skills skillToTrain = (DFCareer.Skills)effectTemplate.Properties.MagicSkill;

                    int skillAdvancementMultiplier = DaggerfallSkills.GetAdvancementMultiplier(skillToTrain);
                    short tallyAmount = (short)((totalTrainingPoints / spell.Effects.Length) * skillAdvancementMultiplier);
                    GameManager.Instance.PlayerEntity.TallySkill(skillToTrain, tallyAmount);
                    SilentMessage(string.Format("ConfirmTrade_OnButtonClickPlayer. {0}/{1}: totalTrainingPoints={2}, key={3}, skillToTrain={4}, skillAdvancementMultiplier={5}, tallyAmount={6}",
                        i+1, spell.Effects.Length, totalTrainingPoints, spell.Effects[i].Key, (int)skillToTrain, skillAdvancementMultiplier, tallyAmount));
                }

                MMMXPTallies.CalculationDebugToPlayer = calculationDebugToPlayerBefore;
                CloseWindow();
            }
            else
            {
                SilentMessage("ConfirmTrade_OnButtonClickPlayer: the player has apparently chosen No.");
                CloseWindow();
            }
        }       

        protected virtual void SpellHasBeenInscribed_OnClose()
        {
            SetDefaults();
            iconPicker.ResetScrollPosition();
        }

        protected virtual void NewSpellButton_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            SetDefaults();
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        }

        protected virtual void ExitButton_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            CloseWindow();
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        }

        protected virtual void CasterOnlyButton_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            SetSpellTarget(TargetTypes.CasterOnly);
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        }

        protected virtual void ByTouchButton_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            SetSpellTarget(TargetTypes.ByTouch);
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        }

        protected virtual void SingleTargetAtRangeButton_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            SetSpellTarget(TargetTypes.SingleTargetAtRange);
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        }

        protected virtual void AreaAroundCasterButton_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            SetSpellTarget(TargetTypes.AreaAroundCaster);
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        }

        protected virtual void AreaAtRangeButton_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            SetSpellTarget(TargetTypes.AreaAtRange);
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        }

        protected virtual void FireBasedButton_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            SetSpellElement(ElementTypes.Fire);
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        }

        protected virtual void ColdBasedButton_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            SetSpellElement(ElementTypes.Cold);
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        }

        protected virtual void PoisonBasedButton_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            SetSpellElement(ElementTypes.Poison);
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        }

        protected virtual void ShockBasedButton_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            SetSpellElement(ElementTypes.Shock);
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        }

        protected virtual void MagicBasedButton_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            SetSpellElement(ElementTypes.Magic);
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        }

        private void NextIconButton_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            int index = selectedIcon.index + 1;
            if (index >= DaggerfallUI.Instance.SpellIconCollection.GetIconCount(selectedIcon.key))
                index = 0;

            selectedIcon.index = index;
            SetIcon(selectedIcon);
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        }

        protected virtual void SelectIconButton_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
            uiManager.PushWindow(iconPicker);
        }

        protected virtual void PreviousIconButton_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            int index = selectedIcon.index - 1;
            if (index < 0)
                index = DaggerfallUI.Instance.SpellIconCollection.GetIconCount(selectedIcon.key) - 1;

            selectedIcon.index = index;
            SetIcon(selectedIcon);
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        }

        protected virtual void NameSpellButton_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
            DaggerfallInputMessageBox mb = new DaggerfallInputMessageBox(uiManager, this);
            mb.TextBox.Text = spellNameLabel.Text;
            mb.SetTextBoxLabel(TextManager.Instance.GetLocalizedText("enterSpellName") + " ");
            mb.OnGotUserInput += EnterName_OnGotUserInput;
            mb.Show();
        }

        protected virtual void AddEffectGroupListBox_OnUseSelectedItem()
        {
            // Clear existing

            effectSubGroupPicker.ListBox.ClearItems();

            if (!IsAvailable(effectGroupPicker.ListBox.SelectedItem))
            {
                DaggerfallMessageBox messageBox = new DaggerfallMessageBox(uiManager, this);
                messageBox.SetText(NeedToFindOtherTeacherMessage);      // for some reason, it didn't work with ",this" added
                messageBox.ClickAnywhereToClose = true;
                messageBox.Show();
            }           // if unavailable, send appropriate messagebox 

            if (knowableEffects[effectGroupPicker.ListBox.SelectedItem].Length == 1)
            {
                effectGroupPicker.CloseWindow();
                IEntityEffect effectTemplate = GameManager.Instance.EntityEffectBroker.GetEffectTemplate(knowableEffects[effectGroupPicker.ListBox.SelectedItem][0]);        // get effect template 
                AddAndEditSlot(effectTemplate);
                //uiManager.PushWindow(effectEditor);
                return;
            }
            // should always have at least 1 element, because otherwise it would have been discarded in data generation step

            effectSubGroupPicker.ListBox.AddItems(knowableEffects[effectGroupPicker.ListBox.SelectedItem]);
            effectSubGroupPicker.ListBox.SelectedIndex = 0;

            uiManager.PushWindow(effectSubGroupPicker);

            /*
            enumeratedEffectTemplates.Clear();

            // TODO: here we work from effectGroupPicker.ListBox.SelectedItem - this has the text that was selected
            //  if text UnAvailable, send messagebox
            //          otherwise do as before, just begin with knowableEffects[effectGroupPicker.ListBox.SelectedItem]

            // Enumerate subgroup effect key name pairs

            enumeratedEffectTemplates = GameManager.Instance.EntityEffectBroker.GetEffectTemplates(effectGroupPicker.ListBox.SelectedItem, thisMagicStation);       // this only gets templates for the selected group
                                                                                                                                                                    
            if (enumeratedEffectTemplates.Count < 1)
                throw new Exception(string.Format("Could not find any effect templates for group {0}", effectGroupPicker.ListBox.SelectedItem));

            // If this is a solo effect without any subgroups names defined (e.g. "Regenerate") then go straight to effect editor
            if (enumeratedEffectTemplates.Count == 1 && string.IsNullOrEmpty(enumeratedEffectTemplates[0].SubGroupName))
            {
                effectGroupPicker.CloseWindow();
                AddAndEditSlot(enumeratedEffectTemplates[0]);
                //uiManager.PushWindow(effectEditor);
                return;
            }
                    
            // Sort list by subgroup name
            enumeratedEffectTemplates.Sort((s1, s2) => s1.SubGroupName.CompareTo(s2.SubGroupName));

            // Populate subgroup names in list box
            foreach (IEntityEffect effect in enumeratedEffectTemplates)
            {
                effectSubGroupPicker.ListBox.AddItem(effect.SubGroupName);
            }
            effectSubGroupPicker.ListBox.SelectedIndex = 0; 

            // Show effect subgroup picker
            // Note: In classic the group name is now shown (and mostly obscured) behind the picker at first available effect slot
            // This is not easily visible and not sure if this really communicates anything useful to user
            // Daggerfall Unity also allows user to cancel via escape back to previous dialog, so changing this beheaviour intentionally
            uiManager.PushWindow(effectSubGroupPicker); */
        }

        protected virtual void AddEffectSubGroup_OnUseSelectedItem()
        {
            DaggerfallMessageBox messageBox = new DaggerfallMessageBox(uiManager, this);

            if (!IsAvailable(effectSubGroupPicker.ListBox.SelectedItem))
            {
                messageBox.SetText(NeedToFindOtherTeacherMessage);      // for some reason, it didn't work with ",this" added
                messageBox.ClickAnywhereToClose = true;
                messageBox.Show();
                return;
            }           // if unavailable, send appropriate messagebox 

            // Close effect pickers
            effectGroupPicker.CloseWindow();
            effectSubGroupPicker.CloseWindow();

            // Get selected effect from those on offer
            IEntityEffect effectTemplate = GameManager.Instance.EntityEffectBroker.GetEffectTemplate(effectSubGroupPicker.ListBox.SelectedItem);
            // old version IEntityEffect effectTemplate = enumeratedEffectTemplates[effectSubGroupPicker.ListBox.SelectedIndex];
            if (effectTemplate != null)
            {
                AddAndEditSlot(effectTemplate);
                //Debug.LogFormat("Selected effect {0} {1} with key {2}", effectTemplate.GroupName, effectTemplate.SubGroupName, effectTemplate.Key);
            }
        }

        protected virtual void EditOrDeleteSpell_OnCancel(DaggerfallPopupWindow sender)
        {
            editOrDeleteSlot = -1;
        }

        protected virtual void EditOrDeleteSpell_OnButtonClick(DaggerfallMessageBox sender, DaggerfallMessageBox.MessageBoxButtons messageBoxButton)
        {
            sender.CloseWindow();
        }

        protected virtual void DeleteButton_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            // Delete effect entry
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
            ClearPendingDeleteEffectSlot();
            UpdateSpellCosts();
        }

        protected virtual void EditButton_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            // Edit effect entry
            effectEditor.EffectEntry = effectEntries[editOrDeleteSlot];
            uiManager.PushWindow(effectEditor);
        }

        #endregion

        #region Effect Editor Events

        protected virtual void EffectEditor_OnClose()
        {
            editOrDeleteSlot = -1;
            UpdateAllowedButtons();
        }

        protected virtual void EffectEditor_OnSettingsChanged()
        {
            UpdateSpellCosts();
        }

        protected virtual void IconPicker_OnClose()
        {
            if (iconPicker.SelectedIcon != null)
                SetIcon(iconPicker.SelectedIcon.Value);
        }

        protected virtual void Effect1NamePanel_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            EditOrDeleteSlot(0);
        }

        protected virtual void Effect2NamePanel_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            EditOrDeleteSlot(1);
        }

        protected virtual void Effect3NamePanel_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            EditOrDeleteSlot(2);
        }

        protected virtual void EnterName_OnGotUserInput(DaggerfallInputMessageBox sender, string input)
        {
            spellNameLabel.Text = input;
        }

        #endregion

        #region Tip Events

        protected bool lockTip = false;
        protected virtual void TipButton_OnMouseEnter(BaseScreenComponent sender)
        {
            // Lock tip if already has text, this means we are changing directly to adjacent button
            // This prevents OnMouseLeave event from previous button wiping tip text of new button
            if (!string.IsNullOrEmpty(tipLabel.Text))
                lockTip = true;

            tipLabel.Text = TextManager.Instance.GetLocalizedText(sender.Tag as string);
            if (sender is Button)
            {
                Button buttonSender = (Button)sender;
                if (buttonSender.Hotkey != HotkeySequence.None)
                    tipLabel.Text += string.Format(" ({0})", buttonSender.Hotkey);
            }
        }

        protected virtual void TipButton_OnMouseLeave(BaseScreenComponent sender)
        {
            // Clear tip when not locked, otherwise reset tip lock
            if (!lockTip)
                tipLabel.Text = string.Empty;
            else
                lockTip = false;
        }

        #endregion
        
    }
}