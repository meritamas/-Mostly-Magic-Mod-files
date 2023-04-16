// Project:         MeriTamas's (Mostly) Magic Mod for Daggerfall Unity (http://www.dfworkshop.net)
// Copyright:       Copyright (C) 2023 meritamas
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Author:          meritamas (meritamas@meritamas.net) - ceated based on other mod authors' work

// Original copyright notice for the code meritamas used as a source:
// Project:         MeanerMonsters mod for Daggerfall Unity (http://www.dfworkshop.net)
// Copyright:       Copyright (C) 2020 Ralzar
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Author:          Hazelnut & Ralzar


using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using DaggerfallWorkshop.Utility;
using UnityEngine;

namespace MTMMM
{
    public static class MTMeanerMonsters
    {
        static string messagePrefix = "MTMeanerMonsters: ";

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

        // [Invoke(StateManager.StateTypes.Start, 0)]
        public static void Init(InitParams initParams)
        {
            
            //Mod unleveledMobs = ModManager.Instance.GetMod("Unleveled Mobs");
            //mod = initParams.Mod;
            //var go = new GameObject(mod.Title);
            //go.AddComponent<MeanerMonsters>();

            /* THIS PART WILL PROBABLY NOT BE NEEDED - integrate into UnleveledMobs part
            if (unleveledMobs != null)
            {

                Debug.Log("[Meaner Monsters] Unleveled Mobs wilderness spawn: add Alternate Dragonling");

                RandomEncounters.EncounterTables[21] = new RandomEncounterTable()
                {
                    Enemies = new MobileTypes[]
                    {
                    MobileTypes.GiantScorpion,
                    MobileTypes.Nymph,
                    MobileTypes.GiantBat,
                    MobileTypes.Centaur,
                    MobileTypes.Orc,
                    MobileTypes.Dragonling,
                    MobileTypes.Dragonling_Alternate,       // MT: should be rare and mostly only in caves
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
                    MobileTypes.Dragonling_Alternate,
                    MobileTypes.GiantScorpion,
                    MobileTypes.Nymph,
                    },
                };

                RandomEncounters.EncounterTables[36] = new RandomEncounterTable()
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
                    MobileTypes.Dragonling_Alternate,       // MT: should be rare and mostly only in caves
                    MobileTypes.Harpy,
                    MobileTypes.Giant,
                    MobileTypes.Imp,
                    MobileTypes.GrizzlyBear,
                    },
                };
            }
            else { Debug.Log("[Meaner Monsters] Unleveled Mobs not active.");  } */
        }

        /*void Awake()      THIS WILL NOT BE NEEDED
        {
            InitMod();

            mod.IsReady = true;
        }*/

        public static void InitMod()
        {
            SilentMessage("Begin init: MMM integrated version of Hazelnut & Ralzar's MeanerMonsters");

            //Iterate over the new mob enemy data array and load into DFU enemies data.
            foreach (EnemyData mobData in mobEnemyDataArray)
            {
                // Log a message indicating the enemy mob being updated.
                SilentMessage("Updating enemy data for {0}.", EnemyBasics.Enemies[mobData.ID]);
                //Debug.LogFormat("Updating enemy data for {0}.", EnemyBasics.Enemies[mobData.ID].Name);

                if (mobData.Level != -1)
                    EnemyBasics.Enemies[mobData.ID].Level = mobData.Level;
                //if (mobData.Name != "")
                //    EnemyBasics.Enemies[mobData.ID].Name = mobData.Name;
                if (mobData.MinHp != -1)
                    EnemyBasics.Enemies[mobData.ID].MinHealth = mobData.MinHp;
                if (mobData.MaxHp != -1)
                    EnemyBasics.Enemies[mobData.ID].MaxHealth = mobData.MaxHp;
                if (mobData.Armor != -1)
                    EnemyBasics.Enemies[mobData.ID].ArmorValue = mobData.Armor;
                if (mobData.MinDmg != -1)
                    EnemyBasics.Enemies[mobData.ID].MinDamage = mobData.MinDmg;
                if (mobData.MaxDmg != -1)
                    EnemyBasics.Enemies[mobData.ID].MaxDamage = mobData.MaxDmg;
                if (mobData.MinDmg2 != -1)
                    EnemyBasics.Enemies[mobData.ID].MinDamage2 = mobData.MinDmg2;
                if (mobData.MaxDmg2 != -1)
                    EnemyBasics.Enemies[mobData.ID].MaxDamage2 = mobData.MaxDmg2;
                if (mobData.MinDmg3 != -1)
                    EnemyBasics.Enemies[mobData.ID].MinDamage3 = mobData.MinDmg3;
                if (mobData.MaxDmg3 != -1)
                    EnemyBasics.Enemies[mobData.ID].MaxDamage3 = mobData.MaxDmg3;
                if (mobData.MoveSnd != -1)
                    EnemyBasics.Enemies[mobData.ID].MoveSound = mobData.MoveSnd;
                if (mobData.BarkSnd != -1)
                    EnemyBasics.Enemies[mobData.ID].BarkSound = mobData.BarkSnd;
                if (mobData.AttackSnd != -1)
                    EnemyBasics.Enemies[mobData.ID].AttackSound = mobData.AttackSnd;
                if (mobData.CorpseTex != -1)
                    EnemyBasics.Enemies[mobData.ID].CorpseTexture = mobData.CorpseTex;
            }

            SilentMessage("Finished init: MMM integrated version of Hazelnut & Ralzar's MeanerMonsters");
        }

