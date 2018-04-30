using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseController : MonoBehaviour {

    ArmyManager armyManager;
    HexComponent currentHexGO;


    delegate void OnUpdate();
    OnUpdate onUpdate;

	private void Start()
	{
        armyManager = FindObjectOfType<ArmyManager>();
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
                    currentHexGO = hit.transform.GetComponentInParent<HexComponent>();

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
                // Only want to know if we hit a hex
                if (hit.transform.gameObject.layer == LayerMask.NameToLayer("HexTile"))
                {
                    // Ignore if clicking on same hex
                    if (hit.transform.GetComponentInParent<HexComponent>() != currentHexGO)
                    {
                        // Deselect current HexComponent
                        if (currentHexGO != null)
                        {
                            currentHexGO.Deselected();

                            // Deactivate token colliders
                            for (int i = 0; i < currentTokens.Length; i++)
                            {

                                if (currentTokens[i] != null)
                                {
                                    currentTokens[i].GetComponent<Token>().DeactivateCollider();

                                    currentTokens[i] = null;
                                }
                            }
                        }


                        // Get HexComponent and associated Hex
                        currentHexGO = hit.transform.GetComponentInParent<HexComponent>();

                        //Hex h = currentHexGO.GetHex();

                        currentTokens = currentHexGO.Selected();
                        // Activate Token Colliders
                        foreach (var t in currentTokens)
                        {
                            if(t!=null)
                                t.GetComponent<Token>().ActivateCollider();
                        }

                    }
                }

                // Check if we hit an active token
                if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Token"))
                {

                    hit.transform.GetComponent<Token>().ToggleSelect();


                }


            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            // Only care if there are tokens to move
            // TODO Fix this too care about something else
            if (currentTokens != null)
            {
                // Raycast
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                // If we hit a hex, tell the HexComponent
                if (Physics.Raycast(ray, out hit, 100))
                {
                    if (hit.transform.gameObject.layer == LayerMask.NameToLayer("HexTile"))
                    {
                        // If it is a new hex, Register Selected tokens to move there
                        if (hit.transform.GetComponentInParent<HexComponent>() != currentHexGO)
                        {
                            currentHexGO.RegisterTokensToMove(hit.transform.GetComponentInParent<HexComponent>());
                            //warManager.MoveTokens(currentTokens,hit.transform.GetComponent<HexComponent>().TokenPlacement);
                        }
                    }


                }
            }


        }
    }

    void ClearCurrentTokens()
    {
        for (int i = 0; i < currentTokens.Length; i++)
        {
            if (currentTokens[i] != null)
            {
                currentTokens[i].GetComponent<Token>().Deselect();
                currentTokens[i] = null;
            }
        }
    }

    public void StartingMove()
    {
        ClearCurrentTokens();
        currentHexGO.Deselected();

    }

}
