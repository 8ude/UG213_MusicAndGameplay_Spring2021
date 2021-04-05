using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EyeBall : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	/*void Update () {
        Vector2 offSet = new Vector2(-30, 30);
        Vector2 lookPt = new Vector2(-999, -999);
        Collider2D[] coll = Physics2D.OverlapAreaAll(Vector2.zero, new Vector2(30f, 30f), LayerMask.GetMask("Block"));
        for (int i = 0; i < coll.Length; i++) {
            if ((coll[i].transform.position - transform.position).magnitude < ((Vector3) lookPt - transform.position).magnitude) {
                lookPt = coll[i].transform.position;
            }
        }
        transform.localPosition = (lookPt - (Vector2)transform.position).normalized * .1f;
	}*/
}
