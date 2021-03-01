using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Beat;

/// <summary>
/// No longer relevant - everything powered from wwise
/// </summary>
[System.Serializable]
public class BeatMapEvent
{
    //a container for our Measure-Beat-Tick settings for our beatmap events
    //we will be editing these when making our beatmaps
    public MBT eventMBT;

    //Using Keyboard for now, eventually we'll use the input manager;
    [Tooltip("Make sure this matches one of your PlayerInputKeys!")]
    public KeyCode inputKey;

    public string unityInput;


    //this will be assigned at runtime;
    [HideInInspector] public double cueTime;

}




public class Beatmap : MonoBehaviour
{

}
