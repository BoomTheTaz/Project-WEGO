using System.Collections.Generic;
using UnityEngine;


public class HexComponent : MonoBehaviour
{

	//=====================================

	// TODO ASAP: Store old tokens somewhere.
	//  Currently parenting tokens immediately causes them to be registered
	// As on a separate tile.  Clicking that new tile will move them again


	bool[][] SpotAvailable = new bool[2][];

    Hex hex;
    string type;
    
    public Transform TokenPlacement;
	public Transform[][] TokenLocations = new Transform[2][];
	public Transform[] TokenLocations0;
	public Transform[] TokenLocations1;

    public Transform[] LeaderLocations;
    public GameObject Outliner;

    public Material RedOutline;
    public Material BlueOutline;
    public Material YellowOutline;
    public Material WhiteOutline;

	public bool IsHill;
	public bool IsWetland;
	public bool IsForest;

    int minMovement;
    int minAttack;
    int prevMinMovement;
    int prevMinAttack;

	float[] currentDamage = new float[2];

    bool allowTokensToUpdateValidHexes = false;
    
    public bool IsSelected;
	public bool ValidStarting0 = false;
	public bool ValidStarting1 = false;
    bool SelectedThisTurn = false;
    
    Token[] Tokens = new Token[7];

    HexStats hexStats;
    HexMap hexMap;

    static int maxRange = 4;
    List<HexComponent>[] ValidMoveHexes = new List<HexComponent>[maxRange];
    List<HexComponent>[] ValidAttackHexes = new List<HexComponent>[maxRange];

	public Goodness[] HexGoodness { get; private set; }

    enum States { Moving, Attacking, Defending };

    int currentState;

	private void Awake()
	{
		SpotAvailable[0] = new bool[] { true, true, true, true, true, true };
        SpotAvailable[1] = new bool[] { true, true, true, true, true, true };
		TokenLocations[0] = TokenLocations0;
        TokenLocations[1] = TokenLocations1;
		HexGoodness = new Goodness[2];
		HexGoodness[0] = new Goodness(0, 0, 0);
        HexGoodness[1] = new Goodness(0, 0, 0);
	}

	private void Start()
	{
       

        for (int i = 0; i < maxRange; i++)
        {
            ValidMoveHexes[i] = new List<HexComponent>();
            ValidAttackHexes[i] = new List<HexComponent>();
        }


	}

	public void SetHexMap(HexMap map)
    {
        hexMap = map;
    }

    public void SetHex(Hex h)
    {
        hex = h;

    }

    public Hex GetHex()
	{
		return hex;
	}

    // Set Height of Token placement
    void SetTokenHeight(float f)
    {
        TokenPlacement.position = transform.localPosition + Vector3.up * f;
    }


    // Function to take Token and arrange it on tile
    public bool AddToken(GameObject token, int player)
    {
        if (token != null)
        {

			if (token.GetComponent<Token>().GetMovementSpeed() > MaxMovement)
				MaxMovement = token.GetComponent<Token>().GetMovementSpeed();
			if (token.GetComponent<Token>().GetAttackRange() > MaxAttack)
				MaxAttack = token.GetComponent<Token>().GetAttackRange();

            // TODO Make this better
            if (token.GetComponent<LeaderToken>() != null)
            {
                token.transform.parent = LeaderLocations[player];
                token.GetComponent<Token>().SetCurrentHex(this);
                


                // Move to parent location
                token.transform.localPosition = Vector3.zero;
                return true;
            }

            if (AnyVacancies(player) == true)
            {
                token.GetComponent<Token>().SetCurrentHex(this);
                
                Transform t = GetNextAvailableTokenLocation(player);

                // Set Parent
                token.transform.parent = t;

                // Tell token which location number it is on
                token.GetComponent<Token>().SetCurrentLocationOnHex(t);

                // Move to parent location
                token.transform.localPosition = Vector3.zero;

                // Rotate to parent rotation
                token.transform.rotation = t.rotation;

                return true;
            }
            else
            {
                //Debug.Log(string.Format("Maximum number of tokens reached on hex at ({0},{1})", hex.Q, hex.R));
                return false;
            }

        }
        return true;
    }


    int MaxMovement;
    int MaxAttack;

