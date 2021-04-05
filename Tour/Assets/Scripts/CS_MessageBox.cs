using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class CS_MessageBox : MonoBehaviour {

	private static CS_MessageBox instance = null;

	//========================================================================
	public static CS_MessageBox Instance {
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
	}
	//========================================================================

	public void LoadScene (string g_scene) {
		Time.timeScale = 1;
		if (g_scene == "Menu") {
			CS_AudioManager.Instance.StartMenu();
		}
		SceneManager.LoadScene (g_scene);
	}
}
