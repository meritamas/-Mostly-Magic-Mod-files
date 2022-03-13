// Project:         MeriTamas's (Mostly) Magic Mod for Daggerfall Unity (http://www.dfworkshop.net)
// Copyright:       Copyright (C) 2021 meritamas
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Author:          meritamas (meritamas@outlook.com)

/*
 * The code is sloppy at various places - this is partly due to the fact that this mod was created by merging three smaller mods.
 * Some things are redundant, comments are missing, some comments are not useful anymore etc.
 * I have the intention of cleaning it up in the future.
 * For now, it seems to work as intended or - let's rather say - reasonably well.
*/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Wenzil.Console;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.Items;
using DaggerfallWorkshop.Game.Formulas;
using DaggerfallWorkshop.Game.Utility;
using DaggerfallWorkshop.Game.Utility.ModSupport;   //required for modding features
using DaggerfallWorkshop.Game.Utility.ModSupport.ModSettings;
using DaggerfallWorkshop.Game.UserInterfaceWindows;
using DaggerfallWorkshop.Game.MagicAndEffects;
using DaggerfallConnect;

/*  The modules:
 *  - Unleveled Enemies
 *  - Magic-Related Rules Changes
 *  - New Effects and Spells
 *  - Quality of Life (currently to raise timescale while running to travel+train at the same time)
 *  - Experimental
*/

namespace MTMMM
{
    public class MTMostlyMagicMod : MonoBehaviour
    {
        static Mod modInstance;
        static ModSettings ourModSettings;

            // field used both by Advanced Teleportation and Unleveled Enemies
        static DaggerfallMessageBox MessageBox;
        public static int HUDLineLength = 120;

            // these fields are for Advanced Teleportation

        public static bool shouldEraseAdvancedTeleportEffect = false;

        static bool problem = false;    // used for Advanced Teleportation

            // these fields are for Unleveled Enemies
        static public int luckMod;
        static public int matRoll;
        static public int region = 0;
        static public int dungeon = 0;
        static public int savedDQL = -1;

        public static string greetingMessageToPlayerUnleveledEnemies = "Enemy prefab changed? Init hung...";   // this is used to communicate to the appropriate method the string message that needs to be conveyed

            // these fields are for UnleveledSpells        
        public static string BasicGreetingsMessage = "Spell effects unleveled:";
        public static string greetingMessageToPlayerUnleveledSpells;   // this is used to communicate to the appropriate method the string message that needs to be conveyed 

            // these fields are for TimeAccelerator
        static float baseFixedDeltaTime;
        static float baseTimeScale;
        static int timeAcceleratorMultiple;
        static int diseaseCount;

        #region Common helpers and routines
        /// <summary>
        /// A helper to display messages on the screen on the HUD or in a window
        /// </summary>
        public static void DisplayMessageOnScreen(string message)
        {
            if (message.Length < HUDLineLength)
                DaggerfallUI.AddHUDText(message);
            else
            {
                DaggerfallUI.AddHUDText(message.Substring(0, HUDLineLength));        // this will do for now
                /*if (MessageBox == null)       // TODO: add something here to write the first 120 chars onto the HUD
                {
                    MessageBox = new DaggerfallMessageBox(DaggerfallWorkshop.Game.DaggerfallUI.UIManager, null, true);
                    MessageBox.AllowCancel = true;
                    MessageBox.ClickAnywhereToClose = true;
                    MessageBox.ParentPanel.BackgroundColor = Color.clear;
                }
                MessageBox.SetText(message);
                DaggerfallUI.UIManager.PushWindow(MessageBox); */
            }
        }        

        /// <summary>
        /// Method to send debug messages, meant to be used by MMMFormulaHelper
        /// </summary>
        public static void Message(string message)
        {
            Message(message, true, true, false, false);
        }

        /// <summary>
        /// Silent method to send debug messages, meant to be used by MMMFormulaHelper - sends only to player.log, not to the HUD
        /// </summary>
        public static void SilentMessage (string message)
        {
            Message(message, false);
        }

