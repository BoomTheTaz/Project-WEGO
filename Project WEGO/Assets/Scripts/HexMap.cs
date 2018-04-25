using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexMap : MonoBehaviour {


    public GameObject HexPrefab;
    public int MapTileWidth = 10;
    public int MapTileHeight = 20;

    float xPos;
    float zPos;
    float MapActualHeight=0;
    float MapActualWidth=0;
    GameObject HexGO;

    // Store all hex Data in a dictionary
    readonly Dictionary<string, Hex> Hexes = new Dictionary<string, Hex>();

	// Use this for initialization
	void Start () {
        GenerateMap();
	}


	private void Update()
	{
        // Testing the GetHexAt function
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Hex testHex = GetHexAt(Random.Range(0, MapTileHeight), Random.Range(0, MapTileWidth));

            Debug.Log(string.Format("Got Tile at ({0},{1})", testHex.Row, testHex.Column));
        }
	}


    // Generic map generation with hard coded map dimensions
	void GenerateMap()
    {
        // Cycle through each (row,column) combination
        for (int row = 0; row < MapTileHeight; row++)
        {
            for (int column = 0; column < MapTileWidth; column++)
            {
                // Create new Hex data object with row and column
                Hex h = new Hex(row, column);

                // Instantiate a hex prefab at the hex data's indicated position
                HexGO = Instantiate(HexPrefab, h.Position(), Quaternion.identity, this.transform);

                // Give the hex prefab a copy of the hex data
                HexGO.GetComponent<HexComponent>().SetHex(h);

                // Store new hex in Hexes Dictionary, using "row" + "column"
                Hexes.Add(row.ToString() + column.ToString(), h);

                // Prevent moving of the hex
                HexGO.isStatic = true;

                // Obtain Width of map on last column
                if (MapActualWidth == 0 && column == MapTileWidth - 1)
                    MapActualWidth = HexGO.transform.position.x;
            }

            // Obtain Height of map on last row
            if (MapActualHeight == 0 && row == MapTileHeight - 1)
                MapActualHeight = HexGO.transform.position.z;
        }
    }



    public float GetMapWidth()
    {
        return MapActualWidth;
    }

    public float GetMapHeight()
    {
        return MapActualHeight;
    }

    /// <summary>
    /// Gets the Hex at (r,c)
    /// </summary>
    /// <returns>The <see cref="T:Hex"/>.</returns>
    /// <param name="r">Row.</param>
    /// <param name="c">Column.</param>
    public Hex GetHexAt(int r, int c)
    {
        if (Hexes.ContainsKey(r.ToString() + c.ToString()) == false)
        {
            Debug.LogError("Cannot find Hex at (" + r.ToString() + "," + c.ToString() + ")");
            return null;

        }
        else
            return Hexes[r.ToString() + c.ToString()];
    }
}
