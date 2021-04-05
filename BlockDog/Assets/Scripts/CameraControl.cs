using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour {
    public GameObject player;
    public static CameraControl me;
    public float shakeAmnt;
    public Vector3 defPos;
    public ColorWiggler wigl;
    public Color flashColor;
    public Color baseColor;
    public Texture curFrame;
    public Texture2D lastFrame;
    public Material trailMat;
    public SpriteRenderer flashSprite;
    // Use this for initialization

    private void Awake() {
        me = this;
    }
    void Start () {
        baseColor = Camera.main.backgroundColor;
        defPos = transform.position;

	}
	
	// Update is called once per frame
	void Update () {
        transform.position = defPos + new Vector3(Random.Range(-shakeAmnt, shakeAmnt), Random.Range(-shakeAmnt, shakeAmnt), 0);
        shakeAmnt = Mathf.Lerp(shakeAmnt, 0, .05f * 60f * Time.deltaTime);
        shakeAmnt -= .05f * 60f * Time.deltaTime;
        shakeAmnt = Mathf.Max(shakeAmnt, 0);

        wigl.baseColor = Color.Lerp(wigl.baseColor, baseColor, .1f);
        //lastFrame.ReadPixels(new Rect(0f, 0f, Screen.width, Screen.height), 0,0);

        //Graphics.Blit(curFrame, (RenderTexture)lastFrame);
	}
    public void Shake(float amnt) {
        shakeAmnt = amnt;
    }

    public void Flash(Color col) {
        //StartCoroutine(FlashCo(col));
        wigl.baseColor = col;
    }
    
    public void Flash(float flashTime) {
        StartCoroutine(FlashCo(flashTime));
    }

    public IEnumerator FlashCo(float flashTime) {
        flashSprite.gameObject.SetActive(true);
        yield return new WaitForSeconds(flashTime);
        flashSprite.gameObject.SetActive(false);
    }

    /*public IEnumerator FlashCo(Color col) {
        
        yield return new WaitForSeconds(.25f);
        wigl.baseColor = baseColor;

    }*/

}
