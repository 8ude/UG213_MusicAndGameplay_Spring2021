using UnityEngine;
using System.Collections;

public class CS_Animator : MonoBehaviour {
	[SerializeField] Animator myAnimator;

	public void DestroyMyAnimator () {
		Destroy (myAnimator);
		Destroy (this);
	}
}
