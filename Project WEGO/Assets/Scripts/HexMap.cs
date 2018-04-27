﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexMap : MonoBehaviour {


    public GameObject HexPrefab;
    int MapTileWidth;
    int MapTileHeight;

    float xPos;
    float zPos;
    float MapActualHeight;
    float MapActualWidth;
    GameObject HexGO;

    public Material FlatMat;
    public Material HillMat;
    public Material BoulderMat;
    public Material PondMat;
    public Material ForestFlatMat;
    public Material ForestHillMat;
    public Material WetlandMat;

    // Store all hex Data in a dictionary
    readonly Dictionary<int, Hex> Hexes = new Dictionary<int, Hex>();
    Dictionary<Hex, GameObject> HexToGameObject = new Dictionary<Hex, GameObject>();

	// Use this for initialization
	void Start () {
        MapTileWidth = 12;
        MapTileHeight = 20;
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
        AddTopography(7,7);
        UpdateVisuals();
    }

    // Test function to add features to the map
    // Types of terrain:
    // - Flatlands
    // - Hills
    // - Forests
    // - Boulders (i.e. impassable objects)
    // - Wetlands
    // - TallGrass(?)
    // - Pond



    /// <summary>
    /// Adds features to the map
    /// </summary>
    /// <param name="d">Difficulty of terrain elevation.</param>
    /// <param name="w">Water level difficulty.</param>
    void AddTopography(int d, int w)
    {

        // Alter elevation
        for (int i = 0; i < d; i++)
        {
            Hex mid = null;
            int rad = Random.Range(0, 3);
            while (mid == null)
            {
                int randQ = Random.Range(0, MapTileWidth);
                int randR = Random.Range(-MapTileWidth / 2, MapTileHeight);

                if (Hexes.ContainsKey(QRtoKey(randQ,randR)))
                {
                    mid = GetHexAt(randQ, randR);
                }

            }

            // If there is a single tile, just make a boulder and move on
            if (rad == 0)
            {
                mid.AddElevation(10);
                mid.AddMoisture(-10);
                mid = null;
                continue;
            }

            Hex[] toElevate = GetHexesWithinRange(mid, rad);

            foreach (Hex h in toElevate)
            {
                h.AddElevation(Mathf.Lerp(.5f, .3f, Hex.Distance(h, mid) / rad)+Mathf.PerlinNoise(Random.Range(0f,1f),Random.Range(0f, 1f))*.4f);
            }

            mid = null;

        }


        // Alter water levels
        for (int l = 0; l < w; l++)
        {
            Hex mid = null;
            int rad = Random.Range(0, 3) * 2;
            while(mid == null)
            {
                int randQ = Random.Range(0, MapTileWidth);
                int randR = Random.Range(-MapTileWidth / 2, MapTileHeight);

                if (Hexes.ContainsKey(QRtoKey(randQ, randR)))
                {
                    mid = GetHexAt(randQ, randR);
                    break;
                }

            }

            // Pond Condition
            if (rad == 0)
            {
                mid.AddMoisture(10);
                mid.AddElevation(-10);
                continue;
            }

            Hex[] toHydrate = GetHexesWithinRange(mid, rad);

            foreach (Hex h in toHydrate)
            {
                h.AddMoisture(Mathf.Lerp(.3f, .1f, (float)Hex.Distance(h, mid) / rad) + Mathf.PerlinNoise(Random.Range(0f, 1f), Random.Range(0f, 1f)) * .1f);
            }

            mid = null;

        }



    }

    // Function to update the visuals of the map
    void UpdateVisuals()
    {
        foreach (Hex h in Hexes.Values)
        {
            GameObject hexGO = HexToGameObject[h];

            MeshRenderer mr = hexGO.GetComponentInChildren<MeshRenderer>();

            Debug.Log(h.GetMoisture());

            // ======= Material Conditions ======
            // TODO Tweak these numbers

            // Boulder
            if (h.GetElevation() > 1)
                mr.material = BoulderMat;

            // Pond
            else if (h.GetMoisture() > 1)
                mr.material = PondMat;

            // Hill
            else if (h.GetElevation() > 0.6)
            {
                // Forested Hill
                if (h.GetMoisture() > 0.7)
                    mr.material = ForestHillMat;

                // Plain Hill
                else
                    mr.material = HillMat;
            }

            // Wetland
            else if (h.GetMoisture() > 0.8)
                mr.material = WetlandMat;

            // Flat Forest
            else if (h.GetMoisture() > 0.3)
                mr.material = ForestFlatMat;

            // Flatland
            else
                mr.material = FlatMat;


                
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
                if (Hexes.ContainsKey(QRtoKey(center.Q + dx, center.R + dy)))
                    hexesInRange.Add(Hexes[QRtoKey( center.Q + dx, center.R + dy)]);
            }
        }

        return hexesInRange.ToArray();
    }

}
