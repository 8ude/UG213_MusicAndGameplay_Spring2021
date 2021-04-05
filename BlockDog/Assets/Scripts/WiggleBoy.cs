using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WiggleBoy : MonoBehaviour {
    public GameObject spr;
    public float height;
    public float wiggleSpd;
    public float baseHeight;
    public float baseWidth;
    public Vector2 prevVel;
    public Rigidbody2D rb;
	// Use this for initialization
	void Start () {
        baseHeight = spr.transform.localScale.y;
        baseWidth = spr.transform.localScale.x;
        rb = GetComponent<Rigidbody2D>();
        height = 1f;
    }
	
	// Update is called once per frame
	void Update () {
        wiggleSpd += (1f - height) * 2.8f * Time.deltaTime;
        wiggleSpd -= wiggleSpd * 5.8f * Time.deltaTime;
        height += wiggleSpd;
        spr.transform.localScale = new Vector3(baseWidth * (2f - height), baseHeight * height, 1);
    }

    private void FixedUpdate() {
        //print(prevVel.y - rb.velocity.y);
        wiggleSpd += (prevVel.y - rb.velocity.y) * .003f;
        prevVel = rb.velocity;
        
    }
}
