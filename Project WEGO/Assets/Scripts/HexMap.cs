using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class HexMap : MonoBehaviour {


    public GameObject HexPrefab;
    int MapTileWidth;
    int MapTileHeight;

    float xPos;
    float zPos;
    float MapActualHeight;
    float MapActualWidth;
    GameObject HexGO;

    // Test Materials to differentiate terrain
    public Material FlatMat;
    public Material HillMat;
    public Material BoulderMat;
    public Material PondMat;
    public Material ForestFlatMat;
    public Material ForestHillMat;
    public Material WetlandMat;

    // Simple meshes to distinguish elevations
    public Mesh Flatland;
    public Mesh Hill0;
    public Mesh BoulderMesh;
    public int CurrentState;

    // Tree prefabs
    public GameObject InnerTreePrefab;
    public GameObject OuterTreePrefab;

    // Store all hex Data in a dictionary
    readonly Dictionary<int, Hex> Hexes = new Dictionary<int, Hex>();
    Dictionary<Hex, GameObject> HexToGameObject = new Dictionary<Hex, GameObject>();

    HexComponent CurrentHexGO;

	List<HexComponent>[] ValidStartingHexes = new List<HexComponent>[2];

	// Use this for initialization
	void Start () {
        MapTileWidth = 11;
        MapTileHeight = 15;

		ValidStartingHexes[0] = new List<HexComponent>();
		ValidStartingHexes[1] = new List<HexComponent>();

        GenerateMap();
        
        for (int i = 0; i < MaxRange; i++)
        {
            MoveToReturn[i] = new List<HexComponent>();
            AttackToReturn[i] = new List<HexComponent>();
        }
    }


    // Generic map generation with hard coded map dimensions
	void GenerateMap()
    {

		// Cycle through each (row,column) combination
		// Building up one row at a time to make it easier to terminate a row early
		// or cut off start of row
		for (int row = -MapTileWidth / 2; row < MapTileHeight; row++)
		{
			for (int column = 0; column < MapTileWidth; column++)
			{
				// Square off bottom right corner
				if (column / 2 + row < 0)
					continue;

				// Check if full row is needed, i.e. square off the top
				if (row + Mathf.Ceil(column / 2f) > MapTileHeight - 1)
					break;

				// Create new Hex data object with row and column
				Hex h = new Hex(column, row);

				// Instantiate a hex prefab at the hex data's indicated position
				HexGO = Instantiate(HexPrefab, h.Position(), Quaternion.identity, this.transform);

				// Pass self as HexMap
				HexGO.GetComponent<HexComponent>().SetHexMap(this);

				// Give the hex prefab a copy of the hex data
				HexGO.GetComponent<HexComponent>().SetHex(h);

				// Store new hex in Hexes Dictionary, using "column" + "row"
				Hexes.Add(QRtoKey(column, row), h);

				// Store Hex GameObject in dictionary with key Hex
				HexToGameObject.Add(h, HexGO);

				// Prevent moving of the hex
				HexGO.isStatic = true;

				// Store Valid starting hexes, i.e. 3 Hexes from top and bottom edges
				if (row + column / 2f < 1.5)
				{
					HexGO.GetComponent<HexComponent>().SetValidStarting(0);
					ValidStartingHexes[0].Add(HexGO.GetComponent<HexComponent>());
				}

				if (row + column / 2f > MapTileHeight - 2.5)
				{
					HexGO.GetComponent<HexComponent>().SetValidStarting(1);
					ValidStartingHexes[1].Add(HexGO.GetComponent<HexComponent>());
				}

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
            int rad = Random.Range(0, 5);
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
                h.AddElevation(
                    Mathf.PerlinNoise(Random.Range(0f,1f),Random.Range(0f, 1f))*.3f+ // Randomness
                    Mathf.Lerp(0.3f,0.1f,Mathf.Max(Mathf.Abs(h.R-mid.R),Mathf.Abs(h.S - mid.S))/rad)); // Lateral Bias
                
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
                h.AddMoisture(
                    Mathf.PerlinNoise(Random.Range(0f, 1f), Random.Range(0f, 1f)) * .6f + // Randomness
                    Mathf.Lerp(0.2f,0.1f,(float)Hex.Distance(mid,h)/rad)       // Center Bias
                );
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
            MeshFilter mf = hexGO.GetComponentInChildren<MeshFilter>();

            // ======= Material Conditions ======
            // TODO Tweak these numbers

            string s = "";

            // Boulder
            if (h.GetElevation() > 1)
            {
                mf.mesh = BoulderMesh;
                mr.material = BoulderMat;
                hexGO.transform.rotation *= Quaternion.AngleAxis(60 * Random.Range(0, 7), Vector3.up);

                s = "Boulder";

            }

            // Pond
            else if (h.GetMoisture() > 1)
            {
                mr.material = PondMat;
                s = "Pond";

            }

            // Hill
            else if (h.GetElevation() > 0.7)
            {
                mf.mesh = Hill0;

                // Forested Hill
                if (h.GetMoisture() > 0.7)
                {
                    mr.material = ForestHillMat;
                    Instantiate(OuterTreePrefab, h.Position() + Vector3.up * .4f, Quaternion.identity * Quaternion.AngleAxis(60 * Random.Range(0, 7), Vector3.up), hexGO.transform);
                    Instantiate(InnerTreePrefab, h.Position() + Vector3.up * 0.5f, Quaternion.identity * Quaternion.AngleAxis(Random.Range(-180, 180), Vector3.up), hexGO.transform);
                    s = "HillForest";

                }

                // Plain Hill
                else
                {
                    mr.material = HillMat;
                    s = "Hill";

                }
            }

            // Wetland
            else if (h.GetMoisture() > 0.8)
            {
                mr.material = WetlandMat;
                s = "Wetland";

            }
            // Flat Forest
            else if (h.GetMoisture() > 0.5)
            {
                mr.material = ForestFlatMat;

                Instantiate(OuterTreePrefab, h.Position(), Quaternion.identity * Quaternion.AngleAxis(60 * Random.Range(0, 7), Vector3.up), hexGO.transform);
                Instantiate(InnerTreePrefab, h.Position() + Vector3.up * .05f, Quaternion.identity * Quaternion.AngleAxis(Random.Range(-180, 180), Vector3.up), hexGO.transform);
                s = "FlatForest";

            }

            // Flatland
            else
            {
                mr.material = FlatMat;
                s = "Flatland";

            }

            hexGO.GetComponent<HexComponent>().SetHexAsType(s);
                
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
        
        if (q < 0 || q >= MapTileWidth || Hexes.ContainsKey(QRtoKey(q,r)) == false)
        {
            //Debug.Log("Cannot find Hex at (" + q.ToString() + "," + r.ToString() + ")");
            return null;

        }
        else
            return Hexes[QRtoKey(q,r)];
    }

    public GameObject GetHexComponentAt(int q, int r)
    {

        GameObject result = HexToGameObject[GetHexAt(q, r)];


        return result;

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

    // TODO: Decide if this is worthwhile or if Hex should just have knowledge of HexComponents
    public Dictionary<Hex,HexComponent> GetHexGOWithinRange(Hex center, int radius)
    {
        if (radius < 0)
        {
            Debug.LogError("Cannot find hexes within a negative radius.");
            return null;
        }

        Hex[] hexes = GetHexesWithinRange(center, radius);
        Dictionary<Hex, HexComponent> results = new Dictionary<Hex, HexComponent>();

        foreach (var h in hexes)
        {
            results.Add(h, HexToGameObject[h].GetComponent<HexComponent>());
        }


        return results;
    }





    static int MaxRange = 4;
    List<HexComponent>[] MoveToReturn = new List<HexComponent>[MaxRange];
    List<HexComponent>[] AttackToReturn = new List<HexComponent>[MaxRange];

    // Sets Valid Attack and movement hexes within inputted HexComponent
    // TODO: Don't search for neighbors of a hex that has an enemy on it
    public List<HexComponent>[] GetValidMoveHexes(HexComponent h)
    {
        // Clear List Array
        for (int i = 0; i < MaxRange; i++)
        {
            MoveToReturn[i].Clear();
        }

        // Get Center Hex
        Hex center = h.GetHex();
        Hex testHex;

        // Get range for search, Using max and then storing in HexComponent
        int move = h.GetMaxMovement();

        // loop times equals movement range
        for (int i = 0; i < move; i++)
        {
            // For first loop, just look at center hex
            if (i == 0)
            {
                for (int j = 0; j < 6; j++)
                {
                    testHex = GetHexNeighbor(center, j);

                    if (testHex != null)
                        AnalyzeHexToMove(testHex,i);
                }
            }

            // Otherwise, loop through previous list in array and check all neighbors
            else
            {
                foreach (var item in MoveToReturn[i-1])
                {
                    for (int j = 0; j < 6; j++)
                    {
                        testHex = GetHexNeighbor(item.GetHex(), j);

                        // Don't count center hex or null hexes
                        if (testHex == center || testHex == null)
                            continue;
                        
                        AnalyzeHexToMove(testHex, i);
                    }
                }

            }
        }
        int sum = 0;

        foreach (var l in MoveToReturn)
        {
            sum += l.Count();
        }

        // Add Hex to list of hexes accessed this turn to enable reset on turns end
        if (HexesThisTurn.Contains(h) == false)
            HexesThisTurn.Add(h);

        return MoveToReturn;
    }

    // Call this on all hexes to see if it should be added to the Movement List array
    void AnalyzeHexToMove(Hex h, int num)
    {
        // Iterate through num + 1, in case hex was added already this iteration
        for (int i = 0; i < num+1; i++)
        {
            // Do not Add if hex already has been added
            if (MoveToReturn[i].Contains(HexToGameObject[h].GetComponent<HexComponent>()))
            {
                return;
            }
        }

        // Case depends on hex type
        switch (HexToGameObject[h].GetComponent<HexComponent>().GetHexType())
        {

            case "Flatland":
                MoveToReturn[num].Add(HexToGameObject[h].GetComponent<HexComponent>());
                break;
          
            // Hill
            case "Hill":
                MoveToReturn[num].Add(HexToGameObject[h].GetComponent<HexComponent>());
                break;

            // Wetlands
            case "Wetland":
                MoveToReturn[num].Add(HexToGameObject[h].GetComponent<HexComponent>());
                break;

            // FlatForest
            case "FlatForest":
                MoveToReturn[num].Add(HexToGameObject[h].GetComponent<HexComponent>());
                break;

            // HillForest
            case "HillForest":
                MoveToReturn[num].Add(HexToGameObject[h].GetComponent<HexComponent>());
                break;

            default:
                break;
        }
    }

    // TODO Write this code, use ring method
    public List<HexComponent>[] GetValidAttackHexes(HexComponent h)
    {
        // Clear List Array
        for (int i = 0; i < MaxRange; i++)
        {
            AttackToReturn[i].Clear();
        }

        // Get Center Hex
        Hex center = h.GetHex();
        bool OnHill = false;

        if (h.GetElevation() > 0.2)
            OnHill = true;


        List<Hex> rings;
        Hex test = null;

        // Get range for search, Using max and then storing in HexComponent
        int attack = h.GetMaxAttack();

        // loop times equals movement range
        for (int i = 0; i < attack; i++)
        {
            // Add all neighbors, unless Mountain
            if (i == 0)
            {
                for (int j = 0; j < 6; j++)
                {
                    test = GetHexNeighbor(center, j);

                    if (test == null)
                        continue;

                    // In first ring, just add all neighbors except boulders
                    if (HexToGameObject[test].GetComponent<HexComponent>().GetHexType() != "Boulder" )
                        AttackToReturn[i].Add(HexToGameObject[test].GetComponent<HexComponent>());
                }


            }

            // Check Hex ring and send hexes to be analyzed
            else
            {
                // Get Next ring of hexes
                rings = GetHexRing(center, i + 1);

                // Analyze each hex for if it is able to be attacked
                foreach (var item in rings)
                {
                    if (item != null)
                        AnalyzeHexToAttack(center, item, i, OnHill);
                }
            }
        }
        int sum = 0;

        foreach (var l in AttackToReturn)
        {
            sum += l.Count();
        }


        // Add Hex to list of hexes accessed this turn to enable reset on turns end
        if (HexesThisTurn.Contains(h) == false)
            HexesThisTurn.Add(h);

        List<HexComponent> toRemove = new List<HexComponent>();

        // Go through and remove ponds
        for (int i = 0; i < MaxRange; i++)
        {
            foreach (var item in AttackToReturn[i])
            {
                if (item.GetHexType() == "Pond")
                    toRemove.Add(item);
            }

            foreach (var item in toRemove)
            {
                AttackToReturn[i].Remove(item);
            }
            toRemove.Clear();
        }



        return AttackToReturn;
    }

    // Call this on all hexes to see if it should be added to the Attack List array
    void AnalyzeHexToAttack(Hex c, Hex h, int num, bool onHill)
    {
        Hex neighbor;
        List<Hex> validNeighbors = new List<Hex>();
        List<Hex> InvalidNeighbors = new List<Hex>();

        // Check how many neighbors, closer radially to center, are in previous ring
        for (int i = 0; i < 6; i++)
        {
            neighbor = GetHexNeighbor(h, i);

            if (neighbor == null)
                continue;

            if (Hex.Distance(neighbor, c) > num)
                continue;

            // Only want hexes on previous, smaller ring
            if (AttackToReturn[num - 1].Contains(HexToGameObject[neighbor].GetComponent<HexComponent>()))
            {
                validNeighbors.Add(neighbor);
            }
            else
                InvalidNeighbors.Add(neighbor);
                

        }


        bool AdjacentBoulder = false;
        foreach (var item in InvalidNeighbors)
        {
            if (HexToGameObject[item].GetComponent<HexComponent>().GetHexType() == "Boulder" &&
                (item.Q == c.Q || item.R == c.R || item.S == c.S))
                AdjacentBoulder = true;
        }

        // ==== CONDITIONS FOR ANALYSIS =====

        // REJECT, if Boulder
        if (HexToGameObject[h].GetComponent<HexComponent>().GetHexType() == "Boulder")
            return;

        // ACCEPT, if on a grid line and have a valid neighbors
        if ((c.Q == h.Q || c.R == h.R || c.S == h.S) && validNeighbors.Count == 1)
        {
            AttackToReturn[num].Add(HexToGameObject[h].GetComponent<HexComponent>());
            return;
        }

        // ACCEPT, If on odd ring and one valid neighbor and other is boulder
        if (num % 2 == 0 && validNeighbors.Count == 1 && AdjacentBoulder == true)
        {
            AttackToReturn[num].Add(HexToGameObject[h].GetComponent<HexComponent>());
            return;
        }

        // REJECT, if on an even ring and no valid neighbors
        if(num % 2 == 1 && validNeighbors.Count<1)
        {
            return;
        }

        // REJECT, if on an odd ring with 1 or fewer valid neighbors
        if (num % 2 == 0 && validNeighbors.Count < 2)
        {
            return;
        }


        AttackToReturn[num].Add(HexToGameObject[h].GetComponent<HexComponent>());
    }


    // TODO FIX ALL OF THIS
    // Check if hills will be in way of attack
    bool CheckHills(List<Hex> h, int num)
    {
        string type = "";
        int numHills = 0;

        foreach (var item in h)
        {
            type = HexToGameObject[item].GetComponent<HexComponent>().GetHexType();
            if (type.Contains("Hill") == true)
                numHills++;
        }
        Debug.Log("Made it here");

        // Odd rings require 2 non-hills, Even rings require 1 non hill
        if ((num % 2 == 1 && numHills > 0) || (num % 2 == 0 && numHills > h.Count-1))
            return false;

        return true;
    }

    // Function to take Token gameObject from armyManager and place it on hex
    public void PlaceTokenOnHex(GameObject token, int q, int r)
    {
        GameObject hexTemp = HexToGameObject[GetHexAt(q, r)];

        hexTemp.GetComponent<HexComponent>().AddToken(token);
    }


    // Return Hex in given direction
    // ======== DIRECTION CODE ==========
    // 0: N
    // 1: NE
    // 2: SE
    // 3: S
    // 4: SW
    // 5: NW

    Hex GetHexNeighbor(Hex h, int dir)
    {

        Hex result = null;

        switch (dir)
        {
            case 0:
                result = GetHexAt(h.Q, h.R+1);
                break;

            case 1:
                result = GetHexAt(h.Q+1, h.R);
                break;

            case 2:
                result = GetHexAt(h.Q+1, h.R-1);
                break;

            case 3:
                result = GetHexAt(h.Q, h.R-1);
                break;

            case 4:
                result = GetHexAt(h.Q-1, h.R);
                break;

            case 5:
                result = GetHexAt(h.Q-1, h.R+1);
                break;

            default:
                Debug.Log("Invalid Neighbor range. Inputted Neighbor: " + dir.ToString());
                break;
        }

        return result;
    }


    List<Hex> hexRing = new List<Hex>();
    List<Hex> GetHexRing(Hex center, int radius)
    {
        // Clear previous HexRing
        hexRing.Clear();

        // Return center if 0 radius, throw error if negative radius
        if (radius == 0)
        {
            hexRing.Add(center);
            return hexRing;
        }
        else if (radius < 0)
        {
            Debug.LogError("Cannot find Hex Ring at a negative radius.");
            return null;
        }

        // Find Start Hex
        Hex toAdd = center;

        // Go in direction 4 to make finding ring easier
        int testQ = center.Q - radius;
        int testR = center.R;

        // Loop through every direction for radius numbers of neighbors
        for (int i = 0; i < 6; i++)
        {
            // Update q and r depending on direction going
            // storing q and r because neighbors returns null and breaks ring

            for (int j = 0; j < radius; j++)
            {
                switch (i)
                {
                    case 0:
                        testQ += 0;
                        testR += 1;
                        break;

                    case 1:
                        testQ += 1;
                        testR += 0;
                        break;

                    case 2:
                        testQ += 1;
                        testR += -1;
                        break;

                    case 3:
                        testQ += 0;
                        testR += -1;
                        break;

                    case 4:
                        testQ += -1;
                        testR += 0;
                        break;

                    case 5:
                        testQ += -1;
                        testR += 1;
                        break;

                    default:
                        Debug.Log("How did we get here....");
                        break;
                }

                toAdd = GetHexAt(testQ, testR);

                if (toAdd != null)
                {
                    hexRing.Add(toAdd);
                }
            }
        }


        return hexRing;

    }

    List<HexComponent> HexesThisTurn = new List<HexComponent>();
    public void OnTurnEnd()
    {
        // Reset HexComponents that were selected this turn

        foreach (var h in HexesThisTurn)
        {
            h.ResetForNewTurn();
        }

    }

    public void ChangeStates(int i)
    {
        CurrentState = i;

        // Check current hexgo
        if (CurrentHexGO != null)
            CurrentHexGO.UpdateCurrentState(CurrentState);
    }


    public void SetCurrentHexGO(HexComponent h)
    {
        CurrentHexGO = h;
    }

    public void ClearStartingHexes()
	{
		foreach (var item in ValidStartingHexes[0])
		{
			item.NoOutline();
		}

		foreach (var item in ValidStartingHexes[1])
        {
            item.NoOutline();
        }

		//ValidStartingHexes = null;
	}
}
