using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[System.Serializable]
public class Head
{
    public int PPQ;
    public int[] timeSignature;
    public float bpm;
}

[System.Serializable]
public class MusicNote
{
    public string name;
    public int midi;
    public float time;
    public float gameTime;
    public float velocity;
    public float duration;
    public float beatTime;
    public GameObject go;
    public Beat.MBT mbtValue;
}

[System.Serializable]
public class Track
{
    public float startTime;
    public float duration;
    public int length;
    public int id;
    public string name;
    public int channelNumber;
    public bool isPercussion;
    public MusicNote[] notes;
}

[System.Serializable]
public class Song
{
    public Head header;
    public float startTime;
    public float duration;
    public Track[] tracks;
}

[System.Serializable]
public class SongInfo
{
    public string midiFileName;
    public string songName;
    public float arcStartPos;

    public string[] noteMap;
    public string bgMusicName;
    public string correctSFXName;
    public string wrongSFXName;
    public float manuallyDelay;
    public int failIncrease;
    public float playerInitAngle;
    public float playerEndAngle;
    public float playerRotateSpeed;
    public float arcAngle;
    public float arcRotateSpeed;
}

public class SongSource : MonoBehaviour
{
    public static Song song;


    public static Song getSong(string fileName)
    {
        TextAsset file = Resources.Load(fileName) as TextAsset;
        song = JsonUtility.FromJson<Song>(file.text);
        return song;
    }
}



