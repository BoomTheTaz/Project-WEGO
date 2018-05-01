using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexComponent : MonoBehaviour
{

    //=====================================

    // TODO ASAP: Store old tokens somewhere.
    //  Currently parenting tokens immediately causes them to be registered
    // As on a separate tile.  Clicking that new tile will move them again



    readonly bool[] SpotAvailable = {true,true,true,true,true,true};

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

    bool allowTokensToUpdateValidHexes = false;

    public bool IsSelected;

    Token[] Tokens = new Token[7];

    HexStats hexStats;
    HexMap hexMap;


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
    public Token[] Selected()
    {
        OutlineWhite();

        for (int c = 0; c < TokenLocations.Length; c++)
        {
            if (TokenLocations[c].childCount != 0)
                Tokens[c] = TokenLocations[c].GetChild(0).gameObject.GetComponent<Token>();
            else
                Tokens[c] = null;

        }

        if (LeaderLocation.childCount != 0)
            Tokens[6] = LeaderLocation.GetChild(0).gameObject.GetComponent<Token>();
        else
            Tokens[6] = null;


        SelectAllTokens();
        IsSelected = true;
        allowTokensToUpdateValidHexes = true;

        GetHexesInMovementRange();

        if (ValidHexes == null)
            Debug.Log("Valid Hexes still null.");

        OutlineValidHexes(null);
        NoneSelected = false;
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
        allowTokensToUpdateValidHexes = false;

        if (minMovement != -1)
        {
            UnoutlineValidHexes();
            minMovement = -1;
        }

        ValidHexes = null;
        NoneSelected = false;
        foreach (var t in Tokens)
        {
            if (t != null)
                t.Deselect();
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

                t.Deselect();
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

    // TODO better this once finalize hex designs
    public void SetHexAsType(string s)
    {
        type = s;
        // Set up hex stats
        hexStats = HexTemplate.GetStatsForType(s);

        SetTokenHeight(hexStats.TokenHeight);

    }

    Dictionary<Hex,HexComponent> ValidHexes;
    int prevMinMovement;
    bool NoneSelected = false;

    public void GetHexesInMovementRange()
    {


        if (allowTokensToUpdateValidHexes == false)
        {
            Debug.Log("WHY HEERE");

            return;
        }

        prevMinMovement = minMovement;



        // Case for when no Tokens are selected
        if (GetMinMovement() == false)
        {
            if (ValidHexes != null)
                UnoutlineValidHexes();

            NoneSelected = true;
            return;
        }

        Dictionary<Hex, HexComponent> tempValidHexes;
        List<HexComponent> HexesToOutline = new List<HexComponent>();


        // If minMovement has not changed, no need to find valid hexes again
        if (prevMinMovement == minMovement && minMovement != -1)
        {
            Debug.Log("No change in min movement.");
            return;
        }

        if (minMovement < 0)
        {
            return;
        }
        tempValidHexes = hexMap.GetHexGOWithinRange(hex, minMovement);
        tempValidHexes = RemoveUntraversable(tempValidHexes);

        tempValidHexes.Remove(hex);

        // We need to compare previous and current valid hexes
        // to outline necessary hexes and prevent over calling of outlining
        if (ValidHexes != null)
        {
            // Need To outline more
            if (tempValidHexes.Count > ValidHexes.Count)
            {
                foreach (var h in tempValidHexes.Keys)
                {
                    // If previous valid hexes does not contain key, outline that Hex
                    if (ValidHexes.ContainsKey(h) == false)
                        HexesToOutline.Add(tempValidHexes[h]);
                    
                }
            }

            // Need To outline some
            else if (tempValidHexes.Count < ValidHexes.Count)
            {

                foreach (var h in ValidHexes.Keys)
                {

                    // If previous valid hex has one that current doesn't, outline it
                    if (tempValidHexes.ContainsKey(h) == false)
                        ValidHexes[h].NoOutline();

                }

            }
        }

        ValidHexes = tempValidHexes;

        // Case when previous Deselection led to none being drawn,
        // It is then necessary to redraw before proceeding

        if (NoneSelected == true)
        {
            OutlineValidHexes(null);
            NoneSelected = false;
        }
        else
        // This should only evaluate outline on new hexes
            OutlineValidHexes(HexesToOutline);
    }

    // returns true if there is at least one token selected, false if none selected
    bool GetMinMovement()
    {
        minMovement = -1;
        bool AnySelected = false;
        foreach (Token t in Tokens)
        {
            // Count movement only if token is currently selected
            if (t != null && t.IsCurrentlySelected() && (t.GetMovementSpeed() < minMovement || minMovement < 0))
            {
                minMovement = t.GetMovementSpeed();
                AnySelected = true;
            }
        }

        return AnySelected;
    }

    Dictionary<Hex,HexComponent> RemoveUntraversable(Dictionary<Hex,HexComponent> d)
    {
        List<Hex> toRemove = new List<Hex>();
        foreach (var h in d.Keys)
        {
            if (d[h].IsTraversable() == false)
                toRemove.Add(h);
        }

        foreach (var h in toRemove)
        {
            d.Remove(h);
        }

        return d;
    }


    void OutlineValidHexes(List<HexComponent> l)
    {
        if (minMovement < 0)
            return;


        if (l == null)
        {

            foreach (var h in ValidHexes.Keys)
            {
                OutlineAppropriately(ValidHexes[h]);
            }
        }

        else
        {
            foreach (var i in l)
            {
                OutlineAppropriately(i);
            }
        }
    }


    void OutlineAppropriately(HexComponent h)
    {
        // TODO Change this to vary based on level
        // ======== OR ========
        // remove fatigue level altogether going back to bool
        if (h.GetFatigueLevel() > 0)
            h.OutlineYellow();
        else
            h.OutlineBlue();
    }


    // TODO: Maybe compare with new outline to prevent over calling outliner.SetActive
    void UnoutlineValidHexes()
    {
        foreach (var h in ValidHexes.Keys)
        {
            ValidHexes[h].NoOutline();
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
}
