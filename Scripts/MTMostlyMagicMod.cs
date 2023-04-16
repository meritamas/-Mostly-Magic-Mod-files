// Project:         MeriTamas's (Mostly) Magic Mod for Daggerfall Unity (http://www.dfworkshop.net)
// Copyright:       Copyright (C) 2023 meritamas
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Author:          meritamas (meritamas@outlook.com)

/*
 * The code is sloppy at various places: some things are redundant, comments are missing, some comments are not useful anymore etc.
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
using DaggerfallWorkshop.Game.Utility.ModSupport;
using DaggerfallWorkshop.Game.Utility.ModSupport.ModSettings;
using DaggerfallWorkshop.Game.UserInterfaceWindows;
using DaggerfallWorkshop.Game.MagicAndEffects;
using DaggerfallWorkshop.Game.Serialization;
using DaggerfallWorkshop.Utility;
using DaggerfallConnect;
using DaggerfallConnect.Arena2;
using DaggerfallConnect.Utility;

/*  The modules:
 *  - Unleveled Enemies
 *  - Magic-Related Rule Changes
 *  - New Effects and Spells
 *  - Quality of Life (currently to raise timescale while running to travel+train at the same time)
 *  - Experimental
*/

namespace MTMMM
{
    #region SaveLoad Definitions    

    [FullSerializer.fsObject("v1")]
    public class MMMSaveData
    {        
        public SpellXPTally[] SpellXPTallies;
        public int AlchemyXPTally;
        public int EXPTally;          // E=Extra or Enchantment - will likely not need this for enchantment, but will keep this here just in case

        public Dictionary<uint, Dictionary<string, int>> SpellEffectConfidentialityLevels;
    }
    #endregion

    public class MTMostlyMagicMod : MonoBehaviour, IHasModSaveData
    {
        //static MMMSaveData savedData = new MMMSaveData();        
        static MTMostlyMagicMod instance;       // this was added to facilitate the SaveLoad function
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
        static public int savedDDL = -1;
        
        static public int mapXCoord = 0;
        static public int mapYCoord = 0;

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

        public void Awake()
        {
            RegisterOurWindows();            // eventually will need restructuring: other preconditions could prompt the necessity of registering our SpellBookWindow 

            if (ourModSettings.GetValue<bool>("EverydayNonMagic", "Earth-LikeSunLightDirections"))
            {
                //  GameManager.Instance.SunlightManager.Angle = -40f;      this code does not work ; but this is where the code that does the job should go 
            }

            if (ourModSettings.GetValue<bool>("EverydayNonMagic", "IntegrateMeanerMonsters"))
                MTMeanerMonsters.InitMod();         // this should initialize the Meaner Monsters part - integrated from Hazelnut&Ralzar's mod

            // this is to activate Jay_H's unleveledmobs functionality
            if (ourModSettings.GetValue<bool>("EverydayNonMagic", "IntegrateUnleveledMobs"))
            {
                MTUnleveledMobs.SetTables();
                MTUnleveledMobs.UpdateTables();
                SilentMessage("UnleveledEnemies part: Set up spawn tables");
            }
        }

        public void Start()
        {
            if (ourModSettings.GetValue<bool>("AndExtraStrongEnemies", "UnleveledAndExtraStrongEnemies-Main"))
                InitUnleveledEnemiesOnAwake();
        }

                // These are the things the mod should do after each 'update' (I think every ??? seconds or so)
        void Update()
        {
            if (ourModSettings.GetValue<bool>("QualityOfLife", "ExerciseFastTravel"))
                TimeAcceleratorUpdate();

            // MMMInputManager.OnModUpdate();          // this will ensure that MMMInputManager gets to check for relevant things each update cycle - THIS NOT NEEDED YET
        }

        public static void ElapseSeconds(int numberOfSecondsToPass)
        {
            DaggerfallDateTime now = DaggerfallUnity.Instance.WorldTime.Now;
            now.RaiseTime(numberOfSecondsToPass);
        }

        public static void ElapseMinutes(int numberOfMinutesToPass)
        {
            DaggerfallDateTime now = DaggerfallUnity.Instance.WorldTime.Now;
            now.RaiseTime(DaggerfallDateTime.SecondsPerMinute * numberOfMinutesToPass);
        }

        public static void ElapseHours(int numberOfHoursToPass)
        {
            DaggerfallDateTime now = DaggerfallUnity.Instance.WorldTime.Now;
            now.RaiseTime(DaggerfallDateTime.SecondsPerHour * numberOfHoursToPass);
        }

        #endregion

        #region ModInit common parts

