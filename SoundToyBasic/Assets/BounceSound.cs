using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BounceSound : MonoBehaviour
{
    private AudioSource collisionAudio;

    // Start is called before the first frame update
    void Start()
    {
        collisionAudio = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        //when the player presses spacebar, add a random force to the object
        if(Input.anyKeyDown)
        {
            GetComponent<Rigidbody>().AddForce(new Vector3(Random.Range(-4.0f, 4.0f), Random.Range(-4.0f, 4.0f), Random.Range(-4.0f, 4.0f)), ForceMode.Impulse); 
        }
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        float vel = collision.relativeVelocity.magnitude;
        Debug.Log("velocity value: " + vel);

        //collisionAudio.Play();
        collisionAudio.pitch = Mathf.Clamp01(vel);
        collisionAudio.PlayOneShot(collisionAudio.clip, Mathf.Clamp01(vel));

    }
    
    //remaps a float from one range onto another
    public static float Remap(float value, float from1, float to1, float from2, float to2)
    {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }
}
