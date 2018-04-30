using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitStats
{

    public float Attack { get; private set; }
    public float Defense { get; private set; }
    public int Movement { get; private set; }
    public string Type { get; private set; }


    public UnitStats(float atk, float def, int move)
    {
        Attack = atk;
        Defense = def;
        Movement = move;

    }

    public void SetUnitAs(string s)
    {
        Type = s;
    }
}
