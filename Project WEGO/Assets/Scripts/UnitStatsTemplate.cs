using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class UnitStatsTemplate {


    public static UnitStats GetStatsForUnit(string s) {

        UnitStats result = null;

        switch (s)
        {
            case "Ranged":
                result = new UnitStats(1, 2, 1);
                break;

            case "Melee":
                result = new UnitStats(2, 1, 1);
                break;

            case "Leader":
                result = new UnitStats(3, 3, 3);
                break;


            default:
                break;
        }

        result.SetUnitAs(s);

        return result;


    }

}
