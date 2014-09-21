using UnityEngine;
using System.Collections;

public class MenuCamera : MonoBehaviour {
	// Use this for initialization
	void Start () {
		CameraFade.StartAlphaFade(Color.black,true,5);
	}
	
	// Update is called once per frame
	void Update () {
        camera.transform.Rotate(new Vector3(0f, Time.deltaTime * 2, 0f));
	}
}
