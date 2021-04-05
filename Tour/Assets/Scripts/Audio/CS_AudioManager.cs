
using UnityEngine;
using System.Collections;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class CS_AudioManager : MonoBehaviour {
	
	private static CS_AudioManager instance = null; 
	
	//we need an SFX Prefab - these will be instantiated for the purpose of playing sounds
	[SerializeField] GameObject myPrefabSFX;

	//we also need two audio sources for game music and menu music
	[Header("by default, increases in volume relative to score")]
	[SerializeField] AudioSource gameMusicAudioSource;
	[Header("main menu music")]
	[SerializeField] AudioSource menuMusicAudioSource;

	//this is where our game SFX are going to live
	
	//clips that play when we activate different objects
	[Header("Activation Sounds")]
	public AudioClip[] activateTreeSounds;
	public AudioClip[] activateSiteSounds;
	public AudioClip[] activateFriendSounds;
	public AudioClip[] activateStationSounds;
	
	//loops that play when objects are moving around 
	[Header("Movement Loops")]
	public AudioClip[] friendFlyingSounds;
	public AudioClip playerMoveSound;
	
	//clip that plays every time a train moves
	[Header("Train Moving Sound")]
	public AudioClip rideTrainSound;
	
	//clip that plays when the game ends
	[Header("Game State Notifications")]
	public AudioClip gameEndSound;
	public AudioClip startGameSound;
	
	//our audio mixer groups, which we are routing our sfx to
	[Header("Mixer Groups")] 
	public AudioMixerGroup treeSoundGroup;
	public AudioMixerGroup siteSoundGroup,
		friendSoundGroup,
		stationSoundGroup,
		rideTrainGroup,
		gameEndGroup,
		menuMusicGroup,
		gameMusicGroup;

	//Mixer snapshots let us crossfade easily between game states.
	//We can also add weights to multiple snapshots in order to blend them.
	[Header("Mixer Snapshots")] 
	public AudioMixerSnapshot menuMixerSnapshot;
	public AudioMixerSnapshot gameMixerSnapshot;

	private int numTreesFound;

	public float gameScore;

	//========================================================================
	//Singleton Pattern
	public static CS_AudioManager Instance {
		get { 
			return instance;
		}
	}

	void Awake () {
		if (instance != null && instance != this) {
			Destroy(this.gameObject);
		} else {
			instance = this;
		}

		DontDestroyOnLoad(this.gameObject);

		if (SceneManager.GetActiveScene().name == "Menu") {
			StartMenu();
		}

		numTreesFound = 0;
		gameScore = 0f;
	}
	//========================================================================

	
	//In tour, once we activate a site, it illuminates and plays a tone.
	//This is currently set up so that we randomly select a sound effect from the corresponding array,
	//Then route it to the appropriate group in the mixer
	public void PlayActivateSiteSound() {
		//Plays a random site sound when we find a site (one of the large circles)
		
		//First find a clip randomly from the array
		AudioClip randomActivateSiteClip = activateSiteSounds[Random.Range(0, activateSiteSounds.Length - 1)];
		
		//Then we play this clip - note that nothing is changing for panning and volume is set at 1.0
		PlaySFX(randomActivateSiteClip, 1.0f, 0f, siteSoundGroup);
	}
	
	public void PlayActivateFriendSound() {
		
		AudioClip randomActivateSiteClip = activateFriendSounds[Random.Range(0, activateFriendSounds.Length - 1)];
		PlaySFX(randomActivateSiteClip, 1.0f, 0f, friendSoundGroup);
		
	}
	
	public void PlayActivateStationSound() {

		AudioClip randomActivateSiteClip = activateStationSounds[Random.Range(0, activateStationSounds.Length - 1)];
		PlaySFX(randomActivateSiteClip, 1.0f, 0f, stationSoundGroup);
	}
	
	public void PlayActivateTreeSound() {
		
		
		AudioClip randomActivateSiteClip = activateTreeSounds[Random.Range(0, activateTreeSounds.Length - 1)];
		PlaySFX(randomActivateSiteClip, 1.0f, 0f, treeSoundGroup);

		
		//////////////////////////////////////////////////////////////////////////////////////////
		/// I added a counter in order to map a mixer parameter onto the number of trees found ///
        /// UnComment these to hear what it does 
        
		//numTreesFound++;
		
		//menuMusicGroup.audioMixer.SetFloat("EchoWetmix",
			//RemapFloat(Mathf.Clamp(numTreesFound, 0f, 50f), 0f, 50f, 0f, 1f));
	}
	
	//========================================================================

	private void Update() {
		//right now, the main game music is connected to score.

		//you can connect other stems, music tracks, etc to musical parameters using this same pattern
		//check the LoadStage game object for the max number of different objects, 
		//then set up counters corresponding to trees, stations, in the methods above 

		float newMusicVolume = RemapFloat(Mathf.Clamp(gameScore, 0f, 500f), 0f, 500f, 0f, 1f);
		gameMusicAudioSource.volume = newMusicVolume;
	}
	
	
	//These sounds are for beginning and ending the game

	public void EndGameSound() {
		if (gameEndSound != null) {
			PlaySFX(gameEndSound, 1.0f, 0f, gameEndGroup);
		}
	}

	public void StartGameSound() {
		if (startGameSound != null) {
			PlaySFX(startGameSound, 1.0f, 0f, gameEndGroup);
		}
	}
	
	//========================================================================
	
	//this one is set up to play every time a train leaves the station.
	//you may need to fiddle with the train spatialization settings so it isn't too cacophanous 
	
	public void PlayRideTrainSound() {
		if (rideTrainSound != null) {
			PlaySFX(rideTrainSound, 1.0f, 0f, rideTrainGroup);
		}
	}

	
	
	//This is a general method to instantiate our SFX prefab with the settings that we want, then destroy it when it's
	//done playing
	public void PlaySFX (AudioClip g_SFX, float g_Volume, float g_Pan, AudioMixerGroup g_destGroup) {
		GameObject t_SFX = Instantiate (myPrefabSFX) as GameObject;
		t_SFX.name = "SFX_" + g_SFX.name;
		t_SFX.GetComponent<AudioSource> ().clip = g_SFX;
		t_SFX.GetComponent<AudioSource> ().volume = g_Volume;
		t_SFX.GetComponent<AudioSource> ().panStereo = g_Pan;
		t_SFX.GetComponent<AudioSource> ().outputAudioMixerGroup = g_destGroup;
		t_SFX.GetComponent<AudioSource> ().Play ();
		DestroyObject(t_SFX, g_SFX.length);
	}


	/// <summary>
	/// This is called when we enter the menu scene.
	/// Provided that we've set up our snapshots appropriately, we will also crossfade between music/ambience sources.
	/// </summary>
	public void StartMenu() {
		if (menuMusicAudioSource != null && !menuMusicAudioSource.isPlaying) {
			Debug.Log("Play Menu Music");
			menuMusicAudioSource.outputAudioMixerGroup = menuMusicGroup;
			menuMusicAudioSource.Play();
		}

		if (menuMixerSnapshot != null) {
			menuMixerSnapshot.TransitionTo(1.0f);
		}
	}
	
	/// <summary>
	/// This is called when we enter the game scene
	/// </summary>
	public void StartGame() {
		if (gameMusicAudioSource != null && !gameMusicAudioSource.isPlaying) {
			Debug.Log("Play Game Music");
			gameMusicAudioSource.outputAudioMixerGroup = gameMusicGroup;
			gameMusicAudioSource.Play();
		}

		if (gameMixerSnapshot != null) {
			gameMixerSnapshot.TransitionTo(1.0f);
		}
		
	}
	
	

	
	
	//I've included a remap float function, which might be useful if you want to scale audio values based
	//on arbitrary gameplay ranges
	public static float RemapFloat (float inputValue, float inputLow, float inputHigh, float outputLow, float outputHigh) {
		return (inputValue - inputLow) / (inputHigh - inputLow) * (outputHigh - outputLow) + outputLow;
	}
	
}
