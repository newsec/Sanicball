using UnityEngine;
using System.Collections;

public class CameraPivotSpectator : MonoBehaviour {

	public GUIStyle guistyle;

	RaceController raceController;
	Racer target;
	int targetNum = 0;

	// Use this for initialization
	void Start () {
		raceController = GameObject.FindObjectOfType<RaceController>();
		target = raceController.GetRacers()[0];
	}
	
	// Update is called once per frame
	void Update () {
		if (target != null) {
			transform.position = target.transform.position;
		}
		if (Input.GetMouseButtonDown(0)) {
			//Next
			targetNum++;
			if (targetNum >= raceController.GetRacers().Length) targetNum = 0;

			target = raceController.GetRacers()[targetNum];
		}
		if (Input.GetMouseButtonDown(1)) {
			//Previous
			targetNum--;
			if (targetNum < 0) targetNum = raceController.GetRacers().Length-1;

			target = raceController.GetRacers()[targetNum];
		}
	}

	void OnGUI() {
		if (target != null) {
			Rect rect = new Rect(0,Screen.height-50,Screen.width,50);
			string text = "Spectating "+target.racerName;
			GUIX.ShadowLabel(rect,text,guistyle,2);
			GUI.Label(rect,text,guistyle);
		}
	}
}
