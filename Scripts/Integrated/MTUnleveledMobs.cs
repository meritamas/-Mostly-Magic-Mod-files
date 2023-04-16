// Project:         MeriTamas's (Mostly) Magic Mod for Daggerfall Unity (http://www.dfworkshop.net)
// Copyright:       Copyright (C) 2023 meritamas
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Author:          meritamas (meritamas@meritamas.net) - ceated based on other mod author's work

// Original copyright notice for the code meritamas used as a source:
// Project:         UnleveledMobs mod for Daggerfall Unity (http://www.dfworkshop.net)
// Copyright:       Copyright (C) 2019 JayH
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Author:          JayH

using DaggerfallConnect;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using DaggerfallWorkshop.Utility;
using UnityEngine;
using System.Collections.Generic;   //for lists

namespace MTMMM
{
    public static class MTUnleveledMobs
    {
        static string messagePrefix = "MTUnleveledMobs: ";

        static void Message(string message)
        {
            MTMostlyMagicMod.Message(messagePrefix + message);
        }

        static void SilentMessage(string message)
        {
            MTMostlyMagicMod.SilentMessage(messagePrefix + message);
        }

        static void SilentMessage(string message, params object[] args)
        {
            MTMostlyMagicMod.SilentMessage(messagePrefix + message, args);
        }
        // static Mod mod;
        static RandomEncounterTable[] vanillaTables = RandomEncounters.EncounterTables;
        static RandomEncounterTable[] unleveledTables;

        /*[Invoke(StateManager.StateTypes.Start, 0)] NOT NEEDED
        public static void Init(InitParams initParams)
        {
            //mod = initParams.Mod;

            // SetTables();
            //PlayerEnterExit.OnPreTransition += OnTransition;

            //mod.IsReady = true;
        }*/

        /*private static int GetScalingMax() THIS WILL NOT BE NEEDED
        {
            return mod.GetSettings().GetValue<int>("Section", "Scaling");
        }*/

        public static void UpdateTables()
        {
            SilentMessage("Begin updating: UnleveledMobs");

            //calculate scaling percent
            //float percent = 100f * level / GetScalingMax();
            //if (percent > 100f) percent = 100;

            // Set new encounter tables.
            RandomEncounters.EncounterTables = unleveledTables;
           //     (percent == 0f) ? vanillaTables :       //vanilla
           //     (percent == 100f) ? unleveledTables :   //unleveled
           //     GetMixed(percent);                      //mixed

            SilentMessage("Finished updating: UnleveledMobs");
        }

        /*private static RandomEncounterTable[] GetMixed(float percent) THIS WILL NOT BE NEEDED
        {
            List<RandomEncounterTable> tables = new List<RandomEncounterTable>();

            for (int i = 0; i < vanillaTables.Length; i++)
            {
                //pick enemies
                MobileTypes[] vanillaEnemies = vanillaTables[i].Enemies;
                MobileTypes[] unleveledEnemies = unleveledTables[i].Enemies;

                List<MobileTypes> enemies = new List<MobileTypes>();
                for (int j = 0; j < vanillaEnemies.Length; j++)
                {
                    //pick enemy using scaling as chances
                    MobileTypes enemy =
                        (DFRandom.random_range(100) < percent) ?
                        unleveledEnemies[DFRandom.random_range(unleveledEnemies.Length)] :   //unleveled
                        vanillaEnemies[j];    //vanilla

                    //save enemy
                    enemies.Add(enemy);
                }

                //save table
                tables.Add(new RandomEncounterTable()
                {
                    DungeonType = vanillaTables[i].DungeonType,
                    Enemies = enemies.ToArray()
                });
            }

            return tables.ToArray();
        } */

        /*public static void OnTransition(PlayerEnterExit.TransitionEventArgs args) THIS WILL NOT BE NEEDED
        {
            UpdateTables(GameManager.Instance.PlayerEntity.Level);
        } */

