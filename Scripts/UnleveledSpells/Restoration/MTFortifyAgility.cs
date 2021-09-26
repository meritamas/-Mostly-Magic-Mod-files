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
    /// FortifyAgility, inherited from classic for MMM
    /// </summary>
    public class MTFortifyAgility : FortifyAgility
    {
        /// <summary>
        /// The 'strength' of FortifyAgility is in part its Duration. (the other factor is Magnitude)
        /// Here we override Start() to do exactly as its parent but have Duration calculated by MMMFormulaHelper.  
        /// </summary>
        public override void Start(EntityEffectManager manager, DaggerfallEntityBehaviour caster = null)
        {
            base.Start(manager, caster);
            RoundsRemaining = MMMFormulaHelper.GetSpellDuration(this);
        }
        /// <summary>
        /// The 'strength' of FortifyAgility is in part its Magnitude, its effect on the affected attribute. (the other factor is Duration)
        /// Here we override BecomeIncumbent() to increase the attribute affected by an amount calculated by MMMFormulaHelper.  
        /// </summary>
        protected override void BecomeIncumbent()
        {
            // Incumbent changes fortify magnitude            
            int magnitude = MMMFormulaHelper.GetSpellMagnitude(this, manager); // maxmagnitude            
            ChangeStatMod(fortifyStat, magnitude);
            ChangeStatMaxMod(fortifyStat, magnitude / 2); // minmagnitude - in case stat already maxed out: one half of maxmagnitude
        }
    }
}