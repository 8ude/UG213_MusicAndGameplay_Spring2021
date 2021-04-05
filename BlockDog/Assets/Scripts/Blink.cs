using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blink : MonoBehaviour {
    float blinkTimer;
    bool blinkedDown;
	// Use this for initialization
	void Start () {
        blinkTimer = Random.Range(0f, 2f);
    }
	
	// Update is called once per frame
	void Update () {
		if (blinkTimer <= 0) {
            if (!blinkedDown) {
                transform.localScale = new Vector3(1, Mathf.Max(transform.localScale.y - .1f, 0), 0);
            } else {
                transform.localScale = new Vector3(1, Mathf.Min(transform.localScale.y + .1f, 1), 0);
            }
            if (transform.localScale.y == 0) {
                blinkedDown = true;
            }
            if (blinkedDown && transform.localScale.y == 1) {
                blinkedDown = false;
                blinkTimer = Random.Range(0f, 2f);
            }
           
        }
        blinkTimer -= Time.deltaTime;
    }
}
