using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent (typeof(Client))]
public class PlayerList : MonoBehaviour {
	public GUISkin skin;

	bool hidden = true;
	Vector2 playerListScroll;
	Client client; //Client to fetch player list from
	bool mouseWasLocked = false;

	KeyCode key = KeyCode.Tab;

	public bool Hidden {get {return hidden;}}

	void Start() {
		client = GetComponent<Client>();
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown(key)) {
			mouseWasLocked = Screen.lockCursor;
			Screen.lockCursor = false;
		}
		if (Input.GetKeyUp(key)) {
			Screen.lockCursor = mouseWasLocked;
		}

		hidden = !Input.GetKey(key);
	}

	void OnGUI() {
		if (!hidden) {
			GUI.skin = skin;
			GUIStyle smallButton = GUI.skin.GetStyle("SmallButton");

			Rect playerListRect = new Rect(Screen.width/2-480,Screen.height/2-320,960,640);
			GUI.Box(playerListRect,"");
			GUILayout.BeginArea(playerListRect);
			List<ServerPlayer> playerList = client.GetPlayerList();
			GUILayout.Label(playerList.Count + " player(s)");
			playerListScroll = GUILayout.BeginScrollView(playerListScroll);
			if (client != null) {
				int i = 0;
				foreach (ServerPlayer sp in playerList) {
					GUIStyle labelStyle = GUI.skin.label;
					GUIStyle buttonStyle = smallButton;
					if (i % 2 == 0) {
						labelStyle = GUI.skin.GetStyle("LabelOdd");
						buttonStyle = GUI.skin.GetStyle("SmallButtonOdd");
					}
					string readystring = sp.ready ? "Ready" : "Not ready";
					GUILayout.BeginHorizontal();
					GUILayout.Label(sp.name,labelStyle,GUILayout.ExpandWidth(true));

					GUIStyle typeStyle = new GUIStyle(labelStyle);
					typeStyle.normal.textColor = PlayerTypeHandler.GetPlayerColor(sp.type);

					GUILayout.Label(sp.type.ToString(),typeStyle,GUILayout.Width(200));
					GUILayout.Label(readystring,labelStyle,GUILayout.Width(125));
					if (Network.isServer && sp.player != Network.player) {
						if (GUILayout.Button("Kick",buttonStyle,GUILayout.Width(62))) {
							FindObjectOfType<Server>().KickPlayer(sp.player,"You have been kicked by the host.");
						}
					} else {
						GUILayout.Label("",labelStyle,GUILayout.Width(62));
					}
					GUILayout.EndHorizontal();
					i++;
				}
			}
			GUILayout.EndScrollView();
			GUILayout.EndArea();
		}
	}
}
