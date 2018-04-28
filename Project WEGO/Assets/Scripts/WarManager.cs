using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Use this class to manage the various war states
public class WarManager : MonoBehaviour {

    public bool IsSettingUp = true;
    MouseController mouse;
    UIManager UI;

    bool thingsToMove = false;

	// Use this for initialization
	void Start () {

        mouse = FindObjectOfType<MouseController>();
        UI = FindObjectOfType<UIManager>();

        UI.InSetup();

	}
	
    // Update is called once per frame
	void Update () {
       
        if (thingsToMove)
        {
            foreach (var t in ToMove)
            {
                if (t != null)
                {
                    t.transform.position = Vector3.SmoothDamp(t.transform.position, Target.position, ref velocity, 0.25f);

                    // Stop if close enough
                    if (Vector3.Distance(t.transform.position, Target.position) < .01f)
                    {
                        thingsToMove = false;
                    }
                }
            }
        }

	}


    public void FinishedSettingUp()
    {
        IsSettingUp = false;
        mouse.ToGameplay();
        UI.LeavingSetup();
        Debug.Log("Setup is complete!");
    }

    Vector3 velocity;
    Transform Target;
    GameObject[] ToMove;
    public void MoveTokens(GameObject[] go, Transform target)
    {
        thingsToMove = true;
        ToMove = go;
        Target = target;

    }
}
