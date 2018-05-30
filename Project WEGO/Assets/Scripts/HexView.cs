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



	// Use this for initialization
	void Start () {

		MoveButton.onClick.AddListener(OnMoveButtonClick);
		AttackButton.onClick.AddListener(OnAttackButtonClick);
		DefendButton.onClick.AddListener(OnDefendButtonClick);


	}

    
	public void Setup(HexComponent hex, int player)
	{
		if (hex.IsPlayerOn(player) == false)
		{
			gameObject.SetActive(false);
			return;
		}

		header.text = string.Format("Hex: ({0},{1})", hex.GetHex().Q,hex.GetHex().R);

		Token[] tokens = hex.GetTokens(player);


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

		currentHex = hex;

		gameObject.SetActive(true);
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
