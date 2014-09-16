using UnityEngine;
using System.Collections;
using System.Linq;

public class ServerBrowser : MonoBehaviour {
	public GUISkin skin;

	public Client clientObject;
	bool hidden = true;
	Vector2 scroll;
	float timeout;
	
	string header = "";
	ServerItem[] serverList = new ServerItem[0];
	ServerSortMode currentSortMode = ServerSortMode.None;
	bool isSortingReversed = false;
	ConnectionStatus connectionStatus = new ConnectionStatus();

	public void Toggle() {
		if (hidden)
			Show();
		else
			Hide();
	}

	public void Show() {
		hidden = false;
		UpdateServerList();
	}

	public void Hide() {
		hidden = true;
	}

	void UpdateServerList() {
		header = "Refreshing..";
		timeout = 7;
		MasterServer.ClearHostList();
		MasterServer.RequestHostList("sanicball");
	}

	void OnMasterServerEvent(MasterServerEvent e) {
		if (e == MasterServerEvent.HostListReceived) {
			timeout = 0;
			HostData[] data = MasterServer.PollHostList();
			serverList = new ServerItem[data.Length];
			for (int i=0;i<data.Length;i++) {
				serverList[i] = new ServerItem(data[i]);
				//Debug.Log(data[i].gameName + " uses NAT: " + data[i].useNat);
			}
			string serverStr = "servers";
			if (serverList.Length == 1) {
				serverStr = "server";
			}
			header = serverList.Length + " " + serverStr;
			SortServers(currentSortMode,false);
		}
	}

	void Update() {
		if (timeout > 0) {
			timeout -= Time.deltaTime;
			if (timeout <= 0) {
				header = "Failed to connect to the master server!";
			}
		}
	}

	void SortServers(ServerSortMode mode, bool changeReversal) {
		switch(mode) {
		case ServerSortMode.ServerName:
			serverList = serverList.OrderBy(x => x.hostData.gameName).ToArray();
			break;
		case ServerSortMode.Status:
			serverList = serverList.OrderBy(x => x.status).ToArray();
			break;
		case ServerSortMode.PlayerCount:
			serverList = serverList.OrderByDescending(x => x.hostData.connectedPlayers).ToArray();
			break;
		case ServerSortMode.Version:
			serverList = serverList.OrderByDescending(x => x.versionFloat).ToArray();
			break;
		}
		if (changeReversal) {
			if (currentSortMode == mode) { //Reverse the list
				isSortingReversed = !isSortingReversed;
			} else {
				isSortingReversed = false;
			}
		}
		if (isSortingReversed) {
			serverList = serverList.Reverse().ToArray();
		}
		currentSortMode = mode;
	}

