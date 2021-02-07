using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Mime;
using System.Linq;
using Beat;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

/// <summary>
/// This Script creates platforms by clicking once for the first point,
/// dragging the mouse, and releasing for the second point.
///
/// The Extended version also has platform prefabs that the player can cycle through,
/// in order to have different synth settings, etc
///
/// </summary>
public class ExtendedPlatformCreator : MonoBehaviour {

    //PUBLIC VARIABLES - things to manipulate in the inspector
   
    //the minimum width of our platforms, so we don't get weird behaviors with invisible platforms
    //or tiny colliders
    public float minWidth = 0.1f;
    
    //a list that contains all the platforms on the screen
    //note - all the children of the InitialPlatforms game object should be in this
    //list at start, otherwise the player wont be able to clear them.
   
    //In fact, you might just want to delete these initial game objects,
    //as changes to the platformModel won't affect
    //the initial platforms
    public List<Transform> allPlatforms = new List<Transform>();

    //Clear and undo keys, which you can change if you want
    public KeyCode clearKey = KeyCode.C;
    public KeyCode undoKey = KeyCode.U;

    public GameObject instructions;
    
    
    //------
    //NEW VARIABLES - controlling synth and collision variables
    //------
    
    //setting this lower (~10) results in higher octaves for the synth
    [Range(5f, 60f)]
    public float platformMaxVelocity;

    public bool quantizeNotes;

    //Rhythmic subdivision for the newly created platform
    public Beat.TickValue platformRhythmicValue;

    [Header("Each platform prefab has different settings")]
    public GameObject[] platformPrefabs;
    [Header("Connect to the PlatformPrefabColor object under Instructions")]
    public Image platformPrefabColor;

    [Header("Connect to the QuantizeDuration object under Instructions")]
    public Text quantizeDuration;
    

    
    //PRIVATE VARIABLES which will change during runtime
    private Transform currentPlatform;
    private Vector2 startPosition;

    private bool hoverBallSpawner = false;
    private int platformPrefabIndex = 0;
    
    private Transform platformModel;

    void Start() 
    {
        platformModel = platformPrefabs[0].transform;
        
        platformPrefabColor.color = platformModel.GetComponent<Renderer>().sharedMaterial.color;
        
    }

    void MapLengthToSomething(float platformLength)
    {
        //this is called right after a platform is created
        //right now the length of platforms isn't mapped to anything.  
        
        //one idea:
        //mapping the length to the filter on the synth:
        //if(currentPlatform.GetComponent<RhythmBounce>())
        //{
        //    currentPlatform.gameObject.GetComponent<RhythmBounce>().straxSynth.cutoff = MathUtil.Map(platformLength, 0, 20f, 0.3f, 1f);
        //}
        
    }

    //Update is called every frame
    void Update()
    {   
        ProcessKeyboardInputs();
        ProcessMouseInputs();
        
    }

    void ProcessMouseInputs() {
        //our mouse position starts in "screen" space, so we need to convert it into
        //"world" space in order for it to be useful
        Vector3 position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                
        //now with our mouse position, we run our methods for
        //beginning, updating, and releasing platforms
        TryInitialize(position);
        TryUpdate(position);
        TryRelease(position);
    }
    
    void ProcessKeyboardInputs() {
        //check if the player pressed the "clear" or "undo" keys and run the appropriate method
        if (Input.GetKeyDown(clearKey))
            ClearPlatforms();
        if (Input.GetKeyDown(undoKey))
            UndoPlatform();
        
        //Hide Instructions
        if(Input.GetKeyDown(KeyCode.X))
            instructions.SetActive(!instructions.activeSelf);
        
        //Cycle Through Platform Prefabs
        if (Input.GetKeyDown(KeyCode.Tab))
            ChangePlatformPrefabIndex();
        
        //adding things to change platform pitch range
        if (Input.GetKey(KeyCode.Period))
            DecreasePlatformMaxSpeed();
        if (Input.GetKey(KeyCode.Comma))
            IncreasePlatformMaxSpeed();
        
        //change rhythmic value of platform
        if (Input.GetKeyDown(KeyCode.LeftBracket))
            DecreaseQuantizeDuration();
        if (Input.GetKeyDown(KeyCode.RightBracket))
            IncreaseQuantizeDuration();
    }
    
    
    #region KEYBOARD INPUTS
    /// <summary>
    /// Destroys all the platforms in the scene and clears the list of allPlatforms
    /// </summary>
    void ClearPlatforms()
    {
        foreach (Transform platform in allPlatforms)
            Destroy(platform.gameObject);

        allPlatforms.Clear();
    }

    /// <summary>
    /// this should destroy the last platform created
    /// </summary>
    void UndoPlatform()
    {
        //don't do anything if our list is empty
        if (allPlatforms.Count == 0)
            return;

        //the last platform created will be at the end of the allPlatforms list
        //since list indices start at 0 (just like arrays), the index that we
        //need is going to be one less than the length ("Count") of the list
        int index = allPlatforms.Count - 1;
        
        //destroy the platform and remove it from the list
        Destroy(allPlatforms[index].gameObject);
        allPlatforms.RemoveAt(index);
    }