        /// <summary>
        /// Does the stuff at game load
        /// </summary>
        [Invoke(StateManager.StateTypes.Game, 1)]
        public static void GiveInfoToPlayer(InitParams initParams)
        {
                    //GiveInfoToPlayer
            if (problem)
                Message("Problem adding Advanced Teleport", true, true, true, true);
            else
                SilentMessage("Override for Teleport (Recall) (Advanced Teleportation) successfully registered.");

            if (ourModSettings.GetValue<bool>("QualityOfLife", "ExerciseFastTravel"))
                InitTimeAcceleratorPart();            

            if (ourModSettings.GetValue<int>("EverydayMagic", "PlayerSpellMissileSpeed") == 1)
                            // TODO: check if condition correct
            {
                MMMXPTallies.SetMissileSpeedsInPrefabs();        // set the missile speeds once when the game is loaded
                GameManager.Instance.PlayerSpellCasting.OnReleaseFrame += MMMXPTallies.SetMissileSpeedsInPrefabs;
                                // register the routine to set the missile speeds after each player spellcast based on new experience data
            }
        }

        /// <summary>
        /// Init the class as a first step before work begins:
        /// - gets ModInstance and ModSettings
        /// - send appropriate message to debug system
        /// </summary>
        public static void StartInit(string modTitle)
        {
            Debug.Log("MT MMM: (Mostly) Magic Mod Init Started");
            modInstance = ModManager.Instance.GetMod(modTitle);
            ourModSettings = modInstance.GetSettings();
            MMMXPTallies.CalculationDebugToPlayer = true;
        }        

        /// <summary>
        /// Initialize the Unleveled Enemies parts
        /// </summary>
        public static void InitUnleveledEnemiesOnStart()
        {
            GameObject go = DaggerfallUnity.Instance.Option_EnemyPrefab.transform.gameObject;
            go.AddComponent<MTMMM.MTRecalcStats>();

            // GameObject go2 = new GameObject(modInstance.Title);      // part added to general init in order to init save/load features
            // go2.AddComponent<MTMostlyMagicMod>();   // initializing the Unleveled and Extra Strong enemies part

            SilentMessage("Enemy prefab changed successfully");            

            SilentMessage("InitUnleveledEnemies module init method Finished");
        }        

        /// <summary>
        /// finish initialization:
        /// - turn the ready flag true
        /// - send appropriate message to debug system
        /// </summary>
        public static void EndInit()
        {
            modInstance.IsReady = true;         // set the mod's IsReady flag to true
            SilentMessage("(Mostly) Magic Mod Init Finished");
        }

        /// <summary>
        /// The main class init method. Initializes mod features via the helpers defined above.
        /// </summary>
        [Invoke(StateManager.StateTypes.Start, 0)]
        public static void Init(InitParams initParams)
        {
            StartInit(initParams.ModTitle);        // init the class as a first step before work begins

            GameObject go2 = new GameObject(modInstance.Title);
            instance = go2.AddComponent<MTMostlyMagicMod>();
            modInstance.SaveDataInterface = instance;               // this part is responsible for initializing the Save/Load feature      

            InitNewSpellsAndEffects();

            if (ourModSettings.GetValue<bool>("UnleveledAndExtraStrongEnemies", "UnleveledAndExtraStrongEnemies-Main"))
                InitUnleveledEnemiesOnStart();
            MTRecalcStats.extraStrongMonsters = ourModSettings.GetValue<bool>("UnleveledAndExtraStrongEnemies", "ExtraStrongMonsters");
            SilentMessage("Extra Strong Monsters Enabled = " + MTRecalcStats.extraStrongMonsters);
            MTRecalcStats.enemySpellMissileSpeeds = ourModSettings.GetValue<int>("UnleveledAndExtraStrongEnemies", "EnemySpellMissileSpeed");
            SilentMessage("Enemy Spell Missile Speed Choice = " + MTRecalcStats.enemySpellMissileSpeeds);

            InitEverydayMagic();

            if (ourModSettings.GetValue<bool>("EverydayNonMagic", "Roads"))
            {
                ConsoleCommandsDatabase.RegisterCommand("MT-GetRegionName", MMMConsoleCommands.MTGetRegionName);
                ConsoleCommandsDatabase.RegisterCommand("MT-GetRegionIndex", MMMConsoleCommands.MTGetRegionIndex);
                ConsoleCommandsDatabase.RegisterCommand("MT-AboutLocation", MMMConsoleCommands.MTAboutLocation);
            }

            EndInit();          // Finish Mod Initialization            
        }
        #endregion