        public static int CorpseTexture(int archive, int record)
        {
            return ((archive << 16) + record);
        }

        private class EnemyData
        {
            public int ID { get { return id; } }          // ID of this mobile
            public string Name { get { return name; } }     // Monster Name
            public int Level { get { return level; } }       // Monster Level
            public int MinHp { get { return minHp; } }       // Minimum health
            public int MaxHp { get { return maxHp; } }       // Maximum health
            public int Armor { get { return armor; } }       // Armor value
            public int MinDmg { get { return minDmg; } }      // Minimum damage per first hit of attack
            public int MaxDmg { get { return maxDmg; } }      // Maximum damage per first hit of attack
            public int MinDmg2 { get { return minDmg2; } }     // Minimum damage per second hit of attack
            public int MaxDmg2 { get { return maxDmg2; } }     // Maximum damage per second hit of attack
            public int MinDmg3 { get { return minDmg3; } }     // Minimum damage per third hit of attack
            public int MaxDmg3 { get { return maxDmg3; } }     // Maximum damage per third hit of attack
            public int MoveSnd { get { return moveSnd; } }     // Movement sound file
            public int BarkSnd { get { return barkSnd; } }     // Bark sound file
            public int AttackSnd { get { return attackSnd; } }   // Attack sound file
            public int CorpseTex { get { return corpseTex; } }  // Monster Corpse texture

            private readonly int id, level, minHp, maxHp, armor;
            private readonly string name;
            private readonly int minDmg, maxDmg, minDmg2, maxDmg2, minDmg3, maxDmg3;
            private readonly int moveSnd, barkSnd, attackSnd, corpseTex;

            public EnemyData(int id = -1, string name = "", int level = -1, int minHp = -1, int maxHp = -1, int armor = -1,
                            int minDmg = -1, int maxDmg = -1, int minDmg2 = -1, int maxDmg2 = -1, int minDmg3 = -1, int maxDmg3 = -1,
                            int moveSnd = -1, int barkSnd = -1, int attackSnd = -1, int corpseTex = -1)
            {
                this.id = id;
                this.name = name;
                this.level = level;
                this.minHp = minHp;
                this.maxHp = maxHp;
                this.armor = armor;
                this.minDmg = minDmg;
                this.maxDmg = maxDmg;
                this.minDmg2 = minDmg2;
                this.maxDmg2 = maxDmg2;
                this.minDmg3 = minDmg3;
                this.maxDmg3 = maxDmg3;
                this.moveSnd = moveSnd;
                this.barkSnd = barkSnd;
                this.attackSnd = attackSnd;
                this.corpseTex = corpseTex;
            }

        }

