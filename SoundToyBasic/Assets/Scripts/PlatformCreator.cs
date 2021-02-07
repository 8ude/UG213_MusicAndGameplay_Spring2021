using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This Script creates platforms by clicking once for the first point,
/// dragging the mouse, and releasing for the second point.
/// I -believe- this will work on mobile, though I am not completely certain. 
/// </summary>
public class PlatformCreator : MonoBehaviour {

    //PUBLIC VARIABLES - things to manipulate in the inspector
    
    //a reference to our platform prefab, which also has our audio samples on it
    //modify this prefab in the Prefabs/2dBouncePlatformModel
	public GameObject platformModel;
    
    //the minimum width of our platforms, so we don't get weird behaviors with invisible platforms
    //or tiny colliders
    public float minWidth = 0.1f;
    
    //a list that contains all the platforms on the screen
    //note - all the children of the InitialPlatforms game object should be in this
    //list at start, otherwise the player wont be able to clear them.
   
    //In fact, you might just want to delete these, as changes to the platformModel won't affect
    //the initial platforms
    public List<GameObject> allPlatforms = new List<GameObject>();

    //Clear and undo keys, which you can change if you want
    public KeyCode clearKey = KeyCode.C;
    public KeyCode undoKey = KeyCode.U;
    
    
    //------
    //new variables - controlling synth and collision variables
    //------
    
    //setting this lower (~10) results in higher octaves for the synth
    [Range(5f, 60f)]
    public float platformMaxVelocity;

    public float platformSynthRelease;

    //Rhythmic subdivision for the newly created platform
    public Beat.TickValue platformRhymicValue;

    
    //PRIVATE VARIABLES --  which will change during runtime
    GameObject currentPlatform;
    Vector2 startPosition;

    /// <summary>
    /// When the player clicks, we set the first point of the platform
    /// we call it every frame, but it only runs on the frame that the player clicks the mouse
    /// </summary>
    /// <param name="position">the position of the mouse, in "world" coordinates</param>
    void TryInitialize(Vector2 position)
    {
        if (Input.GetMouseButtonDown(0))
        {
            //set the start position as the input position
            startPosition = position;

            //instantiate the prefab, and set it as our "currentPlatform" so we
            //don't make more than one at a time
            currentPlatform = Instantiate(platformModel);

            //NEW ADDITION - for rhythmic version.
            //This sets the max speed and platform rhythm according to spawner values
            if (currentPlatform.GetComponent<SynthRhythmBounce>()) {
                SynthRhythmBounce rhythmBounce = currentPlatform.GetComponent<SynthRhythmBounce>();
                currentPlatform.GetComponent<pxStrax>().release = platformSynthRelease;
                rhythmBounce.platformRhythm = platformRhymicValue;
                rhythmBounce.maxSpeed = platformMaxVelocity;

            }
            //set the prefab's position accordingly
            currentPlatform.transform.position = startPosition;
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
        currentPlatform.transform.position = center;
        currentPlatform.transform.right = delta;

        //now we need to scale the platform according to 
        //the difference between our current position and our start position...
        Vector3 localScale = currentPlatform.transform.localScale;
        
        //...but we only want to scale the "x" value, so that the platform
        //stays the same thickness
        localScale.x = delta.magnitude;
        currentPlatform.transform.localScale = localScale;
    }

    /// <summary>
    /// Destroys all the platforms in the scene and clears the list of allPlatforms
    /// </summary>
    void ClearPlatforms()
    {
        foreach (GameObject platform in allPlatforms)
            Destroy(platform);

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

    //Update is called every frame
    void Update()
    {
        //our mouse position starts in "screen" space, so we need to convert it into
        //"world" space in order for it to be useful
        Vector3 position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        
        //now with our mouse position, we run our methods for
        //beginning, updating, and releasing platforms
        TryInitialize(position);
        TryUpdate(position);
        TryRelease(position);

        //check if the player pressed the "clear" or "undo" keys and run the appropriate method
        if (Input.GetKeyDown(clearKey))
            ClearPlatforms();
        if (Input.GetKeyDown(undoKey))
            UndoPlatform();
        
        //adding things to change platform pitch range
        if (Input.GetKey(KeyCode.Period))
            IncreasePlatformMaxSpeed();
        if (Input.GetKey(KeyCode.Comma))
            DecreasePlatformMaxSpeed();
    }

    void IncreasePlatformMaxSpeed() {
        float newPlatformMaxSpeed = platformMaxVelocity + Time.deltaTime;

        platformMaxVelocity = Mathf.Clamp(newPlatformMaxSpeed, 5f, 60f);
    }
    void DecreasePlatformMaxSpeed() {
        float newPlatformMaxSpeed = platformMaxVelocity - Time.deltaTime;

        platformMaxVelocity = Mathf.Clamp(newPlatformMaxSpeed, 5f, 60f);
    }
    
    
}
