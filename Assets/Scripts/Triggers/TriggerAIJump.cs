using UnityEngine;
using System.Collections;

public class TriggerAIJump : MonoBehaviour {

	void OnTriggerEnter(Collider other) {
		if (other.GetComponent<BallControlAI>() != null) {
			other.GetComponent<BallControlAI>().TriggerJump();
		}
	}
}
