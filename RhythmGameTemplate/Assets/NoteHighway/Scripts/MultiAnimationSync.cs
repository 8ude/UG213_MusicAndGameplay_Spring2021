using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script synchronizes looping animations to the Clock, and allows you to hot-swap the animation.
/// useful for animations that switch immediately (doesn't do anything with blending, etc)
/// 
/// Credit to Graham Tattersall for the base code --- https://www.gamasutra.com/blogs/GrahamTattersall/20190515/342454/Coding_to_the_Beat__Under_the_Hood_of_a_Rhythm_Game_in_Unity.php
/// 
/// </summary>
public class MultiAnimationSync : MonoBehaviour
{

    
    //public variables
    [Header("animation length in beats")]
    public int numBeatsInLoop;

    //This is the big difference between this and the other AnimationSync Class
    //for your own code, you probably want to be more explicit about what each animation clip represents,
    //especially if you're using it in a gameplay-significant way
    [Header("animation clips to hot-swap")]
    public AnimationClip[] animationClips;


    //private variables
    [Header("numbers for visualization purposes - do not edit")]
    [SerializeField] float beatLoopPosition;

    //number of loops completed
    [SerializeField] int numLoops;

    //our loop position on a 0-1 scale;
    //for debug purposes
    [SerializeField] float loopPositionNormalized;

    //animator variables    
    Animator animator;
    AnimatorStateInfo animatorStateInfo;
    AnimatorOverrideController aoc;

    int currentState;
    int animationClipIndex;

    private void Awake()
    {
        numLoops = 0;
        animationClipIndex = 0;
    }

    void Start()
    {
        animator = GetComponent<Animator>();

        //Get the info about the current animator state
        animatorStateInfo = animator.GetCurrentAnimatorStateInfo(0); 

        //Convert the current state name to an integer hash for identification
        currentState = animatorStateInfo.fullPathHash;
    }

    void Update()
    {

        //This is what you would want to change to make something more specific to your game; 
        //right now I'm just using the "A" key as a test input
        if (Input.GetKeyDown(KeyCode.A))
        {
            animationClipIndex += 1;
            if (animationClipIndex >= animationClips.Length)
            {
                animationClipIndex = 0;
            }

            ChangeAnimation(animationClips[animationClipIndex]);
        }

        //make sure our clock has started (Clock has a bit of a start delay)
        if (Beat.Clock.Instance.TimeMS >= 0d)
        {
            //Start playing the current animation from wherever the current beat loop is
            animator.Play(currentState, -1, GetNormalizedLoopPosition(Beat.Clock.Instance.TimeMS));
            //Set the speed to 0 so it will only change frames when you next update it
            animator.speed = 0;
        }
    }

    public float GetNormalizedLoopPosition(double timeMS)
    {
        //our Clock script doesn't intrinsically have the time in beats, so we need to do a bit of conversion
        //note that this assumes that timeMS starts at the same time as our song!  make sure your song file has no silence at the beginning.

        float timeInBeats = (float)timeMS * 0.001f / Beat.Clock.Instance.BeatLength();

        //update our loop if we get to the end of it
        if (timeInBeats > (numLoops + 1) * numBeatsInLoop)
        {
            numLoops++;
        }

        //update our loop position (in number of beats
        beatLoopPosition = timeInBeats - (numLoops * numBeatsInLoop);

        //then normalize beatLoopPosition on a 0-1 scale
        loopPositionNormalized = beatLoopPosition / numBeatsInLoop;

        return loopPositionNormalized;

    }

    public void ChangeAnimation (AnimationClip aClip)
    {
        //Load the animator attached to this object
        animator = GetComponent<Animator>();

        //afaik this is the only current way to make animator controllers at runtime 
        //(basically swap clips at runtime)
        aoc = new AnimatorOverrideController(animator.runtimeAnimatorController);
        var anims = new List<KeyValuePair<AnimationClip, AnimationClip>>();
        foreach (var a in aoc.animationClips)
            anims.Add(new KeyValuePair<AnimationClip, AnimationClip>(a, aClip));
        aoc.ApplyOverrides(anims);

        animator.runtimeAnimatorController = aoc;
    }
}
