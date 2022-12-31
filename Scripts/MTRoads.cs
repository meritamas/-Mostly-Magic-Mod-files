// Project:         MeriTamas's (Mostly) Magic Mod for Daggerfall Unity (http://www.dfworkshop.net)
// Copyright:       Copyright (C) 2022 meritamas
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Author:          meritamas (meritamas@outlook.com)

using System;
using DaggerfallConnect;
using DaggerfallConnect.Arena2;
using DaggerfallWorkshop;

namespace MTMMM
{
    public class MTRoads
    {
        struct BuildingTypeCounter
        {
            public int Type;
            public int Count;
        }

        static string[] RegionNames = MapsFile.RegionNames;        
        static DFRegion ourDFRegion;
        static DFLocation ourDFLocation;

        public static void LoadLocation (string regionName, string locationName)
        {
            ourDFLocation = DaggerfallUnity.Instance.ContentReader.MapFileReader.GetLocation(regionName, locationName);
        }

        public static string LocationsAndTypesInRegion (string regionName)
        {
            return " NULL ";
        }

        public static string PrintLocationCharacteristics()
        {
            string stringToReturn = "";
            stringToReturn += ourDFLocation.Name + ", " + ourDFLocation.RegionName + ", HasDungeon=" + ourDFLocation.HasDungeon + Environment.NewLine;
            stringToReturn += "LocationType=" + ourDFLocation.MapTableData.LocationType + ", DungeonType=" + ourDFLocation.MapTableData.DungeonType + Environment.NewLine;
            stringToReturn += "BuildingCount=" + ourDFLocation.Exterior.BuildingCount+", number of entries in Buildings: "+ ourDFLocation.Exterior.Buildings.Length+Environment.NewLine;

            int counterPopulation = 0;
            BuildingTypeCounter[] counter = new BuildingTypeCounter[50];
            
            for (int i = 0; i < ourDFLocation.Exterior.BuildingCount; i++)
            {
                bool found = false;
                DFLocation.BuildingTypes type = ourDFLocation.Exterior.Buildings[i].BuildingType;
                int j=0;
                for (j=0; j<counterPopulation; j++)
                {
                    if (counter[j].Type == (int) type)
                    {
                        counter[j].Count++;
                        found = true;
                        break;
                    }
                }
                
                if (found)
                    counter[j].Count++;
                else
                { // add new buildingtype
                    counter[counterPopulation].Type = (int)type;
                    counter[counterPopulation].Count = 1;
                    counterPopulation++;
                }
            }

            for (int i = 0; i<counterPopulation; i++)
            {
                stringToReturn += counter[i].Count + " X " + (DFLocation.BuildingTypes)counter[i].Type + "   ";
            }
            stringToReturn += Environment.NewLine;

            return stringToReturn;
        }

    }
}
