using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Use this class to manage the various war states
public class WarManager : MonoBehaviour {

    public bool IsSettingUp = true;
    MouseController mouse;
    UIManager UI;

    List<Token> TokensToMove = new List<Token>();
    List<Token> TokensToRemove = new List<Token>();

    // Delegate to trigger movement of tokens
    delegate void MovementUpdate();
    MovementUpdate UpdateTokenMovement;


    Vector3 velocity;
    Transform Target;
    GameObject[] ToMove;


	// Use this for initialization
	void Start () {

        mouse = FindObjectOfType<MouseController>();
        UI = FindObjectOfType<UIManager>();

        UI.InSetup();

	}
	
    // Update is called once per frame
	void Update () {

        // Test key for activating token movement
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Instructing to move tokens");
            UpdateTokenMovement += MoveTokens;
            mouse.StartingMove();
        }

        // Call Token movement delegate if not null
        if (UpdateTokenMovement != null)
            UpdateTokenMovement();

	}

    // Call after setup complete, ensures that everything knows setup is done
    public void FinishedSettingUp()
    {
        IsSettingUp = false;
        mouse.ToGameplay();
        UI.LeavingSetup();
        Debug.Log("Setup is complete!");
    }

    // Add inputted token to List of TokensToMove
    public void RegisterTokenToMove(Token token)
    {
        TokensToMove.Add(token);
    }

    // Remove inputted token from List of TokensToMove
    public void UnregisterTokenToMove(Token token)
    {
        
        // FIXME Decide if this if check is necessary

        if (TokensToMove.Contains(token))
            TokensToMove.Remove(token);


    }



    // Move all registered Tokens
    void MoveTokens()
    {
        // Move every token registered
        foreach (var t in TokensToMove)
        {
            
            // If token.move() is true, that means close enough to target position,
            // so unregister that token
            if (t.Move() == true)
            {
                TokensToRemove.Add(t);

            }
        }

        if (TokensToRemove.Count > 0)
        {
            foreach (var r in TokensToRemove)
            {
                TokensToMove.Remove(r);
                r.Deselect();
            }

            TokensToRemove.Clear();
        }

        // If there are no tokens left to move, unregister delegate
        if (TokensToMove.Count == 0)
        {
            Debug.Log("All tokens done moving");
            UpdateTokenMovement = null;
        }
    }


}