	void OnGUI() {
		if (hidden) return;
		GUI.skin = skin;
		GUIStyle smallButton = GUI.skin.GetStyle("SmallButton");
		GUIStyle smallButtonOdd = GUI.skin.GetStyle("SmallButtonOdd");
		GUIStyle labelOdd = GUI.skin.GetStyle("LabelOdd");

		int width = Mathf.Min(960,Screen.width - 400);
		int height = Screen.height;

		Rect rect = new Rect((Screen.width-400)-width,Screen.height/2-height/2,width,height);

		GUI.Box(rect,"");
		GUILayout.BeginArea(rect);
		/*string sortmodeStr = "";
		if (serverList.Length > 0 && currentSortMode != ServerSortMode.None && header != "Refreshing..") {
			sortmodeStr = ", sorting by "+GetSortModeString(currentSortMode);
			if (isSortingReversed) {
				sortmodeStr += " (reversed)";
			}
		}*/
		GUILayout.Label(header);
		scroll = GUILayout.BeginScrollView(scroll);
		//Column widths
		float serverNameMinWidth = 400;
		float statusWidth = 130;
		float playerCountWidth = 110;
		float versionWidth = 190;
		//Column labels
		GUILayout.BeginHorizontal();
		GUIStyle sortModeStyle = new GUIStyle(smallButton);
		sortModeStyle.fontStyle = (currentSortMode == ServerSortMode.ServerName) ? FontStyle.Bold : FontStyle.Normal;
		if (GUILayout.Button("Server name",sortModeStyle,GUILayout.MinWidth(serverNameMinWidth),GUILayout.ExpandWidth(true))) {
			SortServers(ServerSortMode.ServerName,true);
		}
		sortModeStyle.fontStyle = (currentSortMode == ServerSortMode.Status) ? FontStyle.Bold : FontStyle.Normal;
		if (GUILayout.Button("Status",sortModeStyle,GUILayout.Width(statusWidth))) {
			SortServers(ServerSortMode.Status,true);
		}
		sortModeStyle.fontStyle = (currentSortMode == ServerSortMode.PlayerCount) ? FontStyle.Bold : FontStyle.Normal;
		if (GUILayout.Button("Players",sortModeStyle,GUILayout.Width(playerCountWidth))) {
			SortServers(ServerSortMode.PlayerCount,true);
		}
		sortModeStyle.fontStyle = (currentSortMode == ServerSortMode.Version) ? FontStyle.Bold : FontStyle.Normal;
		if (GUILayout.Button("Game version",sortModeStyle,GUILayout.Width(versionWidth))) {
			SortServers(ServerSortMode.Version,true);
		}
		GUILayout.EndHorizontal();
		for (int i=0;i<serverList.Length;i++) {
			GUIStyle labelStyle = GUI.skin.label;
			GUIStyle buttonStyle = smallButton;
			if (i % 2 == 0 ) {
				labelStyle = labelOdd;
				buttonStyle = smallButtonOdd;
			}
			GUILayout.BeginHorizontal();
			ServerItem server = serverList[i];
			//Server name
			if (GUILayout.Button(server.hostData.gameName,buttonStyle,GUILayout.MinWidth(serverNameMinWidth),GUILayout.ExpandWidth(true))) {
				Connect(server.hostData);
			}
			//Status
			GUILayout.Label(server.status,labelStyle,GUILayout.Width(statusWidth));
			//Player count
			GUILayout.Label(server.hostData.connectedPlayers + "/" + server.hostData.playerLimit,labelStyle,GUILayout.Width(playerCountWidth));
			//Version
			GUILayout.Label(server.versionString,labelStyle,GUILayout.Width(versionWidth));
			GUILayout.EndHorizontal();
		}
		GUILayout.FlexibleSpace();

		GUILayout.EndScrollView();
		//Bottom bar
		GUILayout.BeginHorizontal();
		if (GUILayout.Button("Close",smallButton,GUILayout.ExpandWidth(false))) {
			Hide ();
		}
		if (GUILayout.Button("Refresh",smallButton,GUILayout.ExpandWidth(false))) {
			UpdateServerList();
		}
		if (GUILayout.Button("Join by IP",smallButton,GUILayout.ExpandWidth(false))) {
			GetComponent<MenuMain>().SetPage(MenuPage.JoinGame);
		}
		if (connectionStatus.isConnecting) {
			if (GUILayout.Button("Cancel",smallButtonOdd,GUILayout.ExpandWidth(false))) {
				Cancel ();
			}
		}
		GUIStyle rightAlign = new GUIStyle(GUI.skin.label);
		rightAlign.alignment = TextAnchor.MiddleRight;
		GUILayout.Label(connectionStatus.message,rightAlign,GUILayout.ExpandWidth(true));
		GUILayout.EndHorizontal();

		GUILayout.EndArea();
	}

	void Connect(HostData data) {
		/*MenuMain mainMenu = GetComponent<MenuMain>();
		mainMenu.SetPage(MenuPage.JoinGame);
		MenuPageJoinGame joinGamePage = mainMenu.joinGame;
		
		string tmpIP = "";
		for (int j=0;j<data.ip.Length;j++) {
			tmpIP = data.ip[j] + " ";
		}
		joinGamePage.ip = tmpIP;
		joinGamePage.port = server.hostData.port.ToString();
		
		joinGamePage.Connect();*/
		if (GameObject.FindObjectOfType<Client>() != null) return;
		Client c = (Client)GameObject.Instantiate(clientObject);
		connectionStatus = c.Connect(data);
	}

	void Cancel() {
		Destroy(FindObjectOfType<Client>().gameObject);
		connectionStatus.message = "Cancelled.";
		connectionStatus.isConnecting = false;
		Network.Disconnect();
	}

	string GetSortModeString(ServerSortMode mode) {
		switch(mode) {
		case ServerSortMode.None:
			return "nothing";
		case ServerSortMode.PlayerCount:
			return "player count";
		case ServerSortMode.ServerName:
			return "server name";
		case ServerSortMode.Status:
			return "server status";
		case ServerSortMode.Version:
			return "server version";
		}
		return "nothing";
	}

}

public class ServerItem {
	public HostData hostData;
	public string status;
	public float versionFloat;
	public string versionString;

	public ServerItem(HostData data) {
		string[] comment = data.comment.Split(';');
		status = comment[0];
		if (!float.TryParse(comment[1],out versionFloat)) {
			Debug.LogError("Failed to parse "+data.gameName+" server version float");
		}
		versionString = comment[2];

		hostData = data;
	}
}

public enum ServerSortMode {
	None,ServerName,Status,PlayerCount,Version
}
