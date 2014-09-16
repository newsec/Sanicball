using UnityEngine;
using System.Collections;

[RequireComponent (typeof(Camera))]
public class CameraOverview : MonoBehaviour {

	public GUISkin skin;
	public string stageName;
	float pos = 0;
	float delay = 1;

	void Start() {
		CameraFade.StartAlphaFade(Color.black,true,1);
	}

	void Update() {
		if (delay > 0) {
			delay-=Time.deltaTime;
		} else {
			pos = Mathf.Lerp(pos,204,Time.deltaTime*2);
		}
	}

	void FixedUpdate() {
		//transform.Rotate(new Vector3(0,0.1f,0),Space.World);
	}

	void OnGUI() {
		if (!camera.enabled) return;

		GUI.skin = skin;

		Rect rect = new Rect(0,Screen.height-pos,Screen.width,204);

		GUIStyle smallStyle = new GUIStyle(skin.label);
		smallStyle.alignment = TextAnchor.MiddleCenter;

		GUIStyle tinyStyle = new GUIStyle(smallStyle);
		tinyStyle.fontSize = 14;

		GUIStyle bigStyle = new GUIStyle(smallStyle);
		bigStyle.fontSize = 90;
		bigStyle.fontStyle = FontStyle.Bold;

		GUI.Box(rect,"");
		GUILayout.BeginArea(rect);

		GUILayout.Label("Welcome to",smallStyle);
		GUILayout.Label(stageName,bigStyle);
		GUILayout.Label("Game should be starting shortly",smallStyle);
		GUILayout.Label("(If it doesn't, something must have broken. In that case, use the pause menu to return to lobby.)",tinyStyle);

		GUILayout.EndArea();
	}
}
