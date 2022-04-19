// Project:         MeriTamas's (Mostly) Magic Mod for Daggerfall Unity (http://www.dfworkshop.net)
// Copyright:       Copyright (C) 2022 meritamas
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Author:          meritamas (meritamas@outlook.com)

/*
 * The code is sloppy at various places - this is partly due to the fact that this mod was created by merging three smaller mods.
 * Some things are redundant, comments are missing, some comments are not useful anymore etc.
 * I have the intention of cleaning it up in the future.
 * For now, it seems to work as intended or - let's rather say - reasonably well.
*/

// Class is based on the Teleport class which is part of the Daggerfall Tools For Unity project - many parts retained unchanged
// Copyright:       Copyright (C) 2009-2020 Daggerfall Workshop
// Web Site:        http://www.dfworkshop.net
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Source Code:     https://github.com/Interkarma/daggerfall-unity
// Original Author: Gavin Clayton (interkarma@dfworkshop.net)


using UnityEngine;
using DaggerfallConnect;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.Serialization;
using DaggerfallWorkshop.Game.UserInterfaceWindows;
using DaggerfallWorkshop.Game.MagicAndEffects;
using DaggerfallWorkshop.Game.MagicAndEffects.MagicEffects;
using DaggerfallWorkshop.Game.Utility;
using DaggerfallWorkshop.Game.UserInterface;
using FullSerializer;

namespace MTMMM
{
    /// <summary>
    /// Teleport
    /// </summary>
    public class MTTeleport : IncumbentEffect
    {
        static string messagePrefix = "MTTeleport: ";
        public static readonly string EffectKey = "Teleport-Effect";
        public static readonly string WhatToDoMessageText = "As you feel magic powers flow through your body, you direct them to...";
        public static readonly string AnchorCreationFailedText = "Despite your best efforts, the mystical connection between you and this place does not feel stable and strong enough.";
        public static readonly string PleaseNameYourAnchorText = "You sense that there now exists a stable mysterious connection between you and this place. You will invoke this connection by focusing on ";
        public static readonly string ShortPleaseNameYourAnchorText = "You feel a mysterious connection between you and this place: ";
        public static readonly string TeleportFailedText = "As you feel the magic powers of the spell leave your body, you notice that you are still where you were.";
        public static readonly string TeleportSuccessfulText = "As you feel the magic powers that propelled you through space leave your body, you need a few seconds to adjust.";
        public static readonly string TeleportToAnotherDestinationExtraText = "You have a faint memory of losing focus just before leaving. 'Is this the place I wanted to go?'";
        public static readonly string TeleportedIntoABuildingText = "Please exit and re-enter this building to find the place as it should be.";

        public static readonly string AdvancedTeleportationEffectErasePending = "Flagged the advanced teleportation effect for erase. Should be gone in a couple of seconds.";

        #region Fields

        // Constants
        const int maximumNumberOfAnchors = 128;         // 128 is arbitrary but should be enough
        const int teleportOrSetAnchor = 4000;
        const int achorMustBeSet = 4001;        

        // Effect data to serialize
        int numberOfAnchorsSet = 0;
        PlayerPositionData_v1[] anchorPositions = new PlayerPositionData_v1[maximumNumberOfAnchors];
        string[] anchorNames = new string [maximumNumberOfAnchors];
        int forcedRoundsRemaining = 1;

        // Volatile references
        SerializablePlayer serializablePlayer = null;
        PlayerEnterExit playerEnterExit = null;

        int indexWhereWeAreTeleporting = 0; // used to inform the ending routine PlayerEnterExit_OnRespawnerComplete
        DaggerfallListPickerWindow pickAnchor;      // used to manage anchor picking
        int indexOfAnchorWeAreSetting = 0;
        #endregion

        #region Overrides

        public override void SetProperties()
        {
            properties.Key = EffectKey;
            properties.ClassicKey = MakeClassicKey(43, 255);            
            properties.AllowedTargets = EntityEffectBroker.TargetFlags_Self;
            properties.AllowedElements = EntityEffectBroker.ElementFlags_MagicOnly;
            properties.AllowedCraftingStations = MagicCraftingStations.SpellMaker;
            properties.ShowSpellIcon = false;
            properties.MagicSkill = DFCareer.MagicSkills.Mysticism;
        }

        protected override int RemoveRound()
        {
            return forcedRoundsRemaining;
        }

        public override int RoundsRemaining
        {
            get { return forcedRoundsRemaining; }
        }

