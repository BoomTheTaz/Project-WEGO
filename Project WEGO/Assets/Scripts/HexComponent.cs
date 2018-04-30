using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexComponent : MonoBehaviour
{

    //=====================================

    // TODO ASAP: Store old tokens somewhere.
    //  Currently parenting tokens immediately causes them to be registered
    // As on a separate tile.  Clicking that new tile will move them again



    readonly bool[] SpotAvailable = new bool[6]{true,true,true,true,true,true};

    Hex hex;

    public Transform TokenPlacement;
    public Transform[] TokenLocations;
    public Transform LeaderLocation;
    public GameObject Outliner;

    public Material RedOutline;
    public Material BlueOutline;
    public Material YellowOutline;

    int MaxTokens = 6;

    public bool IsSelected;

    GameObject[] Tokens = new GameObject[7];


	public void SetHex(Hex h)
    {
        hex = h;
            
    }

    public Hex GetHex()
    {
        return hex;
    }


    // Set Height of Token placement
    public void SetTokenHeight(float f)
    {
        TokenPlacement.position = transform.localPosition + Vector3.up * f;
    }


    // Function to take Token and arrange it on tile
    public bool AddToken(GameObject token)
    {
        if (token != null)
        {

            // TODO Make this better
            if(token.GetComponent<LeaderToken>() != null)
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


    // Function called when clicked on during gameplay, prepares tokens, etc.
    public GameObject[] Selected()
    {
        Outliner.SetActive(true);
        Outliner.GetComponent<MeshRenderer>().material = YellowOutline;

        for (int c = 0; c < TokenLocations.Length; c++)
        {
            if (TokenLocations[c].childCount != 0)
                Tokens[c] = TokenLocations[c].GetChild(0).gameObject;
            else
                Tokens[c] = null;

        }

        if (LeaderLocation.childCount != 0)
            Tokens[6] = LeaderLocation.GetChild(0).gameObject;
        else
            Tokens[6] = null;


        SelectAllTokens();
        IsSelected = true;
        return Tokens;
    }

    public void Deselected()
    {
        // TODO Figure this ish out

        //for (int i = 0; i < Tokens.Length; i++)
        //{
        //    Tokens[i] = null;
        //}

        Outliner.SetActive(false);
        IsSelected = false;

        foreach (var t in Tokens)
        {
            if (t != null)
                t.GetComponent<Token>().Deselect();
        }
    }

    public void SelectAllTokens()
    {
        foreach (var t in Tokens)
        {
            if (t != null)
                t.GetComponent<Token>().Select();
        }
    }

    public void RegisterTokensToMove(HexComponent hexGO)
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

                t.GetComponent<Token>().RegisterToMove(hexGO);

                t.GetComponent<Token>().Deselect();
            }
        }
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

    int NumTokensCurrently()
    {
        int count = 0;
        foreach (var l in TokenLocations)
        {
            if (l.childCount != 0)
                count++;
        }

        return count;
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

}
