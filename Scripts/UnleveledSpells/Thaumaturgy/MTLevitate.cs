// Project:         MeriTamas's (Mostly) Magic Mod for Daggerfall Unity (http://www.dfworkshop.net)
// Copyright:       Copyright (C) 2021 meritamas
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Author:          meritamas (meritamas@outlook.com)
// Credits due to the DFU developers based on the work of whom this class could be created.

using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.MagicAndEffects;
using DaggerfallWorkshop.Game.MagicAndEffects.MagicEffects;

namespace MTMMM
{
    /// <summary>
    /// Levitate, inherited from classic for MMM
    /// </summary>
    public class MTLevitate : Levitate
    {
        /// <summary>
        /// The 'strength' of Levitate is its Duration.
        /// We override Start() to do exactly as its parent but have Duration calculated by MMMFormulaHelper.  
        /// </summary>
        public override void Start(EntityEffectManager manager, DaggerfallEntityBehaviour caster = null)
        {
            base.Start(manager, caster);
            RoundsRemaining = MMMFormulaHelper.GetSpellDuration(this);
        }

    }
}