        protected override bool IsLikeKind(IncumbentEffect other)
        {
            return (other is MTTeleport);
        }

        public override void Start(EntityEffectManager manager, DaggerfallEntityBehaviour caster = null)
        {
            base.Start(manager, caster);
            CacheReferences();
        }

        public override void Resume(EntityEffectManager.EffectSaveData_v1 effectData, EntityEffectManager manager, DaggerfallEntityBehaviour caster = null)
        {
            base.Resume(effectData, manager, caster);
            CacheReferences();
        }

        protected override void BecomeIncumbent()
        {
            base.BecomeIncumbent();
            PromptPlayer();
        }

        protected override void AddState(IncumbentEffect incumbent)
        {
            // Prompt from incumbent as it has the position data for teleport
            (incumbent as MTTeleport).PromptPlayer();
        }

        public override void End()
        {
            anchorPositions = null;     // TODO: possibly later some code to kill the array elements first
            base.End();
        }

        public void Kill()
        {
            for (int i=0; i<maximumNumberOfAnchors; i++)
                if (anchorPositions[i]!=null)
                {
                    anchorPositions[i] = null;
                    anchorNames[i] = null;
                    numberOfAnchorsSet--;
                }

            forcedRoundsRemaining = 0;
            ResignAsIncumbent();
        }

        #endregion

        #region Debug Methods
        static void Message(string message)
        {
            MTMostlyMagicMod.Message(messagePrefix + message);
        }

        static void SilentMessage(string message)
        {
            MTMostlyMagicMod.SilentMessage(messagePrefix + message);
        }
        #endregion

        #region Private Methods

        void PromptPlayer()
        {
            // Get peered entity gameobject
            DaggerfallEntityBehaviour entityBehaviour = GetPeeredEntityBehaviour(manager);
            if (!entityBehaviour)
                return;

            // Target must be player - no effect on other entities
            if (entityBehaviour != GameManager.Instance.PlayerEntityBehaviour)
                return;

            if (MTMostlyMagicMod.shouldEraseAdvancedTeleportEffect)
            {
                Kill();
                DaggerfallUI.AddHUDText(AdvancedTeleportationEffectErasePending);
            }
            else
            {
                // Prompt for outcome
                DaggerfallMessageBox mb = new DaggerfallMessageBox(DaggerfallUI.Instance.UserInterfaceManager, DaggerfallMessageBox.CommonMessageBoxButtons.AnchorTeleport, WhatToDoMessageText, DaggerfallUI.Instance.UserInterfaceManager.TopWindow);

                // QoL, does not match classic. No magicka refund, though
                mb.AllowCancel = true;
                mb.OnButtonClick += EffectActionPrompt_OnButtonClick;
                mb.Show();
            }
        }

        private void AnchorButtonClicked()
        {
            pickAnchor = new DaggerfallListPickerWindow(DaggerfallUI.Instance.UserInterfaceManager);

            int howManyAnchorsAreAvailable = MMMFormulaHelper.GetSpellLevel(this) * 2;

            for (int i = 0; i < howManyAnchorsAreAvailable; i++)
                if (anchorPositions[i] != null)
                    pickAnchor.ListBox.AddItem(anchorNames[i]);
            else
                    pickAnchor.ListBox.AddItem("<available slot #"+(i+1)+">");

            pickAnchor.OnItemPicked += SetAnchorAnchorPickerWindow_AnchorPicked;
            //pickAnchor.Draw();
            DaggerfallUI.UIManager.PushWindow(pickAnchor);
        }

        private int ChanceOfSuccess(int spellLevel, int index)
        {
            if (index <= spellLevel / 4) return 100;
            if (index <= spellLevel / 2) return 99;
            if (index >= spellLevel * 2) return 0;

            return 100 - (200 * index - 100 * spellLevel) / (3 * spellLevel);  // 100 - (2 / 3 SL) (# - SL/2)
        }

        private void TeleportButtonClicked()
        {
            pickAnchor = new DaggerfallListPickerWindow(DaggerfallUI.Instance.UserInterfaceManager);
            int spellLevel = MMMFormulaHelper.GetSpellLevel(this);
            int howManyAnchorsAreAvailable = spellLevel * 2;

            for (int i = 0; i < howManyAnchorsAreAvailable; i++)
                if (anchorPositions[i] != null)
                    pickAnchor.ListBox.AddItem("("+i.ToString("D3")+") "+anchorNames[i]/*+" ("+ChanceOfSuccess(spellLevel, i)+")"*/);            

            pickAnchor.OnItemPicked += TeleportToAnchorPickerWindow_AnchorPicked;
            //pickAnchor.Draw();
            DaggerfallUI.UIManager.PushWindow(pickAnchor);            
        }

