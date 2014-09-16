using UnityEngine;
using System.Collections;

[RequireComponent (typeof(Camera))]
public class LobbyCamera : MonoBehaviour {

	public bool zoomAtPlayer = false;

	public float zoomedFov = 20;

	Quaternion defaultRotation;
	float defaultFov;
	
	// Use this for initialization
	void Start () {
		defaultRotation = transform.rotation;
		defaultFov = camera.fieldOfView;
	}
	
	// Update is called once per frame
	void Update () {
		float speed = 2.5f*Time.deltaTime;
		if (zoomAtPlayer) {
			BallControlInput target = FindObjectOfType<BallControlInput>();//Try to find the player
			if (target != null) {
				Vector3 relativePos = target.transform.position - transform.position;
				transform.rotation = Quaternion.Lerp(transform.rotation,
				                                     Quaternion.LookRotation(relativePos),
				                                     speed);
				//camera.transform.LookAt(t.transform.position);
			}
			camera.fieldOfView = Mathf.Lerp(camera.fieldOfView,zoomedFov,speed);
		} else {
			transform.rotation = Quaternion.Lerp(transform.rotation,defaultRotation,speed);
			camera.fieldOfView = Mathf.Lerp(camera.fieldOfView,defaultFov,speed);
		}
	}
}
