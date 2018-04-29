using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Token : MonoBehaviour
{

    public SpriteRenderer spriteRenderer;
    public Material MainMaterial;
    public Material AccentMaterial;
    public GameObject Outliner;
    public Color OutlineColor;

    bool isCurrentlySelected = false;
    bool isRegisteredToMove = false;

    BoxCollider coll;
    WarManager warManager;
    Transform targetTransform;

    public float TimeToMove = 0.3f;

    private void Start()
    {
        coll = GetComponent<BoxCollider>();


        // Start with the token colliders inactive
        coll.enabled = false;
    }

    // Only called first time, sets material colors and sprite
    public void SetUp(Color main, Color accent, Sprite s, WarManager w)
    {
        if (transform.GetComponent<LeaderToken>() != null)
            Debug.Log("Setting pu leader");

        spriteRenderer.color = accent;
        spriteRenderer.sprite = s;

        if (Outliner.GetComponent<MeshRenderer>() == null)
            Debug.Log("Well");

        Outliner.GetComponent<MeshRenderer>().material.color = OutlineColor;
        MainMaterial.color = main;
        AccentMaterial.color = accent;

        warManager = w;

    }


    // Function for when material has already been set,
    // Only necessary to change sprite renderer color and set sprite
    public void SetUp(Color accent, Sprite s, WarManager w)
    {
        spriteRenderer.color = accent;
        spriteRenderer.sprite = s;
        Outliner.GetComponent<MeshRenderer>().material.color = OutlineColor;

        warManager = w;
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

    }

    public void Deselect()
    {
        isCurrentlySelected = false;
        Outliner.gameObject.SetActive(false);

        //AccentMaterial.DisableKeyword("_EMISSION");

    }

    public void ToggleSelect()
    {
        if (isCurrentlySelected == true)
            Deselect();
        else
            Select();
    }


    public void RegisterToMove(Transform t)
    {
        if (t == null)
        {
            Debug.Log("Cannot move there. Move components on tile first.");
            return;
        }
        if (isCurrentlySelected == true)
        {
            if (isRegisteredToMove == true)
            {
                warManager.UnregisterTokenToMove(this);
            }
            SetTarget(t);
            warManager.RegisterTokenToMove(this);
            isRegisteredToMove = true;
        }
    }

    public void SetTarget(Transform t)
    {
        targetTransform = t;
        transform.parent = t;
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

        if (StartingDistance < 0)
            StartingDistance = Vector3.Distance(transform.position, targetTransform.position);
        

        currentDistance = Vector3.Distance(transform.position, targetTransform.position);

        // If less than half way to target, start arcing to position above
        if (currentDistance / StartingDistance > 0.5)
            additionalHeight = Vector3.up * currentDistance/2;
        else
            additionalHeight = Vector3.zero;

       
        transform.position = Vector3.SmoothDamp(transform.position, targetTransform.position + additionalHeight, ref velocity, currentDistance/StartingDistance*TimeToMove);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetTransform.rotation,(StartingDistance-currentDistance)/StartingDistance);

        // Return true if both angle and rotation are close enough to target
        if (Vector3.Distance(transform.position, targetTransform.position) < .01f && Quaternion.Angle(transform.rotation, targetTransform.rotation) < 0.5f)
        {
            //transform.parent = targetTransform;
            StartingDistance = -1;
            return true;
        }
        else
            return false;
    }

    public Transform GetTargetTransform()
    {
        return targetTransform;
    }
}
