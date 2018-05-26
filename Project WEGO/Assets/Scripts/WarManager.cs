using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Use this class to manage the various war states
public class WarManager : MonoBehaviour
{

    public Color MainColor1;
    public Color AccentColor1;
    public Color MainColor2;
    public Color AccentColor2;

    public GameObject ArmyTokenPrefab1;
    public GameObject ArmyTokenPrefab2;
    public GameObject LeaderTokenPrefab1;
    public GameObject LeaderTokenPrefab2;

    public bool IsSettingUp = true;
    MouseController mouse;
    UIManager UI;
    HexMap hexMap;

    List<Token> TokensToMove = new List<Token>();
    List<Token> TokensToRemove = new List<Token>();
	List<Token> TokensToAttack = new List<Token>();

    // Delegate to trigger movement of tokens
    delegate void MovementUpdate();
    MovementUpdate UpdateTokenMovement;


    Vector3 velocity;
    Transform Target;
    GameObject[] ToMove;

	bool OneArmyDone = false;

	bool[] IsAI = { true, true };

    string[] ArmyUnits1 = {
		"Melee","Melee","Melee","Melee","Melee","Melee","Melee","Melee","Melee","Melee","Melee","Melee",
		"Melee","Melee","Melee","Melee","Melee","Melee","Melee","Melee","Melee","Melee","Melee","Melee",
		"Cavalry","Cavalry","Cavalry","Cavalry","Cavalry","Cavalry","Cavalry","Cavalry","Cavalry",
		"Cavalry","Cavalry","Cavalry","Cavalry","Cavalry","Cavalry","Cavalry","Cavalry","Cavalry",
		"Ranged","Ranged","Ranged","Ranged","Ranged","Ranged","Ranged","Ranged","Ranged",
		"Ranged","Ranged","Ranged","Ranged","Ranged","Ranged","Ranged","Ranged","Ranged"
	};

	//   string[] ArmyUnits2 = {
	//	"Melee","Melee","Melee","Melee","Melee","Melee","Melee","Melee","Melee","Melee","Melee","Melee","Melee","Melee",
	//	"Melee","Melee","Melee","Melee","Melee","Melee","Melee","Melee","Melee","Melee","Melee","Melee","Melee","Melee",
	//	"Cavalry","Cavalry","Cavalry","Cavalry","Cavalry","Cavalry",
	//	"Cavalry","Cavalry","Cavalry","Cavalry","Cavalry","Cavalry",
	//	"Ranged","Ranged","Ranged","Ranged","Ranged","Ranged","Ranged","Ranged","Ranged","Ranged",
	//	"Ranged","Ranged","Ranged","Ranged","Ranged","Ranged","Ranged","Ranged","Ranged","Ranged"
	//};

	string[] ArmyUnits2 = {
		"Ranged", "Ranged", "Ranged", "Ranged", "Ranged", "Ranged", "Ranged", "Ranged", "Ranged", "Ranged", "Ranged", "Ranged"
	};


    ArmyManager[] Armies = new ArmyManager[2];

	// Use this for initialization
	void Start () {

        mouse = FindObjectOfType<MouseController>();
        UI = FindObjectOfType<UIManager>();
        hexMap = FindObjectOfType<HexMap>();

        UI.InSetup();
        SetupArmies();
	}




    void SetupArmies()
    {
        // Change Material colors
        ArmyTokenPrefab1.GetComponent<Token>().SetColors(MainColor1, AccentColor1);
        ArmyTokenPrefab2.GetComponent<Token>().SetColors(MainColor2, AccentColor2);
		LeaderTokenPrefab1.GetComponent<LeaderToken>().SetColors(MainColor1, AccentColor1);
		LeaderTokenPrefab2.GetComponent<LeaderToken>().SetColors(MainColor2, AccentColor2);

        Token temp;

		if (IsAI[0] == false)
		    Armies[0] = new ArmyManager(0, ArmyUnits1.Length,this);
		else
			Armies[0] = new AIArmy(0, ArmyUnits1.Length, this, hexMap);

        foreach (var a in ArmyUnits1)
        {
			temp = Instantiate(ArmyTokenPrefab1, transform.position + Vector3.down, Quaternion.identity, transform).GetComponent<Token>();
            temp.SetUp(a, this, 0, IsAI[0]);
            
            Armies[0].AddTokenToArmy(temp);
        }

        // Set army 1 leader token
		temp = Instantiate(LeaderTokenPrefab1,transform.position + Vector3.down, Quaternion.identity, transform).GetComponent<Token>();
		temp.SetUp("Leader", this, 0, IsAI[0]);
		Armies[0].SetLeader(temp.GetComponent<LeaderToken>());

        
		if (IsAI[1] == false)
			Armies[1] = new ArmyManager(1, ArmyUnits2.Length, this);
		else
			Armies[1] = new AIArmy(1, ArmyUnits2.Length, this, hexMap);
		
        foreach (var a in ArmyUnits2)
        {
            temp = Instantiate(ArmyTokenPrefab2, transform.position + Vector3.down, Quaternion.identity, transform).GetComponent<Token>();
			temp.SetUp(a, this, 1, IsAI[1]);

            Armies[1].AddTokenToArmy(temp);
        }

		temp = Instantiate(LeaderTokenPrefab2, transform.position + Vector3.down, Quaternion.identity, transform).GetComponent<Token>();
		temp.SetUp("Leader", this, 1, IsAI[1]);
        Armies[1].SetLeader(temp.GetComponent<LeaderToken>());



		// Setup AI Tokens
		if (IsAI[0] == true)
			Armies[0].SetupTokens();

		if (IsAI[1] == true)
            Armies[1].SetupTokens();

    }
	