        #region Spells and Effects
        /// <summary>
        /// Initialize the New Spells and Effects parts
        /// </summary>
        public static void InitNewSpellsAndEffects()
        {
            if (ourModSettings.GetValue<bool>("EffectsAndSpells", "MultipleAnchorsForRecall"))
            {
                MTTeleport.DispelDoesNotRemoveAnchors = ourModSettings.GetValue<bool>("EffectsAndSpells", "DispelDoesNotEraseAnchors");                

                BaseEntityEffect ourTeleportEffect = new MTTeleport();

                problem = !GameManager.Instance.EntityEffectBroker.RegisterEffectTemplate(ourTeleportEffect, true);

                if (problem)
                    SilentMessage("MTTeleport effect could not be registered.");
                else
                    SilentMessage("MTTeleport effect successfully registered.");

                ConsoleCommandsDatabase.RegisterCommand("advanced-teleport-kill", MMMConsoleCommands.SaveOrNotToSaveLegacyAdvancedTeleportation);
            }                                 

            if (ourModSettings.GetValue<bool>("EffectsAndSpells", "FixItem"))
                RegisterFixItem();

            if (ourModSettings.GetValue<bool>("EffectsAndSpells", "RebalanceSlowFallToThaumaturgy"))
                RebalanceSlowfall();

            if (ourModSettings.GetValue<bool>("EffectsAndSpells", "ConjurationCreation"))
                RegisterConjurationCreationEffectsAndSpells();


            if (ourModSettings.GetValue<bool>("EffectsAndSpells", "ClimatesCalories"))
                RegisterClimatesCaloriesEffectsAndSpells(); // We'll test for C&C where we need to and prepare the code for the case C&C is not present 

        }

        public static void RebalanceSlowfall()
        {
            MTSlowfall templateEffect = new MTSlowfall();
            if (!GameManager.Instance.EntityEffectBroker.RegisterEffectTemplate(templateEffect, true))
            {
                Message("Slowfall effect could not be rebalanced to Thaumaturgy");                
            }
            else
            {
                SilentMessage("Slowfall effect successfully rebalanced to Thaumaturgy");
            }
        }

        
        public static void RegisterFixItem()           
        {
            // First register custom effect with broker
            // This will make it available to crafting stations supported by effect

            MTRepairItem templateEffect = new MTRepairItem();
            // templateEffect.CurrentVariant = 0;
            if (!GameManager.Instance.EntityEffectBroker.RegisterEffectTemplate(templateEffect))
            {
                Message("MTRepairItem effect could not be registered");
                return;
            }

            Message("MTRepairItem effect successfully registered");

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

            EffectSettings effectSettings2 = new EffectSettings()   {   MagnitudeBaseMin = 1, MagnitudeBaseMax = 1, MagnitudePlusMin = 2, MagnitudePlusMax = 2, MagnitudePerLevel = 1,      };
           
            EffectEntry effectEntry2 = new EffectEntry()            {   Key = templateEffect.Properties.Key,         Settings = effectSettings2,          };
                        
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
            
            EntityEffectBroker.CustomSpellBundleOffer repairOffer = new EntityEffectBroker.CustomSpellBundleOffer()
            {
                Key = "Reparo-CustomOffer",                           
                Usage = EntityEffectBroker.CustomSpellBundleOfferUsage.SpellsForSale, 
                                                                                     
                BundleSetttings = repairSpell               
            };
           
            GameManager.Instance.EntityEffectBroker.RegisterCustomSpellBundleOffer(repairOffer);            
        }


        public static void RegisterConjurationCreationEffectsAndSpells()
        {
                    // first, registering the override to DFU's CreateItem, our MTSummonSimpleItem - a Conjuration effect that will create permanent items, but won't always succeed
            BaseEntityEffect registeredEffect = new MTSummonSimpleItem();

            problem = !GameManager.Instance.EntityEffectBroker.RegisterEffectTemplate(registeredEffect, true);

            if (problem)
                Message("MTSummonSimpleItem effect could not be registered.");
            else
                SilentMessage("MTSummonSimpleItem effect successfully registered.");

                    // now, to register MTMultiplyProvisions - a simple Conjuration effect that will based on player choice multiply either the water in your waterskin (refill), your rations (regenerate food) or arrows (summon) 

            registeredEffect = new MTMultiplyProvisions();

            problem = !GameManager.Instance.EntityEffectBroker.RegisterEffectTemplate(registeredEffect, true);

            if (problem)
                Message("MTMultiplyProvisions effect could not be registered.");
            else
                SilentMessage("MTMultiplyProvisions effect successfully registered.");

                    // now, registering Abundantia, an MTMultiplyProvisions spell
            EffectSettings effectSettings = new EffectSettings() { MagnitudeBaseMin = 1, MagnitudeBaseMax = 1, MagnitudePlusMin = 1, MagnitudePlusMax = 1, MagnitudePerLevel = 1, };
            EffectEntry effectEntry = new EffectEntry() { Key = registeredEffect.Properties.Key, Settings = effectSettings, };
            EffectBundleSettings effectBundleSettings = new EffectBundleSettings()
            {
                Version = EntityEffectBroker.CurrentSpellVersion,
                BundleType = BundleTypes.Spell,
                TargetType = TargetTypes.CasterOnly,
                ElementType = ElementTypes.Magic,
                Name = "Abundantia",
                IconIndex = 2,
                Effects = new EffectEntry[] { effectEntry },
            };
            EntityEffectBroker.CustomSpellBundleOffer registeredOffer = new EntityEffectBroker.CustomSpellBundleOffer()
            {
                Key = "Abundantia-CustomOffer",
                Usage = EntityEffectBroker.CustomSpellBundleOfferUsage.SpellsForSale,
                BundleSetttings = effectBundleSettings
            };
            GameManager.Instance.EntityEffectBroker.RegisterCustomSpellBundleOffer(registeredOffer);
        }