        /// <summary>
        /// Method to send messages, depending on settings either to HUD or the Player, or both or neither
        /// </summary>
        public static void Message(string message, bool toHud=true, bool toPlayer=true, bool forceToHUD=false, bool forceToPlayer=false)
        {
            string msg = "MT MMM: " + message;

            if (forceToHUD || (toHud && ourModSettings.GetValue<bool>("Debug", "DebugToHUD")))
                DisplayMessageOnScreen(message);

            if (forceToPlayer || (toPlayer && ourModSettings.GetValue<bool>("Debug", "DebugToLog")))
                Debug.Log(msg);
        }

        public void Awake()
        {
            if (ourModSettings.GetValue<bool>("Magic-relatedRuleChanges", "IntelligenceRequirementForPurchasingSpells"))
                RegisterMTSpellBookWindow();

            if (ourModSettings.GetValue<bool>("NonMagicRuleChanges", "AlternativeSunLightDirections"))
            {
                //  GameManager.Instance.SunlightManager.Angle = -40f;      this code does not work ; but this is where the code that does the job should go 
            }
        }

        public void Start()
        {
            if (ourModSettings.GetValue<bool>("UnleveledEnemies", "Main"))
                InitUnleveledEnemiesOnAwake();
        }

        void Update()
        {
            TimeAcceleratorUpdate();
        }

        #endregion

        #region ModInit common parts

        /// <summary>
        /// Gives the player info on init success.      // TODO: re-evaluate the need for this
        /// </summary>
        [Invoke(StateManager.StateTypes.Game, 1)]
        public static void GiveInfoToPlayer(InitParams initParams)
        {
            if (problem)
                Message("Problem adding Advanced Teleport", true, true, true, true);
            else
                Message("Override for Teleport (Recall) (Advanced Teleportation) successfully registered.");

            InitTimeAcceleratorPart();
        }

        /// <summary>
        /// Init the class as a first step before work begins:
        /// - gets ModInstance and ModSettings
        /// - send appropriate message to debug system
        /// </summary>
        public static void StartInit(string modTitle)
        {
            Debug.Log("(Mostly) Magic Mod Init Started");
            modInstance = ModManager.Instance.GetMod(modTitle);
            ourModSettings = modInstance.GetSettings();

            MMMFormulaHelper.MMMFormulaHelperInfoMessage = Message;
            MMMFormulaHelper.MMMFormulaHelperSilentInfoMessage = SilentMessage;
        }        

        /// <summary>
        /// Initialize the Unleveled Enemies parts
        /// </summary>
        public static void InitUnleveledEnemiesOnStart()
        {
            GameObject go = DaggerfallUnity.Instance.Option_EnemyPrefab.transform.gameObject;
            go.AddComponent<MTMMM.RecalcStats>();

            GameObject go2 = new GameObject(modInstance.Title);
            go2.AddComponent<MTMostlyMagicMod>();   // initializing the UnleveledLoot part

            greetingMessageToPlayerUnleveledEnemies = "Enemy prefab changed successfully";
            Debug.Log("InitUnleveledEnemies module init method Finished");
        }        

        /// <summary>
        /// finish initialization:
        /// - turn the ready flag true
        /// - send appropriate message to debug system
        /// </summary>
        public static void EndInit()
        {
            modInstance.IsReady = true;         // set the mod's IsReady flag to true
            Debug.Log("(Mostly) Magic Mod Init Finished");
        }

        /// <summary>
        /// The main class init method. Initializes mod features via the helpers defined above.
        /// </summary>
        [Invoke(StateManager.StateTypes.Start, 0)]
        public static void Init(InitParams initParams)
        {
            StartInit(initParams.ModTitle);        // init the class as a first step before work begins

            
            InitNewSpellsAndEffects();
            if (ourModSettings.GetValue<bool>("UnleveledEnemies", "Main"))
                InitUnleveledEnemiesOnStart();


            InitRuleChanges();
            


            EndInit();          // Finish Mod Initialization            
        }
        #endregion

