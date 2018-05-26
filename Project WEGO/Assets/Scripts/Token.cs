
using UnityEngine;

public class Token : MonoBehaviour
{
    // Customizing Options
    public SpriteRenderer LeftSprite;
    public SpriteRenderer RightSprite;
    public Material MainMaterial;
    public Material AccentMaterial;
    public GameObject Outliner;
    public Color OutlineColor;

    // External Components
    BoxCollider coll;
    WarManager warManager;

    // Current Locations
    HexComponent CurrentHexGO;
    int currentLocationOnHex;

    // Hex Moving to
    HexComponent NextHexGO;
    Transform targetTransform;
    int nextLocationOnHex;

	// Hex Attacking
	HexComponent HexToAttack;

    // Unit Data
    UnitStats unitStats;

    // Flags
    bool isCurrentlySelected = false;
    bool isRegisteredToMove = false;
	bool isRegisteredToAttack = false;
	bool isAIControlled = false;

	int playerID;

    public float TimeToMove = 0.3f;

    private void Start()
    {
        coll = GetComponent<BoxCollider>();


        // Start with the token colliders inactive
        coll.enabled = false;
    }


	public virtual void SetColors(Color main, Color accent)
    {
        MainMaterial.color = main;
        AccentMaterial.color = accent;

        LeftSprite.color = accent;
        RightSprite.color = accent;

    }

	// Only called first time, sets material colors and sprite
	public void SetUp(string type, WarManager w, int player, bool isAI)
    {
        LeftSprite.sprite = Resources.Load<Sprite>(type + "_L");
        RightSprite.sprite = Resources.Load<Sprite>(type + "_R");

        // Store WarManager
        warManager = w;

        unitStats = UnitStatsTemplate.GetStatsForUnit(type);

		playerID = player;

		isAIControlled = isAI;

    }

    // Activate the colliders when hex is selected
    public void ActivateCollider()
    {
        coll.enabled = true;
    }

    // Deactivate the colliders
    public void DeactivateCollider()
    {
        coll.enabled = false;
    }

    public void Select()
    {
        isCurrentlySelected = true;
        Outliner.gameObject.SetActive(true);

        CurrentHexGO.UpdateMinValues();

    }

    public void Deselect()
    {
        isCurrentlySelected = false;
        Outliner.gameObject.SetActive(false);

        CurrentHexGO.UpdateMinValues();

    }

    public void ToggleSelect()
    {
        if (isCurrentlySelected == true)
            Deselect();
        else
            Select();
    }


    public void RegisterToMove(HexComponent h)
	{
        
		// Break out immediately if not selected
		if (isCurrentlySelected == false && isAIControlled == false)
            return;


		if (isRegisteredToAttack == true)
		{
			warManager.UnregisterTokenToAttack(this);
			isRegisteredToAttack = false;
		}

        // Check if Leader Token
        if (GetComponent<LeaderToken>() != null)
        {
            if (isRegisteredToMove == true)
                warManager.UnregisterTokenToMove(this);

            NextHexGO = h;
            SetTarget(NextHexGO.LeaderLocations[playerID]);
            warManager.RegisterTokenToMove(this);
            isRegisteredToMove = true;
            return;
        }

        // If the token can reserve a spot on the new hex
		if (h.AnyVacancies(playerID) == true)
        {
            // If token already registered elsewhere, be sure to decrement 
            // reservations on previously assigned NextHexGO
            if (isRegisteredToMove == true)
            {
                warManager.UnregisterTokenToMove(this);

				NextHexGO.DecrementReservation(nextLocationOnHex, playerID);
            }

            // Set next hexGO
            NextHexGO = h;

            // Unreserve current spot
			CurrentHexGO.DecrementReservation(currentLocationOnHex,playerID);

			SetTarget(NextHexGO.GetNextAvailableTokenLocation(playerID));
            SetNextLocationOnHex(targetTransform);
            warManager.RegisterTokenToMove(this);
            isRegisteredToMove = true;

        }
        else
        {
            Debug.Log("Cannot move there. Move components on tile first.");
            return;
        }
    }

	public void RegisterToAttack(HexComponent h)
	{
		// Break out immediately if not selected
        if (isCurrentlySelected == false && isAIControlled == false)
            return;

		if (isRegisteredToMove == true)
		{
			warManager.UnregisterTokenToMove(this);
			isRegisteredToMove = false;
		}

		if (isRegisteredToAttack == true)
			warManager.UnregisterTokenToAttack(this);
		
		HexToAttack = h;
		warManager.RegisterTokenToAttack(this);
		isRegisteredToAttack = true;


	}
    
    public void Attack()
	{
		// TODO: Find equation to describe damage
		HexToAttack.TakeDamage(unitStats.Attack * Mathf.Sqrt(unitStats.Health),playerID);

		isRegisteredToAttack = false;
	}
    