        private void DoTheTeleportThingy(int index)
        {
            if (numberOfAnchorsSet == 0 || anchorPositions[index] == null)           
            {
                DaggerfallMessageBox mb = new DaggerfallMessageBox(DaggerfallUI.Instance.UserInterfaceManager, DaggerfallUI.Instance.UserInterfaceManager.TopWindow);
                mb.SetTextTokens(achorMustBeSet);
                mb.ClickAnywhereToClose = true;
                mb.Show();                
                return;
            }

            int spellLevel = MMMFormulaHelper.GetSpellLevel(this);
            int whereAreWeGoing;

            if (Dice100.SuccessRoll(ChanceOfSuccess(spellLevel, index)))
            {
                whereAreWeGoing = index;
                DaggerfallUI.AddHUDText(TeleportSuccessfulText);
            }
            else
            {
                if (Dice100.FailedRoll(75))
                {
                    DaggerfallUI.AddHUDText(TeleportFailedText);
                    return;     // if fail, then do nothing
                }
                // else choose from the anchors - one of them will be the one we will be taking the player to
                int anchorToUse = Random.Range(0, numberOfAnchorsSet) + 1;
                // the number of the anchor to use (if there are 3 anchors in total, one at index 2, one at index 5 and one at index 7, this will be 1..3) 
                int numberOfAnchorsFound = 0;
                int i;

                for (i = 0; numberOfAnchorsFound < anchorToUse; i++)
                    if (anchorPositions[i] != null) numberOfAnchorsFound++;     // find the (anchorToUse)th valid anchor
                whereAreWeGoing = i - 1;
                DaggerfallUI.AddHUDText(TeleportSuccessfulText);
                DaggerfallUI.AddHUDText(TeleportToAnotherDestinationExtraText/*+ whereAreWeGoing*/);
            }           

            TeleportPlayer(whereAreWeGoing);          
        }

        void SetAnchor(int index, string anchorName)
        {            
                // Validate references
            if (!serializablePlayer || !playerEnterExit)
                return;

            bool areWeSettingUpANewAnchor = (anchorPositions[index] == null);

            // Get position information
            anchorPositions[index] = serializablePlayer.GetPlayerPositionData();        
            anchorNames[index] = anchorName;
            if (playerEnterExit.IsPlayerInsideBuilding)
            {
                anchorPositions[index].exteriorDoors = playerEnterExit.ExteriorDoors;  
                anchorPositions[index].buildingDiscoveryData = playerEnterExit.BuildingDiscoveryData;  
            }

            if (areWeSettingUpANewAnchor) numberOfAnchorsSet++;                 // if we are setting up a new anchor, increase number of total set up
        }

        void TeleportPlayer(int index)
        {
            // Validate references
            if (!serializablePlayer || !playerEnterExit)
                return;

            // Is player in same interior as anchor?
            if (IsSameInterior(index))
            {
                // Just need to move player
                serializablePlayer.RestorePosition(anchorPositions[index]);
            }
            else
            {
                // Cache scene before departing
                if (!playerEnterExit.IsPlayerInside)
                    SaveLoadManager.CacheScene(GameManager.Instance.StreamingWorld.SceneName);      // Player is outside
                else if (playerEnterExit.IsPlayerInsideBuilding)
                    SaveLoadManager.CacheScene(playerEnterExit.Interior.name);                      // Player inside a building
                else // Player inside a dungeon
                    playerEnterExit.TransitionDungeonExteriorImmediate();

                // Need to load some other part of the world again - player could be anywhere
                indexWhereWeAreTeleporting = index;                         // leave a message for private void PlayerEnterExit_OnRespawnerComplete()
                PlayerEnterExit.OnRespawnerComplete += PlayerEnterExit_OnRespawnerComplete;
                playerEnterExit.RestorePositionHelper(anchorPositions[index], false, true);        

                // Restore building summary data  ---  COMMENTING BECAUSE OF ISSUE
                if (anchorPositions[index].insideBuilding)                    
                    playerEnterExit.BuildingDiscoveryData = anchorPositions[index].buildingDiscoveryData;

                // When moving anywhere other than same interior trigger a fade so transition appears smoother
                DaggerfallUI.Instance.FadeBehaviour.FadeHUDFromBlack();
            }
        }

