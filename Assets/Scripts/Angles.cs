using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Angles : MonoBehaviour {

    public GameObject other;
    public Text walldis;

    // Use this for initialization
    void Start () {
        
        //transform.position = other.transform.position;
    }

    private void FixedUpdate()
    {
        //Debug.Log(Vector3.Distance(other.transform.position, transform.position));
        //Vector3 heading = transform.position - other.transform.position;
        //Debug.Log(this.GetComponent<Renderer>().bounds.size.y);

        walldis.text = "Camera-Wall Distance: " + (transform.position.z - other.transform.position.z);
        // Debug.Log(other.transform.rotation.y);
        //Debug.Log(Vector3.Dot(heading, other.transform.forward));



    }
}
