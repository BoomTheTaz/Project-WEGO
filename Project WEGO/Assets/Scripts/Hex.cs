using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hex {

    // Constructor sets the row and column 
    public Hex(int r, int c)
    {
        Row = r;
        Column = c;
    }

    public readonly int Row;
    public readonly int Column;

    int HexRadius = 1;
    int HexWidth = 2;
    float HexHeight = Mathf.Sqrt(3);

    public Vector3 Position()
    {
        float xPos = Column * HexWidth * 0.75f;
        float zPos = Row * HexHeight;

        if (Column % 2 == 1)
            zPos += HexHeight / 2;

        return new Vector3(xPos, 0, zPos);
    }

	
}
