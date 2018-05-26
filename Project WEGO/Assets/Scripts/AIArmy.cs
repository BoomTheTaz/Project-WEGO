using System.Collections.Generic;
using UnityEngine;
using System;

public enum WarPhases { Advancing, Engaging, Retreating };

public class AIArmy : ArmyManager
{

	List<HexComponent> currentHexes;  // Used to store currently occupied hexes
	HexMap hexMap;
	HexComponent[] ValidHexes;

	SortRangedGoodness sortRanged = new SortRangedGoodness();
	SortMeleeGoodness sortMelee = new SortMeleeGoodness();
	SortCavalryGoodness sortCavalry = new SortCavalryGoodness();
	SortEffectiveRow sortEffectiveRow = new SortEffectiveRow();


	int currentPhase;

	bool HumanControlled;

	// Constructor
	public AIArmy(int id, int size, WarManager w, HexMap map) : base(id, size, w)
	{
		currentHexes = new List<HexComponent>();
		hexMap = map;

		currentPhase = (int)WarPhases.Advancing;

		// For testing purposes, allow control over first army
		// Will be placing check before registering token actions
		if (playerID == 0)
			HumanControlled = true;

		sortRanged.PlayerID = playerID;
		sortMelee.PlayerID = playerID;
		sortCavalry.PlayerID = playerID;
		sortEffectiveRow.PlayerID = playerID;
	}

	float ForwardStartScale = 0.4f;

	// Called on war start to set up tokens on hexes
	public override void SetupTokens()
	{
		// Evaluate hexes, assign values, choose tokens to place
		//Debug.Log("Setting up AI tokens.");

		ValidHexes = hexMap.GetStartingHexes(playerID);
        
        // Bias starting position towards center
		foreach (var item in ValidHexes)
		{
			Hex hex = item.GetHex();
			if (hex.Q / 2f + hex.R == 0 + hexMap.GetMapTileHeight()*playerID + 1 - 2*playerID)
			{
				item.AddGoodnessForUnit((int)UnitTypes.Melee, ForwardStartScale, playerID);
				item.AddGoodnessForUnit((int)UnitTypes.Cavalry, ForwardStartScale, playerID);
			}
		}



		// Army should always start with melee
		string currentType = Army[0].GetUnitType();
		Resort(ValidHexes, currentType);

		// Add leader to best melee hex
        ValidHexes[0].AddToken(Leader.gameObject, playerID);

		int counter = 0;
		// Assign Tokens to best hexes
		for (int i = 0; i < armySize; i++)
		{
			string type = Army[i].GetUnitType();
			if (type != currentType)
			{
				Resort(ValidHexes, type);

				currentType = type;
				counter = 0;
			}

            // If failed to add token to desired hex, resort and find next best option
			if (ValidHexes[counter].AddToken(Army[i].gameObject, playerID) == false)
			{
				counter = 0;
				Resort(ValidHexes, currentType);

				// Increment counter if cannot place token on preferred hex
                while (ValidHexes[counter].AddToken(Army[i].gameObject, playerID) == false)
                    counter++;
			}
            
            // Add to list of current hexes
			if (currentHexes.Contains(ValidHexes[counter]) == false)
				currentHexes.Add(ValidHexes[counter]);
            
			if (type != "Ranged")
				hexMap.ImproveGoodness(ValidHexes[counter],playerID);
		}



		// Undo Bias starting position towards center
        foreach (var item in ValidHexes)
        {
            Hex hex = item.GetHex();
            if (hex.Q / 2f + hex.R == 0 + hexMap.GetMapTileHeight() * playerID + 1 - 2 * playerID)
            {
                item.AddGoodnessForUnit((int)UnitTypes.Melee, -ForwardStartScale, playerID);
                item.AddGoodnessForUnit((int)UnitTypes.Cavalry, -ForwardStartScale, playerID);
            }
        }

		// Alert WarManger that finished setting up
		warManager.FinishedSettingUp();
	}


	void printValidHexes()
	{
		for (int i = 0; i < ValidHexes.Length; i++)
		{
			Debug.Log(string.Format("Hex {0}:\tR:{1}\tM:{2}\tC:{3}", i, ValidHexes[i].HexGoodness[playerID].Ranged, ValidHexes[i].HexGoodness[playerID].Melee, ValidHexes[i].HexGoodness[playerID].Cavalry));
		}
	}

