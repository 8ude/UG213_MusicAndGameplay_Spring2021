using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This is just to demonstrate what an alternative pattern would look like, using scriptable objects
//I would use something similar to this if I was going to make a game that would scale larger

[CreateAssetMenu(menuName = "AudioObject")]
public class SOAudioExample : ScriptableObject
{
    public AudioClip[] audioClips;
    public bool playSequentially;
    [Range(0f, 1f)] public float volume;
    [Range(0f, 1f)] public float volRand;
    [Range(0f, 1f)] public float pitchRand;

    
    private int lastClipPlayed = -1;

    //usually you don't want to repeat the last clip played.  you could extend this to include "shuffle bag" style randomization.
    public AudioClip PickClip()
    {
        if(playSequentially)
        {
            int i = (lastClipPlayed + 1) % audioClips.Length;
            lastClipPlayed++;
            return audioClips[i];
        }
        else
        {
            int i = Random.Range(0, audioClips.Length);
            if (i == lastClipPlayed)
            {
                i++;
                i %= audioClips.Length;
            }

            lastClipPlayed = i;
            return audioClips[i];
        }
    }
}
