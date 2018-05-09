using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class AIArmy : ArmyManager {

	List<HexComponent> currentHexes;  // Used to store currently occupied hexes
	HexMap hexMap;
	HexComponent[] ValidHexes;

    // Constructor
	public AIArmy(int id, int size, WarManager w, HexMap map) : base(id, size, w)
    {
		currentHexes = new List<HexComponent>();
		hexMap = map;
	}
    
    // Called on war start to set up tokens on hexes
	public override void SetupTokens()
	{
		// Evaluate hexes, assign values, choose tokens to place
		Debug.Log("Setting up AI tokens.");

		ValidHexes = hexMap.GetStartingHexes(playerID);
		float[] goodness = new float[ValidHexes.Length];

		for (int i = 0; i < ValidHexes.Length; i++)
		{
			goodness[i] = EvaluateStartingHex(ValidHexes[i]);
		}

		Array.Sort(goodness, ValidHexes);

		Array.Reverse(ValidHexes);
		Array.Reverse(goodness);
        
		int counter = 0;
        // Assign Tokens to best hexes
		for (int i = 0; i < armySize; i++)
		{
			if (i % 6 == 0 && i != 0)
				counter++;

			ValidHexes[counter].AddToken(Army[i].gameObject);
		}

        // Add leader to best hex
		ValidHexes[0].AddToken(Leader.gameObject);
  
        // Alert WarManger that finished setting up
		warManager.FinishedSettingUp();
	}




	public override void RegisterTokenActions()
	{
		Debug.Log("Time to register some tokens!!!");
	}





	// TODO: Update this to be more complex
	float EvaluateStartingHex(HexComponent h)
	{
		float goodness = 0;
		// Add value depending on hex type
		goodness += GoodnessFromHexType(h.GetHexType());
		goodness += GoodnessFromDistanceToCenter(h.GetEffectiveRow());
		goodness += GoodnessFromNeighbors(h);
		goodness += GoodnessFromColumn(h);

		return goodness;
	}

	float GoodnessFromHexType(string s)
	{
		switch (s)
		{
            case "FlatForest":
				return 0.4f;

            case "Hill":
				return 0.3f;

            case "HillForest":
				return 0.5f;
               
            case "Wetland":
				return -0.5f;

			case "Pond":
                return -2f;

			case "Boulder":
                return -2f;

                
            default:
				return 0;
        }
        
	}

	float GoodnessFromDistanceToCenter(float i)
	{
		// 2 "rows" away
		if (Mathf.Abs(i - BaseRow) >= 1)
			return 0.4f;

		// 1 "row" away
		else if (Mathf.Abs(i - BaseRow) >= 0.5f)
			return 0.2f;

		else
			return 0;

	}

	float GoodnessFromNeighbors(HexComponent h)
	{
		float g = 0;
		Hex neighbor = null;

		// Check neighbors in front of spaces

        // For player 0, that means directions 5,0,1
		if (playerID == 0)
		{
			int[] dir = { 5, 0, 1 };

            for (int i = 0; i < 3; i++)
			{
				neighbor = hexMap.GetHexNeighbor(h.GetHex(), i);

				if (neighbor == null)
                {
                    g += -.1f;
                    continue;
                }

				g += GoodnessFromHexType(neighbor.GetHexType()) / 3f;
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
                    g += -.1f;
                    continue;
                }

                g += GoodnessFromHexType(neighbor.GetHexType()) / 3f;
            }

        }
            

		return g;
	}

	float GoodnessFromColumn(HexComponent hexGO)
	{
		Hex hex = hexGO.GetHex();
		float g = 0;
		int column = hex.Q;
		int dir = 1 - 2 * playerID;
		Hex next = hex;

		for (int i = 0; i < HexMap.MapTileHeight/2; i++)
		{
			next = hexMap.GetHexAt(column, next.R + dir);

			g += GoodnessFromHexType(next.GetHexType()) / 3f;
		}



		return g;
	}
}
