using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour {

    public GameObject cameraEye;

	// Update is called once per frame
	void Update () {
        //Debug.Log(cameraEye.transform.localRotation.eulerAngles.y);
        //transform.rotation =  new Quaternion(45f, 180f + cameraEye.transform.rotation.y, 0, 1f);
        //transform.SetPositionAndRotation(new Vector3(3.72f, 1.27f, -4.933f), new Quaternion(45f, 180f + cameraEye.transform.localRotation.eulerAngles.y, 0, 1f));
        //Debug.Log((transform.position.x - cameraEye.transform.position.x) + "," + (transform.position.y - cameraEye.transform.position.y) + "," + (transform.position.z - cameraEye.transform.position.z));
    }


	/*private float smoothSpeed = 0.125f;
    Vector3 offset  = new Vector3(0.01774287f,-0.01400661f,0.0739913f);

	void FixedUpdate ()
	{
		Vector3 desiredPosition = cameraEye.transform.position + offset;
		Vector3 smoothedPosition = Vector3.Lerp(cameraEye.transform.position, desiredPosition, smoothSpeed);
		transform.position = smoothedPosition;

		transform.LookAt(cameraEye.transform);
	}*/
}