        public static void RegisterClimatesCaloriesEffectsAndSpells()
        {
                    // first registering the MTUmbrella effect 
            BaseEntityEffect ourRegisteredEffect = new MTUmbrella();

            problem = !GameManager.Instance.EntityEffectBroker.RegisterEffectTemplate(ourRegisteredEffect, true);

            if (problem)
                Message("MTUmbrella effect could not be registered.");
            else
                SilentMessage("MTUmbrella effect successfully registered.");

                    // now, registering two standardized MTUmbrella spells: Brevi Tempore Sine Pluvia and Sine Pluvia
                    // first, Brevi Tempore Sine Pluvia
            EffectSettings effectSettings = new EffectSettings()  { DurationBase = 2, DurationPlus = 1, DurationPerLevel = 1, };
            EffectEntry effectEntry = new EffectEntry()  {   Key = ourRegisteredEffect.Properties.Key, Settings = effectSettings, };
            EffectBundleSettings effectBundleSettings = new EffectBundleSettings()            {
                Version = EntityEffectBroker.CurrentSpellVersion,
                BundleType = BundleTypes.Spell,
                TargetType = TargetTypes.CasterOnly,
                ElementType = ElementTypes.Magic,
                Name = "Brevi Sine Pluvia",
                IconIndex = 2,
                Effects = new EffectEntry[] { effectEntry },            };
            EntityEffectBroker.CustomSpellBundleOffer registeredOffer = new EntityEffectBroker.CustomSpellBundleOffer()            {
                Key = "BreviTemporeSinePluvia-CustomOffer",                           
                Usage = EntityEffectBroker.CustomSpellBundleOfferUsage.SpellsForSale,                                                                                       
                BundleSetttings = effectBundleSettings                    };
            GameManager.Instance.EntityEffectBroker.RegisterCustomSpellBundleOffer(registeredOffer);
            
                    // second  Sine Pluvia
            effectSettings = new EffectSettings() { DurationBase = 1, DurationPlus = 3, DurationPerLevel = 1, };
            effectEntry = new EffectEntry() { Key = ourRegisteredEffect.Properties.Key, Settings = effectSettings, };
            EffectBundleSettings umbrellaSpell = new EffectBundleSettings()            {
                Version = EntityEffectBroker.CurrentSpellVersion,
                BundleType = BundleTypes.Spell,
                TargetType = TargetTypes.CasterOnly,
                ElementType = ElementTypes.Magic,
                Name = "Sine Pluvia",
                IconIndex = 2,
                Effects = new EffectEntry[] { effectEntry },            };
            EntityEffectBroker.CustomSpellBundleOffer umbrellaOffer = new EntityEffectBroker.CustomSpellBundleOffer()            {
                Key = "SinePluvia-CustomOffer",                           
                Usage = EntityEffectBroker.CustomSpellBundleOfferUsage.SpellsForSale, 
                BundleSetttings = umbrellaSpell,             };
            GameManager.Instance.EntityEffectBroker.RegisterCustomSpellBundleOffer(umbrellaOffer);

                        // now, registering the MTDryClothes effect 
            ourRegisteredEffect = new MTDryClothes();

            problem = !GameManager.Instance.EntityEffectBroker.RegisterEffectTemplate(ourRegisteredEffect, true);

            if (problem)
                Message("MTDryClothes effect could not be registered.");
            else
                SilentMessage("MTDryClothes effect successfully registered.");

                    // now, registering a modest 'cloak-drier' spell, Ariditas
            effectSettings = new EffectSettings() { MagnitudeBaseMin = 10, MagnitudeBaseMax = 10, MagnitudePlusMin = 4, MagnitudePlusMax = 6, MagnitudePerLevel = 1, };
            effectEntry = new EffectEntry() { Key = ourRegisteredEffect.Properties.Key, Settings = effectSettings, };
            effectBundleSettings = new EffectBundleSettings()
            {
                Version = EntityEffectBroker.CurrentSpellVersion,
                BundleType = BundleTypes.Spell,
                TargetType = TargetTypes.CasterOnly,
                ElementType = ElementTypes.Magic,
                Name = "Ariditas",
                IconIndex = 2,
                Effects = new EffectEntry[] { effectEntry },
            };
            registeredOffer = new EntityEffectBroker.CustomSpellBundleOffer()
            {
                Key = "Ariditas-CustomOffer",
                Usage = EntityEffectBroker.CustomSpellBundleOfferUsage.SpellsForSale,
                BundleSetttings = effectBundleSettings
            };
            GameManager.Instance.EntityEffectBroker.RegisterCustomSpellBundleOffer(registeredOffer);

            // TODO: code to register RepelWater
        }

        
        #endregion

