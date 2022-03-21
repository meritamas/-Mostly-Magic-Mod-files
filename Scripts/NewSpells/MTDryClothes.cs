// Project:         MeriTamas's (Mostly) Magic Mod for Daggerfall Unity (http://www.dfworkshop.net)
// Copyright:       Copyright (C) 2022 meritamas
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Author:          meritamas (meritamas@outlook.com)  
// 
// Notes:
//

using DaggerfallConnect;
using DaggerfallConnect.Arena2;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.MagicAndEffects;
using DaggerfallWorkshop.Game.MagicAndEffects.MagicEffects;

namespace MTMMM
{
    /// <summary>
    /// DryClothes
    /// </summary>
    public class MTDryClothes : BaseEntityEffect
    {
        public static readonly string EffectKey = "DryClothes";

        public override void SetProperties()
        {
            properties.Key = EffectKey;            
            properties.SupportMagnitude = true;
            properties.AllowedTargets = EntityEffectBroker.TargetFlags_Self;
            properties.AllowedElements = EntityEffectBroker.ElementFlags_MagicOnly;
            properties.AllowedCraftingStations = MagicCraftingStations.SpellMaker;
            properties.MagicSkill = DFCareer.MagicSkills.Alteration;
            properties.MagnitudeCosts = MakeEffectCosts(2, 4);            // these numbers should do for now
        }        

        public override void MagicRound()
        {
            base.MagicRound();

            // Get peered entity gameobject
            DaggerfallEntityBehaviour entityBehaviour = GetPeeredEntityBehaviour(manager);
            if (!entityBehaviour)
                return;

            // Implement effect
            int magnitude = MMMFormulaHelper.GetSpellMagnitude(this, manager);
            MMMFormulaHelper.MMMFormulaHelperInfoMessage("MTDryClothes: Decreasing wetness by " + magnitude+" points.");
                    // code to send the message to CLimates & Calories
        }
    }
}
