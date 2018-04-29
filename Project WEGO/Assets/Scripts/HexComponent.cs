using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexComponent : MonoBehaviour {

    //=====================================

    // TODO ASAP: Store old tokens somewhere.
    //  Currently parenting tokens immediately causes them to be registered
    // As on a separate tile.  Clicking that new tile will move them again





    Hex hex;

    public Transform TokenPlacement;
    public Transform[] TokenLocations;
    public Transform LeaderLocation;

    int NumTokens;
    int MaxTokens = 6;

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

                // Move to parent location
                token.transform.localPosition = Vector3.zero;
                return true;
            }

            if (NumTokens < MaxTokens)
            {
                // Set Parent
                token.transform.parent = TokenLocations[NumTokens];

                // Move to parent location
                token.transform.localPosition = Vector3.zero;

                // Rotate to parent rotation
                token.transform.rotation = TokenLocations[NumTokens].rotation;

                NumTokens++;
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
        for (int c = 0; c < TokenLocations.Length; c++)
        {
            if (TokenLocations[c].childCount != 0)
                Tokens[c] = TokenLocations[c].GetChild(0).gameObject;
            else
                Tokens[c] = null;

        }

        if (LeaderLocation.childCount != 0)
            Tokens[6] = LeaderLocation.GetChild(0).gameObject;

        SelectAllTokens();

        return Tokens;
    }

    public void Deselected()
    {
        // TODO Figure this ish out

        //for (int i = 0; i < Tokens.Length; i++)
        //{
        //    Tokens[i] = null;
        //}

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
                if (t.GetComponent<LeaderToken>() != null)
                {
                    Debug.Log("Trying to set leader target to " + LeaderLocation.position);
                    t.GetComponent<Token>().RegisterToMove(hexGO.LeaderLocation);
                }
                else
                    t.GetComponent<Token>().RegisterToMove(hexGO.GetNextAvailableTokenLocation());
            }
        }
    }


    public Transform GetNextAvailableTokenLocation()
    {
        Transform result = null;

        foreach (Transform l in TokenLocations)
        {
            if(l.childCount == 0)
            {
                result =  l;
                break;
            }
        }

        return result;
    }

}
