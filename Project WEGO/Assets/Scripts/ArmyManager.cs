using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmyManager : MonoBehaviour {

    public GameObject TokenPrefab;
    public Sprite[] Sprites;

    public Color MainColor;
    public Color AccentColor;

    WarManager warManager;
    GameObject[] Army;

    int armySize=10;
    int AvailableUnits;

    bool firstToken = true;

	// Use this for initialization
	void Start () {

        Army = new GameObject[armySize];

        warManager = FindObjectOfType<WarManager>();

        CreateArmy();
	}


    void CreateArmy()
    {
        for (int i = 0; i < armySize; i++)
        {
            // Create Token
            GameObject t = Instantiate(TokenPrefab);

            // Place under map for now
            // TODO: Find better way to handle this, probably create on command
            //       and just track how many there are left
            t.GetComponent<Transform>().position += Vector3.down;

            // Setup Token with Desired colors and sprites

            if (firstToken == true)
            {
                t.GetComponent<Token>().SetUp(MainColor, AccentColor, Sprites[Random.Range(0, 2)]);
                firstToken = false;
            }
            else
                t.GetComponent<Token>().SetUp(AccentColor, Sprites[Random.Range(0,2)]);

            // Add token to the army
            Army[AvailableUnits] = t;

            // Increment number of available units
            AvailableUnits++;

        }
    }

    // Retrieve a token from the army list
    // TODO: Make this take input parameters for specific type of Token
    //       ALSO Prevent token from being taken if not used on hex

    public GameObject GetToken()
    {
        if (AvailableUnits > 0)
        {
            AvailableUnits--;

            return Army[AvailableUnits];

        }
        else
        {
            Debug.Log("AHHHH!!! THE ARMY IS EMPTY. WE SHOULDN'T BE HERE");
            return null;
        }

    }

    public void UsedToken(bool b)
    {
        if (b == false)
            AvailableUnits++;
        else if (AvailableUnits == 0)
            warManager.FinishedSettingUp();
    }


}