        private static EnemyData[] mobEnemyDataArray = new EnemyData[]
        {
            new EnemyData
            (   // Rat
                id: 0,
                minDmg: 1, maxDmg: 4,
                minHp: 15, maxHp: 25,
                level: 1,
                armor: 8
            ),                                                  
            new EnemyData
            (   // Giant Bat
                id: 3,
                minDmg: 1, maxDmg: 4,
                minHp: 5, maxHp: 15,
                level:2,
                armor: 4                                                                 
                //moveSnd: (int)SoundClips.EnemyGiantMove  // <- example of setting a level and a sound clip.
            ),
            new EnemyData
            (   // Grizzly Bear
                id: 4,
                minDmg: 1, maxDmg: 2,
                minDmg2: 8, maxDmg2: 12,
                minDmg3: 10, maxDmg3: 20,
                minHp: 50, maxHp: 100,
                armor: 7
            ),  
            new EnemyData
            (   // Sabertooth Tiger
                id: 5,
                minDmg: 8, maxDmg: 15,
                minDmg2: 8, maxDmg2: 20,
                minDmg3: 10, maxDmg3: 25,
                minHp: 40, maxHp: 80,
                armor: 2
            ),
            new EnemyData
            (   // Spider
                id: 6,
                minDmg: 4, maxDmg: 10,
                minHp: 20, maxHp: 50,
                level: 2,
                armor: 6
            ),
            new EnemyData
            (   // Werewolf
                id: 9,
                minDmg : 8, maxDmg: 10,
                minDmg2: 8, maxDmg2: 10,
                minDmg3: 15, maxDmg3: 30,
                minHp: 25, maxHp: 50,
                level: 8,
                armor: 1
            ),
            new EnemyData
            (   // Wereboar
                id: 14,
                minDmg: 5, maxDmg: 8,
                minDmg2: 5, maxDmg2: 8,
                minDmg3: 10, maxDmg3: 25,
                minHp: 80, maxHp: 120,
                level: 8,
                armor: 7
            ),
            new EnemyData
            (   // Giant
                id: 16,
                minDmg: 10, maxDmg: 30,
                minHp: 150, maxHp: 200,
                level: 10,
                armor: 5
            ),
            new EnemyData
            (   // Zombie
                id: 17,
                minDmg: 1, maxDmg: 5,
                minHp: 60, maxHp: 100,
                level: 5,
                armor: 7
            ),
            new EnemyData
            (   // Mummy
                id: 19,
                minDmg: 5, maxDmg: 15,
                minHp: 120, maxHp: 190,
                level: 15,
                armor: -2
            ),
            new EnemyData
            (   // Giant Scorpion
                id: 20,
                minDmg: 10, maxDmg: 15,
                minHp: 15, maxHp: 70,
                level: 4,
                armor: 2
            ),
            new EnemyData
            (   // Vampire Ancient
                id: 30,
                minDmg: 25, maxDmg: 60,
                minHp: 80, maxHp: 200,
                level: 20,
                armor: -5
            ),
            new EnemyData
            (   // Daedra Lord
                id: 31,
                minDmg: 40, maxDmg: 100,
                minHp: 100, maxHp: 240,
                level: 21,
                armor: -10
            ),
            new EnemyData
            (   // Lich
                id: 32,
                minDmg: 80, maxDmg: 110,
                minHp: 60, maxHp: 200,
                level: 20,
                armor: -8
            ),
            new EnemyData
            (   // Ancient Lich
                id: 33,
                minDmg: 100, maxDmg: 130,
                minHp: 80, maxHp: 210,
                level: 21,
                armor: -12
            ),
            new EnemyData
            (   // Orc
                id: 7,
                minDmg: 8, maxDmg: 15,
                minHp: 30, maxHp: 60,
                level: 6,
                armor: 5
            ),
            new EnemyData
            (   // Orc Sargeant
                id: 12,
                minDmg: 10, maxDmg: 30,
                minHp: 40, maxHp: 100,
                level: 9,
                armor: 2
            ),
            new EnemyData
            (   // Orc Shaman
                id: 21,
                minDmg: 8, maxDmg: 30,
                minHp: 50, maxHp: 110,
                level: 15,
                armor: 6
            ),
            new EnemyData
            (   // Orc Warlord
                id: 24,
                minDmg: 15, maxDmg: 50,
                minHp: 80, maxHp: 150,
                level: 19,
                armor: -5
            ),
            new EnemyData
            (   // Fire Atronach
                id: 35,
                minDmg: 15, maxDmg: 30,
                minHp: 25, maxHp: 130,
                level: 16,
                armor: 6
            ),
            new EnemyData
            (   // Iron Atronach
                id: 35,
                minDmg: 5, maxDmg: 15,
                minHp: 25, maxHp: 130,
                level: 16,
                armor: -2
            ),
            new EnemyData
            (   // Flesh Atronach
                id: 35,
                minDmg: 5, maxDmg: 15,     // MT corr.: minDmg 55=>5
                minHp: 150, maxHp: 350,
                level: 16,
                armor: 6
            ),
            new EnemyData
            (   // Ice Atronach
                id: 35,
                minDmg: 5, maxDmg: 15,
                minHp: 25, maxHp: 130,
                level: 16,              // MT corr.
                armor: 6
            ),
            new EnemyData
            (   // Dragon
                name: "Large Dragonling",
                id: 40,
                minDmg: 70, maxDmg: 210,    // MT increased by 40%
                minHp: 210, maxHp: 375,     // MT increased by 50%
                level: 21,
                armor: -12
            ),
        };       
    }
}