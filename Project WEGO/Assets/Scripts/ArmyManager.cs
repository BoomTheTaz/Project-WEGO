using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmyManager {

    WarManager warManager;
	Token[] Army;
    LeaderToken Leader;

	int playerID;
    int armySize;
	int TokensPlaced;
	int positionTracker;

    string[] types = { "Melee", "Ranged", "Cavalry" };


	public ArmyManager(int id, int size, WarManager w)
	{
		playerID = id;
		armySize = size;
       
		Army = new Token[size];

		warManager = w;

        // Initialize
		positionTracker = 0;
	}
    

    public void AddTokenToArmy(Token t)
    {
		Army[positionTracker] = t;

		positionTracker++;
    }

    public void SetLeader(LeaderToken l)
    {
        Leader = l;

        // Reset for taking out of Tokens, TEMPORARY FUNCTION
		positionTracker = 0;
    }


    // Retrieve a token from the army list
    // TODO: Make this take input parameters for specific type of Token
    //       ALSO Prevent token from being taken if not used on hex

	public GameObject GetToken()
    {
		if (positionTracker < armySize)
		{
			return Army[positionTracker].gameObject;

		}
		else if (positionTracker == armySize)
		{
			// TODO Function that alerts that end of army
			return Leader.gameObject;
		}
		else
		{
			Debug.Log("No Pieces left to place");
			return null;
		}

    }

    public void UsedToken(bool b)
    {
		if (positionTracker == armySize)
            warManager.FinishedSettingUp();

		if (b == true)
			positionTracker++;

    }
    

}
