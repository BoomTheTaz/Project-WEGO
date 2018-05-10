using System.Collections;

// =======================================
//  This is where all necessary sort functions will be stored
// =======================================


// Sort by ranged field in a Goodness object
public class SortRangedGoodness : IComparer
{

    public int Compare(object x, object y)
	{
		Goodness a = (Goodness)x;
        Goodness b = (Goodness)y;

        if (a.Ranged < b.Ranged)
            return 1;
        if (a.Ranged > b.Ranged)
            return -1;

        return 0;
        
	}

}

// Sort by Melee field in a Goodness object
public class SortMeleeGoodness : IComparer
{

    public int Compare(object x, object y)
    {
        Goodness a = (Goodness)x;
        Goodness b = (Goodness)y;

		if (a.Melee < b.Melee)
            return 1;
		if (a.Melee > b.Melee)
            return -1;

        return 0;

    }

}

// Sort by ranged field in a Goodness object
public class SortCavalryGoodness : IComparer
{

    public int Compare(object x, object y)
    {
        Goodness a = (Goodness)x;
        Goodness b = (Goodness)y;

		if (a.Cavalry < b.Cavalry)
            return 1;
		if (a.Cavalry > b.Cavalry)
            return -1;

        return 0;

    }

}