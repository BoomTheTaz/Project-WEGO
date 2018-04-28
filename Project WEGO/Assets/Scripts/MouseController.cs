using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseController : MonoBehaviour {

    ArmyManager armyManager;
    HexComponent currentHexGO;
    WarManager warManager;


    delegate void OnUpdate();
    OnUpdate onUpdate;

	private void Start()
	{
        armyManager = FindObjectOfType<ArmyManager>();
        warManager = FindObjectOfType<WarManager>();
        onUpdate = SettingUp;
	}

	// Update is called once per frame
	void Update () {
        if (onUpdate != null)
            onUpdate();

		
	}


    void SettingUp()
    {
        // On left click, check if hit hex, then do something
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);


            if (Physics.Raycast(ray, out hit, 100))
            {
                if (hit.transform.gameObject.layer == LayerMask.NameToLayer("HexTile"))
                {
                    currentHexGO = hit.transform.GetComponent<HexComponent>();

                    //Hex h = currentHexGO.GetHex();

                    bool usedToken = currentHexGO.AddToken(armyManager.GetToken());


                    armyManager.UsedToken(usedToken);
                }


            }

        }
    }


    public void ToGameplay()
    {
        currentHexGO = null;
        onUpdate = InGameplay;
    }

    GameObject[] currentTokens;
    Vector3 velocity = Vector3.zero;
    void InGameplay()
    {

        // Left Mouse Click in gameplay should select tile/Units on tile
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            // If we hit a hex, tell the HexComponent
            if (Physics.Raycast(ray, out hit, 100))
            {
                if (hit.transform.gameObject.layer == LayerMask.NameToLayer("HexTile"))
                {

                    if (hit.transform.GetComponent<HexComponent>() != currentHexGO)
                    {
                        // Deselect current HexComponent
                        if (currentHexGO != null)
                        {
                            currentHexGO.Deselected();
                            currentTokens = null;
                        }

                        // Get HexComponent and associated Hex
                        currentHexGO = hit.transform.GetComponent<HexComponent>();

                        //Hex h = currentHexGO.GetHex();

                        currentTokens = currentHexGO.Selected();

                    }
                }


            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            if (currentTokens != null)
            {
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                // If we hit a hex, tell the HexComponent
                if (Physics.Raycast(ray, out hit, 100))
                {
                    if (hit.transform.gameObject.layer == LayerMask.NameToLayer("HexTile"))
                    {

                        if (hit.transform.GetComponent<HexComponent>() != currentHexGO)
                        {
                            foreach (var t in currentTokens)
                            {
                                if (t != null)
                                    t.transform.parent = hit.transform.GetComponent<HexComponent>().TokenPlacement;
                            }

                            warManager.MoveTokens(currentTokens,hit.transform.GetComponent<HexComponent>().TokenPlacement);
                        }
                    }


                }
            }


        }
    }

}
