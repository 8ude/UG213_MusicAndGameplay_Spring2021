using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//this functionality has been extended by SynthRhythmBounce.cs, which adds the option to quantize sound to the beat
public class SynthBounceOrig : MonoBehaviour {


	//organize sequentially from low to high
	
	public float maxSpeed = 1.0f;

	private AudioSource _audioSource;

	private pxStrax _straxSynth;
	
	//natural minor scale
	private float[] notes = { 0, 2f, 3f, 5f, 7f, 9f, 12f };

	void Start() {
		//caching our components
		_audioSource = GetComponent<AudioSource>();
		_straxSynth = GetComponent<pxStrax>();
	}

	void OnCollisionEnter2D(Collision2D collision)
	{
		//mapping our note from the strength of the collision
		int noteIndex = GetCollisionStrength(collision);
		
		//making sure we remain in the array
		float scaleDegree = notes[noteIndex % notes.Length];
		
		//finding our octave, and transposing to an audible range
		float octave = Mathf.Floor(noteIndex / 12f) + 3;

		//finding the key to play from the scale degree and the octave.
		//Clamping to standard MIDI range.
		float keyToPlay = Mathf.Clamp(scaleDegree + (octave * 12f), 0f, 128);
		
		_straxSynth.KeyOn(keyToPlay);

	}
	
	/// <summary>
	/// gets the velocity of the ball relative to the normal (90 degree angle) of the
	/// platform surface (if our platform was a table, the normal would be "up"),
	/// then maps this to our array of tones and gives us an index to pick from
	/// </summary>
	/// <param name="collision">unity physics engine information about the collision</param>
	/// <returns>an index of the tone to play, based on the collision strength</returns>
	int GetCollisionStrength(Collision2D collision)
	{
		//project the velocity onto the normal of the platform
		Vector3 normal = collision.contacts[0].normal;
		Vector3 bounceAmount = Vector3.Project(collision.relativeVelocity, normal);
		float speed = bounceAmount.magnitude;
		
		//we want to map the ball's speed to a 0-1 range
		float clampedSpeed = Mathf.Clamp(speed / maxSpeed, 0.0f, 1f);

		//we're expanding to a somewhat arbitrary range, which we will be mapping to midi notes
		return Mathf.FloorToInt(clampedSpeed * 50);
	}

    //ideas for expansion: 
    // **something that changes the base synth note
    // **something that changes synth parameters


}
