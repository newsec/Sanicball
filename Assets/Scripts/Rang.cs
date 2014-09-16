using UnityEngine;
using System.Collections;

[RequireComponent (typeof (AudioSource))]

public class Rang : MonoBehaviour {
	
	public bool isGrounded;
	public bool respawn;
	
	int timer;
	
	void Start() {
		if (isGrounded) {
			RaycastHit hit;
			if (Physics.Raycast(transform.position,Vector3.down,out hit)) {
				transform.position = new Vector3(hit.point.x,hit.point.y+collider.bounds.extents.y-1f,hit.point.z);
			}
		}
	}
	
	void OnTriggerEnter (Collider other) {
		if (other.GetComponent<Racer>() != null) {
			//other.GetComponent<Racer>().Rangs++;
			GetComponent<AudioSource>().Play();
			renderer.enabled = false;
			collider.enabled = false;
			if (respawn) {
				timer = 100;
			}
		}
	}

	void FixedUpdate() {
		transform.Rotate(Vector3.up*5);
		if (timer>0) {
			timer--;
			if (timer <= 0) {
				renderer.enabled = true;
				collider.enabled = true;
			}
		}
	}
	
	void OnDrawGizmos() {
		if (isGrounded) {
			Gizmos.DrawRay(transform.position,Vector3.down);
		}
	}
}
