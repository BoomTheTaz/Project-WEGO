using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hex {

    // Constructor sets the row and column 
    public Hex(int q, int r)
    {
        R = r;
        Q = q;
        S = -(Q + R);
    }

    public readonly int R;
    public readonly int Q;
    public readonly int S;

    //int HexRadius = 1;
    int HexWidth = 2;
    float HexHeight = Mathf.Sqrt(3);

    // Variables that describe the land
    float Elevation;
    float Moisture;

    public Vector3 Position()
    {
        // x posiiton is column times 3/4 width
        float xPos = Q * HexWidth * 0.75f;

        // z position is hex height times sum of row plus 1/2 column
        float zPos = (R + Q/2f) * HexHeight;
       
        return new Vector3(xPos, 0, zPos);
    }

    // Simple code to calculate the distance in tiles between two hexes
    public static int Distance (Hex a, Hex b)
    {
        return 
            Mathf.Max(
                Mathf.Abs(a.Q - b.Q), 
                Mathf.Abs(a.R - b.R), 
                Mathf.Abs(a.S - b.S)
            );
    }

	
    public void AddElevation(float f)
    {
        Elevation += f;
    }

    public float GetElevation()
    {
        return Elevation;
    }

    public void AddMoisture(float f)
    {
        Moisture += f;
    }

    public float GetMoisture()
    {
        return Moisture;
    }
}
