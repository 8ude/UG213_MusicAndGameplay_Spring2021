using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class arrow : MonoBehaviour {
    public bool flying;
    public Animator anim;
    Rigidbody2D rb;
    public ParticleSystem[] pSys;
    bool inWall;


    void Start () {
        flying = true;
        rb = GetComponent<Rigidbody2D>();
        rb.AddForce(transform.right * 25f, ForceMode2D.Impulse);
    }

    // Update is called once per frame
    void LateUpdate () {
        //rb.MoveRotation(Geo.ToAng(rb.velocity));
        //rb.AddTorque(100f);
        if (!inWall) {
            transform.eulerAngles = new Vector3(0, 0, Geo.ToAng(rb.velocity));
        }
        //transform.rotation = Quaternion.AngleAxis(180 + Time.timeSinceLevelLoad, Vector3.forward);
        //print(Geo.ToAng(rb.velocity));
	}

    private void OnCollisionEnter2D(Collision2D collision)
    {
        inWall = true;
        flying = false;
        GetComponent<Rigidbody2D>().isKinematic = true;

        anim.SetTrigger("hitWall");
        rb.velocity = Vector3.zero;
        for (int i = 0; i < pSys.Length; i++) {
            pSys[i].Play();
        }
        
    }
}
