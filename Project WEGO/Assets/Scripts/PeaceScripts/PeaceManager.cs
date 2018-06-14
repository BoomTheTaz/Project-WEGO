using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PeaceManager : MonoBehaviour {

	PlayerCity[] Players;

	// Use this for initialization
	void Start () {

		Players = new PlayerCity[2];
		Players[0] = new PlayerCity(0);
		Players[1] = new PlayerCity(1);


	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown(KeyCode.Space))
		{
			NextTurn();
		}
	}

    void NextTurn()
	{
		for (int i = 0; i < Players.Length; i++)
		{
			Players[i].AdvanceToNextTurn();
		}
	}
}
