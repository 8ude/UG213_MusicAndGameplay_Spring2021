using UnityEngine;
using System.Collections;

public class CS_Button : MonoBehaviour {
	[SerializeField] GameObject myTargetGameObject;
	[SerializeField] string myTargetFunction;
	[SerializeField] string myMessage;

	void OnMouseDown () {
		if (myTargetGameObject == null)
			myTargetGameObject = CS_MessageBox.Instance.gameObject;
		
		if (myMessage == "")
			myTargetGameObject.SendMessage (myTargetFunction);
		else
			myTargetGameObject.SendMessage (myTargetFunction, myMessage);
	}
}