        #endregion

        #region Helpers

        // Cache required references
        bool CacheReferences()
        {
            // Get peered SerializablePlayer and PlayerEnterExit
            if (!serializablePlayer)
                serializablePlayer = caster.GetComponent<SerializablePlayer>();

            if (!playerEnterExit)
                playerEnterExit = caster.GetComponent<PlayerEnterExit>();

            if (!serializablePlayer || !playerEnterExit)
            {
                Debug.LogError("Teleport effect could not find both SerializablePlayer and PlayerEnterExit components.");
                return false;
            }

            return true;
        }

        // Checks if player is in same building or dungeon interior as anchor
        bool IsSameInterior(int index)
        {
            // Reject if outside or anchor not set
            if (!playerEnterExit.IsPlayerInside || anchorPositions[index] == null)          
                return false;

            // Test depends on if player is inside a building or a dungeon
            if (playerEnterExit.IsPlayerInsideBuilding && anchorPositions[index].insideBuilding)          
            {
                // Compare building key
                if (anchorPositions[index].buildingDiscoveryData.buildingKey == playerEnterExit.BuildingDiscoveryData.buildingKey)      
                {
                    // Also compare map pixel, in case we're unlucky https://forums.dfworkshop.net/viewtopic.php?f=24&t=2018
                    DaggerfallConnect.Utility.DFPosition anchorMapPixel = DaggerfallConnect.Arena2.MapsFile.WorldCoordToMapPixel(anchorPositions[index].worldPosX, anchorPositions[index].worldPosZ);
                                                                   
                    DaggerfallConnect.Utility.DFPosition playerMapPixel = GameManager.Instance.PlayerGPS.CurrentMapPixel;
                    if (anchorMapPixel.X == playerMapPixel.X && anchorMapPixel.Y == playerMapPixel.Y)
                        return true;
                }
            }
            else if (playerEnterExit.IsPlayerInsideDungeon && anchorPositions[index].insideDungeon)     
            {
                // Compare map pixel of dungeon (only one dungeon per map pixel allowed)
                DaggerfallConnect.Utility.DFPosition anchorMapPixel = DaggerfallConnect.Arena2.MapsFile.WorldCoordToMapPixel(anchorPositions[index].worldPosX, anchorPositions[index].worldPosZ);
                                                                                                                                 
                DaggerfallConnect.Utility.DFPosition playerMapPixel = GameManager.Instance.PlayerGPS.CurrentMapPixel;

                if (anchorMapPixel.X == playerMapPixel.X && anchorMapPixel.Y == playerMapPixel.Y)
                {
                    GameManager.Instance.PlayerEnterExit.PlayerTeleportedIntoDungeon = true;
                    return true;
                }
            }

            return false;
        }        

        #endregion

        #region Event Handlers

        private void PlayerEnterExit_OnRespawnerComplete()
        {
            // Must have a caster and it must be the player
            if (caster == null || caster != GameManager.Instance.PlayerEntityBehaviour)
                return;

            // Get peered SerializablePlayer and PlayerEnterExit if they haven't been cached yet
            if (!CacheReferences())
                return;

            // Restore final position and unwire event
            serializablePlayer.RestorePosition(anchorPositions[indexWhereWeAreTeleporting]);                 
            PlayerEnterExit.OnRespawnerComplete -= PlayerEnterExit_OnRespawnerComplete;

            // Restore scene cache on arrival
            if (!playerEnterExit.IsPlayerInside)
                SaveLoadManager.RestoreCachedScene(GameManager.Instance.StreamingWorld.SceneName);      // Player is outside
            else if (playerEnterExit.IsPlayerInsideBuilding)
                SaveLoadManager.RestoreCachedScene(playerEnterExit.Interior.name);                      // Player inside a building
        }

        private void EffectActionPrompt_OnButtonClick(DaggerfallMessageBox sender, DaggerfallMessageBox.MessageBoxButtons messageBoxButton)
        {
            sender.CloseWindow();

            if (messageBoxButton == DaggerfallMessageBox.MessageBoxButtons.Anchor)
            {
                AnchorButtonClicked();
            }
            else if (messageBoxButton == DaggerfallMessageBox.MessageBoxButtons.Teleport)
            {
                TeleportButtonClicked();
            }
        }

