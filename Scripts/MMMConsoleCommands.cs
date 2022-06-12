// Project:         MeriTamas's (Mostly) Magic Mod for Daggerfall Unity (http://www.dfworkshop.net)
// Copyright:       Copyright (C) 2022 meritamas
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
    public static class MMMConsoleCommands
    {
        #region debug
        static string messagePrefix = "MMMConsoleCommands: ";

        static void Message(string message)
        {
            MTMostlyMagicMod.Message(messagePrefix + message);
        }

        static void SilentMessage(string message)
        {
            MTMostlyMagicMod.SilentMessage(messagePrefix + message);
        }
        #endregion

        #region SpellXPTallies

        /// <summary>
        /// Increment a specifies Spell XP Tally
        /// </summary>
        public static string IncrementSpellXPTally(string[] args)
        {
            int newTallyValue = MMMXPTallies.SpellXPTally(args[0], float.Parse(args[1]));
            
            return "IncrementSpellXPTally: Tally '"+args[0]+"' - new value: "+newTallyValue;
                    // TODO: check and error msgs in case of bad input
        }

        #endregion

        #region Legacy Advanced Teleportation

        /// <summary>
        /// Executes the advanced-teleport-kill console command
        /// </summary>
        public static string SaveOrNotToSaveLegacyAdvancedTeleportation(string[] args)
        {
            if (args[0].CompareTo("yes") == 0)
            {
                MTMostlyMagicMod.shouldEraseAdvancedTeleportEffect = true;
                return "Advanced Teleportation Kill Flag set to true. " + Environment.NewLine +
                    "Next time you invoke the spell effect, it will erase itself. Save games made afterwards will NOT contain Advanced Teleportation data but WILL be compatible with game without Advanced Teleportation mod.";
            }

            if (args[0].CompareTo("no") == 0)
            {
                MTMostlyMagicMod.shouldEraseAdvancedTeleportEffect = false;
                return "Advanced Teleportation Kill Flag set to false. " + Environment.NewLine +
                    "Will be saving this effect. Save games made afterwards WILL contain Advanced Teleportation data but will NOT be compatible with game without Advanced Teleportation mod.";
            }

            return "advanced-teleportation-kill: unrecognized parameters, please use either 'advanced-teleport-kill yes' or 'advanced-teleport-kill no'.";
        }

        #endregion


    }
}