        #region New Spells and Effects part
        /// <summary>
        /// Initialize the New Spells and Effects parts
        /// </summary>
        public static void InitNewSpellsAndEffects()
        {
            if (ourModSettings.GetValue<bool>("NewEffectsAndSpells", "AdvancedTeleporation"))
            {
                BaseEntityEffect ourTeleportEffect = new MTTeleport();

                problem = !GameManager.Instance.EntityEffectBroker.RegisterEffectTemplate(ourTeleportEffect, true);
                ConsoleCommandsDatabase.RegisterCommand("advanced-teleport-kill", SaveOrNotToSave);
            }
                        


            if (ourModSettings.GetValue<bool>("NewEffectsAndSpells", "FixItem"))
                RegisterFixItem();
        }


        
        public static void RegisterFixItem()           
        {
            // First register custom effect with broker
            // This will make it available to crafting stations supported by effect

            MTRepairItem templateEffect = new MTRepairItem();
            // templateEffect.CurrentVariant = 0;
            if (!GameManager.Instance.EntityEffectBroker.RegisterEffectTemplate(templateEffect))
            {
                Message("FixItem effect could not be registered");
                return;
            }

            Message("FixItem effect successfully registered");

            // Create effect settings for our custom spell
            // These are Chance, Duration, and Magnitude required by spell - usually seen in spellmaker
            // No need to define settings not used by effect            
            EffectSettings effectSettings = new EffectSettings()
            {
                MagnitudeBaseMin = 1,
                MagnitudeBaseMax = 1,
                MagnitudePlusMin = 0,
                MagnitudePlusMax = 0,
                MagnitudePerLevel = 1,
            };

            // Create an EffectEntry
            // This links the effect key with settings
            // Each effect entry in bundle needs its own settings - most spells only have a single effect
            EffectEntry effectEntry = new EffectEntry()
            {
                Key = templateEffect.Properties.Key,
                Settings = effectSettings,
            };

            // Create a custom spell bundle
            // This is a portable version of the spell for other systems
            // For example, every spell in the player's spellbook is a bundle
            // Bundle target and elements settings should follow effect requirements
            EffectBundleSettings minorRepairSpell = new EffectBundleSettings()
            {
                Version = EntityEffectBroker.CurrentSpellVersion,
                BundleType = BundleTypes.Spell,
                TargetType = TargetTypes.CasterOnly,
                ElementType = ElementTypes.Magic,
                Name = "Reparo Parvulus",
                IconIndex = 2,
                Effects = new EffectEntry[] { effectEntry },
            };

            // Create a custom spell offer
            // This informs other systems if they can use this bundle
            EntityEffectBroker.CustomSpellBundleOffer minorRepairOffer = new EntityEffectBroker.CustomSpellBundleOffer()
            {
                Key = "ReparoParvulus-CustomOffer",                           // This key is for the offer itself and must be unique
                Usage = EntityEffectBroker.CustomSpellBundleOfferUsage.SpellsForSale, //|               Available in spells for sale
                                                                                      //EntityEffectBroker.CustomSpellBundleOfferUsage.CastWhenUsedEnchantment |    // Available for "cast on use" enchantments
                                                                                      //EntityEffectBrokerCustomSpellBundleOfferUsage.CastWhenHeldEnchantment,    // Available for "cast on held" enchantments
                BundleSetttings = minorRepairSpell //,                          The spell bundle created earlier
                //EnchantmentCost = 250,                                          // Cost to use spell at item enchanter if enabled
            };

            // Register the offer
            GameManager.Instance.EntityEffectBroker.RegisterCustomSpellBundleOffer(minorRepairOffer);

            EffectSettings effectSettings2 = new EffectSettings()
            {
                MagnitudeBaseMin = 1,
                MagnitudeBaseMax = 1,
                MagnitudePlusMin = 2,
                MagnitudePlusMax = 2,
                MagnitudePerLevel = 1,
            };

            // Create an EffectEntry
            // This links the effect key with settings
            // Each effect entry in bundle needs its own settings - most spells only have a single effect
            EffectEntry effectEntry2 = new EffectEntry()
            {
                Key = templateEffect.Properties.Key,
                Settings = effectSettings2,
            };

            // Create a custom spell bundle
            // This is a portable version of the spell for other systems
            // For example, every spell in the player's spellbook is a bundle
            // Bundle target and elements settings should follow effect requirements
            EffectBundleSettings repairSpell = new EffectBundleSettings()
            {
                Version = EntityEffectBroker.CurrentSpellVersion,
                BundleType = BundleTypes.Spell,
                TargetType = TargetTypes.CasterOnly,
                ElementType = ElementTypes.Magic,
                Name = "Reparo",
                IconIndex = 1,
                Effects = new EffectEntry[] { effectEntry2 },
            };

            // Create a custom spell offer
            // This informs other systems if they can use this bundle
            EntityEffectBroker.CustomSpellBundleOffer repairOffer = new EntityEffectBroker.CustomSpellBundleOffer()
            {
                Key = "Reparo-CustomOffer",                           // This key is for the offer itself and must be unique
                Usage = EntityEffectBroker.CustomSpellBundleOfferUsage.SpellsForSale, //|               Available in spells for sale
                                                                                      //EntityEffectBroker.CustomSpellBundleOfferUsage.CastWhenUsedEnchantment |    // Available for "cast on use" enchantments
                                                                                      //EntityEffectBrokerCustomSpellBundleOfferUsage.CastWhenHeldEnchantment,    // Available for "cast on held" enchantments
                BundleSetttings = repairSpell //,                          The spell bundle created earlier
                //EnchantmentCost = 250,                                          // Cost to use spell at item enchanter if enabled
            };

            // Register the offer
            GameManager.Instance.EntityEffectBroker.RegisterCustomSpellBundleOffer(repairOffer);            
        }        

