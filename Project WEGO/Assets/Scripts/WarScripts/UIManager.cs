using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour {
    
    public GameObject SidePanel;
	public GameObject HexViewPanel;


    

	int playerID = 0;

	public void InSetup()
    {
        SidePanel.SetActive(true); 
    }

    public void LeavingSetup()
    {
        SidePanel.SetActive(false);
    }

	public void SetupHexView(HexComponent hex)
	{

		HexViewPanel.GetComponent<HexView>().Setup(hex,playerID);

	}
}