        #region Unleveled and Extra Strong Enemies
            // some accelarator code too
        void InitUnleveledEnemiesOnAwake()              // MTMT TODO 
        {
            PlayerEnterExit.OnPreTransition += SetDungeon_OnPreTransition;
            PlayerEnterExit.OnTransitionExterior += ClearData_OnTransitionExterior;     // TODO: re-eval if things can be done better
            PlayerGPS.OnMapPixelChanged += SetWildernessDifficultyStuff_OnMapPixelChanged;
            EnemyDeath.OnEnemyDeath += RemoveMMMObjects_OnEnemyDeath;
        }
                // what this seems to do is generate the dungeon difficulty level when the player actually enters the dungeon (prior to the transition to the dungeon)
                // potential problem: this might not be needed before all transitions           // TODO: evaluate and possibly correct this
        private static void SetDungeon_OnPreTransition(PlayerEnterExit.TransitionEventArgs args)
        {
            region = GameManager.Instance.PlayerGPS.CurrentRegionIndex;
            dungeon = (int)GameManager.Instance.PlayerGPS.CurrentLocation.MapTableData.DungeonType;
            savedDDL = determineDungeonDifficulty();            
        }

        private static void ClearData_OnTransitionExterior(PlayerEnterExit.TransitionEventArgs args)
        {
            dungeon = -1;
            savedDDL = -1;
        }        

        private static void SetWildernessDifficultyStuff_OnMapPixelChanged(DFPosition mapPixel)
        {
            mapXCoord = mapPixel.X;
            mapYCoord = mapPixel.Y;            
            SilentMessage(string.Format("MT MMM: SetWildernessDifficultyStuff_OnMapPixelChanged: Current map pixel is now [{0},{1}].", mapXCoord, mapYCoord));
        }

        public static int determineDungeonDifficulty()
        {
            int[] modifierTable = { 0, 1, 2, 1, 0, -1, -2, -1 };

            int modifierBasedOnHash = 0;
            // this should be a modifier from -2 to +2 that is contingent on a hash from the name (or coordinates) of the dungeon and the time (like which quarter it is)


            int currentYear = DaggerfallUnity.Instance.WorldTime.Now.Year;
            int currentMonth = DaggerfallUnity.Instance.WorldTime.Now.Month;
            int X = GameManager.Instance.PlayerGPS.CurrentMapPixel.X;           // could possibly use mapXCoord:mapYCoord instead
            int Y = GameManager.Instance.PlayerGPS.CurrentMapPixel.Y;           // could possibly use mapXCoord:mapYCoord instead

            int sequence = (X + Y + currentYear * 4 + currentMonth / 3) % 8;
            modifierBasedOnHash = modifierTable[sequence];
            // idea: fluctuating strength based on (X coord + Y coord + year + number) % 8

            int DQL = 0;

            switch (dungeon)
            {

                case (int)DFRegion.DungeonTypes.DragonsDen:
                case (int)DFRegion.DungeonTypes.VampireHaunt:                
                    DQL = 21 + modifierBasedOnHash;
                    break;

                case (int)DFRegion.DungeonTypes.VolcanicCaves:
                case (int)DFRegion.DungeonTypes.Coven:
                case (int)DFRegion.DungeonTypes.DesecratedTemple:
                case (int)DFRegion.DungeonTypes.Crypt:
                    DQL = 18 + modifierBasedOnHash;
                    break;

                case (int)DFRegion.DungeonTypes.OrcStronghold:
                case (int)DFRegion.DungeonTypes.BarbarianStronghold:
                case (int)DFRegion.DungeonTypes.HumanStronghold:
                case (int)DFRegion.DungeonTypes.Laboratory:
                    DQL = 15 + modifierBasedOnHash;
                    break;

                case (int)DFRegion.DungeonTypes.Cemetery:
                case (int)DFRegion.DungeonTypes.GiantStronghold:
                case (int)DFRegion.DungeonTypes.Prison:
                    DQL = 12 + modifierBasedOnHash;
                    break;

                case (int)DFRegion.DungeonTypes.RuinedCastle:
                case (int)DFRegion.DungeonTypes.HarpyNest:
                    DQL = 8 + modifierBasedOnHash;
                    break;
                
                case (int)DFRegion.DungeonTypes.NaturalCave:            // MT added, missing from Ralzar mod
                case (int)DFRegion.DungeonTypes.ScorpionNest:
                case (int)DFRegion.DungeonTypes.SpiderNest:
                case (int)DFRegion.DungeonTypes.Mine:
                    DQL = 5 + modifierBasedOnHash;
                    break;

                    SilentMessage("determineDungeonDifficulty: Possible erroneous dungeon type."); // MT: code should not reach this point
                    DQL = 8 + modifierBasedOnHash;
                    break;
            }            

            SilentMessage("Determining Dungeon Difficulty; Year=" + currentYear + " Month=" + currentMonth + " X=" + X + " Y=" + Y + " sequence=" + sequence + " DQModifier=" + modifierBasedOnHash + " DQ=" + DQL);
            return DQL;
        }