        public static void SetTables()
        {
            unleveledTables = (RandomEncounterTable[])vanillaTables.Clone();

            // Crypt - Index0
            unleveledTables[(int)DFRegion.DungeonTypes.Crypt] = new RandomEncounterTable()
            {
                DungeonType = DFRegion.DungeonTypes.Crypt,
                Enemies = new MobileTypes[]
                {
                    MobileTypes.SkeletalWarrior,
                    MobileTypes.Mummy,
                    MobileTypes.Ghost,
                    MobileTypes.Wraith,
                    MobileTypes.SkeletalWarrior,
                    MobileTypes.Lich,
                    MobileTypes.Ghost,
                    MobileTypes.Wraith,
                    MobileTypes.Vampire,
                    MobileTypes.Mummy,
                    MobileTypes.SkeletalWarrior,
                    MobileTypes.Wraith,
                    MobileTypes.Lich,
                    MobileTypes.Mummy,
                    MobileTypes.Ghost,
                    MobileTypes.Wraith,
                    MobileTypes.SkeletalWarrior,
                    MobileTypes.Mummy,
                    MobileTypes.Ghost,
                    MobileTypes.Wraith,
                },
            };

            // Orc Stronghold - Index1
            unleveledTables[(int)DFRegion.DungeonTypes.OrcStronghold] = new RandomEncounterTable()
            {
                DungeonType = DFRegion.DungeonTypes.OrcStronghold,
                Enemies = new MobileTypes[]
                {
                    MobileTypes.Orc,
                    MobileTypes.OrcWarlord,
                    MobileTypes.Orc,
                    MobileTypes.Orc,
                    MobileTypes.OrcSergeant,
                    MobileTypes.Orc,
                    MobileTypes.Orc,
                    MobileTypes.OrcShaman,
                    MobileTypes.Orc,
                    MobileTypes.OrcWarlord,
                    MobileTypes.OrcSergeant,
                    MobileTypes.Orc,
                    MobileTypes.Orc,
                    MobileTypes.Orc,
                    MobileTypes.Orc,
                    MobileTypes.OrcSergeant,
                    MobileTypes.OrcShaman,
                    MobileTypes.Warrior,            // MT add a humanoid to the mix
                    MobileTypes.Orc,
                    MobileTypes.OrcWarlord,
                },
            };

            // Human Stronghold - Index2
            unleveledTables[(int)DFRegion.DungeonTypes.HumanStronghold] = new RandomEncounterTable()
            {
                DungeonType = DFRegion.DungeonTypes.HumanStronghold,
                Enemies = new MobileTypes[]
                {
                    MobileTypes.Warrior,
                    MobileTypes.Knight,
                    MobileTypes.Archer,
                    MobileTypes.Spellsword,
                    MobileTypes.Archer,
                    MobileTypes.Warrior,
                    MobileTypes.Knight,
                    MobileTypes.Archer,
                    MobileTypes.Spellsword,
                    MobileTypes.Archer,
                    MobileTypes.Warrior,
                    MobileTypes.Knight,
                    MobileTypes.Archer,
                    MobileTypes.Spellsword,
                    MobileTypes.Archer,
                    MobileTypes.Warrior,
                    MobileTypes.Knight,
                    MobileTypes.Archer,
                    MobileTypes.Spellsword,
                    MobileTypes.Archer,
                },
            };

            // Prison - Index3
            unleveledTables[(int)DFRegion.DungeonTypes.Prison] = new RandomEncounterTable()
            {
                DungeonType = DFRegion.DungeonTypes.Prison,
                Enemies = new MobileTypes[]
                {
                    MobileTypes.Thief,
                    MobileTypes.Rogue,
                    MobileTypes.Nightblade,
                    MobileTypes.Thief,
                    MobileTypes.Burglar,
                    MobileTypes.Thief,
                    MobileTypes.Rogue,
                    MobileTypes.Nightblade,
                    MobileTypes.Thief,
                    MobileTypes.Burglar,
                    MobileTypes.Thief,
                    MobileTypes.Rogue,
                    MobileTypes.Nightblade,
                    MobileTypes.Thief,
                    MobileTypes.Burglar,
                    MobileTypes.Thief,
                    MobileTypes.Rogue,
                    MobileTypes.Nightblade,
                    MobileTypes.Thief,
                    MobileTypes.Burglar,
                },
            };

            // Desecrated Temple - Index4
            unleveledTables[(int)DFRegion.DungeonTypes.DesecratedTemple] = new RandomEncounterTable()
            {
                DungeonType = DFRegion.DungeonTypes.DesecratedTemple,
                Enemies = new MobileTypes[]
                {
                    MobileTypes.Imp,
                    MobileTypes.Lich,
                    MobileTypes.Mage,
                    MobileTypes.Healer,
                    MobileTypes.Imp,
                    MobileTypes.DaedraSeducer,
                    MobileTypes.AncientLich,
                    MobileTypes.Mage,
                    MobileTypes.Healer,
                    MobileTypes.Lich,
                    MobileTypes.Imp,
                    MobileTypes.Imp,
                    MobileTypes.Mage,
                    MobileTypes.Healer,
                    MobileTypes.AncientLich,
                    MobileTypes.DaedraSeducer,
                    MobileTypes.Lich,
                    MobileTypes.Mage,
                    MobileTypes.Healer,
                    MobileTypes.Imp,
                },
            };

            // Mine - Index5
            unleveledTables[(int)DFRegion.DungeonTypes.Mine] = new RandomEncounterTable()
            {
                DungeonType = DFRegion.DungeonTypes.Mine,
                Enemies = new MobileTypes[]
                {
                    MobileTypes.Rat,
                    MobileTypes.GiantBat,
                    MobileTypes.SabertoothTiger,
                    MobileTypes.GiantScorpion,
                    MobileTypes.Rat,
                    MobileTypes.GiantBat,
                    MobileTypes.SabertoothTiger,
                    MobileTypes.GiantScorpion,
                    MobileTypes.Rat,
                    MobileTypes.GiantBat,
                    MobileTypes.SabertoothTiger,
                    MobileTypes.GiantScorpion,
                    MobileTypes.Rat,
                    MobileTypes.GiantBat,
                    MobileTypes.SabertoothTiger,
                    MobileTypes.GiantScorpion,
                    MobileTypes.Rat,
                    MobileTypes.GiantBat,
                    MobileTypes.Warrior,            // MT add a humanoid to the mix
                    MobileTypes.GiantScorpion,
                },
            };

            // Natural Cave - Index6
            unleveledTables[(int)DFRegion.DungeonTypes.NaturalCave] = new RandomEncounterTable()
            {
                DungeonType = DFRegion.DungeonTypes.NaturalCave,
                Enemies = new MobileTypes[]
                {
                    MobileTypes.Rat,
                    MobileTypes.GiantBat,
                    MobileTypes.GrizzlyBear,
                    MobileTypes.SabertoothTiger,
                    MobileTypes.Healer,
                    MobileTypes.Spider,
                    MobileTypes.Rat,
                    MobileTypes.GiantBat,
                    MobileTypes.GrizzlyBear,
                    MobileTypes.SabertoothTiger,
                    MobileTypes.Healer,
                    MobileTypes.Spider,
                    MobileTypes.Rat,
                    MobileTypes.GiantBat,
                    MobileTypes.GrizzlyBear,
                    MobileTypes.Warrior,            // MT add a humanoid warrior to the mix
                    MobileTypes.Healer,
                    MobileTypes.Spider,
                    MobileTypes.Rat,
                    MobileTypes.GiantBat,
                },
            };

            // Coven - Index7
            unleveledTables[(int)DFRegion.DungeonTypes.Coven] = new RandomEncounterTable()
            {
                DungeonType = DFRegion.DungeonTypes.Coven,
                Enemies = new MobileTypes[]
                {
                    MobileTypes.DaedraSeducer,
                    MobileTypes.FireDaedra,
                    MobileTypes.FrostDaedra,
                    MobileTypes.Mage,
                    MobileTypes.Battlemage,
                    MobileTypes.Gargoyle,
                    MobileTypes.DaedraSeducer,
                    MobileTypes.FireDaedra,
                    MobileTypes.FrostDaedra,
                    MobileTypes.Sorcerer,
                    MobileTypes.Battlemage,
                    MobileTypes.Gargoyle,
                    MobileTypes.DaedraSeducer,
                    MobileTypes.FireDaedra,
                    MobileTypes.FrostDaedra,
                    MobileTypes.Mage,
                    MobileTypes.Battlemage,
                    MobileTypes.Gargoyle,
                    MobileTypes.DaedraSeducer,
                    MobileTypes.FireDaedra,
                },
            };

            // Vampire Haunt - Index8
            unleveledTables[(int)DFRegion.DungeonTypes.VampireHaunt] = new RandomEncounterTable()
            {
                DungeonType = DFRegion.DungeonTypes.VampireHaunt,
                Enemies = new MobileTypes[]
                {
                    MobileTypes.Vampire,
                    MobileTypes.VampireAncient,
                    MobileTypes.Mummy,
                    MobileTypes.Zombie,
                    MobileTypes.Vampire,                // MT: put in BattleMages, Sorcerers, things like that
                    MobileTypes.Vampire,
                    MobileTypes.Vampire,
                    MobileTypes.Mummy,
                    MobileTypes.Zombie,
                    MobileTypes.VampireAncient,
                    MobileTypes.Vampire,
                    MobileTypes.Vampire,
                    MobileTypes.Vampire,
                    MobileTypes.Vampire,
                    MobileTypes.Mummy,
                    MobileTypes.Zombie,
                    MobileTypes.Vampire,
                    MobileTypes.Vampire,
                    MobileTypes.Vampire,
                    MobileTypes.VampireAncient,
                },
            };

            // Laboratory - Index9
            unleveledTables[(int)DFRegion.DungeonTypes.Laboratory] = new RandomEncounterTable()
            {
                DungeonType = DFRegion.DungeonTypes.Laboratory,
                Enemies = new MobileTypes[]
                {
                    MobileTypes.FireAtronach,
                    MobileTypes.FleshAtronach,
                    MobileTypes.IronAtronach,
                    MobileTypes.IceAtronach,
                    MobileTypes.Spellsword,
                    MobileTypes.Mage,
                    MobileTypes.Battlemage,
                    MobileTypes.FireAtronach,
                    MobileTypes.FleshAtronach,
                    MobileTypes.IronAtronach,
                    MobileTypes.IceAtronach,
                    MobileTypes.Spellsword,
                    MobileTypes.Mage,
                    MobileTypes.Battlemage,
                    MobileTypes.FireAtronach,
                    MobileTypes.FleshAtronach,
                    MobileTypes.IronAtronach,
                    MobileTypes.IceAtronach,
                    MobileTypes.Spellsword,
                    MobileTypes.Mage,
                },
            };

            // Harpy Nest - Index10
            unleveledTables[(int)DFRegion.DungeonTypes.HarpyNest] = new RandomEncounterTable()
            {
                DungeonType = DFRegion.DungeonTypes.HarpyNest,
                Enemies = new MobileTypes[]
                {
                    MobileTypes.Rat,
                    MobileTypes.Harpy,
                    MobileTypes.GiantBat,
                    MobileTypes.Harpy,
                    MobileTypes.Harpy,
                    MobileTypes.Harpy,
                    MobileTypes.Rat,
                    MobileTypes.Harpy,
                    MobileTypes.GiantBat,
                    MobileTypes.Harpy,
                    MobileTypes.Harpy,
                    MobileTypes.Harpy,
                    MobileTypes.Rat,
                    MobileTypes.Harpy,
                    MobileTypes.GiantBat,
                    MobileTypes.Harpy,
                    MobileTypes.Harpy,
                    MobileTypes.Harpy,
                    MobileTypes.Warrior,            // MT add a humanoid to the mix - perhaps a healer?
                    MobileTypes.Harpy,
                },
            };

            // Ruined Castle - Index11
            unleveledTables[(int)DFRegion.DungeonTypes.RuinedCastle] = new RandomEncounterTable()
            {
                DungeonType = DFRegion.DungeonTypes.RuinedCastle,
                Enemies = new MobileTypes[]
                {
                    MobileTypes.Assassin,
                    MobileTypes.Bard,
                    MobileTypes.Barbarian,
                    MobileTypes.Orc,
                    MobileTypes.Warrior,
                    MobileTypes.Knight,
                    MobileTypes.Assassin,
                    MobileTypes.Bard,
                    MobileTypes.Barbarian,
                    MobileTypes.Orc,
                    MobileTypes.Warrior,
                    MobileTypes.Knight,
                    MobileTypes.Assassin,
                    MobileTypes.Bard,
                    MobileTypes.Barbarian,
                    MobileTypes.Orc,
                    MobileTypes.Warrior,
                    MobileTypes.Knight,
                    MobileTypes.Assassin,
                    MobileTypes.Bard,
                },
            };

            // Spider Nest - Index12
            unleveledTables[(int)DFRegion.DungeonTypes.SpiderNest] = new RandomEncounterTable()
            {
                DungeonType = DFRegion.DungeonTypes.SpiderNest,
                Enemies = new MobileTypes[]
                {
                    MobileTypes.Spider,
                    MobileTypes.Spider,
                    MobileTypes.GiantBat,
                    MobileTypes.Spider,
                    MobileTypes.Spider,
                    MobileTypes.Rat,
                    MobileTypes.Spider,
                    MobileTypes.Spider,
                    MobileTypes.GiantBat,
                    MobileTypes.Spider,
                    MobileTypes.Spider,
                    MobileTypes.Rat,
                    MobileTypes.Spider,
                    MobileTypes.Spider,
                    MobileTypes.GiantBat,
                    MobileTypes.Spider,
                    MobileTypes.Spider,
                    MobileTypes.Warrior,            // MT add a humanoid to the mix
                    MobileTypes.Spider,
                    MobileTypes.Spider,
                },
            };

            // Giant Stronghold - Index13
            unleveledTables[(int)DFRegion.DungeonTypes.GiantStronghold] = new RandomEncounterTable()
            {
                DungeonType = DFRegion.DungeonTypes.GiantStronghold,
                Enemies = new MobileTypes[]
                {
                    MobileTypes.Giant,
                    MobileTypes.Giant,
                    MobileTypes.Giant,
                    MobileTypes.Giant,
                    MobileTypes.SabertoothTiger,
                    MobileTypes.Gargoyle,
                    MobileTypes.Giant,
                    MobileTypes.Giant,
                    MobileTypes.Giant,
                    MobileTypes.Giant,
                    MobileTypes.SabertoothTiger,
                    MobileTypes.Giant,
                    MobileTypes.Giant,
                    MobileTypes.Gargoyle,
                    MobileTypes.Giant,
                    MobileTypes.Giant,
                    MobileTypes.Warrior,            // MT add a humanoid to the mix
                    MobileTypes.SabertoothTiger,
                    MobileTypes.Giant,
                    MobileTypes.Giant,
                },
            };

            // Dragon's Den - Index14
            unleveledTables[(int)DFRegion.DungeonTypes.DragonsDen] = new RandomEncounterTable()
            {
                DungeonType = DFRegion.DungeonTypes.DragonsDen,
                Enemies = new MobileTypes[]
                {
                    MobileTypes.Dragonling_Alternate,       // MT added large dragon
                    MobileTypes.Gargoyle,
                    MobileTypes.OrcWarlord,
                    MobileTypes.Knight,
                    MobileTypes.Giant,
                    MobileTypes.Werewolf,
                    MobileTypes.DaedraLord,
                    MobileTypes.Dragonling,
                    MobileTypes.Gargoyle,
                    MobileTypes.OrcWarlord,
                    MobileTypes.Knight,
                    MobileTypes.Giant,
                    MobileTypes.Wereboar,
                    MobileTypes.DaedraLord,
                    MobileTypes.Dragonling_Alternate,       // MT added dragon
                    MobileTypes.Gargoyle,
                    MobileTypes.OrcWarlord,
                    MobileTypes.Knight,
                    MobileTypes.Giant,
                    MobileTypes.Werewolf,
                },
            };

            // Barbarian Stronghold - Index15
            unleveledTables[(int)DFRegion.DungeonTypes.BarbarianStronghold] = new RandomEncounterTable()
            {
                DungeonType = DFRegion.DungeonTypes.BarbarianStronghold,
                Enemies = new MobileTypes[]
                {
                    MobileTypes.Warrior,
                    MobileTypes.Barbarian,
                    MobileTypes.Archer,
                    MobileTypes.Rogue,
                    MobileTypes.Werewolf,
                    MobileTypes.VampireAncient,     // MT: too many vampire-ancients perhaps
                    MobileTypes.Barbarian,
                    MobileTypes.Archer,
                    MobileTypes.Rogue,
                    MobileTypes.Wereboar,
                    MobileTypes.VampireAncient,
                    MobileTypes.Barbarian,
                    MobileTypes.Archer,
                    MobileTypes.Rogue,
                    MobileTypes.Werewolf,
                    MobileTypes.VampireAncient,
                    MobileTypes.Barbarian,
                    MobileTypes.Archer,
                    MobileTypes.Rogue,
                    MobileTypes.Wereboar,
                },
            };

            // Volcanic Caves - Index16
            unleveledTables[(int)DFRegion.DungeonTypes.VolcanicCaves] = new RandomEncounterTable()
            {
                DungeonType = DFRegion.DungeonTypes.VolcanicCaves,
                Enemies = new MobileTypes[]
                {
                    MobileTypes.FireAtronach,
                    MobileTypes.Mage,
                    MobileTypes.FireDaedra,
                    MobileTypes.Sorcerer,
                    MobileTypes.Barbarian,
                    MobileTypes.DaedraLord,
                    MobileTypes.FireAtronach,
                    MobileTypes.Mage,
                    MobileTypes.FireDaedra,
                    MobileTypes.Sorcerer,
                    MobileTypes.Barbarian,
                    MobileTypes.DaedraLord,
                    MobileTypes.FireAtronach,
                    MobileTypes.Mage,
                    MobileTypes.FireDaedra,
                    MobileTypes.Sorcerer,
                    MobileTypes.Barbarian,
                    MobileTypes.DaedraLord,
                    MobileTypes.FireAtronach,
                    MobileTypes.Mage,
                },
            };

            // Scorpion Nest - Index17
            unleveledTables[(int)DFRegion.DungeonTypes.ScorpionNest] = new RandomEncounterTable()
            {
                DungeonType = DFRegion.DungeonTypes.ScorpionNest,
                Enemies = new MobileTypes[]
                {
                    MobileTypes.GiantScorpion,
                    MobileTypes.GiantBat,
                    MobileTypes.GiantScorpion,
                    MobileTypes.Rat,
                    MobileTypes.GiantScorpion,
                    MobileTypes.Warrior,
                    MobileTypes.GiantScorpion,
                    MobileTypes.GiantBat,
                    MobileTypes.GiantScorpion,
                    MobileTypes.Rat,
                    MobileTypes.GiantScorpion,
                    MobileTypes.Warrior,
                    MobileTypes.GiantScorpion,
                    MobileTypes.GiantBat,
                    MobileTypes.GiantScorpion,
                    MobileTypes.Rat,
                    MobileTypes.GiantScorpion,
                    MobileTypes.Warrior,
                    MobileTypes.GiantScorpion,
                    MobileTypes.GiantBat,
                },
            };

            // Cemetery - Index18
            unleveledTables[(int)DFRegion.DungeonTypes.Cemetery] = new RandomEncounterTable()
            {
                Enemies = new MobileTypes[]
                {
                    MobileTypes.SkeletalWarrior,
                    MobileTypes.Burglar,
                    MobileTypes.Mummy,
                    MobileTypes.GiantBat,
                    MobileTypes.Thief,
                    MobileTypes.GiantBat,
                    MobileTypes.Warrior,            // MT add a humanoid to the mix
                    MobileTypes.Burglar,
                    MobileTypes.Zombie,
                    MobileTypes.SkeletalWarrior,
                    MobileTypes.Mummy,
                    MobileTypes.GiantBat,
                    MobileTypes.GiantBat,
                    MobileTypes.Zombie,
                    MobileTypes.GiantBat,
                    MobileTypes.GiantBat,
                    MobileTypes.Thief,
                    MobileTypes.SkeletalWarrior,
                    MobileTypes.Burglar,
                    MobileTypes.GiantBat,
                },
            };

            /*
            // Cemetery - DF Unity version
            unleveledTables[(int)DFRegion.DungeonTypes.OrcStronghold] = new RandomEncounterTable()
            {
                DungeonType = DFRegion.DungeonTypes.Cemetery,
                Enemies = new MobileTypes[]
                {
                    MobileTypes.Rat,
                    MobileTypes.SkeletalWarrior,
                    MobileTypes.GiantBat,
                    MobileTypes.Mummy,
                    MobileTypes.Spider,
                    MobileTypes.Zombie,
                    MobileTypes.Ghost,
                    MobileTypes.Wraith,
                    MobileTypes.Vampire,
                    MobileTypes.VampireAncient,
                    MobileTypes.Lich,
                },
            },*/

            // Underwater - Index19
            unleveledTables[19] = new RandomEncounterTable()
            {
                Enemies = new MobileTypes[]
                {
                    MobileTypes.Slaughterfish,
                    MobileTypes.Slaughterfish,
                    MobileTypes.IceAtronach,
                    MobileTypes.Dreugh,
                    MobileTypes.Slaughterfish,
                    MobileTypes.Lamia,
                    MobileTypes.Slaughterfish,
                    MobileTypes.Slaughterfish,
                    MobileTypes.IceAtronach,
                    MobileTypes.Dreugh,
                    MobileTypes.Slaughterfish,
                    MobileTypes.Lamia,
                    MobileTypes.Slaughterfish,
                    MobileTypes.Slaughterfish,
                    MobileTypes.IceAtronach,
                    MobileTypes.Dreugh,
                    MobileTypes.Slaughterfish,
                    MobileTypes.Lamia,
                    MobileTypes.Slaughterfish,
                    MobileTypes.Slaughterfish,
                },
            };

            // Desert, in location, night - Index20
            unleveledTables[20] = new RandomEncounterTable()
            {
                Enemies = new MobileTypes[]
                {
                    MobileTypes.GiantBat,
                    MobileTypes.Rat,
                    MobileTypes.GiantScorpion,
                    MobileTypes.Thief,
                    MobileTypes.Werewolf,
                    MobileTypes.Burglar,
                    MobileTypes.GiantBat,
                    MobileTypes.Rat,
                    MobileTypes.Vampire,
                    MobileTypes.Thief,
                    MobileTypes.Wereboar,
                    MobileTypes.Burglar,
                    MobileTypes.GiantBat,
                    MobileTypes.Rat,
                    MobileTypes.Vampire,
                    MobileTypes.Thief,
                    MobileTypes.Werewolf,
                    MobileTypes.Burglar,
                    MobileTypes.GiantBat,
                    MobileTypes.Rat,
                },
            };

            // Desert, not in location, day - Index21
            unleveledTables[21] = new RandomEncounterTable()
            {
                Enemies = new MobileTypes[]
                {
                    MobileTypes.GiantScorpion,
                    MobileTypes.Nymph,
                    MobileTypes.GiantBat,
                    MobileTypes.Centaur,
                    MobileTypes.Orc,
                    MobileTypes.Dragonling_Alternate,       // MT: should be rare, and strong as hell
                    MobileTypes.GiantScorpion,
                    MobileTypes.Nymph,
                    MobileTypes.GiantBat,
                    MobileTypes.Centaur,
                    MobileTypes.Orc,
                    MobileTypes.Dragonling,
                    MobileTypes.GiantScorpion,
                    MobileTypes.Nymph,
                    MobileTypes.GiantBat,
                    MobileTypes.Centaur,
                    MobileTypes.Orc,
                    MobileTypes.Dragonling,
                    MobileTypes.GiantScorpion,
                    MobileTypes.Nymph,
                },
            };

            // Desert, not in location, night - Index22
            unleveledTables[22] = new RandomEncounterTable()
            {
                Enemies = new MobileTypes[]
                {
                    MobileTypes.GiantScorpion,
                    MobileTypes.Gargoyle,
                    MobileTypes.GiantBat,
                    MobileTypes.Vampire,
                    MobileTypes.SkeletalWarrior,
                    MobileTypes.Zombie,
                    MobileTypes.Wraith,
                    MobileTypes.GiantScorpion,
                    MobileTypes.Gargoyle,
                    MobileTypes.GiantBat,
                    MobileTypes.Vampire,
                    MobileTypes.SkeletalWarrior,
                    MobileTypes.Zombie,
                    MobileTypes.Mummy,
                    MobileTypes.Ghost,
                    MobileTypes.Wraith,
                    MobileTypes.Gargoyle,
                    MobileTypes.Vampire,
                    MobileTypes.SkeletalWarrior,
                    MobileTypes.Zombie,
                },
            };

            // Mountain, in location, night - Index23
            unleveledTables[23] = new RandomEncounterTable()
            {
                Enemies = new MobileTypes[]
                {
                    MobileTypes.Rat,
                    MobileTypes.Warrior,
                    MobileTypes.Rogue,
                    MobileTypes.Thief,
                    MobileTypes.Nightblade,
                    MobileTypes.Assassin,
                    MobileTypes.Werewolf,
                    MobileTypes.Rat,
                    MobileTypes.Warrior,
                    MobileTypes.Rogue,
                    MobileTypes.Thief,
                    MobileTypes.Nightblade,
                    MobileTypes.Assassin,
                    MobileTypes.Wereboar,
                    MobileTypes.Rat,
                    MobileTypes.Warrior,
                    MobileTypes.Rogue,
                    MobileTypes.Thief,
                    MobileTypes.Nightblade,
                    MobileTypes.Assassin,
                },
            };

            // Mountain, not in location, day - Index24
            unleveledTables[24] = new RandomEncounterTable()
            {
                Enemies = new MobileTypes[]
                {
                    MobileTypes.Nymph,
                    MobileTypes.Giant,
                    MobileTypes.Rogue,
                    MobileTypes.OrcSergeant,
                    MobileTypes.Sorcerer,
                    MobileTypes.Gargoyle,
                    MobileTypes.Werewolf,
                    MobileTypes.Nymph,
                    MobileTypes.Giant,
                    MobileTypes.Rogue,
                    MobileTypes.OrcSergeant,
                    MobileTypes.Sorcerer,
                    MobileTypes.Gargoyle,
                    MobileTypes.Werewolf,
                    MobileTypes.Nymph,
                    MobileTypes.Giant,
                    MobileTypes.Rogue,
                    MobileTypes.OrcSergeant,
                    MobileTypes.Sorcerer,
                    MobileTypes.Gargoyle,
                },
            };

            // Mountain, not in location, night - Index25
            unleveledTables[25] = new RandomEncounterTable()
            {
                Enemies = new MobileTypes[]
                {
                    MobileTypes.Zombie,
                    MobileTypes.Daedroth,
                    MobileTypes.Vampire,
                    MobileTypes.Gargoyle,
                    MobileTypes.Mummy,
                    MobileTypes.Wraith,
                    MobileTypes.Daedroth,
                    MobileTypes.SkeletalWarrior,
                    MobileTypes.Zombie,
                    MobileTypes.Mummy,
                    MobileTypes.Gargoyle,
                    MobileTypes.Daedroth,
                    MobileTypes.Vampire,
                    MobileTypes.Zombie,
                    MobileTypes.SkeletalWarrior,
                    MobileTypes.Wraith,
                    MobileTypes.Daedroth,
                    MobileTypes.Vampire,
                    MobileTypes.Zombie,
                    MobileTypes.Mummy,
                },
            };

            // Rainforest, in location, night - Index26
            unleveledTables[26] = new RandomEncounterTable()
            {
                Enemies = new MobileTypes[]
                {
                    MobileTypes.Burglar,
                    MobileTypes.Thief,
                    MobileTypes.Bard,
                    MobileTypes.Vampire,
                    MobileTypes.Barbarian,
                    MobileTypes.Burglar,
                    MobileTypes.Thief,
                    MobileTypes.Werewolf,
                    MobileTypes.Acrobat,
                    MobileTypes.Vampire,
                    MobileTypes.Burglar,
                    MobileTypes.Thief,
                    MobileTypes.Bard,
                    MobileTypes.Acrobat,
                    MobileTypes.Wereboar,
                    MobileTypes.Vampire,
                    MobileTypes.Thief,
                    MobileTypes.Bard,
                    MobileTypes.Acrobat,
                    MobileTypes.Barbarian,
                },
            };

            // Rainforest, not in location, day - Index27
            unleveledTables[27] = new RandomEncounterTable()
            {
                Enemies = new MobileTypes[]
                {
                    MobileTypes.Nymph,
                    MobileTypes.Spriggan,
                    MobileTypes.SabertoothTiger,
                    MobileTypes.Spider,
                    MobileTypes.Gargoyle,
                    MobileTypes.Harpy,
                    MobileTypes.Nymph,
                    MobileTypes.Spriggan,
                    MobileTypes.SabertoothTiger,
                    MobileTypes.Spider,
                    MobileTypes.Gargoyle,
                    MobileTypes.Harpy,
                    MobileTypes.Nymph,
                    MobileTypes.Spriggan,
                    MobileTypes.SabertoothTiger,
                    MobileTypes.Spider,
                    MobileTypes.Gargoyle,
                    MobileTypes.Harpy,
                    MobileTypes.Nymph,
                    MobileTypes.Spriggan,
                },
            };

            // Rainforest, not in location, night - Index28
            unleveledTables[28] = new RandomEncounterTable()
            {
                Enemies = new MobileTypes[]
                {
                    MobileTypes.Wraith,
                    MobileTypes.Ghost,
                    MobileTypes.Zombie,
                    MobileTypes.Spider,
                    MobileTypes.Vampire,
                    MobileTypes.SkeletalWarrior,
                    MobileTypes.Gargoyle,
                    MobileTypes.Wraith,
                    MobileTypes.Ghost,
                    MobileTypes.Zombie,
                    MobileTypes.Spider,
                    MobileTypes.Vampire,
                    MobileTypes.Gargoyle,
                    MobileTypes.SkeletalWarrior,
                    MobileTypes.Wraith,
                    MobileTypes.Ghost,
                    MobileTypes.Zombie,
                    MobileTypes.Spider,
                    MobileTypes.Vampire,
                    MobileTypes.SkeletalWarrior,
                },
            };

            // Subtropical, in location, night - Index29
            unleveledTables[29] = new RandomEncounterTable()
            {
                Enemies = new MobileTypes[]
                {
                    MobileTypes.Burglar,
                    MobileTypes.Thief,
                    MobileTypes.Bard,
                    MobileTypes.Vampire,
                    MobileTypes.Barbarian,
                    MobileTypes.Wereboar,
                    MobileTypes.Thief,
                    MobileTypes.Bard,
                    MobileTypes.Acrobat,
                    MobileTypes.Vampire,
                    MobileTypes.Burglar,
                    MobileTypes.Wereboar,
                    MobileTypes.Bard,
                    MobileTypes.Acrobat,
                    MobileTypes.Barbarian,
                    MobileTypes.Vampire,
                    MobileTypes.Thief,
                    MobileTypes.Bard,
                    MobileTypes.Wereboar,
                    MobileTypes.Barbarian,
                },
            };

            // Subtropical, not in location, day - Index30
            unleveledTables[30] = new RandomEncounterTable()
            {
                Enemies = new MobileTypes[]
                {
                    MobileTypes.Rogue,
                    MobileTypes.Nymph,
                    MobileTypes.SabertoothTiger,
                    MobileTypes.Spider,
                    MobileTypes.Gargoyle,
                    MobileTypes.Harpy,
                    MobileTypes.Barbarian,
                    MobileTypes.Nymph,
                    MobileTypes.SabertoothTiger,
                    MobileTypes.Spider,
                    MobileTypes.Gargoyle,
                    MobileTypes.Harpy,
                    MobileTypes.Archer,
                    MobileTypes.Nymph,
                    MobileTypes.SabertoothTiger,
                    MobileTypes.Spider,
                    MobileTypes.Gargoyle,
                    MobileTypes.Harpy,
                    MobileTypes.Thief,
                    MobileTypes.Nymph,
                },
            };

            // Subtropical, not in location, night - Index31
            unleveledTables[31] = new RandomEncounterTable()
            {
                Enemies = new MobileTypes[]
                {
                    MobileTypes.Wraith,
                    MobileTypes.Ghost,
                    MobileTypes.Zombie,
                    MobileTypes.Spider,
                    MobileTypes.Vampire,
                    MobileTypes.Gargoyle,
                    MobileTypes.SkeletalWarrior,
                    MobileTypes.Wraith,
                    MobileTypes.Ghost,
                    MobileTypes.Zombie,
                    MobileTypes.Spider,
                    MobileTypes.Vampire,
                    MobileTypes.Gargoyle,
                    MobileTypes.SkeletalWarrior,
                    MobileTypes.Wraith,
                    MobileTypes.Ghost,
                    MobileTypes.Zombie,
                    MobileTypes.Spider,
                    MobileTypes.Vampire,
                    MobileTypes.Gargoyle,
                },
            };

            // Swamp/woodlands, in location, night - Index32
            unleveledTables[32] = new RandomEncounterTable()
            {
                Enemies = new MobileTypes[]
                {
                    MobileTypes.Burglar,
                    MobileTypes.Thief,
                    MobileTypes.Bard,
                    MobileTypes.Vampire,
                    MobileTypes.Werewolf,
                    MobileTypes.Burglar,
                    MobileTypes.Thief,
                    MobileTypes.Bard,
                    MobileTypes.Wereboar,
                    MobileTypes.Vampire,
                    MobileTypes.Burglar,
                    MobileTypes.Thief,
                    MobileTypes.Werewolf,
                    MobileTypes.Acrobat,
                    MobileTypes.Barbarian,
                    MobileTypes.Vampire,
                    MobileTypes.Thief,
                    MobileTypes.Wereboar,
                    MobileTypes.Acrobat,
                    MobileTypes.Werewolf,
                },
            };

            // Swamp/woodlands, not in location, day - Index33
            unleveledTables[33] = new RandomEncounterTable()
            {
                Enemies = new MobileTypes[]
                {
                    MobileTypes.Giant,
                    MobileTypes.Werewolf,
                    MobileTypes.Spriggan,
                    MobileTypes.Orc,
                    MobileTypes.Centaur,
                    MobileTypes.Dragonling,
                    MobileTypes.OrcSergeant,
                    MobileTypes.Wereboar,
                    MobileTypes.Giant,
                    MobileTypes.Werewolf,
                    MobileTypes.OrcShaman,
                    MobileTypes.Bard,
                    MobileTypes.Werewolf,
                    MobileTypes.Archer,
                    MobileTypes.Battlemage,
                    MobileTypes.Assassin,
                    MobileTypes.Wereboar,
                    MobileTypes.Werewolf,
                    MobileTypes.Knight,
                    MobileTypes.Spellsword,
                },
            };

            // Swamp/woodlands, not in location, night - Index34
            unleveledTables[34] = new RandomEncounterTable()
            {
                Enemies = new MobileTypes[]
                {
                    MobileTypes.Wraith,
                    MobileTypes.Ghost,
                    MobileTypes.Zombie,
                    MobileTypes.Werewolf,
                    MobileTypes.Vampire,
                    MobileTypes.Gargoyle,
                    MobileTypes.SkeletalWarrior,
                    MobileTypes.Wraith,
                    MobileTypes.Ghost,
                    MobileTypes.Zombie,
                    MobileTypes.Wereboar,
                    MobileTypes.Vampire,
                    MobileTypes.Gargoyle,
                    MobileTypes.SkeletalWarrior,
                    MobileTypes.Wraith,
                    MobileTypes.Ghost,
                    MobileTypes.Zombie,
                    MobileTypes.Werewolf,
                    MobileTypes.Vampire,
                    MobileTypes.Gargoyle,
                },
            };

            // Haunted woodlands, in location, night - Index35
            unleveledTables[35] = new RandomEncounterTable()
            {
                Enemies = new MobileTypes[]
                {
                    MobileTypes.Burglar,
                    MobileTypes.Thief,
                    MobileTypes.Bard,
                    MobileTypes.Vampire,
                    MobileTypes.Barbarian,
                    MobileTypes.Burglar,
                    MobileTypes.Thief,
                    MobileTypes.Bard,
                    MobileTypes.Acrobat,
                    MobileTypes.Vampire,
                    MobileTypes.Burglar,
                    MobileTypes.Thief,
                    MobileTypes.Bard,
                    MobileTypes.Acrobat,
                    MobileTypes.Barbarian,
                    MobileTypes.Vampire,
                    MobileTypes.Thief,
                    MobileTypes.Bard,
                    MobileTypes.Acrobat,
                    MobileTypes.Barbarian,
                },
            };

            // Haunted woodlands, not in location, day - Index36
            unleveledTables[36] = new RandomEncounterTable()
            {
                Enemies = new MobileTypes[]
                {
                    MobileTypes.Imp,
                    MobileTypes.GrizzlyBear,
                    MobileTypes.Spriggan,
                    MobileTypes.Spider,
                    MobileTypes.Centaur,
                    MobileTypes.Nymph,
                    MobileTypes.Dragonling,
                    MobileTypes.Harpy,
                    MobileTypes.Giant,
                    MobileTypes.Imp,
                    MobileTypes.GrizzlyBear,
                    MobileTypes.Spriggan,
                    MobileTypes.Spider,
                    MobileTypes.Centaur,
                    MobileTypes.Nymph,
                    MobileTypes.Dragonling,
                    MobileTypes.Harpy,
                    MobileTypes.Giant,
                    MobileTypes.Imp,
                    MobileTypes.GrizzlyBear,
                },
            };

            // Haunted woodlands, not in location, night - Index37
            unleveledTables[37] = new RandomEncounterTable()
            {
                Enemies = new MobileTypes[]
                {
                    MobileTypes.Wraith,
                    MobileTypes.Ghost,
                    MobileTypes.Gargoyle,
                    MobileTypes.Spider,
                    MobileTypes.Vampire,
                    MobileTypes.SkeletalWarrior,
                    MobileTypes.Zombie,
                    MobileTypes.Wraith,
                    MobileTypes.Ghost,
                    MobileTypes.Gargoyle,
                    MobileTypes.Spider,
                    MobileTypes.Vampire,
                    MobileTypes.SkeletalWarrior,
                    MobileTypes.Zombie,
                    MobileTypes.Wraith,
                    MobileTypes.Gargoyle,
                    MobileTypes.Ghost,
                    MobileTypes.Spider,
                    MobileTypes.Vampire,
                    MobileTypes.SkeletalWarrior,
                },
            };

        }
    }
}