        /// <summary>
        /// Executes the advanced-teleport-kill console command
        /// </summary>
        public static string SaveOrNotToSave(string[] args)
        {
            if (args[0].CompareTo("yes") == 0)
            {
                shouldEraseAdvancedTeleportEffect = true;
                return "Advanced Teleportation Kill Flag set to true. " + Environment.NewLine +
                    "Next time you invoke the spell effect, it will erase itself. Save games made afterwards will NOT contain Advanced Teleportation data but WILL be compatible with game without Advanced Teleportation mod.";
            }

            if (args[0].CompareTo("no") == 0)
            {
                shouldEraseAdvancedTeleportEffect = false;
                return "Advanced Teleportation Kill Flag set to false. " + Environment.NewLine +
                    "Will be saving this effect. Save games made afterwards WILL contain Advanced Teleportation data but will NOT be compatible with game without Advanced Teleportation mod.";
            }

            return "advanced-teleportation-kill: unrecognized parameters, please use either 'advanced-teleport-kill yes' or 'advanced-teleport-kill no'.";
        }
        #endregion

        #region Unleveled Enemies part
            // some accelarator code too
        void InitUnleveledEnemiesOnAwake()
        {
            PlayerEnterExit.OnPreTransition += SetDungeon_OnPreTransition;
            PlayerEnterExit.OnTransitionExterior += ClearData_OnTransitionExterior;            
        }

        private static void SetDungeon_OnPreTransition(PlayerEnterExit.TransitionEventArgs args)
        {
            region = GameManager.Instance.PlayerGPS.CurrentRegionIndex;
            dungeon = (int)GameManager.Instance.PlayerGPS.CurrentLocation.MapTableData.DungeonType;
            savedDQL = determineDungeonQuality();
        }

        private static void ClearData_OnTransitionExterior(PlayerEnterExit.TransitionEventArgs args)
        {
            dungeon = -1;
            savedDQL = -1;
        }