        public static int dungeonDifficulty()
        {
            return savedDDL;
        }

        public static int RemoveMMMObjectContained (ItemCollection itemCollection)
        {            
            List<DaggerfallUnityItem> itemsToRemove = new List<DaggerfallUnityItem>();

            if (itemCollection != null)
            {                
                int numberOfItems = itemCollection.Count;
                if (numberOfItems > 0)
                {
                    for (int i = 0; i < numberOfItems; i++)
                    {
                        DaggerfallUnityItem item = itemCollection.GetItem(i);
                        if (item.shortName.Substring(0, 3) == "MMM")                        
                            itemsToRemove.Add(item);
                        if (item.shortName.Substring(0, 5) == "BLMMM")
                            itemsToRemove.Add(item);
                    }
                }
            }

            foreach (DaggerfallUnityItem item in itemsToRemove)
            {
                itemCollection.RemoveItem(item);
            }

            SilentMessage("RemoveMMMObjectContained: removed "+ itemsToRemove.Count+" MMM items from passed ItemCollection");

            return itemsToRemove.Count;
        }

        public static void RemoveMMMObjects_OnEnemyDeath (object sender, EventArgs e)
        {
            SilentMessage("RemoveMMMObjects_OnEnemyDeath has been called.");
            EnemyDeath enemyDeath = sender as EnemyDeath;
            if (enemyDeath != null)
            {
                DaggerfallEntityBehaviour entityBehaviour = enemyDeath.GetComponent<DaggerfallEntityBehaviour>();
                if (entityBehaviour != null)
                {
                    EnemyEntity enemyEntity = entityBehaviour.Entity as EnemyEntity;
                    if (enemyEntity != null)
                    {
                        RemoveMMMObjectContained(entityBehaviour.CorpseLootContainer.Items);
                    }
                }
            }
        }

        /// <summary>
        /// Gives the player info on whether init was successful.
        /// </summary>
        [Invoke(StateManager.StateTypes.Game, 0)]
        public static void ReportToUser(InitParams initParams)                     
        {
                    // display greeting message to player, if any is needed   
        }
        #endregion

