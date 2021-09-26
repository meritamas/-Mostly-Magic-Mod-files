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
    /// ContinuousDamageSpellPoints, inherited from classic for MMM
    /// </summary>
    public class MTContinuousDamageSpellPoints : ContinuousDamageSpellPoints
    {
        /// <summary>
        /// The 'strength' of ContinuousDamageSpellPoints is in part its Duration. (the other factor is Magnitude)
        /// Here we override Start() to do exactly as its parent but have Duration calculated by MMMFormulaHelper.  
        /// </summary>
        public override void Start(EntityEffectManager manager, DaggerfallEntityBehaviour caster = null)
        {
            base.Start(manager, caster);
            RoundsRemaining = MMMFormulaHelper.GetSpellDuration(this);
        }
        /// <summary>
        /// The 'strength' of ContinuousDamageSpellPoints is in part its Magnitude. (the other factor is Duration)
        /// Here we override MagicRound() to do exactly as its parent but have Magnitude calculated by MMMFormulaHelper.  
        /// </summary>
        public override void MagicRound()
        {
            RemoveRound(); // would prefer base.base.MagicRound() if that were possible, this is the contents ::  potential source of problems if BaseEntityEffect.MagicRound() changes.

            // Get peered entity gameobject
            DaggerfallEntityBehaviour entityBehaviour = GetPeeredEntityBehaviour(manager);
            if (!entityBehaviour)
                return;

            // Implement effect            
            entityBehaviour.DamageMagickaFromSource(this, MMMFormulaHelper.GetSpellMagnitude(this, manager));
        }
    }
}