	void Resort(HexComponent[] h, string s)
	{
		switch (s)
		{
			case "Melee":
				Array.Sort(h, sortMelee);
				break;
                
			case "Ranged":
				Array.Sort(h, sortRanged);
                break;
                
			case "Cavalry":
				Array.Sort(h, sortCavalry);
                break;

			default:
				break;
		}
	}

    // Store Dictionaries of valid hexes from each of the current hexes
	Dictionary<HexComponent, List<HexComponent>[]> ValidMoveHexes = new Dictionary<HexComponent, List<HexComponent>[]>();
	Dictionary<HexComponent, List<HexComponent>[]> ValidAttackHexes = new Dictionary<HexComponent, List<HexComponent>[]>();
    
	public override void RegisterTokenActions()
	{

		if (HumanControlled == false)
		{
			//SetupValidHexes();
            
			switch (currentPhase)
			{
				case (int)WarPhases.Advancing:
					//Debug.Log("Time to register some tokens!!!");
					//Debug.Log("Looking at " + currentHexes.Count.ToString() + " current hexes.");

                    // Sort current hexes with front line first
					HexComponent[] currentHexArray = currentHexes.ToArray();
					Array.Sort(currentHexArray, sortEffectiveRow);

					List<HexComponent> newCurrentHexes = new List<HexComponent>();
                    
                    // Cycle through hexes, move all tokens
					// TODO: maybe move groups of tokens
					foreach (var item in currentHexes)
					{
						item.AISelect(playerID);

						// TODO: Check for valid Attack Hexes
						List<HexComponent>[] tempAttack = hexMap.GetValidAttackHexes(item, playerID);
						List<HexComponent>[] tempMove = hexMap.GetValidMoveHexes(item,playerID);

						Token[] tokens = item.GetTokens(playerID);
                        
						// variable to scale effective row
						int mult = playerID * -2 + 1;

						foreach (var t in tokens)
						{
							
							if (t == null)
								continue;
							
							if ( tempAttack != null )
							{
								List<HexComponent> attackableHexes = new List<HexComponent>();
                                // Get all attackable hexes
								for (int i = 0; i < t.GetAttackRange(); i++)
								{
									if (tempAttack[i].Count > 0)
										attackableHexes.AddRange(tempAttack[i]);
								}

                                // If we found any, attack one
								// TODO: don't just attack first one
								if ( attackableHexes.Count > 0)
								{
									
									t.RegisterToAttack(attackableHexes[0]);
									if (newCurrentHexes.Contains(t.GetCurrentHex()) == false)
									{
										newCurrentHexes.Add(t.GetCurrentHex());
									}
									continue;

								}
							}



							List<HexComponent> tempValidMove = new List<HexComponent>();

							// Add all possible move hexes
                            for (int i = 0; i < t.GetMovementSpeed(); i++)
                            {
                                tempValidMove.AddRange(tempMove[i]);
                            }

							HexComponent[] thisValidMove = tempValidMove.ToArray();
                            
                            Resort(thisValidMove, t.GetUnitType());

							int counter = 0;
                            
							while (counter < thisValidMove.Length)
							{


								// If valid move hex has vacancies and advances towards enemy,
								// set as target and break loop
								if (thisValidMove[counter].AnyVacancies(playerID) == true &&
									thisValidMove[counter].GetEffectiveRow() * mult > t.GetCurrentHex().GetEffectiveRow() * mult)
								{
									hexMap.UpdateGoodness(t.GetCurrentHex(), thisValidMove[counter], playerID);
									t.RegisterToMove(thisValidMove[counter]);

									if (newCurrentHexes.Contains(thisValidMove[counter]) == false)
										newCurrentHexes.Add(thisValidMove[counter]);

									break;
								}

								counter++;
							}
						}
					}

					// Replace currentHexes with new
					currentHexes = newCurrentHexes;

					break;


				default:
					break;
			}

		}
	}



	#region Old Goodness Code

	//// TODO: Update this to be more complex
	//Goodness EvaluateStartingHex(HexComponent h)
	//{
	//	Goodness g = new Goodness(0, 0, 0);

	//	// Add value depending on hex type
	//	g = Goodness.AddGoodness(g, GoodnessFromHexType(h.GetHexType()));
	//	g = Goodness.AddGoodness(g, GoodnessFromDistanceToCenter(h.GetEffectiveRow()));
	//	g = Goodness.AddGoodness(g, GoodnessFromNeighbors(h));
	//	g = Goodness.AddGoodness(g, GoodnessFromColumn(h));

	//	return g;
	//}

