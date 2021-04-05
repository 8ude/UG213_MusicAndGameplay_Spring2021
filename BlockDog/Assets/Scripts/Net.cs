using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Net : MonoBehaviour {
    public float subtractAmnt;
    public float addAmnt;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void BallHit()
    {
        transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y + addAmnt);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y -  subtractAmnt);
    }
}
