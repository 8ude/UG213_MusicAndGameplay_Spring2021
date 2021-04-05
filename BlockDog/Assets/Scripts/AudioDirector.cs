using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using Beat;

public class AudioDirector : MonoBehaviour
{

    //we make this a "singleton" because we know that there is only one of it
    public static AudioDirector Instance;

    //an audio mixer snapshot contains volume levels and effect parameters
    //we can transition to a snapshot, which changes the levels over time
    //right now we have 2 snapshots - for the gameplay, and for gameOver
    public AudioMixerSnapshot gameplaySnapshot, gameOverSnapshot;

    //you can have as many mixer groups as you want, but keep in mind that you need to assign audio sources to them
    public AudioMixerGroup actionSFX, gameStateSFX;

    //there are many ways to manage sound objects.  
    //this particular one incurs a small performance overhead, because we are instantiating and destroying a prefab
    //if I were to make this more efficient, I might start by making an object pool equal to the number of "real" voices
    //that I want to play at a given time...
    public GameObject SoundPrefab;

    //We have 2 looping sources at the moment, which we keep as children of this game object
    public AudioSource musicSource;
    public AudioSource dangerSource;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);

        //Set the appropriate clips and volume on music and danger loop
        //Play scheduled so they start at the same time
        musicSource.clip = backgroundMusic;
        musicSource.volume = musicVolume;
        musicSource.PlayScheduled(Beat.Clock.Instance.AtNextMeasure());

        dangerSource.clip = dangerLoop;
        dangerSource.volume = 0f;
        dangerSource.PlayScheduled(Beat.Clock.Instance.AtNextMeasure());

    }

    [Tooltip("Adjust this for panning intensity (0 is 'centered', 1 is 'full panning')")]
    [Range(0f, 1f)] public float PanningSpread = 0.5f;


    [Header("Game Sound Effects")] public AudioClip gameStartSound;
    [Range(0f, 1f)] public float gameStartVolume = 1.0f;
    [Tooltip("for scoring sounds, we have an array, and will pick a sound at random when we score")]
    public AudioClip[] matchSounds;
    [Range(0f, 1f)] public float matchVolume = 1.0f;
    public AudioClip gameOverSound;
    [Range(0f, 1f)] public float gameOverVolume = 1.0f;

    [Header("Player Action Sound Effects")] public AudioClip jumpSound;
    [Range(0f, 1f)] public float jumpVolume = 1.0f;
    public AudioClip pickupSound;
    [Range(0f, 1f)] public float pickupVolume = 1.0f;
    public AudioClip throwSound;
    [Range(0f, 1f)] public float throwVolume = 1.0f;
    public AudioClip landSound;
    [Range(0f, 1f)] public float landVolume = 1.0f;

    [Header("BlockActionSoundEffects")] public AudioClip[] blockPrepareDrops;
    [Range(0f, 1f)] public float blockPrepVolume = 1.0f;
    public AudioClip[] blockDrops;
    [Range(0f, 1f)] public float blockDropVolume = 1.0f;
    public AudioClip[] blockImpacts;
    [Range(0f, 1f)] public float blockLandVolume = 1.0f;

    [Header("Loops and Background")] public AudioClip backgroundMusic;
    [Range(0f, 1f)] public float musicVolume = 1.0f;
    [Tooltip("This is a loop that plays when the blocks get close to the top, which triggers a game over")]
    public AudioClip dangerLoop;
    [Range(0f, 1f)] public float dangerVolume = 1.0f;

    [Header("Quantization")] public bool useQuantizeSFX = false;
    public TickValue quantizationValue;
    public bool quantizeBlockDrops = true;
    public TickValue blockDropInterval = TickValue.Quarter;
    


    /// <summary>
    /// plays the drop block sound but with column number argument, in case we want to use this as an index for an array
    /// </summary>
    /// <param name="columnNumber"></param>
    public void PlayDropPrepSound(int columnNumber, float xPosition)
    {
        //check to make sure we assigned some audio clips!
        if (blockPrepareDrops.Length == 0) return;

        PlaySound(blockPrepareDrops[columnNumber % blockPrepareDrops.Length], true, xPosition, blockPrepVolume);

    }

    public void PlayDropSound(int columnNumber, float xPosition)
    {

        //check to make sure we assigned some audio clips!
        if (blockPrepareDrops.Length == 0) return;

        PlaySound(blockDrops[columnNumber % blockDrops.Length], true, xPosition, blockDropVolume);
    }

    public void PlayBlockLandSound(float yPos, float xPos)
    {
        //check to make sure we assigned some audio clips!
        if (blockImpacts.Length == 0) return;

        PlaySound(blockImpacts[
            IntRemap(
                yPos,
                0f, 11f,
                0, blockImpacts.Length - 1)
                ], 
            true, xPos, blockLandVolume);
    }





    /// <summary>
    /// This is what we use to play most sounds in the game
    /// </summary>
    /// <param name="clipToPlay">The Sound to Play</param>
    /// <param name="usePanning">Should we use panning?</param>
    /// <param name="xPosition">The X coordinate of the object playing the sound (if needed)</param>
    /// <param name="volume">the volume to play the sound at</param>
    /// <param name="pitchRand">any pitch randomization to use</param>
    /// <param name="gsSound">does this Sound Effect pertain to the game state?</param>
    public void PlaySound(AudioClip clipToPlay, bool usePanning = false, float xPosition = 0f, float volume = 1.0f, float pitchRand = 0f, bool gsSound = false)
    {

        if(clipToPlay == null)
        {
            //Debug.Log("AUDIO CLIP NOT ASSIGNED ON AUDIO DIRECTOR!");
            return;
        }

        GameObject newSound = Instantiate(SoundPrefab, Vector3.zero, Quaternion.identity);
        AudioSource newSoundSource = newSound.GetComponent<AudioSource>();

        //set variables on the audio source accordingly
        newSoundSource.clip = clipToPlay;
        
        if (usePanning)
        {
            newSoundSource.panStereo = PositionToPanning(xPosition);
        }

        newSoundSource.volume = volume;

        if (gsSound)
        {
            newSoundSource.outputAudioMixerGroup = gameStateSFX;
        }
        else
        {
            newSoundSource.outputAudioMixerGroup = actionSFX;
        }

        //if we're quantizing this, then play scheduled
        if(useQuantizeSFX)
        {
            newSoundSource.PlayScheduled(NextQuantizedInterval(quantizationValue));
        }
        else
        {
            newSoundSource.Play();
        }
        
        //destroy the sound effect after the length, plus a little padding
        Destroy(newSound, clipToPlay.length + (float)quantizationValue * 2f);

    }

    /// <summary>
    /// This is --almost-- the same, but we can use it to play sounds from an array.  In fact, if you wanted to, you could make an array from any of the audioclip variables above, and it would pick one at random.
    /// </summary>
    /// <param name="clipToPlay">The Sound to Play</param>
    /// <param name="usePanning">Should we use panning?</param>
    /// <param name="xPosition">The X coordinate of the object playing the sound (if needed)</param>
    /// <param name="volume">the volume to play the sound at</param>
    /// <param name="pitchRand">any pitch randomization to use</param>
    /// <param name="gsSound">does this Sound Effect pertain to the game state?</param>
    public void PlaySound(AudioClip[] clipsToPlay, bool usePanning = false, float xPosition = 0f, float volume = 1.0f, float pitchRand = 0f, bool gsSound = false)
    {
        GameObject newSound = Instantiate(SoundPrefab, Vector3.zero, Quaternion.identity);
        AudioSource newSoundSource = newSound.GetComponent<AudioSource>();

        if (clipsToPlay[0] == null)
        {
            //Debug.Log("AUDIO CLIP NOT ASSIGNED ON AUDIO DIRECTOR!");
            return;
        }

        //pick a random clip from the array
        newSoundSource.clip = clipsToPlay[Random.Range(0, clipsToPlay.Length)];

        if (usePanning)
        {
            newSoundSource.panStereo = PositionToPanning(xPosition);
        }

        newSoundSource.volume = volume;

        if (gsSound)
        {
            newSoundSource.outputAudioMixerGroup = gameStateSFX;
        }
        else
        {
            newSoundSource.outputAudioMixerGroup = actionSFX;
        }

        //if we're quantizing this, then play scheduled
        if (useQuantizeSFX)
        {
            newSoundSource.PlayScheduled(NextQuantizedInterval(quantizationValue));
        }
        else
        {
            newSoundSource.Play();
        }
        //destroy the sound effect after the length, plus a little padding
        Destroy(newSound, newSoundSource.clip.length + (float)quantizationValue * 2f);

    }

    

    //we aren't using the 3D panning options yet, so we need something to put things into left and right speakers
    float PositionToPanning(float xPosition)
    {
        return Remap(xPosition, 1f, 13f, -PanningSpread, PanningSpread);
    }

    //Always good to fade loops in and out, especially music/ambience loops

    public void FadeInAudio(AudioSource source, float destVolume, float timeToFade)
    {
        Debug.Log("fading in audio");
        StartCoroutine(LinearFadeIn(source, destVolume, timeToFade));
    }

    public void FadeOutAudio(AudioSource source, float timeToFade)
    {
        StartCoroutine(LinearFadeOut(source, timeToFade));
    }

    public IEnumerator LinearFadeIn(AudioSource audioSource, float destVol, float time)
    {
        while (audioSource.volume < destVol)
        {
            audioSource.volume += Time.deltaTime / time;
            yield return null;
        }
    }

    public IEnumerator LinearFadeOut(AudioSource audioSource, float time)
    {
        while (audioSource.volume > 0)
        {
            audioSource.volume -= Time.deltaTime / time;
            yield return null;
        }
    }

    //Standard Linear Remapping Function
    public static float Remap(float from, float fromMin, float fromMax, float toMin, float toMax)
    {
        var fromAbs = from - fromMin;
        var fromMaxAbs = fromMax - fromMin;

        var normal = fromAbs / fromMaxAbs;

        var toMaxAbs = toMax - toMin;
        var toAbs = toMaxAbs * normal;

        var to = toAbs + toMin;

        return to;
    }

    //similar but with returning an integer
    //not that this is not going to remap to a scale!
    public static int IntRemap(float from, float fromMin, float fromMax, int toMin, int toMax)
    {
        var fromAbs = from - fromMin;
        var fromMaxAbs = fromMax - fromMin;

        var normal = fromAbs / fromMaxAbs;

        var toMaxAbs = toMax - toMin;
        var toAbs = toMaxAbs * normal;

        var to = toAbs + toMin;

        return Mathf.RoundToInt(to);
    }

    double NextQuantizedInterval(Beat.TickValue quantInterval)
    {
        switch (quantInterval)
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
                Debug.Log("Quantization rhythm not set; returning quarter note");
                return Clock.Instance.AtNextQuarter();
        }
    }



   

}
