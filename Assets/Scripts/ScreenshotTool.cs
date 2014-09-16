using UnityEngine;
using System.Collections;

public class ScreenshotTool : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown(KeyCode.Space)) {
			Debug.Log("Screenshot captured.");
			Application.CaptureScreenshot("screenshot.png",2);
		}
	}
}
