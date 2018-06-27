using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CityPanel : MonoBehaviour {

	static Vector3 NextPanelPosition;
	static Vector3 PrevPanelPosition;
	static Vector3 MiddlePosition;

	Vector3 target;
    
	int index;
   
	public bool IsMoving;

	bool ShouldStayActive;
    
	const float maxSlideDistance = 25f;

	public delegate void OnUnregister(int i);
	public OnUnregister onUnregister;

	public static float width;

	private void Start()
	{

		if (MiddlePosition == Vector3.zero)
		{
			width = GetComponent<RectTransform>().rect.width * FindObjectOfType<Canvas>().scaleFactor;

			NextPanelPosition = transform.position + Vector3.right * width;
			PrevPanelPosition = transform.position + Vector3.left * width;
			MiddlePosition = transform.position;
		}

		index = transform.GetSiblingIndex();
	}
    
    public void Move()
	{
		// Move panel towards target
		transform.position = Vector3.MoveTowards(transform.position, target, maxSlideDistance);

        // If close enough to target, unregister
		if (Mathf.Abs(transform.position.x - target.x) < .001)
		{
			onUnregister(index);
			IsMoving = false;

			if (ShouldStayActive == false)
			    gameObject.SetActive(false);
		}
	}

    public void SetLocation(int i)
	{
		switch (i)
        {
            case -1:
				transform.position = PrevPanelPosition;
                break;
            case 0:
				transform.position = MiddlePosition;
                break;

            case 1:
				transform.position = NextPanelPosition;
                break;


            default:
                Debug.LogError("Invalid Location for City Panel.");
                break;
        }
	}

    public void SetNewTarget(int i)
	{
		switch (i)
		{
			case -1:
				target = PrevPanelPosition;
				ShouldStayActive = false;
				break;
			case 0:
				target = MiddlePosition;
				ShouldStayActive = true;
                break;

			case 1:
				target = NextPanelPosition;
				ShouldStayActive = false;
                break;


			default:
				Debug.LogError("Invalid Target for City Panel.");
				break;
		}

	}

	public void SetLocation(Vector3 v)
	{
		transform.position = v;
	}
}
