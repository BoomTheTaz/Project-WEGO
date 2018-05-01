using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitStats
{

    public float Attack { get; private set; }   // Attack value (per "unit", i.e. health ???
    public float Defense { get; private set; }  // Defense Rating (reduce damage or draws more damage?)
    public int Movement { get; private set; }   // Can move this many hexes per turn
    public string Type { get; private set; }    // Unit type/name
    public int Health { get; private set; }     // Health value (Number of living soldiers per token?)


    public UnitStats(float atk, float def, int move, int health)
    {
        Attack = atk;
        Defense = def;
        Movement = move;
        Health = health;

    }

    public void SetUnitAs(string s)
    {
        Type = s;
    }
}