    // Function called when clicked on during gameplay, prepares tokens, etc.
    public void Selected()
    {
        // Outline this in white
        OutlineWhite();
        //hexMap.SetCurrentHexGO(this);
        currentState = hexMap.CurrentState;

		MaxAttack = 0;
        MaxMovement = 0;

        if (SelectedThisTurn == false)
        {

            // Get Tokens on Hex
            for (int c = 0; c < TokenLocations[0].Length; c++)
            {
                if (TokenLocations[0][c].childCount != 0)
                {
					Tokens[c] = TokenLocations[0][c].GetChild(0).gameObject.GetComponent<Token>();

                    // Update Max Movement as necessary
                    if (Tokens[c].GetMovementSpeed() > MaxMovement)
                        MaxMovement = Tokens[c].GetMovementSpeed();

                    // Update Max Attack as necessary
                    if (Tokens[c].GetAttackRange() > MaxAttack)
                        MaxAttack = Tokens[c].GetAttackRange();
                }
                else
                    Tokens[c] = null;

            }

            // Check for Leader token
            if (LeaderLocations[0].childCount != 0)
            {
                Tokens[6] = LeaderLocations[0].GetChild(0).gameObject.GetComponent<Token>();
                if (Tokens[6].GetMovementSpeed() > MaxMovement)
                    MaxMovement = Tokens[6].GetMovementSpeed();
                if (Tokens[6].GetAttackRange() > MaxAttack)
                    MaxAttack = Tokens[6].GetAttackRange();

            }
            else
                Tokens[6] = null;


            // Get Valid Hexes for movement and Attacking
            List<HexComponent>[] temp = hexMap.GetValidMoveHexes(this,0);
            for (int l = 0; l < maxRange; l++)
            {
                foreach (var i in temp[l])
                {
                    ValidMoveHexes[l].Add(i);
                }
            }

			// TODO: figure out how hex components call this, maybe player object that stores id, etc.
            temp = hexMap.GetValidAttackHexes(this,0);

			if (temp != null)
			{
				for (int l = 0; l < maxRange; l++)
				{
					foreach (var i in temp[l])
					{
						ValidAttackHexes[l].Add(i);
					}
				}
			}
            SelectedThisTurn = true;
        }

        SelectAllTokens();



        if (ValidMoveHexes == null)
            Debug.Log("Valid Hexes still null.");
       
        allowTokensToUpdateValidHexes = true;
        UpdateMinValues();
        DrawOutlines();
        IsSelected = true;

        
    }

    public void AISelect(int player)
	{

		MaxAttack = 0;
        MaxMovement = 0;

		// Get Tokens on Hex
        for (int c = 0; c < TokenLocations[player].Length; c++)
        {
            if (TokenLocations[player][c].childCount != 0)
            {
                Tokens[c] = TokenLocations[player][c].GetChild(0).gameObject.GetComponent<Token>();

                // Update Max Movement as necessary
                if (Tokens[c].GetMovementSpeed() > MaxMovement)
                    MaxMovement = Tokens[c].GetMovementSpeed();

                // Update Max Attack as necessary
                if (Tokens[c].GetAttackRange() > MaxAttack)
                    MaxAttack = Tokens[c].GetAttackRange();
            }
            else
                Tokens[c] = null;

        }

        // Check for Leader token
        if (LeaderLocations[player].childCount != 0)
        {
            Tokens[6] = LeaderLocations[player].GetChild(0).gameObject.GetComponent<Token>();
            if (Tokens[6].GetMovementSpeed() > MaxMovement)
                MaxMovement = Tokens[6].GetMovementSpeed();
            if (Tokens[6].GetAttackRange() > MaxAttack)
                MaxAttack = Tokens[6].GetAttackRange();

        }
        else
            Tokens[6] = null;



		UpdateMinValues(false);
	}

    public void Deselected()
    {
        // TODO Figure this ish out


        Outliner.SetActive(false);
        IsSelected = false;
        allowTokensToUpdateValidHexes = false;

        if (minMovement != -1)
        {
            UnoutlineValidHexes(true);
        }

        foreach (var t in Tokens)
        {
            if (t != null)
            {
                t.Deselect();
            }
        }
    }


    public void SelectAllTokens()
    {
        foreach (var t in Tokens)
        {
            if (t != null)
            {
                t.Select();

            }
        }

    }