	//   // Give each type of unit preferecnces on hex terrain
	//Goodness GoodnessFromHexType(string s)
	//{
	//	switch (s)
	//	{
	//           case "FlatForest":
	//			//return 0.4f;
	//			return new Goodness(
	//                   0.4f,     // Ranged
	//				0.4f,     // Melee
	//				0.4f);    // Cavalry

	//           case "Hill":
	//			//return 0.3f;
	//			return new Goodness(
	//                   0.4f,     // Ranged
	//                   0.3f,     // Melee
	//                   0.3f);    // Cavalry

	//           case "HillForest":
	//			//return 0.5f;
	//			return new Goodness(
	//                   0.5f,     // Ranged
	//                   0.4f,     // Melee
	//                   0.4f);    // Cavalry

	//           case "Wetland":
	//			//return -0.5f;
	//			return new Goodness(
	//                   -1,     // Ranged
	//                   -1,     // Melee
	//                   -1);    // Cavalry

	//		case "Pond":
	//               //return -2f;
	//			return new Goodness(
	//                   -2,     // Ranged
	//                   -2,     // Melee
	//                   -2);    // Cavalry

	//		case "Boulder":
	//               //return -2f;
	//			return new Goodness(
	//                   -2,     // Ranged
	//                   -2,     // Melee
	//                   -2);    // Cavalry


	//           default:
	//			return null;
	//       }

	//}

	//   // Bias selection towards center of battlefield
	//Goodness GoodnessFromDistanceToCenter(float i)
	//{

	//	// 2 "rows" away
	//	if (Mathf.Abs(i - BaseRow) >= 1)
	//	{
	//		return new Goodness(
	//                   0.2f,     // Ranged
	//                   0.4f,     // Melee
	//                   0.4f);    // Cavalry
	//	}

	//	// 1 "row" away
	//	else if (Mathf.Abs(i - BaseRow) >= 0.5f)
	//	{
	//		return new Goodness(
	//                   0.4f,     // Ranged
	//                   0.2f,     // Melee
	//                   0.2f);    // Cavalry
	//	}

	//	return null;

	//}

	//   // Give each type of unit preference on neighbor terrain types in front of it
	//Goodness GoodnessFromNeighbors(HexComponent h)
	//{
	//	Goodness g = new Goodness(0,0,0);
	//	Hex neighbor = null;

	//	// Check neighbors in front of spaces

	//       // For player 0, that means directions 5,0,1
	//	if (playerID == 0)
	//	{
	//		int[] dir = { 5, 0, 1 };

	//           for (int i = 0; i < 3; i++)
	//		{
	//			neighbor = hexMap.GetHexNeighbor(h.GetHex(), dir[i]);

	//			if (neighbor == null)
	//               {
	//				g.AddToAll(-.1f);
	//                   continue;
	//               }

	//			// Evaluate neighbor goodness
	//               Goodness ng = GoodnessFromHexType(neighbor.GetHexType());


	//			if (ng != null)
	//			{
	//				// Scale down
	//				ng.DivideBy(3f);

	//				// Add to current goodness
	//				g = Goodness.AddGoodness(g, ng);
	//			}
	//		}


	//	}

	//	// For player 1, that means directions 2,3,4
	//	if (playerID == 1)
	//       {

	//		for (int i = 2; i < 5; i++)
	//           {
	//               neighbor = hexMap.GetHexNeighbor(h.GetHex(), i);

	//			if (neighbor == null)
	//               {
	//                   g.AddToAll(-.1f);
	//                   continue;
	//               }

	//               // Evaluate neighbor goodness
	//               Goodness ng = GoodnessFromHexType(neighbor.GetHexType());

	//			if (ng != null)
	//			{

	//				// Scale down
	//				ng.DivideBy(3f);

	//				// Add to current goodness
	//				g = Goodness.AddGoodness(g, ng);
	//			}
	//           }

	//       }


	//	return g;
	//}

	//Goodness GoodnessFromColumn(HexComponent hexGO)
	//{
	//	Hex hex = hexGO.GetHex();
	//	Goodness g = new Goodness(0,0,0);
	//	int column = hex.Q;
	//	int dir = 1 - 2 * playerID;
	//	Hex next = hex;

	//	for (int i = 0; i < HexMap.MapTileHeight; i++)
	//	{
	//		next = hexMap.GetHexAt(column, next.R + dir);
	//		if (next == null)
	//			break;

	//		Goodness ng = GoodnessFromHexType(next.GetHexType());

	//		if (ng != null)
	//		{
	//			ng.DivideBy(5f);
	//			g = Goodness.AddGoodness(g, ng);
	//		}
	//	}



	//	return g;
	//}

#endregion
}
