using UnityEngine;
using System.Collections;

public class RaceUI : MonoBehaviour {
	public GUISkin skin;

	Racer racer;

	int sanicspeed;

	string checkpointLapString;
	string checkpointString;
	string checkpointDiffString;
	Color checkpointDiffColor = Color.blue;
	float checkpointTime = 0;

	void Start() {
		racer = GetComponent<Racer>();
	}

	void Update() {
		sanicspeed = Mathf.RoundToInt(rigidbody.velocity.magnitude);

		//Set fast music
		MusicPlayer player = FindObjectOfType<MusicPlayer>();
		if (player != null && GameSettings.sanicSpeedSong) {

			float fastLimit = 230;
			if (FindObjectOfType<MlgMode>() != null) {
				fastLimit = 420;
			}

			if (sanicspeed > fastLimit && !player.fastMode) {
				player.fastMode = true;
			}
			if (sanicspeed <= fastLimit && player.fastMode) {
				player.fastMode = false;
			}
		}

		if (checkpointTime > 0) {
			checkpointTime = Mathf.Max(0,checkpointTime - Time.deltaTime);
		}
	}

	void OnGUI() {
		GUI.skin = skin;

		if (checkpointTime > 0) {
			Rect checkpointLapRect = new Rect(Screen.width/2-200,Screen.height/2-260,400,100);
			GUIStyle checkpointLapStyle = new GUIStyle(GUI.skin.label);
			checkpointLapStyle.alignment = TextAnchor.UpperCenter;
			checkpointLapStyle.fontSize = 48;

			Rect checkpointRect = new Rect(Screen.width/2-200,Screen.height/2-200,400,100);
			GUIStyle checkpointStyle = new GUIStyle(GUI.skin.label);
			checkpointStyle.alignment = TextAnchor.UpperCenter;
			checkpointStyle.fontSize = 40;

			Rect checkpointDiffRect = new Rect(Screen.width/2-200,Screen.height/2-150,400,100);
			GUIStyle checkpointDiffStyle = new GUIStyle(checkpointStyle);
			checkpointDiffStyle.fontSize = 36;
			checkpointDiffStyle.normal.textColor = checkpointDiffColor;

			GUIX.ShadowLabel(checkpointLapRect,checkpointLapString,checkpointLapStyle,2);
			GUI.Label(checkpointLapRect,checkpointLapString,checkpointLapStyle);

			GUIX.ShadowLabel(checkpointRect,checkpointString,checkpointStyle,2);
			GUI.Label(checkpointRect,checkpointString,checkpointStyle);

			GUIX.ShadowLabel(checkpointDiffRect,checkpointDiffString,checkpointDiffStyle,2);
			GUI.Label(checkpointDiffRect,checkpointDiffString,checkpointDiffStyle);
		}

		//Basic info
		Rect infoRect = new Rect(10,Screen.height*0.3f+10,400,400);
		string spdStr = "fasts/h";
		if (FindObjectOfType<MlgMode>() != null) {
			spdStr = "MLGs/h";
		}
		string infoString = sanicspeed.ToString() + " "+spdStr+"\n" +
			"lap "+racer.Lap+" of "+racer.totalLaps;
		GUIStyle infoStyle = new GUIStyle(GUI.skin.label);
		infoStyle.fontSize = 36;

		GUIX.ShadowLabel(infoRect,infoString,infoStyle,2);
		GUI.Label(infoRect,infoString,infoStyle);

		//Time
		Rect timeRect = new Rect(Screen.width/2-200,Screen.height-80,400,80);
		string timeString = Timing.GetTimeString(racer.RaceTime);
		GUIStyle timeStyle = new GUIStyle(GUI.skin.label);
		timeStyle.fontSize = 48;
		timeStyle.alignment = TextAnchor.UpperCenter;

		GUIX.ShadowLabel(timeRect,timeString,timeStyle,3);
		GUI.Label(timeRect,timeString,timeStyle);

		//Finished text
		if (racer.finished) {
			Rect finishedRect = new Rect(Screen.width/2-200,Screen.height-102,400,80);
			GUIStyle finishedStyle = new GUIStyle(timeStyle);
			finishedStyle.fontSize = 24;
			GUIX.ShadowLabel(finishedRect,"Race finished",finishedStyle,2);
			GUI.Label(finishedRect,"Race finished",finishedStyle);
		}

		//Player position
		Rect playerPosRect = new Rect(Screen.width-420,Screen.height-120,400,120);
		string playerPosString = racer.GetPositionString();
		GUIStyle playerPosStyle = new GUIStyle(GUI.skin.label);
		playerPosStyle.fontSize = 86;
		playerPosStyle.fontStyle = FontStyle.Bold;
		playerPosStyle.alignment = TextAnchor.UpperRight;
		if (FindObjectOfType<MlgMode>() && racer.position == 1) {
			playerPosString = "#REKT";
		}
		
		GUIX.ShadowLabel(playerPosRect,playerPosString,playerPosStyle,4);
		GUI.Label(playerPosRect,playerPosString,playerPosStyle);
	}

	public void Checkpoint (string time, string timeDiff, string lapString, Color timeDiffColor) {
		checkpointLapString = lapString;
		checkpointString = time;
		checkpointDiffString = timeDiff;
		checkpointDiffColor = timeDiffColor;
		checkpointTime = 2;
	}
}