    public void RegisterTokensToMove(HexComponent hexGO)
    {
        // Only register if it is a valid move
        if (IsValidMove(hexGO) == true)
        {
            foreach (var t in Tokens)
            {
                if (t != null)
                {
                    //if (t.GetComponent<LeaderToken>() != null)
                    //{
                    //    t.GetComponent<Token>().RegisterToMove(hexGO);
                    //}
                    //else
                    //t.GetComponent<Token>().RegisterToMove(hexGO);

                    t.RegisterToMove(hexGO);
                    allowTokensToUpdateValidHexes = false;
                    t.Deselect();
                    allowTokensToUpdateValidHexes = true;
                    UpdateMinValues();
                }
            }
        }
        else
            Debug.Log("At least one piece cannot move there. Please pick an outlined spot.");
    }

    bool IsValidMove(HexComponent h)
    {
        for (int i = 0; i < minMovement; i++)
        {
            if (ValidMoveHexes[i].Contains(h)) 
                return true;
        }

        return false;
    }


	public void RegisterTokensToAttack(HexComponent hex)
	{
		if (IsValidAttack(hex) == true)
		{

			foreach (var t in Tokens)
			{
				if (t != null)
				{

					t.RegisterToAttack(hex);
					allowTokensToUpdateValidHexes = false;
					t.Deselect();
					allowTokensToUpdateValidHexes = true;
					UpdateMinValues();
				}
			}
		}

	}

	bool IsValidAttack(HexComponent h)
	{
		for (int i = 0; i < minAttack; i++)
        {
            if (ValidAttackHexes[i].Contains(h))
                return true;
        }

        return false;
	}

    //int VacanciesFilled = 0;
    public Transform GetNextAvailableTokenLocation(int player)
    {

        for (int i = 0; i < SpotAvailable[player].Length; i++)
        {
            if (SpotAvailable[player][i] == true)
            {
                SpotAvailable[player][i] = false;
                return TokenLocations[player][i];
            }
        }

        Debug.LogError("No Vacancies!");
        return null;

    }
    
    public void DecrementReservation(int spot, int player)
    {
        SpotAvailable[player][spot] = true;
    }

    public bool AnyVacancies(int player)
    {
		
        for (int i = 0; i < SpotAvailable[player].Length; i++)
        {
            if (SpotAvailable[player][i] == true)
                return true;
        }
        return false;
    }

    // TODO better this once finalize hex designs
    public void SetHexAsType(string s)
    {
        type = s;
        // Set up hex stats
        hexStats = HexTemplate.GetStatsForType(s);

        SetTokenHeight(hexStats.TokenHeight);

		hex.SetHexType(s);

    }

	// Tokens will call this when they are selected/deselected
	public void UpdateMinValues(bool shouldOutline = true)
    {
        
        if (allowTokensToUpdateValidHexes == false)
            return;

        prevMinMovement = minMovement;
        prevMinAttack = minAttack;

        if (prevMinMovement == -1)
            prevMinMovement = 0;

        if (prevMinAttack == -1)
            prevMinAttack = 0;

        minMovement = -1;
        minAttack = -1;

        // Cycle through Each token and check the selected ones for min movement
        foreach (Token t in Tokens)
        {
            if (t != null && t.IsCurrentlySelected())
            {
                // Update if movespeed is less or not set, i.e. -1
                if (t.GetMovementSpeed() < minMovement || minMovement < 0)
                {
                    minMovement = t.GetMovementSpeed();
                }

                // Update if attack range is less or not set, i.e. -1
                if (t.GetAttackRange() < minAttack || minAttack < 0)
                {
                    minAttack = t.GetAttackRange();
                }

            }
        }

		if (shouldOutline == false)
			return;

        // None Selected, Unoutline everything
        if (minMovement == -1)
        {
            minMovement++;
            minAttack++;

            for (int i = 0; i < prevMinMovement; i++)
            {
                foreach (var item in ValidMoveHexes[i])
                {
                    item.NoOutline();
                }
            }
        }

        // If minMovement has changed, update the outlines
        if (prevMinMovement != minMovement  && currentState == (int)States.Moving && IsSelected == true)
        {
            UpdateOutlines();
        }

        // If minMovement has changed, update the outlines
        if (prevMinAttack != minAttack && currentState == (int)States.Attacking && IsSelected == true)
        {
            UpdateOutlines();
        }

    }

    // Called to outline valid hexes, used on hex selection and after no tokens selected
    void DrawOutlines()
    {
        switch ((int)currentState)
        {
            case (int)States.Moving:

                for (int i = 0; i < minMovement; i++)
                {
                    foreach (var item in ValidMoveHexes[i])
                    {
                        OutlineAppropriately(item);
                    }
                }
                break;

            case (int)States.Attacking:

                for (int i = 0; i < minAttack; i++)
                {
                    foreach (var item in ValidAttackHexes[i])
                    {
                        OutlineAppropriately(item);
                    }
                }
                break;


            default:
                break;
        }



    }

