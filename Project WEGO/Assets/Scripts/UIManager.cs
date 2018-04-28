using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour {

    public GameObject SidePanel;

	public void InSetup()
    {
        SidePanel.SetActive(true); 
    }

    public void LeavingSetup()
    {
        SidePanel.SetActive(false);
    }


}
