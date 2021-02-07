using System.Collections;
using System.Collections.Generic;
using Beat;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class ClockTester : MonoBehaviour {

	public AudioClip testSound;

	public AudioSource audioSource;

	private double nextSoundTime = 0;
	private bool isClipScheduled = false;
	
	// Use this for initialization
	void Start () {
		audioSource.clip = testSound;
	}
	
	// Update is called once per frame
	void Update () {
		
		
		
		if (Input.GetKeyDown(KeyCode.Space) && !isClipScheduled) {
			nextSoundTime = Clock.Instance.AtNextEighth();
			isClipScheduled = true;
			audioSource.PlayScheduled(nextSoundTime);
		}
		
		if (AudioSettings.dspTime > nextSoundTime) {
			isClipScheduled = false;
		}

		
		
		
	}
}
