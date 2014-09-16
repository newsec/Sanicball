using UnityEngine;
using System.Collections.Generic;

public class ChatMessage {
	public string name = "";
	public string text = "";
	public Color color = Color.white;

	public ChatMessage(string text) {
		this.text = text;
	}

	public ChatMessage(string text, string name) {
		this.name = name;
		this.text = text;
	}

	public ChatMessage(string name, string text, Color color) {
		this.name = name;
		this.text = text;
		this.color = color;
	}
}

public class Chat : MonoBehaviour {
	public GUISkin skin;

	List<ChatMessage> chatlog = new List<ChatMessage>();
	Vector2 scroll;

	bool isTyping = false;
	string message = "";
	float autoHide = 0;
	bool cursorWasLocked = false;
	
	public Material shrek;//Shrek stuff
	public AudioClip shreksong;

	void Update() {
		if (isTyping) {
			autoHide = 0.3f;
		}
		if (autoHide > 0) {
			autoHide -= Time.deltaTime;
			if (autoHide <= 0) {
				autoHide = 0;
			}
		}
	}

	void OnDestroy() {
		GameSettings.keybinds.isTyping = false;
	}

	void EnableShreking() {
		//Shrek is love, shrek is life
		Racer[] racers = GameObject.FindObjectsOfType<Racer>();
		foreach (Racer r in racers) {
			r.renderer.material = shrek;
		}
		GameObject go = GameObject.Find("music");
		if (go != null) {
			go.audio.clip = shreksong;
			go.GetComponent<MusicPlayer>().Play("Shrek is love, shrek is life");
		}
		//efil si kerhs, evol si kerhs
	}


	
	[RPC]
	public void SendChatMessage(string name, string message, Vector3 color) {
		Color realColor = new Color(color.x,color.y,color.z);
		AddMessage(name, message, realColor);
	}

	void Send() {
		if (message.Trim() != "") {
			string nameToShow = GameSettings.user.playerName;
			ServerPlayer me = GetComponent<Client>().FindServerPlayer(Network.player);
			if (me.spectating) {
				nameToShow = "[Spectating] " + nameToShow;
			}
			if (GetComponent<Client>().IsHost(Network.player)) {
				nameToShow = "[Host] " + nameToShow;
			}
			Color color = PlayerTypeHandler.GetPlayerColor(me.type);
			networkView.RPC("SendChatMessage",RPCMode.All,nameToShow,message,new Vector3(color.r,color.g,color.b));
			if (message.ToLower().Contains("shrek")) {
				EnableShreking();
			}
		}
		message = "";
		isTyping = false;
		GameSettings.keybinds.isTyping = false;
		Screen.lockCursor = cursorWasLocked;
	}

	void AddMessage(string name, string msg, Color c) {
		chatlog.Add(new ChatMessage(name,msg,c));
		autoHide = 5;
		scroll.y = Mathf.Infinity;
	}

	void OnGUI() {
		if (Network.peerType == NetworkPeerType.Disconnected/* || hidden*/) return;
		if (autoHide < 0.25f) {
			GUI.color = new Color(1,1,1,autoHide*4);
		}

		GUI.skin = skin;
		Rect rect = new Rect(0,Screen.height-180,400,180);
		if (Application.loadedLevelName == "Lobby") {
			rect = new Rect(0,Screen.height-168-92,400,168);//Position the chat above the disconnect button
		}

		if (Event.current.type == EventType.Repaint && rect.Contains(Event.current.mousePosition)) {//Show if hovering over
			autoHide = 0.3f;
		}

		GUI.Box(rect,"");
		GUILayout.BeginArea(rect);
		scroll = GUILayout.BeginScrollView(scroll);
		GUIStyle nameStyle = new GUIStyle(GUI.skin.label);
		nameStyle.fontSize = 16;
		nameStyle.padding.right = 0;
		GUIStyle textStyle = new GUIStyle(nameStyle);
		textStyle.padding.left = 0;
		foreach (ChatMessage msg in chatlog) {
			nameStyle.normal.textColor = msg.color;
			textStyle.normal.textColor = Color.white;
			// > greentexting in a video game
			if (msg.text.IndexOf('>') == 0) { 
				textStyle.normal.textColor = new Color(0.6098f,0.78f,0.1794f);
			}
			GUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
			GUILayout.Label(msg.name + ":",nameStyle,GUILayout.ExpandWidth(false));
			GUILayout.Space(4);
			GUILayout.Label(msg.text,textStyle,GUILayout.ExpandWidth(false));
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
		}
		nameStyle.normal.textColor = Color.white;

		GUILayout.EndScrollView();
		GUILayout.FlexibleSpace();
		if (isTyping) {
			if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return) {
				Send();
			}
			GUILayout.Space(8);

			GUIStyle messageStyle = new GUIStyle(GUI.skin.textField);
			messageStyle.fontSize = 16;
			messageStyle.wordWrap = false;
			GUI.SetNextControlName("chat");
			message = GUILayout.TextField(message,128,messageStyle);
			message = message.Replace("\n","");
			GUILayout.Space(8);
			GUI.FocusControl("chat");
		}

		if (Event.current.type == EventType.KeyDown && Event.current.keyCode == GameSettings.keybinds.Find("chat").keyCode) {
			message = "";
			isTyping = true;
			GameSettings.keybinds.isTyping = true;
			cursorWasLocked = Screen.lockCursor;
			Screen.lockCursor = false;
			scroll = new Vector2(0,Mathf.Infinity);
		}
		GUILayout.EndArea();
	}
	
}