        #region Everyday Magic
        /// <summary>
        /// Initialize the Magic-related Rule Changes parts
        /// </summary>
        public static void InitEverydayMagic()
        {
                                // CONFIDENTIALITY LEVELS
            MTSpellBookWindow.confidentialityLevelsApplied = ourModSettings.GetValue<bool>("EverydayMagic", "EffectConfidentialityLevels");
            MMMEffectAndSpellHandler.applyConfidentialityLevels = ourModSettings.GetValue<bool>("EverydayMagic", "EffectConfidentialityLevels");

                                // SPELL INSTRUCTOR THEMES
            MMMEffectAndSpellHandler.applyThemes = ourModSettings.GetValue<bool>("EverydayMagic", "SpellInstructorThemes");

                                // SPELL LEARNING
            MTSpellBookWindow.spellLearning = ourModSettings.GetValue<bool>("EverydayMagic", "SpellLearning");   // this setting takes care of how our windows behave
                    // TODO: apply setting to SpellMaker Window

            MMMFormulaHelper.spellMakerCoefficientFromSettings = ourModSettings.GetValue<float>("EverydayMagic", "SpellCreationTimeCoefficient");
                        // the routine it effects is not called if spell learning turned off
            SilentMessage("Spell Learning parameters. SL on: {0}, SpellCreationTimeCoefficient: {1}", MTSpellBookWindow.spellLearning, MMMFormulaHelper.spellMakerCoefficientFromSettings);                    

                                // XP TALLIES
            MMMFormulaHelper.ApplyExperienceTallies = ourModSettings.GetValue<bool>("EverydayMagic", "ExperienceTallies");
            if (MMMFormulaHelper.ApplyExperienceTallies)
            {               
                ConsoleCommandsDatabase.RegisterCommand("IncrementSpellXPTally", MMMConsoleCommands.IncrementSpellXPTally);
                // registering command to manipulate spell effect tallies by the console

                GameManager.Instance.PlayerSpellCasting.OnReleaseFrame += MMMXPTallies.PlayerSpellCasting_OnReleaseFrame;
            }       // XP tallies mod setting should be okay now: if not, then we omit registering the routine when spells are finished
                    //  and have MMMFormulaHelper ignore any tallies that might have been saved in the past - by returning 1.0f as the XPTally coefficient,
                    //                                              and by bypassing any related code just to make sure we don't end up hanging the game

                                // STRONG SPELLS ADVANCE MAGIC SKILLS MORE     
            MMMXPTallies.strongSpellsAdvanceMagicSkillsMore = ourModSettings.GetValue<bool>("EverydayMagic", "StrongSpellsAdvanceMagicSkillsMore");

                                // DIVERSIFIED SPELL EXPERIENCE REQUIRED FOR MAGIC SKILL ADVANCEMENT
            MMMXPTallies.diversifiedSpellExperienceRequiredForMagicSkillAdvancement = ourModSettings.GetValue<bool>("EverydayMagic", "DiversifiedSpellExperienceRequiredForMagicSkillAdvancement");            

                                // UNLEVELED SPELLS
            if (ourModSettings.GetValue<bool>("EverydayMagic", "UnleveledSpellsAndPotions"))
            {                
                MMMFormulaHelper.SendInfoMessagesOnPCSpellLevel = true;     // temporary solution, intention: save these to player log
                MMMFormulaHelper.SendInfoMessagesOnNonPCSpellLevel = true;  // temporary solution, intention: save these to player log and send message to hud

                SilentMessage("Player-cast levels saved to player.log, non-player cast spells saved to player.log+message to HUD");
                        // verbosity features set up

                FormulaHelper.RegisterOverride(modInstance, "CalculateCasterLevel", (Func<DaggerfallEntity, IEntityEffect, int>)MMMFormulaHelper.GetSpellLevelForGame);
            }

                                // MINIMUM SPELLPOINT COST
            MMMFormulaHelper.castCostFloor = ourModSettings.GetValue<int>("EverydayMagic", "MinimumSpellPointCost");     // if player has set a minimum that is different to 5
            if (MMMFormulaHelper.castCostFloor != 5)
            {                
                FormulaHelper.RegisterOverride(modInstance, "CalculateTotalEffectCosts", (Func<EffectEntry[], TargetTypes, DaggerfallEntity, bool, FormulaHelper.SpellCost>)MMMFormulaHelper.CalculateTotalEffectCosts);
                SilentMessage("Minimum spell cost set to: "+ MMMFormulaHelper.castCostFloor);
                    // consider registering the replacement for CalculateEffectCosts here
            }

                            // MAX LENGTH OF DISPLAYED SPELL NAME STRING
            MTSpellBookWindow.maxLengthOfDisplayedSpellNameString = ourModSettings.GetValue<int>("EverydayMagic", "MaxLengthOfDisplayedSpellNameString");            

                                // DISPLAY EFFECIVE MAGIC SKILL FOR SIMPLE SPELLS IN SPELLBOOK WINDOW
            MTSpellBookWindow.displayEffectiveMagicSkill = ourModSettings.GetValue<bool>("EverydayMagic", "DisplayEffectiveMagicSkill");

                            // GUILD BASE SPELL FEE COEFFICIENT
            MTSpellBookWindow.baseGuildFeeMultiplier = ourModSettings.GetValue<float>("EverydayMagic", "GuildBaseSpellFeeCoefficient");
            MTSpellMakerWindow.baseGuildFeeMultiplier = ourModSettings.GetValue<float>("EverydayMagic", "GuildBaseSpellFeeCoefficient");

                            // SPELL CREATION TIME COEFFICIENT
            MTSpellMakerWindow.spellCreationTimeCoefficient = ourModSettings.GetValue<float>("EverydayMagic", "SpellCreationTimeCoefficient");
            MMMFormulaHelper.spellMakerCoefficientFromSettings = ourModSettings.GetValue<float>("EverydayMagic", "SpellCreationTimeCoefficient");

                            // PLAYER SPELL MISSILE SPEED
            MMMFormulaHelper.SpellMissileSpeedRegime = ourModSettings.GetValue<int>("EverydayMagic", "PlayerSpellMissileSpeed");
                            // the changes are effectuated when game loaded and then as spells are cast - see at init procedure for game state 1 and code in MMMFormulaHelper and MMMXPTallies      

            if (true)           // TODO but can wait : a condition that would be true if the base game has been edited to use my skill&attribute system
            {
                FormulaHelper.RegisterOverride(modInstance, "CalculateEffectCosts", (Func<IEntityEffect, EffectSettings, DaggerfallEntity, FormulaHelper.SpellCost>)MMMFormulaHelper.CalculateEffectCosts);
                SilentMessage("Minimum effect cost override routine registered - effect cost decrease for magic skill levels above 95 will now be slower.");
            }


        }

