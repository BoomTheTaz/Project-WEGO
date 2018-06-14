using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PeaceUI : MonoBehaviour {


	public static PeaceUI instance;
    
	public Text foodText;
	public Text scienceText;
	public Text oreText;
	public Text prodText;
	public Text moneyText;



	// Use this for initialization
	void Start () {
		if (instance == null)
			instance = this;
		else
			Destroy(this);
	}
	
	// Update is called once per frame
	void Update () {
		
	}


	public void UpdateProduction(float[] p)
	{
		foodText.text = (Mathf.Round(p[(int)Productions.Food]*10)/10).ToString();
		scienceText.text = (Mathf.Round(p[(int)Productions.Science] * 10) / 10).ToString();
		oreText.text = (Mathf.Round(p[(int)Productions.Ores] * 10) / 10).ToString();
		prodText.text = (Mathf.Round(p[(int)Productions.Production] * 10) / 10).ToString();
		moneyText.text = (Mathf.Round(p[(int)Productions.Money] * 10) / 10).ToString();
	}
}
