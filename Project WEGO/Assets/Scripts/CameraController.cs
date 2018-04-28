using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

    public float moveSpeed;
    public float offset;
    public float zShift;

    bool CameraHasMoved;
    float mapWidth;
    float mapHeight;


	private void Start()
	{

        transform.position = new Vector3(offset, transform.position.y, offset);

	}

	// Update is called once per frame
	void Update () {
        if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
            CameraHasMoved = true;
            
            
	}

	private void LateUpdate()
	{
        if (CameraHasMoved == true)
        {
            if (mapHeight == 0 || mapWidth == 0)
            {
                HexMap h = FindObjectOfType<HexMap>();

                mapWidth = h.GetMapWidth();
                mapHeight = h.GetMapHeight();
            }


            if (Input.GetAxis("Horizontal") != 0)
                MoveHorizontal();

            if (Input.GetAxis("Vertical") != 0)
                MoveVertical();


            transform.position = new Vector3(Mathf.Clamp(transform.position.x, 0 + offset, mapWidth - offset),
                                         transform.position.y,
                                         Mathf.Clamp(transform.position.z, 0 + zShift, mapHeight + zShift));

            CameraHasMoved = false;
        }
	}


	void MoveHorizontal()
    {
        transform.position += Vector3.right * moveSpeed * Input.GetAxis("Horizontal");
    }



    void MoveVertical()
    {
        transform.position += Vector3.forward * moveSpeed * Input.GetAxis("Vertical");
    }

   
}
