using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitStats
{

    public float Attack { get; private set; }   // Attack value (per "unit", i.e. health ???
    public float Defense { get; private set; }  // Defense Rating (reduce damage or draws more damage?)
    public int Movement { get; private set; }   // Can move this many hexes per turn
    public string Type { get; private set; }    // Unit type/name
    public int Health { get; set; }     // Health value (Number of living soldiers per token?)
    public int AttackRange { get; private set; }
	public int UnitTypeInt { get; private set; }

    public UnitStats(float atk, float def, int move, int health, int atkRange)
    {
        Attack = atk;
        Defense = def;
        Movement = move;
        Health = health;
        AttackRange = atkRange;

    }

    public void SetUnitAs(string s)
    {
        Type = s;

		switch (s)
        {
            case "Melee":
				UnitTypeInt = (int)UnitTypes.Melee;
                break;
            case "Ranged":
				UnitTypeInt = (int)UnitTypes.Ranged;
                break;
            case "Cavalry":
				UnitTypeInt = (int)UnitTypes.Cavalry;
                break;


			case "Leader":
				break;

            default:
				Debug.Log("Invalid Unit Type: " + s);
                break;
        }
    }
}
