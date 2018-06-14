using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NextTurnButton : MonoBehaviour {

	WarManager warManager;
	Button button;

	// Use this for initialization
	void Start () {
		warManager = FindObjectOfType<WarManager>();
		button = GetComponent<Button>();

		button.onClick.AddListener(NextTurn);
	}

    void NextTurn()
	{
		warManager.EvaluateTurn();
	}

}
