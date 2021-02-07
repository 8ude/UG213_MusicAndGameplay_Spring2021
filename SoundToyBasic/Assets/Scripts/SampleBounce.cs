using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Beat;

public class SampleBounce : MonoBehaviour {


	//organize sequentially from low to high
	public AudioClip[] tones;
	
	public float maxSpeed = 1.0f;

	private AudioSource _audioSource;

    public bool useQuantization = false;

    public Beat.TickValue platformRhythm;

    void Start() {
		_audioSource = GetComponent<AudioSource>();
        
	}

	void OnCollisionEnter2D(Collision2D collision)
	{
		int noteIndex = GetCollisionStrength(collision);

        if (useQuantization)
        {
            double nextTiming = NextQuantizedInterval(platformRhythm);
            _audioSource.PlayScheduled(nextTiming);
        }
        else
        {
            _audioSource.PlayOneShot(tones[noteIndex]);
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
		float clampedSpeed = Mathf.Clamp(speed / maxSpeed, 0.0f, 0.99f);

		//we want to round down in order to get the correct index of our tone array
		return Mathf.FloorToInt(clampedSpeed * tones.Length);
	}

    double NextQuantizedInterval(Beat.TickValue bounceInterval)
    {
        switch (bounceInterval)
        {
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
