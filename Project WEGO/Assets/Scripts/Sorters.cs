using System.Collections;
using System.Collections.Generic;

// =======================================
//  This is where all necessary sort functions will be stored
// =======================================


// Sort by ranged field in a Goodness object
public class SortRangedGoodness : IComparer
{
	public int PlayerID { get; set; }
    public int Compare(object x, object y)
	{
		HexComponent a = (HexComponent)x;
		HexComponent b = (HexComponent)y;

		if (a.HexGoodness[PlayerID].Ranged < b.HexGoodness[PlayerID].Ranged)
            return 1;
		if (a.HexGoodness[PlayerID].Ranged > b.HexGoodness[PlayerID].Ranged)
            return -1;

        return 0;
        
	}

}

// Sort by Melee field in a Goodness object
public class SortMeleeGoodness : IComparer
{
	public int PlayerID { get; set; }
    public int Compare(object x, object y)
    {
		HexComponent a = (HexComponent)x;
		HexComponent b = (HexComponent)y;

		if (a.HexGoodness[PlayerID].Melee < b.HexGoodness[PlayerID].Melee)
            return 1;
		if (a.HexGoodness[PlayerID].Melee > b.HexGoodness[PlayerID].Melee)
            return -1;

        return 0;

    }

}

// Sort by ranged field in a Goodness object
public class SortCavalryGoodness : IComparer
{
	public int PlayerID { get; set; }
    public int Compare(object x, object y)
    {
		HexComponent a = (HexComponent)x;
		HexComponent b = (HexComponent)y;

		if (a.HexGoodness[PlayerID].Cavalry < b.HexGoodness[PlayerID].Cavalry)
            return 1;
		if (a.HexGoodness[PlayerID].Cavalry > b.HexGoodness[PlayerID].Cavalry)
            return -1;

        return 0;

    }

}


// Sort by ranged field in a Goodness object
public class SortEffectiveRow : IComparer
{
	public int PlayerID { get; set; }
	public int Compare(object x, object y)
    {
		HexComponent a = (HexComponent)x;
		HexComponent b = (HexComponent)y;

		float xRow = a.GetEffectiveRow();
		float yRow = b.GetEffectiveRow();

        if (PlayerID == 0)
        {
            if (xRow > yRow)
                return -1;
            if (xRow < yRow)
                return 1;
        }

        else if (PlayerID == 1)
        {

            if (xRow > yRow)
                return 1;
            if (xRow < yRow)
                return -1;

        }





        return 0;

    }


}