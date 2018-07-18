using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Angles : MonoBehaviour {

    public GameObject x;
    public GameObject y;

    // Use this for initialization
    void Start () {
    }
	
	// Update is called once per frame
	void Update () {
       /* Vector2 dir = new Vector2(y.transform.position.x, y.transform.position.z) - new Vector2(x.transform.position.x, x.transform.position.z);
        var forward = new Vector2(x.transform.forward.x, x.transform.forward.z);
        float angle = System.Math.Abs(Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
        Debug.Log(angle);*/
    }
}
