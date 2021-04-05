using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Global : MonoBehaviourSingleton<Global> {
    public static Global me;
    /*public float netX;
    public Rigidbody2D ball;
    public GameObject net;*/
    public Color[] blockColors;
    public GameObject player;
    private void Awake()
    {
        me = this;
    }


	void Start () {
		
	}
	
	void Update () {
		
	}
}
