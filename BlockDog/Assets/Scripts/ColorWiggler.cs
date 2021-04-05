using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorWiggler : MonoBehaviour {
    SpriteRenderer spr;
    public Color baseColor;
    float wiggleRate;
    float wiggleTimer;
    public float colorBand = .1f;
    public Camera cam;
    public TextMesh txt;
	void Start () {
        //colorBand = .1f;
        if (cam != null) {
            baseColor = cam.backgroundColor;
            //colorBand = .05f;
        } else if (txt != null) {
            baseColor = txt.color;

        } else {
            spr = GetComponent<SpriteRenderer>();
            baseColor = spr.color;
        }
        wiggleRate = 1f / 60f * 6;
	}
	
	// Update is called once per frame
	void Update () {
        wiggleTimer += Time.deltaTime;
        //print(wiggleTimer + " " + wiggleRate);
        if (wiggleTimer >= wiggleRate) {
            Color col = new Color(baseColor.r + Random.value * colorBand, baseColor.g + Random.value * colorBand, baseColor.b + Random.value * colorBand);
            if (cam != null) {
                cam.backgroundColor = col;
            } else if (txt != null) {
                txt.color = col;
            } else {
                spr.color = col;
            }
            wiggleTimer -= wiggleRate;
            //Debug.Log("wiggling");
        }
	}
}
