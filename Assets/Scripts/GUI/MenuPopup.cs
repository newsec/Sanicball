using UnityEngine;
using System.Collections;

public class MenuPopup : MonoBehaviour {

	public GUISkin skin;

	public string message = "i am error";

	void Start() {
		DontDestroyOnLoad(this.gameObject);
	}

	void OnGUI() {
		GUI.skin = skin;

		Rect rect = new Rect(Screen.width/2-200,Screen.height/2-80,400,160);

		GUI.Box(rect,"");
		GUILayout.BeginArea(rect);
		GUILayout.Label(message);
		GUILayout.FlexibleSpace ();
		if (GUILayout.Button("Close",GUI.skin.GetStyle("SmallButton"))) {
			Destroy(this.gameObject);
		}
		GUILayout.EndArea();

	}
}
