using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RendererEffect : MonoBehaviour {
    public Texture2D lastFrame;
    public RenderTexture thirdRender;
    public Material trailMat;
    public RenderTexture curFrame;
    public RenderTexture secondRender;
    bool startFlag;
    Camera cam;
    // Use this for initialization
    void Awake () {
        cam = GetComponent<Camera>();
        /*curFrame.height = Screen.height;
        curFrame.width = Screen.width;
        curFrame.DiscardContents();
        secondRender = GetComponent<Camera>().targetTexture;
        secondRender.height = Screen.height;
        secondRender.width = Screen.width;
        secondRender.DiscardContents();
        thirdRender.DiscardContents(true, true);

        thirdRender.height = Screen.height;
        thirdRender.width = Screen.width;*/

        startFlag = true;
        /*lastFrame = new Texture2D(Screen.width, Screen.height);
        lastFrame.anisoLevel = 0;
        lastFrame.filterMode = FilterMode.Point;
        Color[] bloop = new Color[1];
        bloop[0] = Color.black;

        for (int y = 0; y < lastFrame.height; y++) {
            for (int x = 0; x < lastFrame.width; x++) {
                //Color color = ((x & y) != 0 ? Color.white : Color.gray);
                lastFrame.SetPixel(x, y, Color.black);
            }
        }
        lastFrame.Apply();*/

        //trailMat.SetTexture("_LastFrameTex", lastFrame);
    }

    // Update is called once per frame
    void Update () {
		
	}
    private void OnPreRender() {
        
    }
    void OnPostRender() {
        //return;
        if (startFlag) {
            Graphics.Blit(Texture2D.blackTexture, cam.targetTexture);
            startFlag = false;
        }
        Graphics.Blit(cam.targetTexture, thirdRender);
        //lastFrame.ReadPixels(new Rect(0f, 0f, Screen.width, Screen.height), 0,0);
        //lastFrame.Apply();
    }
    /*void OnRenderImage(RenderTexture rend, RenderTexture rend2) {
        Graphics.Blit(rend, rend2);
        Graphics.DrawTexture(new Rect(Vector2.zero, new Vector2(1280f, -1000f)), rend);
    }*/
}