        private void SetAnchorAnchorPickerWindow_AnchorPicked(int index, string itemString)
        {
            pickAnchor.CloseWindow();
            // here comes the logic that deals with failed attempts
            int chance = ChanceOfSuccess(MMMFormulaHelper.GetSpellLevel(this), index);

            if (Dice100.FailedRoll(chance))
            {
                DaggerfallUI.AddHUDText(AnchorCreationFailedText/*+chance*/);
                return;
            }
            
            //DaggerfallUI.AddHUDText("Chosen index " + index);

            indexOfAnchorWeAreSetting = index;

            DaggerfallInputMessageBox pleaseNameYourAnchorBox = new DaggerfallInputMessageBox(DaggerfallUI.Instance.UserInterfaceManager);
            pleaseNameYourAnchorBox.SetTextBoxLabel(ShortPleaseNameYourAnchorText);
            if (anchorNames[indexOfAnchorWeAreSetting] == null)
                pleaseNameYourAnchorBox.TextBox.Text = "My Magically Binded Place #" + (indexOfAnchorWeAreSetting+1);
            else
                pleaseNameYourAnchorBox.TextBox.Text = anchorNames[indexOfAnchorWeAreSetting];
            pleaseNameYourAnchorBox.OnGotUserInput += SetAnchorNameWindow_NamePicked;
            //pleaseNameYourAnchorBox.Draw();

            pleaseNameYourAnchorBox.Show();
        }

        private void SetAnchorNameWindow_NamePicked(DaggerfallInputMessageBox sender, string anchorName)
        {            
            SetAnchor(indexOfAnchorWeAreSetting, anchorName);
            sender.CloseWindow();
        }

        private void TeleportToAnchorPickerWindow_AnchorPicked(int index, string itemString)
        {
            pickAnchor.CloseWindow();
            string indexString = itemString.Substring(1, 3);
            int realIndex = int.Parse(indexString);

            //DaggerfallUI.AddHUDText("Chosen index " + realIndex);
            DoTheTeleportThingy(realIndex);     
        }

        #endregion

        #region Serialization

        [fsObject("v1")]

        public struct AnchorSaveData_v1
        {
            public int originalIndexInArray;
            public PlayerPositionData_v1 anchorPosition;
            public string anchorName;
        }

        public struct SaveData_v1
        {
            public int numberOfAnchorsSet;
            public AnchorSaveData_v1[] anchors;
            public int forcedRoundsRemaining;
        }

        public override object GetSaveData()
        {
            SaveData_v1 data = new SaveData_v1();            

            data.numberOfAnchorsSet = numberOfAnchorsSet;
            data.anchors = new AnchorSaveData_v1[numberOfAnchorsSet];            

            int anchorsAlreadyPreppedForSave = 0;

                                // prepping the anchors for save -- will result in an error if there are less valid anchors in the array than declared by numberOfAnchorsSet
            for (int i=0; anchorsAlreadyPreppedForSave<numberOfAnchorsSet; i++)
                if (anchorPositions[i]!=null)           // signals validity. it should be null if the anchor is not valid anymore for whatever reason
                {
                    data.anchors[anchorsAlreadyPreppedForSave].originalIndexInArray = i;                  // anchorsAlreadyPreppedForSave also gives the index where we need to store the next one
                    data.anchors[anchorsAlreadyPreppedForSave].anchorPosition = anchorPositions[i];
                    data.anchors[anchorsAlreadyPreppedForSave].anchorName = anchorNames[i];         // should contain a string if the anchor is valid.
                    anchorsAlreadyPreppedForSave++;         // emphasizing the incrementation
                }
            
            data.forcedRoundsRemaining = forcedRoundsRemaining;

            //DaggerfallUI.AddHUDText("Teleportation Data Saved :: number of anchors="+ numberOfAnchorsSet);

            return data;
        }

        public override void RestoreSaveData(object dataIn)
        {
            if (dataIn == null)
                return;

            SaveData_v1 data = (SaveData_v1)dataIn;

            numberOfAnchorsSet = data.numberOfAnchorsSet;
            if (anchorPositions==null)
                anchorPositions = new PlayerPositionData_v1[maximumNumberOfAnchors];     // possibly redundant  
            if (anchorNames == null)
                anchorNames = new string[maximumNumberOfAnchors];                       // possibly redundant

            for (int i = 0; i < numberOfAnchorsSet; i++)
            {
                anchorPositions[data.anchors[i].originalIndexInArray] = data.anchors[i].anchorPosition;
                anchorNames[data.anchors[i].originalIndexInArray] = data.anchors[i].anchorName;
            }
            
            forcedRoundsRemaining = data.forcedRoundsRemaining;
        }

        #endregion
    }
}
