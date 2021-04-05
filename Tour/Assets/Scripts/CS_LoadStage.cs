using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CS_LoadStage : MonoBehaviour {
	[Header("Site")]
	[SerializeField] GameObject mySite;
	[SerializeField] int mySiteAmount;
	private int mySiteFoundAmount = 0;
	[SerializeField] Vector2 mySitePosition;
	[SerializeField] float mySiteDistance;
	private List<GameObject> mySiteList = new List<GameObject>();
	//[Space(5)]
	[Header("Station")]
	[SerializeField] GameObject myStation;
	[SerializeField] int myStationAmount;
	[SerializeField] Vector2 myStationPosition;
	[SerializeField] float myStationDistance;
	[SerializeField] GameObject myStationLine;
	private List<GameObject> myStationList = new List<GameObject>();
	private List<Vector2> myStationPositionList = new List<Vector2> ();
	[SerializeField] GameObject mySubway;
	[SerializeField] int mySubwayAmount;
	//[Space(5)]
	[Header("Tree")]
	[SerializeField] GameObject myTree;
	[SerializeField] int myTreeCenterAmount;
	[SerializeField] int myTreeAmount;
	[SerializeField] Vector2 myTreePosition;
	[SerializeField] float myTreeDistanceToOthers;
	[SerializeField] float myTreeDistanceMax;
	[SerializeField] float myTreeDistanceMin;
	private List<GameObject> myTreeList = new List<GameObject> ();
	//[Space(5)]
	[Header("Friend")]
	[SerializeField] GameObject myFriend;
	[SerializeField] int myFriendAmount;
	[SerializeField] Vector2 myFriendPosition;
	[SerializeField] float myFriendDistance;
	private List<GameObject> myFriendList = new List<GameObject>();
	//[Space(5)]
	[Header("Worker")]
	[SerializeField] GameObject myWorker;
	[SerializeField] int myWorkerAmount;
	[SerializeField] Vector2 myWorkerPosition;
	private List<GameObject> myWorkerList = new List<GameObject>();
	private List<Vector2> myWorkerTargetList = new List<Vector2>();


	[Header("Others")]
	[SerializeField] GameObject TX_Timer;
	[SerializeField] GameObject TX_Score;
	[SerializeField] GameObject TX_Reset;
	private float Timer = 0;
	private float Score = 0;
	
	private float _scoreTotal = 0;
	public float ScoreTotal {
		get { return _scoreTotal; }
	}

	public float gameEndingScore = 500f;


	[SerializeField] GameObject UI_Pause;

	// Use this for initialization
	void Start () {
		InitSite ();
		InitStation ();
		InitTree ();
		InitFriend ();
		InitWorker ();

		HideInfo ();
		HidePause ();
		
		CS_AudioManager.Instance.StartGame();
	
	}

	private void InitSite () {
		for (int time = 0; time < 1000; time++) {
			if (mySiteList.Count < mySiteAmount) {
				//creat a position
				Vector2 t_position = new Vector2 (
					                     Random.Range (-mySitePosition.x, mySitePosition.x), 
					                     Random.Range (-mySitePosition.y, mySitePosition.y)
				                     );
				//check if it's at a good position
				bool isGood = true;
				isGood = CheckIfFarEnough (t_position, mySiteList, mySiteDistance);

				if (isGood) {
					GameObject t_site = Instantiate (mySite, t_position, Quaternion.identity) as GameObject;
					mySiteList.Add (t_site);
				}
			} else {
				Debug.Log ("InitSites times : " + time);
				break;
			}
		}
	}

	private void InitStation () {
		for (int time = 0; time < 1000; time++) {
			if (myStationList.Count < myStationAmount) {
				//creat a position
				Vector2 t_position = new Vector2 (
					Random.Range (-myStationPosition.x, myStationPosition.x), 
					Random.Range (-myStationPosition.y, myStationPosition.y)
				);
				//check if it's at a good position
				bool isGood = true;
				//check if it's too close to sites
				isGood = CheckIfFarEnough (t_position, mySiteList, myStationDistance);

				//check if it's too close to other stations
				if (isGood)
					isGood = CheckIfFarEnough (t_position, myStationList, myStationDistance);

				if (isGood)
					isGood = CheckIfFarEnoughLine (t_position, myStationList, myStationDistance);

				if (isGood) {
					GameObject t_station = Instantiate (myStation, t_position, Quaternion.identity) as GameObject;
					myStationList.Add (t_station);
				}
			} else {
				Debug.Log ("InitStation times : " + time + "; Count : " + myStationList.Count);
				break;
			}
		}

		//draw line
		for (int i = 0; i < myStationList.Count; i++) {
			int t_next = i + 1;
			if (t_next >= myStationList.Count)
				t_next -= myStationList.Count;
			Vector2 t_direction = myStationList [t_next].transform.position - myStationList [i].transform.position;
			Vector2 t_position = (myStationList [t_next].transform.position + myStationList [i].transform.position) / 2;

			Quaternion t_quaternion = Quaternion.Euler (0, 0, 
				Vector2.Angle (Vector2.up, t_direction) * Vector3.Cross (Vector3.up, (Vector3)t_direction).normalized.z);
			//Debug.Log (Vector3.Cross (Vector3.up, (Vector3)t_direction));
			GameObject t_line = Instantiate (myStationLine, t_position, t_quaternion) as GameObject;
			t_line.transform.localScale = new Vector3 (t_line.transform.localScale.x, t_direction.magnitude, 1);
		}

		//place subway
		for (int i = 0; i < myStationList.Count; i++) {
			myStationPositionList.Add (myStationList [i].transform.position);
		}
		int t_stationsPerSubway = myStationPositionList.Count / mySubwayAmount; 
		for (int i = 0; i < mySubwayAmount; i++) {
			int t_lastStationNumber = i * t_stationsPerSubway;
			GameObject t_subway = Instantiate (
				                      mySubway, 
				                      myStationPositionList [t_lastStationNumber],
				                      Quaternion.identity
			                      ) as GameObject;
			t_subway.GetComponent<CS_Subway> ().Init (myStationPositionList, t_lastStationNumber);
		}
	}

	private void InitTree () {
		//create tree center
		for (int time = 0; time < 1000; time++) {
			if (myTreeList.Count < myTreeCenterAmount) {
				//creat a position
				Vector2 t_position = new Vector2 (
					                     Random.Range (-myTreePosition.x, myTreePosition.x), 
					                     Random.Range (-myTreePosition.y, myTreePosition.y)
				                     );
				//check if it's at a good position
				bool isGood = true;
				//check if it's too close to sites
				isGood = CheckIfFarEnough (t_position, mySiteList, myTreeDistanceToOthers);
				//check if it's too close to other stations
				if (isGood)
					isGood = CheckIfFarEnough (t_position, myStationList, myTreeDistanceToOthers);
				
				if (isGood)
					isGood = CheckIfFarEnoughLine (t_position, myStationList, myTreeDistanceToOthers);

				//check if it's too close to other trees
				if (isGood)
					isGood = CheckIfFarEnough (t_position, myTreeList, myTreeDistanceToOthers);

				if (isGood) {
					GameObject t_Tree = Instantiate (myTree, t_position, Quaternion.identity) as GameObject;
					myTreeList.Add (t_Tree);
				}
			} else {
				Debug.Log ("InitTree times : " + time + "; Count : " + myTreeList.Count);
				break;
			}
		}
			//Debug.Log ("InitTree times : " + time + "; Count : " + myTreeList.Count);

		//create other trees
		for (int time = 0; time < 5000; time++) {
			if (myTreeList.Count < myTreeAmount) {
				//creat a position
				Vector2 t_position = new Vector2 (
					Random.Range (-myTreePosition.x, myTreePosition.x), 
					Random.Range (-myTreePosition.y, myTreePosition.y)
				);
				//check if it's at a good position
				bool isGood = false;
				//check if it's not too far to other trees

				for (int i = 0; i < myTreeList.Count; i++) {
					if (Vector2.Distance (myTreeList [i].transform.position, t_position) < myTreeDistanceMax) {
						isGood = true;
						break;
					}
				}
				//check if it's too close to sites
				if (isGood)
					isGood = CheckIfFarEnough (t_position, mySiteList, myTreeDistanceToOthers);
				//check if it's too close to other stations
				if (isGood)
					isGood = CheckIfFarEnough (t_position, myStationList, myTreeDistanceToOthers);
				
				if (isGood)
					isGood = CheckIfFarEnoughLine (t_position, myStationList, myTreeDistanceToOthers);

				//check if it's too close to other trees
				if (isGood)
					isGood = CheckIfFarEnough (t_position, myTreeList, myTreeDistanceMin);

				if (isGood) {
					GameObject t_Tree = Instantiate (myTree, t_position, Quaternion.identity) as GameObject;
					myTreeList.Add (t_Tree);
				}
			} else {
				Debug.Log ("InitTree times : " + time + "; Count : " + myTreeList.Count);
				break;
			}
		}

	}

	private void InitFriend () {
		//create friend at each site
		for (int i = 0; i < mySiteList.Count; i++) {
			Vector2 t_position = (Vector2) mySiteList [i].transform.position + Random.insideUnitCircle * myFriendDistance;
			GameObject t_Friend = Instantiate (myFriend, t_position, Quaternion.identity) as GameObject;
			myFriendList.Add (t_Friend);
		}
			
		for (int time = 0; time < 1000; time++) {
			if (myFriendList.Count < myFriendAmount) {
				//creat a position
				Vector2 t_position = new Vector2 (
					Random.Range (-myFriendPosition.x, myFriendPosition.x), 
					Random.Range (-myFriendPosition.y, myFriendPosition.y)
				);
				//check if it's at a good position
				bool isGood = true;
				//check if it's too close to other friends
				isGood = CheckIfFarEnough (t_position, myFriendList, myFriendDistance);

				//check if it's too close to other stations
				if (isGood)
					isGood = CheckIfFarEnough (t_position, myStationList, myFriendDistance);

				if (isGood)
					isGood = CheckIfFarEnoughLine (t_position, myStationList, myFriendDistance);

				//check if it's too close to other trees
				if (isGood)
					isGood = CheckIfFarEnough (t_position, myTreeList, myFriendDistance);

				if (isGood) {
					GameObject t_station = Instantiate (myFriend, t_position, Quaternion.identity) as GameObject;
					myFriendList.Add (t_station);
				}
			} else {
				Debug.Log ("InitStation times : " + time + "; Count : " + myFriendList.Count);
				break;
			}
		}
	}

	private void InitWorker () {
		//create wokers target
		for (int i = 0; i < mySiteList.Count; i++) {
			myWorkerTargetList.Add ((Vector2)mySiteList [i].transform.position);
		}

		//create workers everywhere
		for (int i = 0; i < myWorkerAmount;  i++) {
			Vector2 t_position = new Vector2 (
				Random.Range (-myWorkerPosition.x, myWorkerPosition.x), 
				Random.Range (-myWorkerPosition.y, myWorkerPosition.y)
			);
			GameObject t_worker = Instantiate (myWorker, t_position, Quaternion.identity) as GameObject;
			t_worker.GetComponent<CS_Worker> ().InitMyTargetList (myWorkerTargetList);
			t_worker.GetComponent<CS_Worker> ().InitMyTargetList (myWorkerTargetList);
			myWorkerList.Add (t_worker);
		}
	}

	private bool CheckIfFarEnough (Vector2 g_position, List<GameObject> g_list, float g_distance) {
		for (int i = 0; i < g_list.Count; i++) {
			if (Vector2.Distance (g_list [i].transform.position, g_position) < g_distance) {
				return false;
			}
		}
		return true;
	}

	private bool CheckIfFarEnoughLine (Vector2 g_position, List<GameObject> g_list, float g_distance) {
		if (g_list.Count < 2)
			return true;
		
		for (int i = 0; i < g_list.Count; i++) {
			int t_numb = i + 1;
			if (t_numb >= g_list.Count)
				t_numb -= g_list.Count;
			if (CS_Global.DistanceToLine(g_list[i].transform.position, g_list[t_numb].transform.position, g_position) < g_distance) {
				return false;
			}
		}
		return true;
	}
	// Update is called once per frame


	void Update () {

		Timer += Time.deltaTime;

		if (Score - Timer > _scoreTotal){
			_scoreTotal = Score - Timer;
			TX_Score.GetComponent<TextMesh> ().text = _scoreTotal.ToString ("0");
		}
	}

	public void FindASite () {
		mySiteFoundAmount++;

		//this.GetComponent<CS_PlaySFX> ().PlaySFX (mySiteFoundAmount - 1);
		CS_AudioManager.Instance.PlayActivateSiteSound();

		float t_size;
		if (mySiteFoundAmount >= mySiteAmount) {
			//GAME OVER
			
			t_size = CS_Global.CAMERA_SIZE_MAX;
			Camera.main.GetComponent<CS_Camera> ().SetIsClear (true);

			float t_min = Timer / 60;
			float t_sec = Timer % 60;
			TX_Timer.GetComponent<TextMesh> ().text = t_min.ToString ("0") + ":" + t_sec.ToString ("00");
			ShowInfo ();
			CS_AudioManager.Instance.EndGameSound();
		}
		else
			t_size =CS_Global.CAMERA_SIZE_DEFAULT + (CS_Global.CAMERA_SIZE_MAX - CS_Global.CAMERA_SIZE_DEFAULT) * 0.5f * mySiteFoundAmount / mySiteAmount;
		Camera.main.GetComponent<CS_Camera> ().SetSize (t_size);
	}

	//TODO - set up scores for friends, trees and stations
	public void FindAFriend() {
		
	}

	public void FindATree() {
		
	}

	public void FindAStation() {
		
	}

	public void ShowInfo () {
		TX_Timer.SetActive (true);
		TX_Score.SetActive (true);
		
	}

	public void ShowEndingInfo() {
		TX_Reset.SetActive(true);
	}

	public void HideInfo () {
		TX_Timer.SetActive (false);
		TX_Score.SetActive (false);
	}

	public void ShowPause () {
		UI_Pause.SetActive (true);
		Time.timeScale = 0;
	}

	public void HidePause () {
		UI_Pause.SetActive (false);
		Time.timeScale = 1;
	}

	public void AddScore (int g_score) {
		Debug.Log("Added Score: " + g_score);
		Score += g_score;
		CS_AudioManager.Instance.gameScore = Score;
	}
}
