// Project:         MeriTamas's (Mostly) Magic Mod for Daggerfall Unity (http://www.dfworkshop.net)
// Copyright:       Copyright (C) 2021 meritamas
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Author:          meritamas (meritamas@outlook.com)
// Credits due to the DFU developers based on the work of whom this class could be created.

using DaggerfallWorkshop.Game.Utility;
using DaggerfallWorkshop.Game.MagicAndEffects.MagicEffects;

namespace MTMMM
{
    /// <summary>
    /// Dispel Magic, inherited from classic for MMM
    /// </summary>
    public class MTOpen : Open
    {
        /// <summary>
        /// The 'strength' of Open is its Chance of Success.
        /// We override RollChance() to do exactly as its parent but have ChanceOfSuccess calculated by MMMFormulaHelper.  
        /// </summary>
        /// <returns>True if chance roll succeeded.</returns>
        public override bool RollChance()
        {
            if (!Properties.SupportChance)
                return false;

            bool outcome = Dice100.SuccessRoll(MMMFormulaHelper.GetSpellChance(this));                            
            return outcome;
        }
    }
}