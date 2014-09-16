using UnityEngine;
using System.Collections;

public class CameraPivot : MonoBehaviour {
	
	public GameObject target;
	public Camera attachedCamera;
	public Vector3 defaultCameraPosition = new Vector3(6,2.8f,0);
	public float cameraDistance = 1;
	public float cameraDistanceTarget = 1;

	// Use this for initialization
	void Start () {
	
	}
	
	void FixedUpdate() {
		//if (Input.GetKey(KeyCode.LeftArrow)) {
		//	transform.Rotate(Vector3.up*3);
		//}
		//if (Input.GetKey(KeyCode.RightArrow)) {
		//	transform.Rotate(Vector3.down*3);
		//}
	}
	
	// Update is called once per frame
	void LateUpdate () {
		if (target == null) {
			Destroy(this.gameObject);
			return;
		}
		transform.position = target.transform.position;

		cameraDistanceTarget = Mathf.Clamp(cameraDistanceTarget - (Input.GetAxis("Mouse Scroll")/5),0,10);
		cameraDistance = Mathf.Lerp (cameraDistance,cameraDistanceTarget,Time.deltaTime*4);

		//Ground collision
		/*int layer = 1 << LayerMask.NameToLayer("Terrain"); //Select terrain layer
		Vector3 relativeCameraPos = transform.rotation * defaultCameraPosition;
		float distance = Vector3.Distance(Vector3.zero,relativeCameraPos);

		Ray ray = new Ray(transform.position,relativeCameraPos.normalized);
		RaycastHit hit;

		if (Physics.Raycast(ray, out hit, distance, layer)) {
			attachedCamera.transform.position = hit.point;
		} else {*/
			Vector3 targetPoint = defaultCameraPosition * cameraDistance;

			attachedCamera.transform.position = transform.TransformPoint(targetPoint);
		//}
	}
}