    // Update is called once per frame
	void Update () {

        // Test key for activating token movement
        if (Input.GetKeyDown(KeyCode.Space))
        {
            //Debug.Log("Instructing to move tokens");
            UpdateTokenMovement += MoveTokens;

			InitiateAttacks();

            mouse.StartingMove();
            hexMap.OnTurnEnd();
        }

        


		// Call Token movement delegate if not null
        if (UpdateTokenMovement != null)
            UpdateTokenMovement();

	}

    // Call after setup complete, ensures that everything knows setup is done
    public void FinishedSettingUp()
    {
		if (OneArmyDone == false)
		{
			OneArmyDone = true;
			return;
		}

        IsSettingUp = false;
		hexMap.ClearStartingHexes();
        mouse.ToGameplay();
        UI.LeavingSetup();

		NewAITurn();

        //Debug.Log("Setup is complete!");
    }

    // Add inputted token to List of TokensToMove
    public void RegisterTokenToMove(Token token)
    {
        TokensToMove.Add(token);
    }

    // Remove inputted token from List of TokensToMove
    public void UnregisterTokenToMove(Token token)
    {
        
        // FIXME Decide if this if check is necessary

        if (TokensToMove.Contains(token))
            TokensToMove.Remove(token);


    }

	// Add inputted token to List of TokensToAttack
    public void RegisterTokenToAttack(Token token)
    {
		TokensToAttack.Add(token);
    }

	// Remove inputted token from List of TokensToAttack
    public void UnregisterTokenToAttack(Token token)
    {

        // FIXME Decide if this if check is necessary

		if (TokensToAttack.Contains(token))
			TokensToAttack.Remove(token);


    }


    // Move all registered Tokens
    void MoveTokens()
    {
        // Move every token registered
        foreach (var t in TokensToMove)
        {
            
            // If token.move() is true, that means close enough to target position,
            // so unregister that token
            if (t.Move() == true)
            {
                TokensToRemove.Add(t);

            }
        }

        if (TokensToRemove.Count > 0)
        {
            foreach (var r in TokensToRemove)
            {
                TokensToMove.Remove(r);
                r.Deselect();
            }

            TokensToRemove.Clear();
        }

        // If there are no tokens left to move, unregister delegate
        if (TokensToMove.Count == 0)
        {
            //Debug.Log("All tokens done moving");
            UpdateTokenMovement = null;

			NewAITurn();
        }
    }

	List<HexComponent> AttackedHexes = new List<HexComponent>();
	void InitiateAttacks()
	{
		// skip if no tokens attacking
		if ( TokensToAttack.Count > 0)
		{
			// cyce through attacking tokens, trigger attack, add attacked hex to list
			foreach (var item in TokensToAttack)
			{
				item.Attack();

				if (AttackedHexes.Contains(item.GetHexAttacking()) == false)
					AttackedHexes.Add(item.GetHexAttacking());
			}

            // evaluate damage on hexes
			foreach (var item in AttackedHexes)
			{
				item.EvaluateDamage();
			}


			TokensToAttack.Clear();
			AttackedHexes.Clear();
		}

	}

	public GameObject GetToken(int player)
	{
		return Armies[player].GetToken();
	}


    public void UsedToken (int player, bool b)
	{
		Armies[player].UsedToken(b);
	}

	// Call to have the AI decide on a new turn
    void NewAITurn()
	{
		if (IsAI[0] == true)
            Armies[0].RegisterTokenActions();
        if (IsAI[1] == true)
            Armies[1].RegisterTokenActions();
	}

}