        public static int determineDungeonQuality()
        {
            int[] modifierTable = { 0, 1, 2, 1, 0, -1, -2, -1 };

            int modifierBasedOnHash = 0;
            // this should be a modifier from -2 to +2 that is contingent on a hash from the name (or coordinates) of the dungeon and the time (like which quarter it is)


            int currentYear = DaggerfallUnity.Instance.WorldTime.Now.Year;
            int currentMonth = DaggerfallUnity.Instance.WorldTime.Now.Month;
            int X = GameManager.Instance.PlayerGPS.CurrentMapPixel.X;
            int Y = GameManager.Instance.PlayerGPS.CurrentMapPixel.Y;

            int sequence = (X + Y + currentYear * 4 + currentMonth / 3) % 8;
            modifierBasedOnHash = modifierTable[sequence];
            // idea: fluctuating strength based on (X coord + Y coord + year + number) % 8

            int DQL = 0;

            switch (dungeon)
            {
                case (int)DFRegion.DungeonTypes.VolcanicCaves:
                case (int)DFRegion.DungeonTypes.Coven:
                    DQL = 21 + modifierBasedOnHash;
                    break;
                case (int)DFRegion.DungeonTypes.DesecratedTemple:
                case (int)DFRegion.DungeonTypes.DragonsDen:
                    DQL = 18 + modifierBasedOnHash;
                    break;
                case (int)DFRegion.DungeonTypes.BarbarianStronghold:
                case (int)DFRegion.DungeonTypes.VampireHaunt:
                    DQL = 15 + modifierBasedOnHash;
                    break;
                case (int)DFRegion.DungeonTypes.Crypt:
                case (int)DFRegion.DungeonTypes.OrcStronghold:
                    DQL = 12 + modifierBasedOnHash;
                    break;
                case (int)DFRegion.DungeonTypes.Laboratory:
                case (int)DFRegion.DungeonTypes.HarpyNest:
                    DQL = 10 + modifierBasedOnHash;
                    break;
                case (int)DFRegion.DungeonTypes.GiantStronghold:
                case (int)DFRegion.DungeonTypes.NaturalCave:            // MT added, missing from Ralzar mod
                    DQL = 5 + modifierBasedOnHash;
                    break;
                case (int)DFRegion.DungeonTypes.HumanStronghold:
                case (int)DFRegion.DungeonTypes.RuinedCastle:
                case (int)DFRegion.DungeonTypes.Prison:
                    // this should be contingent on a hash from the name (or coordinates) of the dungeon and the time (like which quarter it is) - for now, setting a value of 8  
                    DQL = 8 + modifierBasedOnHash;
                    break;
            }            // dungeon types not covered: ScorpionNest, SpiderNest, Mine, Cemetary

            Debug.Log("Determining Dungeon Quality; Year=" + currentYear + " Month=" + currentMonth + " X=" + X + " Y=" + Y + " sequence=" + sequence + " DQModifier=" + modifierBasedOnHash + " DQ=" + DQL);
            return DQL;
        }

        public static int dungeonQuality()
        {
            return savedDQL;
        }

        /// <summary>
        /// Gives the player info on whether init was successful.
        /// </summary>
        [Invoke(StateManager.StateTypes.Game, 0)]
        public static void ReportToUser(InitParams initParams)                      // TODO: THINK OVER IF THIS IS NEEDED AND IN WHAT FORM AND WHAT NAMES
        {
            Message(greetingMessageToPlayerUnleveledEnemies);                    // display greeting message to player            
        }
        #endregion

        #region Magic-related Rule Changes part
        /// <summary>
        /// Initialize the Magic-related Rule Changes parts
        /// </summary>
        public static void InitRuleChanges()
        {
            if (ourModSettings.GetValue<bool>("Magic-relatedRuleChanges", "UnleveledSpells"))
            {
                // starting to set up Unleveled Spells verbosity features
                string verboseMessage = "Verbose";

                // Setting MMMFormulaHelper verbosity features based on mod settings and adding the features turned on to a string to be displayed to the player  
                if (MMMFormulaHelper.SendInfoMessagesOnPCSpellLevel = ourModSettings.GetValue<bool>("Magic-relatedRuleChanges", "PCLevel"))
                    verboseMessage += " -- PC Spell Level";
                if (MMMFormulaHelper.SendInfoMessagesOnNonPCSpellLevel = ourModSettings.GetValue<bool>("Magic-relatedRuleChanges", "NonPCLevel"))
                    verboseMessage += " -- Non-PC Spell Level";

                Message(verboseMessage);
                // verbosity features set up

                FormulaHelper.RegisterOverride(modInstance, "CalculateCasterLevel", (Func<DaggerfallEntity, IEntityEffect, int>)MMMFormulaHelper.GetSpellLevelForGame);
            }

            if (ourModSettings.GetValue<bool>("Magic-relatedRuleChanges", "MinimumMagickaCostOn"))
            {
                MMMFormulaHelper.castCostFloor = ourModSettings.GetValue<int>("Magic-relatedRuleChanges", "MinimumMagickaCost");
                FormulaHelper.RegisterOverride(modInstance, "CalculateTotalEffectCosts", (Func<EffectEntry[], TargetTypes, DaggerfallEntity, bool, FormulaHelper.SpellCost>)MMMFormulaHelper.CalculateTotalEffectCosts);
                Message("Minimum spell cost set to: "+ MMMFormulaHelper.castCostFloor);
            }
        }

