using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HexView : MonoBehaviour {

	public Text header;
    
	public TokenButton[] tokenButtons;

	public Button MoveButton;
	public Button AttackButton;
	public Button DefendButton;

	HexComponent currentHex;

    // Delegate to slide out hex view
	delegate void OnSlide();
	OnSlide onSlide;


    
	float panelWidth;
	float panelHeight;

	Vector3 slideInTarget;
	Vector3 slideOutTarget;

	bool IsSlidingIn;
	bool IsSlidingOut;

	// Use this for initialization
	void Start () {

		MoveButton.onClick.AddListener(OnMoveButtonClick);
		AttackButton.onClick.AddListener(OnAttackButtonClick);
		DefendButton.onClick.AddListener(OnDefendButtonClick);

		panelWidth = GetComponent<RectTransform>().rect.width;

		slideInTarget = transform.localPosition + Vector3.right * panelWidth;
		slideOutTarget = transform.localPosition;
	}


	private void Update()
	{
        
		if (onSlide != null)
		{
			onSlide();

            // Conditions for stopping slide in
			if (Vector3.Distance(transform.localPosition, slideInTarget) < .01 && IsSlidingIn == true)
			{
				onSlide -= SlideIn;
                
				IsSlidingIn = false;
			}

            // Conditions for stopping slide out
			if (Vector3.Distance(transform.localPosition, slideOutTarget) < .01 && IsSlidingOut == true)
            {
                onSlide -= SlideOut;
				IsSlidingOut = false;

				gameObject.SetActive(false);
            }

		}

	}
    
	public void Setup(HexComponent hex, int player)
	{
		// Clicked on a hex that has no player tokens on it
		if (hex.IsPlayerOn(player) == false)
		{
			// If it is active, start sliding out the panel
			if (gameObject.activeInHierarchy == true)
			{
				refVel = Vector3.zero;

                // If was sliding in, set flag to false instead
				if (IsSlidingIn == true)
				{
					IsSlidingIn = false;
				}

                onSlide = SlideOut;
				IsSlidingOut = true;
			}
			return;
		}

		header.text = string.Format("Hex: ({0},{1})", hex.GetHex().Q,hex.GetHex().R);

		Token[] tokens = hex.GetTokens(player);

        // Setup TokenButtons
		for (int i = 0; i < tokens.Length; i++)
		{
			if (tokens[i] != null)
			{
				tokenButtons[i].Setup(tokens[i]);
				tokenButtons[i].gameObject.SetActive(true);
			}
			else
			{
				tokenButtons[i].gameObject.SetActive(false);
			}
		}

        // Assign current hex
		currentHex = hex;
        
        // If the panel is inactive, start sliding in the panel
		if (gameObject.activeInHierarchy == false || IsSlidingOut)
		{
			if (IsSlidingOut == true)
				IsSlidingOut = false;

			gameObject.SetActive(true);

            refVel = Vector3.zero;
			onSlide = SlideIn;
			IsSlidingIn = true;
		}

	}

	Vector3 refVel;
    void SlideOut()
	{
		transform.localPosition = Vector3.SmoothDamp(transform.localPosition, slideOutTarget, ref refVel, Constants.SidePanelSpeed);

	}

	void SlideIn()
    {
		transform.localPosition = Vector3.SmoothDamp(transform.localPosition, slideInTarget, ref refVel, Constants.SidePanelSpeed);

    }

    void OnMoveButtonClick()
	{
		currentHex.UpdateCurrentState((int)States.Moving);
	}

    void OnAttackButtonClick()
    {
		currentHex.UpdateCurrentState((int)States.Attacking);
	}

    void OnDefendButtonClick()
    {
		currentHex.UpdateCurrentState((int)States.Defending);

    }
}
