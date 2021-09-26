// Project:         MeriTamas's (Mostly) Magic Mod for Daggerfall Unity (http://www.dfworkshop.net)
//
// Parts of code copied from DFU class.
// Copyright:       Copyright (C) 2009-2020 Daggerfall Workshop
// Author of the original DFU class: Gavin Clayton(interkarma@dfworkshop.net)
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Author:          meritamas (meritamas@outlook.com)
//
// Other parts copyright:       Copyright (C) 2021 meritamas
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Author:          meritamas (meritamas@outlook.com)

using System;
using DaggerfallConnect;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.MagicAndEffects;
using DaggerfallWorkshop.Game.MagicAndEffects.MagicEffects;
using FullSerializer;

namespace MTMMM
{
    /// <summary>
    /// Shield, reimplemented after DFU with a slight modification to allow Magnitude to be calculated by MMMFormulaHelper
    /// </summary>
    public class MTShield : IncumbentEffect
    {
        public static readonly string EffectKey = "Shield";

        int startingShield;
        int shieldRemaining;

        public override void SetProperties()
        {
            properties.Key = EffectKey;
            properties.ClassicKey = MakeClassicKey(35, 255);            
            properties.SupportDuration = true;
            properties.SupportMagnitude = true;
            properties.AllowedTargets = EntityEffectBroker.TargetFlags_All;
            properties.AllowedElements = EntityEffectBroker.ElementFlags_MagicOnly;
            properties.AllowedCraftingStations = MagicCraftingStations.SpellMaker;
            properties.MagicSkill = DFCareer.MagicSkills.Alteration;
            properties.DurationCosts = MakeEffectCosts(28, 8);
            properties.MagnitudeCosts = MakeEffectCosts(80, 60);
        }

        protected override bool IsLikeKind(IncumbentEffect other)
        {
            return other is Shield;
        }
                
        protected override void AddState(IncumbentEffect incumbent)
        {
            // Stack rounds onto incumbent
            incumbent.RoundsRemaining += RoundsRemaining;

            // Top up shield amount no more than starting value
            MTShield incumbentShield = incumbent as MTShield;
            incumbentShield.shieldRemaining += MMMFormulaHelper.GetSpellMagnitude(this, manager); // here we also change the way the magnitude is calculated
            if (incumbentShield.shieldRemaining > incumbentShield.startingShield)
                incumbentShield.shieldRemaining = incumbentShield.startingShield;
        }

        /// <summary>
        /// The 'strength' of Shield is its Duration and Magnitude
        /// Here we supply a modified Start() method in order to have Duration and Magnitude calculated by MMMFormulaHelper.  
        /// </summary>
        public override void Start(EntityEffectManager manager, DaggerfallEntityBehaviour caster = null)
        {
            base.Start(manager, caster);

            RoundsRemaining = MMMFormulaHelper.GetSpellDuration(this);
            // Set initial shield amount
            startingShield = shieldRemaining = MMMFormulaHelper.GetSpellMagnitude(this, manager);
        }

        /// <summary>
        /// Apply damage to shield.
        /// </summary>
        /// <param name="amount">Amount of damage to apply.</param>
        /// <returns>Damaged passed through after removing shield amount. Will be 0 if damage amount less than remaining shield amount.</returns>
        public int DamageShield(int amount)
        {
            if (shieldRemaining > 0)
            {
                shieldRemaining -= amount;
                if (shieldRemaining <= 0)
                {
                    // Shield busted - immediately end effect and return shield overflow amount
                    ResignAsIncumbent();
                    RoundsRemaining = 0;
                    manager.UpdateHUDSpellIcons();
                    return Math.Abs(shieldRemaining);
                }

                return 0;
            }
            else
            {
                return amount;
            }
        }

        #region Serialization

        [fsObject("v1")]
        public struct SaveData_v1
        {
            public int startingShield;
            public int shieldRemaining;
        }

        public override object GetSaveData()
        {
            SaveData_v1 data = new SaveData_v1();
            data.startingShield = startingShield;
            data.shieldRemaining = shieldRemaining;

            return data;
        }

        public override void RestoreSaveData(object dataIn)
        {
            if (dataIn == null)
                return;

            SaveData_v1 data = (SaveData_v1)dataIn;
            startingShield = data.startingShield;
            shieldRemaining = data.shieldRemaining;
        }

        #endregion
    }
}