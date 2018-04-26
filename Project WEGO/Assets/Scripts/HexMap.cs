using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexMap : MonoBehaviour {


    public GameObject HexPrefab;
    int MapTileWidth = 12;
    int MapTileHeight = 20;

    float xPos;
    float zPos;
    float MapActualHeight=0;
    float MapActualWidth=0;
    GameObject HexGO;

    public Material Flat;
    public Material Hill;

    // Store all hex Data in a dictionary
    readonly Dictionary<int, Hex> Hexes = new Dictionary<int, Hex>();
    Dictionary<Hex, GameObject> HexToGameObject = new Dictionary<Hex, GameObject>();

	// Use this for initialization
	void Start () {
        GenerateMap();
	}


	private void Update()
	{
        // Testing the GetHexAt function
        if (Input.GetKeyDown(KeyCode.Space))
        {
            //Hex testHex1 = GetHexAt(Random.Range(0, MapTileHeight), Random.Range(0, MapTileWidth));
            //Hex testHex2 = GetHexAt(Random.Range(0, MapTileHeight), Random.Range(0, MapTileWidth));

            //float dist = Hex.Distance(testHex1, testHex2);

            //Debug.Log(string.Format("Got Tiles at ({0},{1}) and ({2},{3}).", testHex1.Q, testHex1.R, testHex2.Q, testHex2.R));
            //Debug.Log("Distance: " + dist.ToString());

            //Hex testHex = GetHexAt(Random.Range(0, MapTileHeight), Random.Range(0, MapTileWidth));
            //int rad = 2;
            //Hex[] tester = GetHexesWithinRange(testHex, rad);

            //Debug.Log(string.Format("Found {0} hexes within {1} tiles of ({2},{3}).", tester.Length, rad, testHex.Q, testHex.R));

        }
	}


    // Generic map generation with hard coded map dimensions
	void GenerateMap()
    {

        // Cycle through each (row,column) combination
        // Building up one row at a time to make it easier to terminate a row early
        // or cut off start of row
        for (int row = -MapTileWidth/2; row < MapTileHeight; row++)
        {
            for (int column = 0; column < MapTileWidth; column++)
            {
                // Square off bottom right corner
                if (row < 0 && column / 2 + row < 0)
                    continue;

                // Check if full row is needed, i.e. square off the top
                if (row + Mathf.Ceil(column / 2f) > MapTileHeight - 1)
                    break;

                // Create new Hex data object with row and column
                Hex h = new Hex(column, row);

                // Instantiate a hex prefab at the hex data's indicated position
                HexGO = Instantiate(HexPrefab, h.Position(), Quaternion.identity, this.transform);

                // Give the hex prefab a copy of the hex data
                HexGO.GetComponent<HexComponent>().SetHex(h);

                // Store new hex in Hexes Dictionary, using "column" + "row"
                Hexes.Add(QRtoKey(column,row), h);

                // Store Hex GameObject in dictionary with key Hex
                HexToGameObject.Add(h, HexGO);

                // Prevent moving of the hex
                HexGO.isStatic = true;

                // Obtain Width of map on last column
                if (MapActualWidth <= 0 && column == MapTileWidth - 1)
                    MapActualWidth = HexGO.transform.position.x;
            }

            // Obtain Height of map on last row
            if (MapActualHeight <= 0 && row == MapTileHeight - 1)
                MapActualHeight = HexGO.transform.position.z;
        }
        AddTopography();
        UpdateVisuals();
    }

    // Test function to add features to the map
    void AddTopography()
    {
        int rad = 4;
        Hex mid = GetHexAt(5, 10);

        Hex[] toChange = GetHexesWithinRange(mid, rad);

        foreach (Hex h in toChange)
        {
            h.AddElevation(Mathf.Lerp(1, 0, (float)Hex.Distance(mid, h) / rad + Random.Range(0f,0.2f)));
        }
    }

    // Function to update the visuals of the map
    void UpdateVisuals()
    {
        foreach (Hex h in Hexes.Values)
        {
            GameObject hexGO = HexToGameObject[h];

            MeshRenderer mr = hexGO.GetComponentInChildren<MeshRenderer>();

            Material mat = new Material(Flat);

            mat.color = Color.Lerp(Color.green, Color.red, h.GetElevation());

            mr.material = mat;
            //if (h.GetElevation() > 0)
            //    mr.material = Hill;
            //else
                //mr.material = Flat;
                
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
    /// Gets the Hex at (q,r)
    /// </summary>
    /// <returns>The <see cref="T:Hex"/> at (q,r).</returns>
    /// <param name="q">Column.</param>
    /// <param name="r">Row.</param>

    public Hex GetHexAt(int q, int r)
    {
        if (Hexes.ContainsKey(QRtoKey(q,r)) == false)
        {
            Debug.LogError("Cannot find Hex at (" + q.ToString() + "," + r.ToString() + ")");
            return null;

        }
        else
            return Hexes[QRtoKey(q,r)];
    }

    /// <summary>
    /// Converts a column and row value into the Hexes Dictionary code
    /// </summary>
    /// <returns>The to key.</returns>
    /// <param name="q">Column.</param>
    /// <param name="r">Row.</param>
    int QRtoKey(int q, int r)
    {
        return r * MapTileWidth + q;
    }

    public Hex[] GetHexesWithinRange(Hex center, int radius)
    {
        List<Hex> hexesInRange = new List<Hex>();

        for (int dx = -radius; dx < radius+1; dx++)
        {
            for (int dy = Mathf.Max(-radius, -dx-radius); dy < Mathf.Min(radius-dx,radius)+1; dy++)
            {
                hexesInRange.Add(Hexes[QRtoKey( center.Q + dx, center.R + dy)]);
            }
        }

        return hexesInRange.ToArray();
    }

}
