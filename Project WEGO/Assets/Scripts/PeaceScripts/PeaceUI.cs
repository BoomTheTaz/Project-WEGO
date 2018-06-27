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

	public CityPanel[] CityPanels;



	int currentPanel;




	delegate void OnPanelSwitch();
	OnPanelSwitch onPanelSwitch;

	private void Awake()
	{
		if (instance == null)
            instance = this;
        else
            Destroy(this);
	}

    
	// Use this for initialization
	void Start () {
		      
		currentPanel = 0;

		CityPanels[currentPanel].gameObject.SetActive(true);

		foreach (var item in CityPanels)
		{
			item.onUnregister = UnregisterPanelMovement;
		}
	}


	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.RightArrow))
			NextPanel();
		if (Input.GetKeyDown(KeyCode.LeftArrow))
			PreviousPanel();
        

		if (onPanelSwitch != null)
		{
			onPanelSwitch();
		}
	}

	public void UpdateProduction(float[] p)
	{
		foodText.text = (Mathf.Round(p[(int)Productions.Food]*10)/10).ToString();
		scienceText.text = (Mathf.Round(p[(int)Productions.Science] * 10) / 10).ToString();
		oreText.text = (Mathf.Round(p[(int)Productions.Ores] * 10) / 10).ToString();
		prodText.text = (Mathf.Round(p[(int)Productions.Production] * 10) / 10).ToString();
		moneyText.text = (Mathf.Round(p[(int)Productions.Money] * 10) / 10).ToString();
	}

    // Go to next panel, i.e. right panel
	public void NextPanel()
	{
		CityPanel old = CityPanels[currentPanel];

        // Set target to left position
		old.SetNewTarget(-1);
        
        // Initiate Movement as needed
		if(old.IsMoving == false)
		{
			old.IsMoving = true;
			onPanelSwitch += old.Move;
		}

		// Update currentPanel
		currentPanel = (currentPanel + 7) % 6;
		CityPanel n = CityPanels[currentPanel];

		// Set new panel to right position and set target to middle

		if (n.IsMoving == false)
		{
			n.gameObject.SetActive(true);

			n.SetLocation(old.transform.position + CityPanel.width*Vector3.right);

			n.IsMoving = true;
			onPanelSwitch += CityPanels[currentPanel].Move;
		}

		n.SetNewTarget(0);

	}

    // Got to previous panel, i.e. left panel
    public void PreviousPanel()
	{
		CityPanel old = CityPanels[currentPanel];

        // Set target to right position
        old.SetNewTarget(1);

        // Initiate Movement as needed
        if (old.IsMoving == false)
        {
            old.IsMoving = true;
            onPanelSwitch += old.Move;
        }

        // Update currentPanel
        currentPanel = (currentPanel + 5) % 6;
        CityPanel n = CityPanels[currentPanel];

        // Set new panel to left position and set target to middle

        if (n.IsMoving == false)
        {
            n.gameObject.SetActive(true);

			n.SetLocation(old.transform.position + CityPanel.width * Vector3.left);

            n.IsMoving = true;
            onPanelSwitch += CityPanels[currentPanel].Move;
        }

        n.SetNewTarget(0);
	}

    // Unregister panel movement, called from panels themselves
    public void UnregisterPanelMovement(int i)
	{
		onPanelSwitch -= CityPanels[i].Move;
	}

}
