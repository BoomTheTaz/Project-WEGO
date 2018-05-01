using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexStats {

    public int FatigueValue;
    public float TokenHeight;
    public bool HasTrees;
    public bool IsTraversable;

    public HexStats(int fatigue, float height, bool trees, bool traversable)
    {
        FatigueValue = fatigue;
        TokenHeight = height;
        HasTrees = trees;
        IsTraversable = traversable;
    }

	
}

