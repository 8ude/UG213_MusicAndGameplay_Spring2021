using UnityEngine;
using System.Collections;

public class CS_PreView : MonoBehaviour {
	[SerializeField] GameObject myObject;

	void OnTriggerEnter2D (Collider2D other) {
		Debug.Log ("OnTriggerEnter");
		if (other.tag == CS_Global.TAG_PLAYER || other.tag == CS_Global.TAG_FRIEND) {

			switch (myObject.name) {
				case "Site": {
					CS_AudioManager.Instance.PlayActivateSiteSound();
					break;
				}
				case "Friend": {
					CS_AudioManager.Instance.PlayActivateFriendSound();
					break;
				}
				case "Station": {
					CS_AudioManager.Instance.PlayActivateStationSound();
					break;
				}
				case "Tree": {
					CS_AudioManager.Instance.PlayActivateTreeSound();
					break;
				}
				default: {
					Debug.Log("no sounds set up for this object");
					break;
				}
				
				
			}
			
			Instantiate (myObject, this.transform.position, Quaternion.identity);
			Destroy (this.gameObject);
		}
	}
}