        public static void RegisterOurWindows()
        {
            if (ourModSettings.GetValue<bool>("EverydayMagic", "SpellLearning") || ourModSettings.GetValue<bool>("EverydayMagic", "EffectConfidentialityLevels"))
            {
                UIWindowFactory.RegisterCustomUIWindow(UIWindowType.SpellBook, typeof(MTSpellBookWindow));
                UIWindowFactory.RegisterCustomUIWindow(UIWindowType.SpellMaker, typeof(MTSpellMakerWindow));
                SilentMessage("registered windows");
            }
            else
                SilentMessage("No need to register MTSpellBookWindow or MTSpellMakerWindow");

            if (ourModSettings.GetValue<bool>("EverydayMagic", "UnleveledSpellsAndPotions"))  // if spells unleveled, need to tackle potions
            {
                UIWindowFactory.RegisterCustomUIWindow(UIWindowType.Inventory, typeof(MTInventoryWindow));
            }
            else
                SilentMessage("No need to register MTInventoryWindow");
        }
        #endregion

        #region TimeAccelarator

        static void InitTimeAcceleratorPart()
        {
            baseFixedDeltaTime = Time.fixedDeltaTime;
            baseTimeScale = Time.timeScale;
            SilentMessage("Initializing Time Accelerator Module. Base timescale = "+ baseTimeScale+"x.");
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
            // SilentMessage("We are updating the time accelerator module.");   // this should not be needed any more

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
                if ((currentDiseaseCount > diseaseCount) && (timeAcceleratorMultiple>1))
                {
                    timeAcceleratorMultiple = 1;
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

            if ((GameManager.Instance.AreEnemiesNearby())&&(timeAcceleratorMultiple>1))
            {
                timeAcceleratorMultiple = 1;
                SetTimeScale(timeAcceleratorMultiple);
                Message("Enemies nearby. Running at " + timeAcceleratorMultiple + "x normal speed (" + Time.timeScale + "x timescale).");
            }
        }

        public static void OnEncounter()
        {
            if (timeAcceleratorMultiple > 1)
            {
                timeAcceleratorMultiple = 1;
                SetTimeScale(1);
                Message("Enemy encounter, slowing back down : running at " + timeAcceleratorMultiple + "x normal speed (" + Time.timeScale + "x timescale).");
            }
        }
        #endregion

        #region SaveLoad Methods        

        public Type SaveDataType
        {
            get { return typeof(MMMSaveData); }
        }

        public object NewSaveData()
        {
            MMMSaveData dataToReturn = new MMMSaveData      // TODO: REVISE
            {
                SpellXPTallies = new SpellXPTally[0],
                SpellEffectConfidentialityLevels = new Dictionary<uint, Dictionary<string, int>>()
            };            
            return dataToReturn;
        }

        public object GetSaveData()
        {
            MMMSaveData savedData = new MMMSaveData
            {
                SpellXPTallies = MMMXPTallies.GetSpellTallyArray(),
                AlchemyXPTally = MMMXPTallies.AlchemyXPTally,
                EXPTally = MMMXPTallies.EXPTally,

                SpellEffectConfidentialityLevels = MMMEffectAndSpellHandler.GetDictionaries()
            };
            return savedData;
        }

        public void RestoreSaveData(object saveData)
        {
            MMMSaveData loadedSaveData = (MMMSaveData)saveData;
            MMMXPTallies.SetSpellTalliesFromArray(loadedSaveData.SpellXPTallies);
            MMMXPTallies.AlchemyXPTally = loadedSaveData.AlchemyXPTally;
            MMMXPTallies.EXPTally = loadedSaveData.EXPTally;

            MMMEffectAndSpellHandler.SetDictionaries(loadedSaveData.SpellEffectConfidentialityLevels);
        }

        #endregion

        #region Debug Methods
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
        public static void SilentMessage(string message)
        {
            Message(message, false);
        }

        public static void SilentMessage(string message, params object[] args)
        {
            SilentMessage(string.Format(message, args));
        }

        /// <summary>
        /// Method to send messages, depending on settings either to HUD or the Player, or both or neither
        /// </summary>
        public static void Message(string message, bool toHud = true, bool toPlayer = true, bool forceToHUD = false, bool forceToPlayer = false, bool prefixMessage = true)
        {
            string msg = message;
            if (prefixMessage)
                msg = "MT MMM: " + msg;

            if (forceToHUD || (toHud && ourModSettings.GetValue<bool>("DebugAndExperimental", "DebugToHUD")))
                DisplayMessageOnScreen(message);

            if (forceToPlayer || (toPlayer && ourModSettings.GetValue<bool>("DebugAndExperimental", "DebugToLog")))
                Debug.Log(msg);
        }
        #endregion
    }
}
 