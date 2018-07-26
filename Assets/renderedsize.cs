using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class renderedsize : MonoBehaviour {

	// Use this for initialization
	void Start () {
        Debug.Log(this.GetComponent<Renderer>().bounds.size.z);
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
