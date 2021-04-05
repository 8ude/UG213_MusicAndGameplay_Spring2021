using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour {
    public SpriteRenderer spr;
    bool flashing;
    float flashTimer;
    float flashTime;
    Vector3 defScale;
    public AudioClip bounceSound;

	// Use this for initialization
	void Start () {
        flashTime = .1f;
        defScale = spr.transform.localScale;
	}
	
	// Update is called once per frame
	void Update () {
	    if (flashing) {
            if (flashTimer >= flashTime)
            {
                flashing = false;
                spr.color = Color.white;
                spr.transform.localScale = defScale;
                flashTimer = 0;
            }
            flashTimer += Time.deltaTime;
        }	
	}

    void GetHit()
    {
        flashing = true;
        spr.color = Color.yellow;
        spr.transform.localScale *= 1.8f;
    }

    private void OnCollisionEnter2D(Collision2D col) {
        Sound.me.PlaySound(bounceSound, 1);
        if (col.gameObject.tag == "Player")
        {

        }
    }
}