	float fortificationModifier = 2f;
    public float GetDefense()
	{
		float def = unitStats.Defense * Mathf.Sqrt(unitStats.Health);
        
		if(isRegisteredToMove == false && isRegisteredToAttack == false)
		{
			Debug.Log("FORTIFYING!!!!");
			def *= fortificationModifier;
		}

		return def;
	}

    public void SetTarget(Transform t)
    {
        targetTransform = t;
    }

    public void SetTargetAsParent()
    {
        transform.parent = targetTransform;
    }

    public bool IsCurrentlySelected()
    {
        return isCurrentlySelected;
    }

    Vector3 velocity;
    float StartingDistance = -1;
    float currentDistance;
    Vector3 additionalHeight;

    public bool Move()
    {
		
        if (targetTransform == null)
        {
            Debug.Log("No Target Transform to move towards. There is probably no room on new hex. Removing from list of TokensToMove.");
            warManager.UnregisterTokenToMove(this);
            return true;
        }

        // Things to do only the first time through
        if (StartingDistance < 0)
        {
            // Try to refactor
            // TODO Find a way to not have to call this on every token, maybe
            RefactorTarget();

            StartingDistance = Vector3.Distance(transform.position, targetTransform.position);
            SetTargetAsParent();
        }
        

        currentDistance = Vector3.Distance(transform.position, targetTransform.position);

        // If less than half way to target, start arcing to position above
        if (currentDistance / StartingDistance > 0.5)
            additionalHeight = Vector3.up * currentDistance/2;
        else
            additionalHeight = Vector3.zero;

       
        transform.position = Vector3.SmoothDamp(transform.position, targetTransform.position + additionalHeight, ref velocity, currentDistance/StartingDistance*TimeToMove);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetTransform.rotation,(StartingDistance-currentDistance)/StartingDistance);

        // Return true if both angle and rotation are close enough to target
        if (Vector3.Distance(transform.position, targetTransform.position) < .0001f && Quaternion.Angle(transform.rotation, targetTransform.rotation) < 0.5f)
        {
            // Reset "flags" and current/Next hexGO
            StartingDistance = -1;
            isRegisteredToMove = false;
            CurrentHexGO = NextHexGO;
            NextHexGO = null;
			targetTransform = null;
            currentLocationOnHex = nextLocationOnHex;
            nextLocationOnHex = -1;
            DeactivateCollider();

            return true;
        }
        else
            return false;
    }
    
    public virtual void RefactorTarget()
    {
        NextHexGO.DecrementReservation(nextLocationOnHex,playerID);
        SetTarget(NextHexGO.GetNextAvailableTokenLocation(playerID));
        SetNextLocationOnHex(targetTransform);
    }

    public Transform GetTargetTransform()
    {
        return targetTransform;
    }

    public void SetCurrentHex(HexComponent h)
    {
        CurrentHexGO = h;
    }

    public bool IsRegisteredToMove()
    {
        return isRegisteredToMove;
    }

	public bool IsRegisteredToAttack()
    {
		return isRegisteredToAttack;
    }

    public void SetCurrentLocationOnHex(Transform t)
    {
        currentLocationOnHex = TransformToLocationOnHex(t);
    }

    public void SetNextLocationOnHex(Transform t)
    {
        nextLocationOnHex = TransformToLocationOnHex(t);
    }

    int TransformToLocationOnHex(Transform t)
    {
        int result;
        switch ((int)t.rotation.eulerAngles.y)
        {
            case 120:
                result = 0;
                break;

            case 180:
                result = 1;
                break;

            case 60:
                result = 2;
                break;

            case 240:
                result = 3;
                break;

            case 0:
                result = 4;
                break;

            case 300:
                result = 5;
                break;


            default:
                result = -1;
                Debug.LogError("THIS SHOULDN'T BE HAPPENING. SEEK HELP IMMEDIATELY!");
                break;
        }
        return result;
    }

    public int GetMovementSpeed()
    {
        return unitStats.Movement;
    }

    public int GetAttackRange()
    {
        return unitStats.AttackRange;
    }

    public string GetUnitType()
	{
		return unitStats.Type;
	}

    public int GetUnitTypeInt()
	{
		return unitStats.UnitTypeInt;
	}

	public HexComponent GetCurrentHex()
	{
		return CurrentHexGO;
	}

	public HexComponent GetHexAttacking()
	{
		return HexToAttack;
	}

    public void TakeDamage(int dmg)
	{
		unitStats.Health -= dmg;

		if (unitStats.Health < 0)
		{
			Debug.Log("Alas! I am slain.");
			CurrentHexGO.DecrementReservation(currentLocationOnHex, playerID);
			Destroy(gameObject);
		}
	}
}
