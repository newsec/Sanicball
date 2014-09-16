using UnityEngine;
using System.Collections.Generic;

public class MenuPause : MonoBehaviour {
	public GUISkin skin;

	bool visible = false;
	public bool spectating = false;

	void Update() {
		if (Input.GetKeyDown(KeyCode.Escape) || GameSettings.keybinds.GetKeyDown("menu")) {
			Toggle();
		}
	}

	void Toggle() {
		visible = !visible;
		Screen.lockCursor = !visible;
		GameObject.FindObjectOfType<SmoothMouseLook>().canControl = !visible;
	}

	void OnGUI() {
		if (!visible) return;
		GUI.skin = skin;
		Rect rect = new Rect(Screen.width-400,0,400,Screen.height);

		GUI.Box(rect,"");
		GUILayout.BeginArea(rect);
		GUILayout.FlexibleSpace();
		if (GUILayout.Button("Close menu")) {
			Toggle();
		}
		if (!spectating) {
			if (GUILayout.Button("Spectate")) {
				GameObject.FindObjectOfType<Server>().SendPublicMsg(GameSettings.user.playerName + " is now spectating.");
				GameObject.FindObjectOfType<Client>().GoToSpectating();
				spectating = true;
				Toggle();
			}
		}
		if (Network.isServer) {
			if (GUILayout.Button("Back to lobby")) {
				GameObject.FindObjectOfType<NetworkView>().RPC("ReturnToLobby",RPCMode.All);
			}
		}
		if (GUILayout.Button("Disconnect")) {
			if (GameObject.FindObjectOfType<Client>() != null)
				GameObject.FindObjectOfType<Client>().Disconnect();
			Application.LoadLevel("Menu");
		}
		if (Network.isServer) {
			GUILayout.Label("You're the host; disconnecting will close the server.");
		}
		GUILayout.FlexibleSpace();
		GUILayout.EndArea();
	}
	
}
