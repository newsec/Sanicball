using UnityEngine;
using System.Collections;

public class MapCamera : MonoBehaviour {
	
	public GameObject targetObject;
	public bool rotateWithTarget;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
		camera.rect = new Rect(0f,0.7f,(camera.pixelHeight*1.3f)/Screen.width,0.3f);
		if (targetObject != null) {
			transform.position = new Vector3(targetObject.transform.position.x,transform.position.y,targetObject.transform.position.z);
			if (rotateWithTarget) {
				transform.rotation = Quaternion.Euler(0,-90,0)*Quaternion.Euler(90,targetObject.transform.rotation.eulerAngles.y,0);
			}
		}
	}
}
