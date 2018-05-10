using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class AIArmy : ArmyManager {

	List<HexComponent> currentHexes;  // Used to store currently occupied hexes
	HexMap hexMap;
	HexComponent[] ValidHexes;

	SortRangedGoodness sortRanged = new SortRangedGoodness();
	SortMeleeGoodness sortMelee = new SortMeleeGoodness();
	SortCavalryGoodness sortCavalry = new SortCavalryGoodness();

	bool HumanControlled;

    // Constructor
	public AIArmy(int id, int size, WarManager w, HexMap map) : base(id, size, w)
    {
		currentHexes = new List<HexComponent>();
		hexMap = map;


        // For testing purposes, allow control over first army
        // Will be placing check before registering token actions
		if (playerID == 0)
			HumanControlled = true;
	}

	// Called on war start to set up tokens on hexes
	public override void SetupTokens()
	{
		// Evaluate hexes, assign values, choose tokens to place
		Debug.Log("Setting up AI tokens.");

		ValidHexes = hexMap.GetStartingHexes(playerID);
		Goodness[] goodness = new Goodness[ValidHexes.Length];

		for (int i = 0; i < ValidHexes.Length; i++)
		{
			goodness[i] = EvaluateStartingHex(ValidHexes[i]);
		}

		// Army should always start with melee
		string currentType = "Melee";
		Array.Sort(goodness, ValidHexes, sortMelee);
        
		int counter = 0;
        // Assign Tokens to best hexes
		for (int i = 0; i < armySize; i++)
		{
			string type = Army[i].GetUnitType();
			if (type != currentType)
			{
				if (type == "Ranged")
					Array.Sort(goodness, ValidHexes, sortRanged);
				else if (type == "Cavalry")
					Array.Sort(goodness, ValidHexes, sortCavalry);
				else
					Debug.LogError("Unknown unit type: " + type);

				currentType = type;
				counter = 0;
			}

            // Increment counter if cannot place token on preferred hex
			while (ValidHexes[counter].AddToken(Army[i].gameObject,playerID) == false)
				counter++;
			if (currentHexes.Contains(ValidHexes[counter]) == false)
				currentHexes.Add(ValidHexes[counter]);
		}

        // Add leader to best hex
		ValidHexes[0].AddToken(Leader.gameObject,playerID);
  
        // Alert WarManger that finished setting up
		warManager.FinishedSettingUp();
	}




	public override void RegisterTokenActions()
	{

		if (HumanControlled == false)
		{

			Debug.Log("Time to register some tokens!!!");
			Debug.Log("Looking at " + currentHexes.Count.ToString() + " current hexes.");
		}
	}





	// TODO: Update this to be more complex
	Goodness EvaluateStartingHex(HexComponent h)
	{
		Goodness g = new Goodness(0, 0, 0);

		// Add value depending on hex type
		g = Goodness.AddGoodness(g, GoodnessFromHexType(h.GetHexType()));
		g = Goodness.AddGoodness(g, GoodnessFromDistanceToCenter(h.GetEffectiveRow()));
		g = Goodness.AddGoodness(g, GoodnessFromNeighbors(h));
		g = Goodness.AddGoodness(g, GoodnessFromColumn(h));

		return g;
	}

    // Give each type of unit preferecnces on hex terrain
	Goodness GoodnessFromHexType(string s)
	{
		switch (s)
		{
            case "FlatForest":
				//return 0.4f;
				return new Goodness(
                    0.4f,     // Ranged
					0.4f,     // Melee
					0.4f);    // Cavalry
                
            case "Hill":
				//return 0.3f;
				return new Goodness(
                    0.4f,     // Ranged
                    0.3f,     // Melee
                    0.3f);    // Cavalry
				
            case "HillForest":
				//return 0.5f;
				return new Goodness(
                    0.5f,     // Ranged
                    0.4f,     // Melee
                    0.4f);    // Cavalry
               
            case "Wetland":
				//return -0.5f;
				return new Goodness(
                    -1,     // Ranged
                    -1,     // Melee
                    -1);    // Cavalry

			case "Pond":
                //return -2f;
				return new Goodness(
                    -2,     // Ranged
                    -2,     // Melee
                    -2);    // Cavalry

			case "Boulder":
                //return -2f;
				return new Goodness(
                    -2,     // Ranged
                    -2,     // Melee
                    -2);    // Cavalry

                
            default:
				return null;
        }
        
	}

    // Bias selection towards center of battlefield
	Goodness GoodnessFromDistanceToCenter(float i)
	{

		// 2 "rows" away
		if (Mathf.Abs(i - BaseRow) >= 1)
		{
			return new Goodness(
                    0.2f,     // Ranged
                    0.4f,     // Melee
                    0.4f);    // Cavalry
		}

		// 1 "row" away
		else if (Mathf.Abs(i - BaseRow) >= 0.5f)
		{
			return new Goodness(
                    0.4f,     // Ranged
                    0.2f,     // Melee
                    0.2f);    // Cavalry
		}

		return null;

	}

    // Give each type of unit preference on neighbor terrain types in front of it
	Goodness GoodnessFromNeighbors(HexComponent h)
	{
		Goodness g = new Goodness(0,0,0);
		Hex neighbor = null;

		// Check neighbors in front of spaces

        // For player 0, that means directions 5,0,1
		if (playerID == 0)
		{
			int[] dir = { 5, 0, 1 };

            for (int i = 0; i < 3; i++)
			{
				neighbor = hexMap.GetHexNeighbor(h.GetHex(), dir[i]);

				if (neighbor == null)
                {
					g.AddToAll(-.1f);
                    continue;
                }

				// Evaluate neighbor goodness
                Goodness ng = GoodnessFromHexType(neighbor.GetHexType());


				if (ng != null)
				{
					// Scale down
					ng.DivideBy(3f);

					// Add to current goodness
					g = Goodness.AddGoodness(g, ng);
				}
			}


		}

		// For player 1, that means directions 2,3,4
		if (playerID == 1)
        {
            
			for (int i = 2; i < 5; i++)
            {
                neighbor = hexMap.GetHexNeighbor(h.GetHex(), i);

				if (neighbor == null)
                {
                    g.AddToAll(-.1f);
                    continue;
                }

                // Evaluate neighbor goodness
                Goodness ng = GoodnessFromHexType(neighbor.GetHexType());

				if (ng != null)
				{

					// Scale down
					ng.DivideBy(3f);

					// Add to current goodness
					g = Goodness.AddGoodness(g, ng);
				}
            }

        }
            

		return g;
	}

	Goodness GoodnessFromColumn(HexComponent hexGO)
	{
		Hex hex = hexGO.GetHex();
		Goodness g = new Goodness(0,0,0);
		int column = hex.Q;
		int dir = 1 - 2 * playerID;
		Hex next = hex;

		for (int i = 0; i < HexMap.MapTileHeight; i++)
		{
			next = hexMap.GetHexAt(column, next.R + dir);
			if (next == null)
				break;
			
			Goodness ng = GoodnessFromHexType(next.GetHexType());

			if (ng != null)
			{
				ng.DivideBy(5f);
				g = Goodness.AddGoodness(g, ng);
			}
		}


        
		return g;
	}
}
