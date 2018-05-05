using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmyManager : MonoBehaviour {

    public GameObject TokenPrefab;
    public GameObject LeaderTokenPrefab;
    public Sprite LeaderSprite;
    public Sprite[] Sprites;

    public Color MainColor;
    public Color AccentColor;

    WarManager warManager;
    GameObject[] Army;

    int armySize=10;
    int AvailableUnits;

    bool firstToken = true;

    GameObject Leader;

	// Use this for initialization
	void Start () {

        Army = new GameObject[armySize+1];

        warManager = FindObjectOfType<WarManager>();

        CreateArmy();
	}


    string[] types = { "Melee", "Ranged", "Cavalry" };

    void CreateArmy()
    {

        // Create Leader Token
        Leader = Instantiate(LeaderTokenPrefab);
        Leader.GetComponent<Transform>().position += Vector3.down;
        Leader.GetComponent<LeaderToken>().SetUp(MainColor, AccentColor, "Leader", warManager);
        Army[0] = Leader;
        AvailableUnits++;


        for (int i = 1; i < armySize+1; i++)
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
                t.GetComponent<Token>().SetUp(MainColor, AccentColor, types[Random.Range(0, types.Length)],warManager);
                firstToken = false;
            }
            else
                t.GetComponent<Token>().SetUp(AccentColor, types[Random.Range(0,types.Length)],warManager);

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
