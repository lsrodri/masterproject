using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Angles : MonoBehaviour {

    public GameObject other;

    // Use this for initialization
    void Start () {
        //Debug.Log(this.GetComponent<Collider>().bounds.size);
        //transform.position = other.transform.position;
    }

    private void Update()
    {
        //Debug.Log(Vector3.Distance(other.transform.position, transform.position));
        //Vector3 heading = transform.position - other.transform.position;

        //Debug.Log(transform.position.x - other.transform.position.x);
        //Debug.Log(Vector3.Dot(heading, other.transform.forward));
        
    }
}
