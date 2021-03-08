using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class NoteHighwayWwiseSync : MonoBehaviour
{
    public AK.Wwise.Event noteHighwayWwiseEvent;

    [HideInInspector] public float secondsPerBeat;

    //Unity events are pretty flexible - you've already used them if you've used UI buttons

    //they are also easy to extend if you ever need to pass arguments -- https://docs.unity3d.com/ScriptReference/Events.UnityEvent_1.html

    public UnityEvent OnR;
    public UnityEvent OnG;
    public UnityEvent OnB;

    public UnityEvent OnLevelEnded;

    public UnityEvent OnEveryGrid;
    public UnityEvent OnEveryBeat;
    public UnityEvent OnEveryBar;


    //id of the wwise event - using this to get the playback position
    uint playingID;

    void Start()
    {

        //most of the time in wwise you just post events and attach them to game objects, 
        
        playingID = noteHighwayWwiseEvent.Post(gameObject,

            //but this is a bit different
            //we want to use a callback - allowing us to set up communication between one system (wwise) and another (unity)

            //wwise gives you the option of sending messages on musically significant times - beats, bars, etc

            (uint)(AkCallbackType.AK_MusicSyncAll
            //we use a bitwise operator - the single | - because there are are a couple things we want communicated.  

            |

            //we also want to get accurate playback position (my tests show it's usually within 5 ms, sometimes as high as 30 ms), which requires a callback as well.
            AkCallbackType.AK_EnableGetMusicPlayPosition), 
            
            //this is the function we define in code, which will fire whenever we get wwise music events
            MusicCallbackFunction);


      

        
    }

    //the music callback gets fed some information from the wwise engine
    void MusicCallbackFunction(object in_cookie, AkCallbackType in_type, AkCallbackInfo in_info)
    {

        //we only want music-specific information, so we cast this info accordingly
        AkMusicSyncCallbackInfo _musicInfo = (AkMusicSyncCallbackInfo)in_info;

        //we're going to use this switchboard to fire off different events depending on wwise sends
        switch (_musicInfo.musicSyncType)
        {
            case AkCallbackType.AK_MusicSyncUserCue:

                CustomCues(_musicInfo.userCueName, _musicInfo);

                secondsPerBeat = _musicInfo.segmentInfo_fBeatDuration;

                break;
            case AkCallbackType.AK_MusicSyncBeat:


                OnEveryBeat.Invoke();
                break;
            case AkCallbackType.AK_MusicSyncBar:
                //I want to make sure that the secondsPerBeat is defined on our first measure.
                secondsPerBeat = _musicInfo.segmentInfo_fBeatDuration;
                Debug.Log("Seconds Per Beat: " + secondsPerBeat);

                OnEveryBar.Invoke();
                break;

            case AkCallbackType.AK_MusicSyncGrid:
                //the grid is defined in Wwise - usually on your playlist.  It can be as small as a 32nd note

                OnEveryGrid.Invoke();
                break;
            default:
                break;


        }


            
    }




    public void CustomCues(string cueName, AkMusicSyncCallbackInfo _musicInfo)
    {
        switch (cueName)
        {

            case "R":
                OnR.Invoke();
                break;
            case "G":
                OnG.Invoke();
                break;
            case "B":
                OnB.Invoke();
                break;
            case "LevelEnded":
                OnLevelEnded.Invoke();
                break;

            case "A":
                //put an A function Here 
                Debug.Log("A stuff");
                break;
            default:
                break;

        }
    }

    //this is pretty straightforward - get the elapsed time
    public int GetMusicTimeInMS()
    {

        AkSegmentInfo segmentInfo = new AkSegmentInfo();

        AkSoundEngine.GetPlayingSegmentInfo(playingID, segmentInfo, true);

        return segmentInfo.iCurrentPosition;
    }

    //We're going to call this when we spawn a gem, in order to determine when it's crossing time should be
    //based on the current playback position, our beat duration, and our beat offset
    public int SetCrossingTimeInMS(int beatOffset)
    {
        AkSegmentInfo segmentInfo = new AkSegmentInfo();

        AkSoundEngine.GetPlayingSegmentInfo(playingID, segmentInfo, true);

        int offsetTime = Mathf.RoundToInt(1000 * secondsPerBeat * beatOffset);

        Debug.Log("setting time: " + segmentInfo.iCurrentPosition + offsetTime);

        return segmentInfo.iCurrentPosition + offsetTime;
    }


}
