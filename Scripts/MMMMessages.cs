// Project:         MeriTamas's (Mostly) Magic Mod for Daggerfall Unity (http://www.dfworkshop.net)
// Copyright:       Copyright (C) 2023 meritamas
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Author:          meritamas (meritamas@outlook.com)

using System;
using System.Collections.Generic;
using UnityEngine;
using DaggerfallConnect;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.Formulas;
using DaggerfallWorkshop.Game.MagicAndEffects;
using Wenzil.Console;

namespace MTMMM
{
    public static class MMMMessages
    {
        static string messagePrefix = "MMMMessages: ";               
    
        /// <summary>
        /// Gets a string array to initialize a MessageBox or other carrier with.
        /// </summary>
        /// <param name="messageIndex">Message index</param>
        /// <param name="args">the arguments that need to be formatted in the message strings</param>
        /// <returns></returns>
        public static string[] GetMessage (string[] baseMessage, params object[] args)
        {
            string[] ourMessage = new string[baseMessage.Length];
            for (int i = 0; i < ourMessage.Length; i++)
            {
                ourMessage[i] = string.Format(baseMessage[i], args);
                // SilentMessage("Processing message string {0}/{1}.", i+1, baseMessage.Length);
            }
            return ourMessage;
        }           // TODO: consider doing a check if the right number and type of args are passed
        /*
        public static DaggerfallMessageBox MessageBox(string[] message, bool wrapText = false, IMacroContextProvider mcp = null)
        {
            DaggerfallMessageBox messageBox = new DaggerfallMessageBox(Instance.uiManager, Instance.uiManager.TopWindow, wrapText);
            messageBox.SetText(message, mcp);
            messageBox.ClickAnywhereToClose = true;
            messageBox.Show();
            return messageBox;
        }*/

        #region debug 
        static void Message(string message)
        {
            MTMostlyMagicMod.Message(messagePrefix + message);
        }

        static void SilentMessage(string message)
        {
            MTMostlyMagicMod.SilentMessage(messagePrefix + message);
        }

        static void SilentMessage(string message, params object[] args)
        {
            MTMostlyMagicMod.SilentMessage(messagePrefix + message, args);
        }
        #endregion
    }

}
