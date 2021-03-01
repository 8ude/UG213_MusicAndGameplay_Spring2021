using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//pretty simple - MIDI note connected to Gameplay Input

[System.Serializable]
public class BeatmapPair
{
    public string midiNote;
    public string unityInput;
}

/// <summary>
/// This functions as table to associate MIDINotes with Unity Inputs, to parse our MIDI files
/// </summary>
[CreateAssetMenu(menuName = "Beatmap/BeatmapParser")]
public class BeatmapParserKey : ScriptableObject
{
    public BeatmapPair[] beatmapPairs;
    
}