    /// <summary>
    /// created this to increase the max velocity clamp of the platform, which usually
    /// results in a lower pitch
    /// </summary>
    void IncreasePlatformMaxSpeed() {
        float newPlatformMaxSpeed = platformMaxVelocity + (Time.deltaTime * 2f);

        platformMaxVelocity = Mathf.Clamp(newPlatformMaxSpeed, 5f, 60f);
    }
    
    
    /// <summary>
    /// created this to decrease the max velocity of the platform, which usually results
    /// in a higher pitch
    /// </summary>
    void DecreasePlatformMaxSpeed() {
        float newPlatformMaxSpeed = platformMaxVelocity - (Time.deltaTime * 2f);

        platformMaxVelocity = Mathf.Clamp(newPlatformMaxSpeed, 5f, 60f);
    }

    /// <summary>
    /// Update Platform Index and set the GUI color accordingly
    /// </summary>
    void ChangePlatformPrefabIndex() {
        platformPrefabIndex++;
        platformModel = platformPrefabs[platformPrefabIndex % platformPrefabs.Length].transform;
        platformPrefabColor.color = platformModel.GetComponent<Renderer>().sharedMaterial.color;
    }

    /// <summary>
    /// increments the quantization to the next longer beat value
    /// (for example, from 32nd Note to 16th Note)
    /// </summary>
    void IncreaseQuantizeDuration() {
        if (platformRhythmicValue != Beat.TickValue.Measure) {
            platformRhythmicValue = platformRhythmicValue.Next();
        }
        if (quantizeDuration != null) {
            quantizeDuration.text = platformRhythmicValue.ToString();
        }
    }
    
    /// <summary>
    /// decrements the quantization to the next shorter beat value
    /// (for example, from 16h note to 32nd Note
    /// </summary>
    void DecreaseQuantizeDuration() {
        if (platformRhythmicValue != Beat.TickValue.ThirtySecond) {
            platformRhythmicValue = platformRhythmicValue.Previous();
        }
        if (quantizeDuration != null) {
            quantizeDuration.text = platformRhythmicValue.ToString();
        }
    }
    #endregion
    
    #region MOUSE INPUTS
    /// <summary>
    /// When the player clicks, we set the first point of the platform
    /// it's called "TryInitialize", because we call it every frame,
    /// but it doesn't do anything except on the frame that the player clicks the mouse
    /// </summary>
    /// <param name="position">the position of the mouse, in "world" coordinates</param>
    void TryInitialize(Vector2 position) {
        if (Input.GetMouseButtonDown(0) && !hoverBallSpawner)
        {
            //set the start position as the input position
            startPosition = position;
            
            //instantiate the prefab, and set it as our "currentPlatform" so we
            //don't make more than one at a time
            
            currentPlatform = Instantiate(platformModel) as Transform;

            //RhythmBounce uses synthesis - behaves a bit different than platform bounce
            if (currentPlatform.GetComponent<SynthRhythmBounce>()) {
                SynthRhythmBounce synthRhythmBounce = currentPlatform.GetComponent<SynthRhythmBounce>();
                synthRhythmBounce.useQuantization = quantizeNotes;
                synthRhythmBounce.platformRhythm = platformRhythmicValue;
                synthRhythmBounce.maxSpeed = platformMaxVelocity;
            }
            else if(currentPlatform.GetComponent<SampleBounce>())
            {
                SampleBounce sampleBounce = currentPlatform.GetComponent<SampleBounce>();
                sampleBounce.useQuantization = quantizeNotes;
                sampleBounce.platformRhythm = platformRhythmicValue;
                sampleBounce.maxSpeed = platformMaxVelocity;
            }

            //set the prefab's position accordingly
            currentPlatform.position = startPosition;
        }
    }

    /// <summary>
    /// on the frame when the player releases the mouse, we set the second point of the platform
    /// </summary>
    /// <param name="position">position of the mouse, again in "world" coordinates</param>
    void TryRelease(Vector2 position)
    {
        //only works on the frame that the mouse is released, and if there's a current platform
        if (Input.GetMouseButtonUp(0) && currentPlatform)
        {
            //some math to see if our platform is too small.  if it is, destroy it
            if ((position - startPosition).sqrMagnitude < minWidth * minWidth)
                Destroy(currentPlatform.gameObject);
            else
            //otherwise, add it to our list of "allPlatforms"...
                allPlatforms.Add(currentPlatform);
            //...and reset our currentPlatform so that we can make another one
            currentPlatform = null;
        }
    }

    /// <summary>
    /// Another function called every frame, this is a bit more math heavy in order to
    /// set the platform's position according to the startPosition and the current mouse position
    /// </summary>
    /// <param name="position">the mouse position, in "world" coordinates</param>
    void TryUpdate(Vector2 position)
    {
        //don't do anything if we haven't started making a platform
        if (currentPlatform == null)
            return;

        //difference between our current and start position
        //the delta vector "points" towards the current position
        //looks something like   (start) X ---------> X (current) 
        Vector3 delta = position - startPosition;
        //finding the center coordinate of the platform by
        //taking the average of the current and start positions
        Vector3 center = (position + startPosition) / 2.0f;
        //set the platform position at the center, and rotate it by setting 
        //it's "right" coordinate (the red arrow if you look in the inspector)
        //in the direction of the current position
        currentPlatform.position = center;
        currentPlatform.right = delta;

        //now we need to scale the platform according to 
        //the difference between our current position and our start position...
        Vector3 localScale = currentPlatform.localScale;
        
        //...but we only want to scale the "x" value, so that the platform
        //stays the same thickness
        localScale.x = delta.magnitude;

        //if we want to map our platform length to something, this is where we'd do that
        MapLengthToSomething(delta.magnitude);

        currentPlatform.localScale = localScale;
    }
    #endregion
    
    
    
}
