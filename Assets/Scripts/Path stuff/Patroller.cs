using UnityEngine;
using System.Collections;

[RequireComponent (typeof (PathFollower))]

public class Patroller : MonoBehaviour {
	
	public int speed;
	public bool loop;
	public GameObject target;
	bool isMoving = false;

	// Use this for initialization
	void FixedUpdate () {
		if (isMoving) {
			Move ();
		}
	}
	
	// Update is called once per frame
	void Move () {
		PathFollower pFollower = GetComponent<PathFollower>();
		pFollower.Move(speed);
		if (pFollower.position>=1) {
			if (loop) {pFollower.position=0;} else {speed=0;}
		}
	}
	
	void OnTriggerStay(Collider other) {
		if (other.gameObject == target) {
			isMoving = true;
		}
	}
	void OnTriggerExit(Collider other) {
		isMoving = false;
	}
}
