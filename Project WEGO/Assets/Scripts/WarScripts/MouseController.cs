﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public enum States { Moving, Attacking, Defending };

public class MouseController : MonoBehaviour {

    WarManager warManager;
	UIManager UI;
    HexComponent currentHexGO;
    Token[] currentTokens;

    delegate void OnUpdate();
    OnUpdate onUpdate;
    HexMap hexMap;
    
	private void Start()
	{
        warManager = FindObjectOfType<WarManager>();
		UI = FindObjectOfType<UIManager>();
        onUpdate = SettingUp;
        //CurrentState = (int)States.Moving;
        hexMap = FindObjectOfType<HexMap>();

		currentTokens = new Token[7];

        //hexMap.ChangeStates(CurrentState);
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

					if (currentHexGO.ValidStarting0 == true)
					{
						bool usedToken = currentHexGO.AddToken(warManager.GetToken(0),0);


						warManager.UsedToken(0, usedToken);
					}

                    else
						Debug.Log("Invalid starting spot for first player.");
                }


            }

        }

        //======  MANUAL CONTROL OF SECOND ARMY =======

		//if (Input.GetMouseButtonDown(1))
     //   {
     //       RaycastHit hit;
     //       Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);


     //       if (Physics.Raycast(ray, out hit, 100))
     //       {
     //           if (hit.transform.gameObject.layer == LayerMask.NameToLayer("HexTile"))
     //           {
     //               currentHexGO = hit.transform.GetComponentInParent<HexComponent>();

					//if (currentHexGO.ValidStarting1 == true)
					//{
					//	bool usedToken = currentHexGO.AddToken(warManager.GetToken(1));


					//	warManager.UsedToken(1, usedToken);
					//}
					//else
						//Debug.Log("Invalid starting spot for second player.");
        //        }


        //    }

        //}
    }


    public void ToGameplay()
    {
        currentHexGO = null;
        onUpdate = InGameplay;
    }

    void InGameplay()
    {

        // Left Mouse Click in gameplay should select tile/Units on tile
		if (Input.GetMouseButtonDown(0) && EventSystem.current.IsPointerOverGameObject() == false)
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

                        }

                        // Get HexComponent and associated Hex
                        currentHexGO = hit.transform.GetComponentInParent<HexComponent>();

						//Hex h = currentHexGO.GetHex();

						UI.SetupHexView(currentHexGO);

						currentHexGO.Selected();

						// TODO: vary player if needed
						currentTokens = currentHexGO.GetTokens(0);

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
            if (currentTokens[0] != null || currentTokens[6] != null)
            {
                // Raycast
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                // If we hit a hex, tell the HexComponent
                if (Physics.Raycast(ray, out hit, 100))
                {
                    if (hit.transform.gameObject.layer == LayerMask.NameToLayer("HexTile"))
                    {

						//if (CurrentState == (int)States.Moving)
						//{
						//	// If it is a new hex, Register Selected tokens to move there
						//	if (hit.transform.GetComponentInParent<HexComponent>() != currentHexGO)
						//	{
						//		currentHexGO.RegisterTokensToMove(hit.transform.GetComponentInParent<HexComponent>());
						//	}
						//}

						//if ( CurrentState == (int)States.Attacking)
						//{
						//	currentHexGO.RegisterTokensToAttack(hit.transform.GetComponentInParent<HexComponent>());
						//}

						currentHexGO.RegisterTokens(hit.transform.GetComponentInParent<HexComponent>());

                    }


                }
            }


        }

        //// Temporary Attacking mode button
        //if (Input.GetKeyDown(KeyCode.X))
        //{
        //    if(CurrentState != (int)States.Attacking)
        //    {
        //        CurrentState = (int)States.Attacking;
        //        hexMap.ChangeStates(CurrentState);
        //    }
        //}

        //// Temporary Moving mode button
        //if (Input.GetKeyDown(KeyCode.Z))
        //{
        //    if (CurrentState != (int)States.Moving)
        //    {
        //        CurrentState = (int)States.Moving;
        //        hexMap.ChangeStates(CurrentState);
        //    }
        //}
    }

    void ClearCurrentTokens()
    {
        for (int i = 0; i < currentTokens.Length; i++)
        {
            if (currentTokens[i] != null)
            {
                currentTokens[i].Deselect();
                //currentTokens[i] = null;
            }
        }
    }

    public void StartingMove()
    {
        if (currentTokens != null)
            ClearCurrentTokens();
        if (currentHexGO != null)
            currentHexGO.Deselected();
        currentHexGO = null;

    }

}
