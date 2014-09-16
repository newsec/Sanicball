using UnityEngine;
using System.Collections;

public class MenuCamera : MonoBehaviour {
	
	public Path[] paths;
	int currentPath = 0;
	GameObject ball;
	
	PathFollower pathFollower;

	// Use this for initialization
	void Start () {
		pathFollower = transform.parent.GetComponent<PathFollower>();
		pathFollower.path = paths[currentPath];
		ball = GameObject.Find ("DisplayBall");
		CameraFade.StartAlphaFade(Color.black,true,5);
	}
	
	// Update is called once per frame
	void Update () {
		camera.rect = new Rect(0,0,1f - 400f/Screen.width,1);
		if (pathFollower.position >= 1) {
			currentPath++;
			if (currentPath >= paths.Length)
				currentPath = 0;
			pathFollower.position = 0;
			pathFollower.path = paths[currentPath];
			ball.GetComponent<RandomMaterial>().Switch();
		}
	}
	
	void FixedUpdate() {
		transform.LookAt(ball.transform.position);
	}
}