    void UpdateOutlines()
    {
        // Outline updates will depend on current state
        switch ((int)currentState)
        {
            // If we are moving, update using move array
            case (int)States.Moving:

                // outline More
                if (prevMinMovement < minMovement)
                {
                    for (int i = prevMinMovement; i < minMovement; i++)
                    {
                        foreach (var item in ValidMoveHexes[i])
                        {
                            OutlineAppropriately(item);
                        }
                    }

                }

                // Unoutline some
                else if (prevMinMovement > minMovement)
                {
                    for (int i = prevMinMovement; i > minMovement; i--)
                    {
                        foreach (var item in ValidMoveHexes[i-1])
                        {
                            item.NoOutline();
                        }
                    }

                }

                break;

            case (int)States.Attacking:

                // outline More
                if (prevMinAttack < minAttack)
                {
                    for (int i = prevMinAttack; i < minAttack; i++)
                    {
                        foreach (var item in ValidAttackHexes[i])
                        {
                            OutlineAppropriately(item);
                        }
                    }

                }

                // Unoutline some
                else if (prevMinAttack > minAttack)
                {
                    for (int i = prevMinAttack; i > minAttack; i--)
                    {
                        foreach (var item in ValidAttackHexes[i - 1])
                        {
                            item.NoOutline();
                        }
                    }

                }

                break;


            default:
                break;
        }
    }

    void OutlineAppropriately(HexComponent h)
    {
        // TODO Change this to vary based on level
        // ======== OR ========
        // remove fatigue level altogether going back to bool
        switch (currentState)
        {
            case (int)States.Moving:
                if (h.GetFatigueLevel() > 0)
                    h.OutlineYellow();
                else
                    h.OutlineBlue();
                break;

            case (int) States.Attacking:
                h.OutlineRed();
                break;

            default:
                break;
        }
    }
   

    // TODO: Maybe compare with new outline to prevent over calling outliner.SetActive
    void UnoutlineValidHexes(bool allStates = false)
    {
        if (allStates == true)
        {
            for (int k = minAttack; k > 0; k--)
            {
                foreach (var i in ValidAttackHexes[k - 1])
                {
                    i.NoOutline();
                }
            }

            for (int k = minMovement; k > 0; k--)
            {
                foreach (var i in ValidMoveHexes[k - 1])
                {
                    i.NoOutline();
                }
            }
            return;
        }

        switch (currentState)
        {
            // Unoutline all valid moving hexes
            case (int)States.Moving:
                for (int k = minMovement; k > 0; k--)
                {
                    foreach (var i in ValidMoveHexes[k-1])
                    {
                        i.NoOutline();
                    }
                }
                break;

            // Unoutline all valid attacking hexes
            case (int)States.Attacking:
                for (int k = minAttack; k > 0; k--)
                {
                    foreach (var i in ValidAttackHexes[k - 1])
                    {
                        i.NoOutline();
                    }
                }
                break;


            default:
                break;
        }

    }

    public void OutlineRed()
    {
        Outliner.SetActive(true);
        Outliner.GetComponent<MeshRenderer>().material = RedOutline;
    }

    public void OutlineBlue()
    {
        Outliner.SetActive(true);
        Outliner.GetComponent<MeshRenderer>().material = BlueOutline;
    }

    public void OutlineYellow()
    {
        Outliner.SetActive(true);
        Outliner.GetComponent<MeshRenderer>().material = YellowOutline;
    }

    public void OutlineWhite()
    {
        Outliner.SetActive(true);
        Outliner.GetComponent<MeshRenderer>().material = WhiteOutline;
    }

    public void NoOutline()
    {
        Outliner.SetActive(false);
    }

    public bool IsTraversable()
    {
        return hexStats.IsTraversable;
    }

    public int GetFatigueLevel()
    {
        return hexStats.FatigueValue;
    }

    public int GetMinMove()
    {
        return minMovement;
    }

    public int GetMinAttack()
    {
        return minAttack;
    }

    public string GetHexType()
    {
        return type;
    }

    public int GetMaxMovement()
    {
        return MaxMovement;
    }

    public int GetMaxAttack()
    {
        return MaxAttack;
    }


