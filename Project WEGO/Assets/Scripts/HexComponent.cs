using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexComponent : MonoBehaviour {

    Hex hex;

    public void SetHex(Hex h)
    {
        hex = h;

        TextMesh tm = GetComponentInChildren<TextMesh>();
        tm.text = string.Format("{0},{1}", hex.Q, hex.R);
    }
}
