using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexComponent : MonoBehaviour {

    Hex hex;

    public Transform TokenPlacement;


    int NumTokens;
    int MaxTokens = 6;
    int[] positionAngles = new int[6] {120,180,60,240,0,300 };


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

            if (NumTokens < MaxTokens)
            {

                token.transform.parent = TokenPlacement;
                token.transform.localPosition = Vector3.zero;
                //token.transform.position = TokenPlacement.transform.position;
                token.transform.rotation *= Quaternion.AngleAxis(positionAngles[NumTokens], Vector3.up);

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

}
