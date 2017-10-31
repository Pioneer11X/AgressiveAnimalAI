using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour {

    public float panSpeed;
    public float zoomFactor = 100.0f;

    public float maxZoomPosX;
    public float minZoomPosX;

    public float maxZoomPosY;
    public float minZoomPosY;

    public float maxZoomPosZ;
    public float minZoomPosZ;

    private float tempY;
    private float tempX;
    private float tempZ;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

        if (Input.GetAxis("Mouse ScrollWheel") < 0f) // forward
        {
            tempY = transform.position.y + zoomFactor * Time.deltaTime;

            tempY = Mathf.Clamp(tempY, minZoomPosY, maxZoomPosY );

            transform.position = new Vector3(transform.position.x, tempY , transform.position.z);
        }
        else if (Input.GetAxis("Mouse ScrollWheel") > 0f) // backwards
        {
            tempY = transform.position.y - zoomFactor * Time.deltaTime;

            tempY = Mathf.Clamp(tempY, minZoomPosY, maxZoomPosY);

            transform.position = new Vector3(transform.position.x, tempY, transform.position.z);
        }

        if ( Input.GetMouseButton(2))
        {

            tempX = transform.position.x - Input.GetAxis("Mouse X") * Time.deltaTime * panSpeed;
            tempZ = transform.position.z - Input.GetAxis("Mouse Y") * Time.deltaTime * panSpeed;

            tempX = Mathf.Clamp(tempX, minZoomPosX - transform.position.y, maxZoomPosX + transform.position.y);
            tempZ = Mathf.Clamp(tempZ, minZoomPosZ - transform.position.y, maxZoomPosZ + transform.position.y);

            transform.position = new Vector3(tempX, transform.position.y, tempZ);

        }

    }
}
