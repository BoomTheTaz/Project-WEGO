using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HexTemplate {


    public static HexStats GetStatsForType(string s)
    {

        HexStats result = null;

        switch (s)
        {

                // Flatlands
            case "Flatland":
                result = new HexStats(0, 0.05f, false, true);
                break;

                // Boulder
            case "Boulder":
                result = new HexStats(0, 0f, false, false);
                break;

                // Pond
            case "Pond":
                result = new HexStats(0, 0f, false, false);
                break;

                // Hill
            case "Hill":
                result = new HexStats(1, 0.5f, false, true);
                break;

                // Wetlands
            case "Wetland":
                result = new HexStats(1, 0.05f, false, true);
                break;

                // FlatForest
            case "FlatForest":
                result = new HexStats(1, 0.05f, true, true);
                break;

                // HillForest
            case "HillForest":
                result = new HexStats(2, 0.5f, true, true);
                break;



            default:
                Debug.Log("No Hex type with the name " + s);
                result = null;
                break;
        }


        return result;




    }

	
}