    public void ResetForNewTurn()
    {
        SelectedThisTurn = false;

        for (int i = 0; i < maxRange; i++)
        {
            ValidMoveHexes[i].Clear();
        }
		currentState = (int)States.Moving;
    }

    public float GetElevation()
    {
        return hexStats.TokenHeight;
    }
    
    public void UpdateCurrentState(int i)
    {
        // Unoutline relevant hexes from previous state
        UnoutlineValidHexes();

        // Update to new current state
        currentState = i;
		hexMap.ChangeStates(i);

		if (currentState == (int)States.Defending)
			;
		
		// Draw relevant hexes for current state
		else
            DrawOutlines();
    }
    
	public void SetValidStarting(int player)
	{
        switch (player)
		{
			case 0:
				ValidStarting0 = true;
				break;

			case 1:
				ValidStarting1 = true;
				break;

			default:
				break;
		}

		OutlineWhite();

	}

    public float GetEffectiveRow()
	{
		return hex.EffectiveRow;
	}

    public void AddGoodness(Goodness g, int player)
	{
		HexGoodness[player] = Goodness.AddGoodness(HexGoodness[player], g);

	}

    public void AddGoodnessForUnit(int type, float amount, int player)
	{

		HexGoodness[player].AddTo(type, amount);
        
	}

	public void SetBaseGoodness(Goodness g)
	{
		if (HexGoodness[1] == null)
			Debug.Log("WTF");
		HexGoodness[0] = Goodness.AddGoodness(HexGoodness[0], g);
		HexGoodness[1] = Goodness.AddGoodness(HexGoodness[1], g);

	}
    
    public bool IsEnemyOn(int player)
	{
		int enemy = (player + 1) % 2;
        
		for (int i = 0; i < TokenLocations[enemy].Length; i++)
		{
			if (TokenLocations[enemy][i].childCount > 0)
				return true;
		}

		return false;

	}

	public bool IsPlayerOn(int player)
	{
		return IsEnemyOn((player + 1) % 2);
	}

	public Token[] GetTokens(int player)
	{
		Token[] result = new Token[6];

        for (int i = 0; i < 6; i++)
		{
			if (TokenLocations[player][i].childCount ==  0)
				continue;

			result[i] = TokenLocations[player][i].GetChild(0).GetComponent<Token>();
		}
         
		return result;
	}
    
    // Store damage to evaluate later
    public void TakeDamage(float f, int player)
	{
        // Storing damage in other playerID for ease in damage evaluation
		int other = (player + 1) % 2;
		currentDamage[other] += f;
	}

    // calculate units lost and deal damage to tokens
    public void EvaluateDamage()
	{
		
		if(currentDamage[0] > 0)
		{
			DamageTokens(0);
		}
		if (currentDamage[1] > 0)
        {
			DamageTokens(1);
        }

		EndAttack();
	}

	int DamageMidpoint = 25;

    void DamageTokens(int player)
	{
		Token[] tokens = GetTokens(player);

		float def = 0;

        foreach (var item in tokens)
		{
			if (item == null)
				continue;
			def += item.GetDefense();
		}

        if (def == 0)
		{
			// Something went horribly wrong, are we getting rid of dead tokens??
            // Maybe we should reevaluate AI moves
			Debug.LogError("How can there be no defense");
            
			return;
		}

		int unitsLost = Mathf.RoundToInt(currentDamage[player] / def * DamageMidpoint);
		Debug.Log("Units Lost: " + unitsLost.ToString());

		foreach (var item in tokens)
		{
			if (item != null)
			    item.TakeDamage(unitsLost);
		}


		// Assign Damage
	}
    
	void EndAttack()
	{
		currentDamage[0] = 0;
		currentDamage[1] = 0;

		// TODO: Decide how to handle token death, self report, check for vacancy, ???
	}

    public bool IsLeaderOn(int player)
	{
		if (LeaderLocations[player].childCount == 1)
			return true;
		return false;
	}

	public void RegisterTokens(HexComponent targetHex)
	{
		switch (currentState)
		{
			case (int)States.Moving:
				RegisterTokensToMove(targetHex);
				break;


			case (int)States.Attacking:
				RegisterTokensToAttack(targetHex);
				break;
            
			case (int)States.Defending:

                // ==========================
				// TODO: fill in this code
                // ==========================

				break;

			default:
				break;
		}

	}

    void SetTokensToDefend()
	{
		foreach (var item in Tokens)
		{
			item.Fortify();
		}
	}
}
