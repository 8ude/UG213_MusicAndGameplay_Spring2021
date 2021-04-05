using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Indicator : MonoBehaviour {
    public SpriteRenderer colSpr;
	// Use this for initialization
	void Start () {
        Color col = GetComponent<SpriteRenderer>().color;
        colSpr.color = new Color(col.r, col.g, col.b, .6f);//.175f);
        colSpr.transform.parent = null;
	}
    private void OnDestroy() {
        if (colSpr == null) return;
        Destroy(colSpr.gameObject);
    }

    // Update is called once per frame
    void Update () {
        transform.localScale += new Vector3(1f * Time.deltaTime, 1f * Time.deltaTime, 0);

	}
}
