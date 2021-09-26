// Project:         MeriTamas's (Mostly) Magic Mod for Daggerfall Unity (http://www.dfworkshop.net)
// Copyright:       Copyright (C) 2021 meritamas
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Author:          meritamas (meritamas@outlook.com)
// Credits due to the DFU developers based on the work of whom this class could be created.

using DaggerfallWorkshop.Game.Utility;
using DaggerfallWorkshop.Game.MagicAndEffects;
using DaggerfallWorkshop.Game.MagicAndEffects.MagicEffects;
using DaggerfallWorkshop.Game.Entity;

namespace MTMMM
{
    /// <summary>
    /// Paralyze, inherited from classic for MMM
    /// </summary>
    public class MTParalyze : Paralyze
    {
        /// <summary>
        /// The 'strength' of Paralyze is in part its Chance of Success (the other parameter is Duration)
        /// Here we override RollChance() to do exactly as its parent but have ChanceOfSuccess calculated by MMMFormulaHelper 
        /// </summary>
        /// <returns>True if chance roll succeeded.</returns>
        public override bool RollChance()
        {
            if (!Properties.SupportChance)
                return false;

            bool outcome = Dice100.SuccessRoll(MMMFormulaHelper.GetSpellChance(this));                            
            return outcome;
        }

        /// <summary>
        /// The 'strength' of Paralyze is in part its Duration (the other parameter is Chance of Success)
        /// Here we override Start() to do exactly as its parent but have Duration calculated by MMMFormulaHelper.  
        /// </summary>
        public override void Start(EntityEffectManager manager, DaggerfallEntityBehaviour caster = null)
        {
            base.Start(manager, caster);
            RoundsRemaining = MMMFormulaHelper.GetSpellDuration(this);
        }
    }
}