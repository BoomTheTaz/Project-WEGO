using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TokenButton : MonoBehaviour {
    
	public Text Name;
	public Text HealthText;
	public Text AttackText;
	public Text DefenseText;
	public Text MovementText;
	public Text RangeText;

	Token currentToken;
	Button button;

	private void Start()
	{
		button = GetComponent<Button>();

		button.onClick.AddListener(OnButtonClick);


	}

	public void Setup(Token token)
	{
		Name.text = token.GetUnitType();
		HealthText.text = token.GetHealth().ToString() + "/100";
		AttackText.text = token.GetBaseAttack().ToString();
		DefenseText.text = token.GetBaseDefense().ToString();
		MovementText.text = token.GetMovementSpeed().ToString();
		RangeText.text = token.GetAttackRange().ToString();

		currentToken = token;
	}

    void OnButtonClick()
	{
		currentToken.ToggleSelect();
	}

    

}
