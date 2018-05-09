using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexComponent : MonoBehaviour
{

    //=====================================

    // TODO ASAP: Store old tokens somewhere.
    //  Currently parenting tokens immediately causes them to be registered
    // As on a separate tile.  Clicking that new tile will move them again



    readonly bool[] SpotAvailable = { true, true, true, true, true, true };

    Hex hex;
    string type;

    public Transform TokenPlacement;
    public Transform[] TokenLocations;
    public Transform LeaderLocation;
    public GameObject Outliner;

    public Material RedOutline;
    public Material BlueOutline;
    public Material YellowOutline;
    public Material WhiteOutline;

    int MaxTokens = 6;
    int minMovement;
    int minAttack;
    int prevMinMovement;
    int prevMinAttack;

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


    enum States { Moving, Attacking, Defending };

    int currentState;


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
    public bool AddToken(GameObject token)
    {
        if (token != null)
        {

            // TODO Make this better
            if (token.GetComponent<LeaderToken>() != null)
            {
                token.transform.parent = LeaderLocation;
                token.GetComponent<Token>().SetCurrentHex(this);

                // Move to parent location
                token.transform.localPosition = Vector3.zero;
                return true;
            }

            if (AnyVacancies() == true)
            {
                token.GetComponent<Token>().SetCurrentHex(this);

                Transform t = GetNextAvailableTokenLocation();

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
                Debug.Log(string.Format("Maximum number of tokens reached on hex at ({0},{1})", hex.Q, hex.R));
                return false;
            }

        }
        return true;
    }


    int MaxMovement;
    int MaxAttack;

    // Function called when clicked on during gameplay, prepares tokens, etc.
    public Token[] Selected()
    {
        // Outline this in white
        OutlineWhite();
        hexMap.SetCurrentHexGO(this);
        currentState = hexMap.CurrentState;

        MaxAttack = 0;
        MaxMovement = 0;

        if (SelectedThisTurn == false)
        {

            // Get Tokens on Hex
            for (int c = 0; c < TokenLocations.Length; c++)
            {
                if (TokenLocations[c].childCount != 0)
                {
                    Tokens[c] = TokenLocations[c].GetChild(0).gameObject.GetComponent<Token>();

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
            if (LeaderLocation.childCount != 0)
            {
                Tokens[6] = LeaderLocation.GetChild(0).gameObject.GetComponent<Token>();
                if (Tokens[6].GetMovementSpeed() > MaxMovement)
                    MaxMovement = Tokens[6].GetMovementSpeed();
                if (Tokens[6].GetAttackRange() > MaxAttack)
                    MaxAttack = Tokens[6].GetAttackRange();

            }
            else
                Tokens[6] = null;


            // Get Valid Hexes for movement and Attacking
            List<HexComponent>[] temp = hexMap.GetValidMoveHexes(this);
            for (int l = 0; l < maxRange; l++)
            {
                foreach (var i in temp[l])
                {
                    ValidMoveHexes[l].Add(i);
                }
            }


            temp = hexMap.GetValidAttackHexes(this);

            for(int l = 0; l < maxRange; l++)
            {
                foreach (var i in temp[l])
                {
                    ValidAttackHexes[l].Add(i);
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

        return Tokens;
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
                t.DeactivateCollider();
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

    //int VacanciesFilled = 0;
    public Transform GetNextAvailableTokenLocation()
    {
        for (int i = 0; i < SpotAvailable.Length; i++)
        {
            if (SpotAvailable[i] == true)
            {
                SpotAvailable[i] = false;
                return TokenLocations[i];
            }
        }

        Debug.LogError("No Vacancies!");
        return null;

    }

    public void DecrementReservation(int spot)
    {
        SpotAvailable[spot] = true;
    }

    public bool AnyVacancies()
    {
        for (int i = 0; i < SpotAvailable.Length; i++)
        {
            if (SpotAvailable[i] == true)
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
    public void UpdateMinValues()
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

        // Draw relevant hexes for current state
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
		return hex.Q / 2f + hex.R;
	}

}
