using UnityEngine;
using System.Collections;

public class VersionChecker : MonoBehaviour {
	public GUISkin skin;

	bool hidden = true;
	string text;

	// Use this for initialization
	IEnumerator Start () {
		if (!Application.isWebPlayer) {
			WWW www = new WWW("http://www.bk-tn.com/sanicball/version.php");
			yield return www;
			CheckVersion(www.text);
		}
	}

	void CheckVersion(string wwwString) {
		float versionFloat = GameVersion.AsFloat;
		string versionString = GameVersion.AsString;
		string[] splitted = wwwString.Split(';');

		if (float.TryParse(splitted[0],out versionFloat)) {
			versionString = splitted[1];
			if (versionFloat > GameVersion.AsFloat) {
				text = 
					"Seems like your version of Sanicball is outdated.\n" + 
					"Your version: " + GameVersion.AsString + "\n" +
					"Latest version: " + versionString + "\n" + 
					"Click this button to download the latest version.";
				hidden = false;
			}
		}
	}
	
	// Update is called once per frame
	void OnGUI () {
		if (hidden) return;
		GUI.skin = skin;

		int width = 420;
		int height = 200;
		Rect rect = new Rect(20,Screen.height - height - 20,width,height);

		GUILayout.BeginArea(rect);

		GUILayout.FlexibleSpace();
		GUIStyle style = new GUIStyle(GUI.skin.button);
		style.wordWrap = true;
		style.fontSize = 18;
		if (GUILayout.Button(text,style)) {
			Application.OpenURL("http://gamejolt.com/games/arcade/sanicball-alpha-v0-5/17385/");
		}
		GUILayout.EndArea();
	}
}
