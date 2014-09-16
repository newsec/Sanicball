using UnityEngine;
using System.Collections;

public class MinimapIcon : MonoBehaviour {
	
	public GameObject objectToFollow;
	public GameObject mapCamera;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (objectToFollow == null) {
			Destroy(this.gameObject);
			return;
		}
		transform.position = new Vector3(objectToFollow.transform.position.x,transform.position.y,objectToFollow.transform.position.z);
		if (mapCamera != null) {
			transform.rotation = mapCamera.transform.rotation;
		}
	}
}
