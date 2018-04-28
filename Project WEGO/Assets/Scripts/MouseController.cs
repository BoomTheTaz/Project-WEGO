using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseController : MonoBehaviour {

    ArmyManager armyManager;
    HexMap hexMap;


	private void Start()
	{
        armyManager = FindObjectOfType<ArmyManager>();
        hexMap = FindObjectOfType<HexMap>();
	}

	// Update is called once per frame
	void Update () {

        // On left click, check if hit hex, then do something
        if(Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);


            if (Physics.Raycast(ray, out hit, 100))
            {
                if (hit.transform.gameObject.layer == LayerMask.NameToLayer("HexTile"))
                {
                    Hex h = hit.transform.GetComponent<HexComponent>().GetHex();
                    Debug.Log(string.Format("Attempting to place a token on hex at ({0},{1})", h.Q, h.R));
                    bool usedToken = hit.transform.GetComponent<HexComponent>().AddToken(armyManager.GetToken());

                    if (usedToken == false)
                        armyManager.DidNotUseToken();
                }


            }

        }

		
	}

}