        public static void RegisterMTSpellBookWindow()
        {
            UIWindowFactory.RegisterCustomUIWindow(UIWindowType.SpellBook, typeof(MTSpellBookWindow));
            MTSpellBookWindow.intelligenceRequirementForPurchasingSpells = ourModSettings.GetValue<bool>("Magic-relatedRuleChanges", "IntelligenceRequirementForPurchasingSpells");
            Debug.Log("registered windows");    // TODO TODO
        }
        #endregion

        #region TimeAccelarator part

        static void InitTimeAcceleratorPart()
        {
            baseFixedDeltaTime = Time.fixedDeltaTime;
            baseTimeScale = Time.timeScale;
            Message("Base timescale = "+ baseTimeScale+"x.");
            GameManager.OnEncounter += OnEncounter;
        }

        private static void SetTimeScale(int timeScale)
        {
            // Must set fixed delta time to scale the fixed (physics) updates as well.
            Time.timeScale = timeScale * baseTimeScale;
            Time.fixedDeltaTime = timeScale * baseFixedDeltaTime; // Default is 0.02 or 50/s
        }

        void TimeAcceleratorUpdate()
        {       
            if (InputManager.Instance.GetKeyDown(KeyCode.KeypadPlus))
            {
                if (GameManager.Instance.AreEnemiesNearby())
                {
                    timeAcceleratorMultiple = 1;
                    SetTimeScale(timeAcceleratorMultiple);
                    Message("Enemies nearby. Running at " + timeAcceleratorMultiple + "x normal speed (" + Time.timeScale + "x timescale).");
                }
                else
                {
                    timeAcceleratorMultiple = (timeAcceleratorMultiple / 4) * 4 + 4;
                    SetTimeScale(timeAcceleratorMultiple);
                    Message("Plus pushed: running at " + timeAcceleratorMultiple + "x normal speed (" + Time.timeScale + "x timescale).");
                }
            }
                        // work in progress
            int currentDiseaseCount = GameManager.Instance.PlayerEffectManager.DiseaseCount;
            if (currentDiseaseCount != diseaseCount)
            {
                if (currentDiseaseCount > diseaseCount)
                {                    
                    SetTimeScale(1);
                    Message("New disease detected, slowing back down : running at " + timeAcceleratorMultiple + "x normal speed (" + Time.timeScale + "x timescale).");
                }
                diseaseCount = currentDiseaseCount;
            }

            if (InputManager.Instance.GetKeyDown(KeyCode.KeypadMinus) )
            {
                timeAcceleratorMultiple = 1;
                SetTimeScale(timeAcceleratorMultiple);
                Message("Minus pushed: running at " + timeAcceleratorMultiple + "x normal speed (" + Time.timeScale + "x timescale).");
            }

            if ((GameManager.Instance.AreEnemiesNearby()&&timeAcceleratorMultiple>1))
            {
                timeAcceleratorMultiple = 1;
                SetTimeScale(timeAcceleratorMultiple);
                Message("Enemies nearby. Running at " + timeAcceleratorMultiple + "x normal speed (" + Time.timeScale + "x timescale).");
            }
        }

        public static void OnEncounter()
        {
            SetTimeScale(1);
            Message("Enemy encounter, slowing back down : running at " + timeAcceleratorMultiple + "x normal speed (" + Time.timeScale + "x timescale).");
        }
        #endregion


    }
}