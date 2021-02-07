using System.Collections;
using System.Collections.Generic;
using Beat;
using UnityEngine;

//this version of our bouncing script will sync the tones to the next quantized interval (32nd, 16th, etc)
[RequireComponent(typeof(pxStrax))]
public class SynthRhythmBounce : MonoBehaviour {
	
	public float maxSpeed = 1.0f;

    public bool useQuantization = false;

	public Beat.TickValue platformRhythm;

	public int baseOctave = 3;

	private AudioSource _audioSource;

	public pxStrax straxSynth;
	
	//natural minor scale
	private float[] notes = { 0, 2f, 3f, 5f, 7f, 8f, 10f, 12f };

	void Start() {
		//caching our components
		_audioSource = GetComponent<AudioSource>();
		straxSynth = GetComponent<pxStrax>();
	}

	void OnCollisionEnter2D(Collision2D collision)
	{
		//mapping our note from the strength of the collision
		int noteIndex = GetCollisionStrength(collision);
		
		//making sure we remain in the array
		float scaleDegree = notes[noteIndex % notes.Length];
		
		//finding our octave, and transposing to an audible range
		float octave = Mathf.Floor(noteIndex / 12f) + baseOctave;

		//finding the key to play from the scale degree and the octave.
		//Clamping to standard MIDI range.
		float keyToPlay = Mathf.Clamp(scaleDegree + (octave * 12f), 0f, 128);

		
		
		
		if(useQuantization)
        {
            double nextTiming = NextQuantizedInterval(platformRhythm);
            straxSynth.KeyOnScheduled(keyToPlay, nextTiming);
        }
        else
        {
            straxSynth.KeyOn(keyToPlay);
        }
		



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

	double NextQuantizedInterval(Beat.TickValue bounceInterval) {
		switch (bounceInterval) {
			case TickValue.ThirtySecond:
				
				return Clock.Instance.AtNextThirtySecond();
			case TickValue.SixteenthTriplet:
				return Clock.Instance.AtNextSixteenthTriplet();
			case TickValue.Sixteenth:
				return Clock.Instance.AtNextSixteenth();
			case TickValue.EighthTriplet:
				return Clock.Instance.AtNextEighthTriplet();
			case TickValue.Eighth:
				return Clock.Instance.AtNextEighth();
			case TickValue.QuarterTriplet:
				return Clock.Instance.AtNextQuarterTriplet();
			case TickValue.Quarter:
				return Clock.Instance.AtNextQuarter();
			case TickValue.Half:
				return Clock.Instance.AtNextHalf();
			case TickValue.Measure:
				return Clock.Instance.AtNextMeasure();
			default:
				Debug.Log("platform rhythm not set; returning quarter note");
				return Clock.Instance.AtNextQuarter();
		}
	}


}
