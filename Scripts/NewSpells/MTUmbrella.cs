// Project:         MeriTamas's (Mostly) Magic Mod for Daggerfall Unity (http://www.dfworkshop.net)
// Copyright:       Copyright (C) 2022 meritamas
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Author:          meritamas (meritamas@outlook.com)

using UnityEngine;
using DaggerfallConnect;
using DaggerfallConnect.Arena2;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.MagicAndEffects;
using DaggerfallWorkshop.Game.MagicAndEffects.MagicEffects;
using DaggerfallWorkshop.Game.Utility.ModSupport;   //required for modding features

namespace MTMMM
{
    /// <summary>
    /// Magical Umbrella
    /// </summary>
    public class MTUmbrella : IncumbentEffect
    {
        public static readonly string EffectKey = "Umbrella";

        static Mod ccModInstance = null;

        public override void SetProperties()
        {
            properties.Key = EffectKey;            
            properties.SupportDuration = true;            
            properties.AllowedTargets = EntityEffectBroker.TargetFlags_Self;
            properties.AllowedElements = EntityEffectBroker.ElementFlags_MagicOnly;
            properties.AllowedCraftingStations = MagicCraftingStations.SpellMaker;
            properties.MagicSkill = DFCareer.MagicSkills.Thaumaturgy;
            properties.DurationCosts = MakeEffectCosts(5, 10);          // these values shoud do for now           
        }

        public override void ConstantEffect()
        {
            base.ConstantEffect();
            //StartUmbrella();
        }

        public override void Start(EntityEffectManager manager, DaggerfallEntityBehaviour caster = null)
        {
            base.Start(manager, caster);
            StartUmbrella();
        }

        public override void Resume(EntityEffectManager.EffectSaveData_v1 effectData, EntityEffectManager manager, DaggerfallEntityBehaviour caster = null)
        {
            base.Resume(effectData, manager, caster);
            StartUmbrella();
        }

        public override void End()
        {
            base.End();
            StopUmbrella();
        } 

        protected override bool IsLikeKind(IncumbentEffect other)
        {
            return (other is MTUmbrella);
        }

        protected override void AddState(IncumbentEffect incumbent)
        {
            // Stack my rounds onto incumbent
            incumbent.RoundsRemaining += RoundsRemaining;
        }

        void StartUmbrella()
        {
            // Get peered entity gameobject
            DaggerfallEntityBehaviour entityBehaviour = GetPeeredEntityBehaviour(manager);
            if (!entityBehaviour)
                return;

            if (ccModInstance is null)
            {
                MMMFormulaHelper.MMMFormulaHelperInfoMessage("Retrieving C&C mod instance.");
                ccModInstance = ModManager.Instance.GetMod("Climates & Calories");
                if (ccModInstance is null)
                    MMMFormulaHelper.MMMFormulaHelperInfoMessage("Failed to retrieve C&C mod instance.");
                else
                    MMMFormulaHelper.MMMFormulaHelperInfoMessage("C&C mod instance retrieved successfully.");
            }

            MMMFormulaHelper.MMMFormulaHelperInfoMessage("Sending message to C&C that the umbrella is there.");
                // Send appropriate message to C&C mod
        }

        void StopUmbrella()
        {
            // Get peered entity gameobject
            DaggerfallEntityBehaviour entityBehaviour = GetPeeredEntityBehaviour(manager);
            if (!entityBehaviour)
                return;

            if (ccModInstance is null)
                ccModInstance = ModManager.Instance.GetMod("Climates & Calories");

            MMMFormulaHelper.MMMFormulaHelperInfoMessage("Sending message to C&C that the umbrella is no longer there.");
                // Send appropriate message to C&C mod
        }
    }
}
