// Project:         MeriTamas's (Mostly) Magic Mod for Daggerfall Unity (http://www.dfworkshop.net)
// Copyright:       Copyright (C) 2022 meritamas
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Author:          meritamas (meritamas@outlook.com)

// This class was based on the DaggerfallSpellBookWindow class which is part of the Daggerfall Tools For Unity project - many parts retained unchanged
// Copyright:       Copyright (C) 2009-2020 Daggerfall Workshop
// Web Site:        http://www.dfworkshop.net
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Source Code:     https://github.com/Interkarma/daggerfall-unity
// Original Author: Lypyl (lypyldf@gmail.com), Gavin Clayton (interkarma@dfworkshop.net)
// Contributors:    Allofich, Hazelnut


using UnityEngine;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game.UserInterface;
using DaggerfallWorkshop.Game.MagicAndEffects;
using DaggerfallWorkshop.Game.Formulas;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.Items;
using DaggerfallConnect;
using DaggerfallConnect.Arena2;
using DaggerfallWorkshop.Utility;
using DaggerfallWorkshop.Utility.AssetInjection;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using DaggerfallConnect.Save;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.UserInterfaceWindows;
using DaggerfallWorkshop.Game.Utility.ModSupport;   //required for modding features
using DaggerfallWorkshop.Game.Utility.ModSupport.ModSettings;

namespace MTMMM
{
    public class MTSpellBookWindow : DaggerfallSpellBookWindow
    {
        public static bool intelligenceRequirementForPurchasingSpells;

        public MTSpellBookWindow(IUserInterfaceManager uiManager, DaggerfallBaseWindow previous = null, bool buyMode = false) : base(uiManager, previous, buyMode)
        {            
        }

        protected override void PopulateSpellsList(List<EffectBundleSettings> spells, int? availableSpellPoints = null)
        {
            foreach (EffectBundleSettings spell in spells)
            {
                // Get spell costs
                // Costs can change based on player skills and stats so must be calculated each time
                FormulaHelper.SpellCost tmpSpellCost = FormulaHelper.CalculateTotalEffectCosts(spell.Effects, spell.TargetType, null, spell.MinimumCastingCost);              

                // Lycanthropy is a free spell, even though it shows a cost in classic
                // Setting cost to 0 so it displays correctly in spellbook
                if (spell.Tag == PlayerEntity.lycanthropySpellTag)
                    tmpSpellCost.spellPointCost = 0;

                // Display spell name and cost
                ListBox.ListItem listItem;
                spellsListBox.AddItem(string.Format("{0} - {1}", tmpSpellCost.spellPointCost, spell.Name), out listItem);
                if (availableSpellPoints != null && availableSpellPoints < tmpSpellCost.spellPointCost)
                {
                    // Desaturate unavailable spells
                    float desaturation = 0.75f;
                    listItem.textColor = Color.Lerp(listItem.textColor, Color.grey, desaturation);
                    listItem.selectedTextColor = Color.Lerp(listItem.selectedTextColor, Color.grey, desaturation);
                    listItem.highlightedTextColor = Color.Lerp(listItem.highlightedTextColor, Color.grey, desaturation);
                    listItem.highlightedSelectedTextColor = Color.Lerp(listItem.highlightedSelectedTextColor, Color.grey, desaturation);
                }
            }
        }

        protected override void LoadSpellsForSale()
        {
            // Load spells for sale
            offeredSpells.Clear();

            var effectBroker = GameManager.Instance.EntityEffectBroker;

            IEnumerable<SpellRecord.SpellRecordData> standardSpells = effectBroker.StandardSpells;
            if (standardSpells == null || standardSpells.Count() == 0)
            {
                Debug.LogError("Failed to load SPELLS.STD for spellbook in buy mode.");
                return;
            }

            // Add standard spell bundles to offer
            foreach (SpellRecord.SpellRecordData standardSpell in standardSpells)
            {
                // Filter internal spells starting with exclamation point '!'
                if (standardSpell.spellName.StartsWith("!"))
                    continue;

                // NOTE: Classic allows purchase of duplicate spells
                // If ever changing this, must ensure spell is an *exact* duplicate (i.e. not a custom spell with same name)
                // Just allowing duplicates for now as per classic and let user manage preference

                // Get effect bundle settings from classic spell
                EffectBundleSettings bundle;
                if (!effectBroker.ClassicSpellRecordDataToEffectBundleSettings(standardSpell, BundleTypes.Spell, out bundle))
                    continue;

                // Store offered spell and add to list box
                offeredSpells.Add(bundle);
            }

            // Add custom spells for sale bundles to list of offered spells
            offeredSpells.AddRange(effectBroker.GetCustomSpellBundles(EntityEffectBroker.CustomSpellBundleOfferUsage.SpellsForSale));

            // Sort spells for easier finding
            offeredSpells = offeredSpells.OrderBy(x => x.Name).ToList();
        }

        protected override void BuyButton_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            if (intelligenceRequirementForPurchasingSpells)
            {
                DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
                const int tradeMessageBaseId = 260;
                const int notEnoughGoldId = 454;
                int tradePrice = GetTradePrice();
                int msgOffset = 0;
                EffectBundleSettings spell = offeredSpells[spellsListBox.SelectedIndex];
                FormulaHelper.SpellCost spellCost = FormulaHelper.CalculateTotalEffectCosts(spell.Effects, spell.TargetType, null, spell.MinimumCastingCost);

                if (!GameManager.Instance.PlayerEntity.Items.Contains(ItemGroups.MiscItems, (int)MiscItems.Spellbook))
                {
                    DaggerfallUI.MessageBox(noSpellBook);
                }
                else if (GameManager.Instance.PlayerEntity.GetGoldAmount() < tradePrice)
                {
                    DaggerfallUI.MessageBox(notEnoughGoldId);
                }
                else if (GameManager.Instance.PlayerEntity.Stats.LiveIntelligence < spellCost.spellPointCost)
                {
                    DaggerfallUI.MessageBox("At present, you are unable to master this spell. Either increase your intelligence or your mastery of the relevant school of magic.");
                }
                else
                {
                    if (presentedCost >> 1 <= tradePrice)
                    {
                        if (presentedCost - (presentedCost >> 2) <= tradePrice)
                            msgOffset = 2;
                        else
                            msgOffset = 1;
                    }

                    DaggerfallMessageBox messageBox = new DaggerfallMessageBox(uiManager, this);
                    TextFile.Token[] tokens = DaggerfallUnity.Instance.TextProvider.GetRandomTokens(tradeMessageBaseId + msgOffset);
                    messageBox.SetTextTokens(tokens, this);
                    messageBox.AddButton(DaggerfallMessageBox.MessageBoxButtons.Yes);
                    messageBox.AddButton(DaggerfallMessageBox.MessageBoxButtons.No);
                    messageBox.OnButtonClick += ConfirmTrade_OnButtonClick;
                    uiManager.PushWindow(messageBox);
                }
            }
            else
            {
                base.BuyButton_OnMouseClick(sender, position);
            }
        }

    }
}