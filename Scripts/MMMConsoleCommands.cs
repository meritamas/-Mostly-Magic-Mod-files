// Project:         MeriTamas's (Mostly) Magic Mod for Daggerfall Unity (http://www.dfworkshop.net)
// Copyright:       Copyright (C) 2023 meritamas
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Author:          meritamas (meritamas@outlook.com)

using System;
using System.Collections.Generic;
using UnityEngine;
using DaggerfallConnect;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.Formulas;
using DaggerfallWorkshop.Game.MagicAndEffects;
using Wenzil.Console;
using DaggerfallConnect.Arena2;

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

        #region RoadsUtilities

        /// <summary>
        /// A console command to calculate the economic power of a settlement.
        /// With two arguments, the first one should be a region name and the second one a location name.
        /// With one argument, the argument is a location name within the current region
        /// If no argument passed, calculates for the current settlement if any
        /// </summary>
        public static string GetEconomicPower(string[] args)
        {
            // first determine number of arguments
            // the two argument version will end with a message that it is not implemented yet


            // second, get the settlement's Location based on the argument 
            //int newTallyValue = MMMXPTallies.SpellXPTally(args[0], float.Parse(args[1]));

            // add the indicators of economic power: resident buildings (consumption)
            // number of shops * quality level (production, capital)
            // lack of a certain shop type, an overabundance of certain shops (demand for commerce)
            // number of taverns * quailty level (tourism)
            // guilds and temples
            return "done";
            
        }

        public static string MTGetRegionName (string[] args)
        {
            string[] RegionNames = MapsFile.RegionNames;
            int regionNumber;

            if (Int32.TryParse(args[0], out regionNumber))
            {
                if (regionNumber < 0 || regionNumber >= DaggerfallUnity.Instance.ContentReader.MapFileReader.RegionCount)
                {
                    return "There is no such region.";
                }
                else
                {
                    return RegionNames[regionNumber];
                }
            }
            else
                return "Argument0 needs to be a valid region number";
        }

        public static string MTGetRegionIndex(string[] args)
        {
            string[] RegionNames = MapsFile.RegionNames;

            for (int i = 0; i < RegionNames.Length; i++)
            {
                if (RegionNames[i] == args[0])
                    return i.ToString();
            }

            return "There is no such region.";                     
        }

        public static string MTAboutLocation(string[] args)
        {
            MTRoads.LoadLocation(args[0], args[1]);
            return MTRoads.PrintLocationCharacteristics();
        }

        #endregion
    }
}
