using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Client : MonoBehaviour {

	//public MenuPageJoinGame menuConnect;
	public RaceSetup raceSetupObject;
	public CameraPivotSpectator spectatorCameraPivot;
	public MenuPopup popupObject;

	ConnectionStatus joinGameMessage;

	bool readyOnLevelLoad = false;
	public bool initLobbyOnLevelLoad = false;
	private bool isInLobby = true;

	List<ServerPlayer> players = new List<ServerPlayer>();
	public NetworkPlayer host;
	Racer[] aiBalls;

	float sendTimer;

	bool intentionalDisconnect = false;
	
	public ConnectionStatus Connect(string ip, int port) {
		ConnectionStatus msg = new ConnectionStatus("Connecting...");
		NetworkConnectionError nce = Network.Connect(ip,port);
		msg.isConnecting = true;
		Application.RegisterLogCallback(LogCallback);
		if (nce != NetworkConnectionError.NoError) {
			msg.message = GetErrorMessage(nce);
			msg.isConnecting = false;
			Destroy(this.gameObject);
		}
		joinGameMessage = msg;
		return msg;
	}

	public ConnectionStatus Connect(HostData data) {
		ConnectionStatus msg = new ConnectionStatus("Connecting...");
		NetworkConnectionError nce = Network.Connect(data);
		msg.isConnecting = true;
		Application.RegisterLogCallback(LogCallback);
		Debug.Log("Registered log callback");
		if (nce != NetworkConnectionError.NoError) {
			msg.message = GetErrorMessage(nce);
			msg.isConnecting = false;
			Destroy(this.gameObject);
		}
		joinGameMessage = msg;
		return msg;
	}
	
	// Hacky solution to look for failed NAT punchthough. From Unity Answers thread: http://answers.unity3d.com/questions/184939/handle-nat-punchthrough-failed-message.html
	void LogCallback(string condition, string stackTrace, LogType type)
	{
		if (type == LogType.Error)
		{
			// Check if it is the NATPunchthroughFailed error
			const string MessageBeginning = "Receiving NAT punchthrough attempt from target";
			
			if (condition.StartsWith(MessageBeginning, System.StringComparison.Ordinal))
			{
				// Call the callback that Unity should be calling.
				OnFailedToConnect(NetworkConnectionError.NATPunchthroughFailed);
				Network.Disconnect();
				Application.RegisterLogCallback(null);
			}
		}
	}

	string GetErrorMessage (NetworkConnectionError nce) {
		switch (nce) {
		case NetworkConnectionError.AlreadyConnectedToAnotherServer:
		case NetworkConnectionError.AlreadyConnectedToServer:
			return "You're already connected a server somehow. Try restarting the game.";
		case NetworkConnectionError.ConnectionBanned:
			return "You're banned from this server (Most likely temporarily).";
		case NetworkConnectionError.ConnectionFailed:
			return "No server response.";
		case NetworkConnectionError.IncorrectParameters:
			return "This isn't a proper IP adress.";
		case NetworkConnectionError.InvalidPassword:
			return "Invalid server password.";
		case NetworkConnectionError.NATPunchthroughFailed:
			return "Your NAT types are not compatible.";
		case NetworkConnectionError.TooManyConnectedPlayers:
			return "This server is full.";
		default:
			return "Something strange happened.";
		}
	}

	//Unity functions

	void OnConnectedToServer() {
		joinGameMessage.message = "Server is checking your game version...";
		networkView.RPC("CheckGameVersion",RPCMode.Server,Network.player,GameVersion.AsFloat);
		Application.RegisterLogCallback(null); //Remove NAT fail checker
	}
	
	void OnFailedToConnect(NetworkConnectionError nce) {
		if (joinGameMessage != null) {
			joinGameMessage.message = GetErrorMessage(nce);
			joinGameMessage.isConnecting = false;
		} else {
			//This happens when connecting to the master server fails.
			Debug.LogError("Master server is offline. Avoiding accidental server shutdown..");
			return;
		}
		Application.RegisterLogCallback(null); //Remove NAT fail checker
		if (Network.peerType != NetworkPeerType.Disconnected) {
			Network.Disconnect(); //Disconnect if not already disconnected
		}
		Destroy(this.gameObject);
	}

	void OnDisconnectedFromServer(NetworkDisconnection info) {
		if (!intentionalDisconnect) {
			MenuPopup popup = (MenuPopup)GameObject.Instantiate(popupObject);
			popup.message = "Lost connection to the server. It might have crashed.";
		}
		Screen.lockCursor = false;
		Application.LoadLevel("Menu");
		Destroy(this.gameObject);//Destroy all network related objects
	}

	void OnLevelWasLoaded(int level) {
		if (readyOnLevelLoad) {
			networkView.RPC("SetReady",RPCMode.All,Network.player,true);
			readyOnLevelLoad = false;
		}
		if (initLobbyOnLevelLoad) {
			foreach (ServerPlayer sp in players) {
				sp.racer = FindObjectOfType<RaceLobby>().SpawnLobbyBall(sp,sp.player == Network.player);
			}
			initLobbyOnLevelLoad = false;
		}
	}

	void FixedUpdate() {
		sendTimer -= Time.deltaTime;
		if (sendTimer <= 0) {
			NetworkUpdate();
			sendTimer = (float)1/Network.sendRate;
		}

	}

	void NetworkUpdate() {
		ServerPlayer sp = FindServerPlayer(Network.player); //Find own player
		if (sp != null) {
			Racer r = sp.racer;
			if (r != null) {
				networkView.RPC("UpdatePlayerBall",RPCMode.Others,Network.player,
				                r.rigidbody.position,r.rigidbody.rotation,r.rigidbody.velocity,r.rigidbody.angularVelocity);
			}
		}
		//Update AIBalls if server
		if (Network.isServer && aiBalls != null) {
			for (int i = 0;i < aiBalls.Length;i++) {
				Racer r = aiBalls[i];
				networkView.RPC("UpdateAIBall",RPCMode.Others,i,
				                r.transform.position,r.rigidbody.rotation,r.rigidbody.velocity,r.rigidbody.angularVelocity);
			}
		}
	}

	#region Public Functions

	public List<ServerPlayer> GetPlayerList() {
		return players;
	}

	public void Disconnect() {
		intentionalDisconnect = true;
		Network.Disconnect();
	}

	public bool EveryoneIsReady() {
		foreach(ServerPlayer sp in players) {
			if (!sp.ready) return false;
		}
		return true;
	}

	public void SetAIBalls(Racer[] balllist) {
		aiBalls = balllist;
	}

	public ServerPlayer FindServerPlayer(NetworkPlayer player) {
		//Seaches for the player with a matching NetworkPlayer
		foreach (ServerPlayer sp in players) {
			if (sp.player == player)
				return sp;
		}
		return null;
	}

	public bool IsHost(NetworkPlayer player) {
		return player == host;
	}

	public void GoToSpectating() {
		networkView.RPC("SetSpectating",RPCMode.All,Network.player,true);
		networkView.RPC("RemovePlayerFromRace",RPCMode.All,Network.player);
		Instantiate(spectatorCameraPivot);
	}

	public bool IsInLobby {
		get {
			return isInLobby;
		}
		set {
			isInLobby = value;
			if (Network.isServer) {
				//Re-register the server
				string statusStr = "In lobby";
				if (!value) {
					statusStr = "In race";
				}
				GetComponent<Server>().RegisterServer(statusStr);
			}
		}
	}

	#endregion

	//RPC ---

	#region Shared

	[RPC]
	void AcceptGameVersion() {
		joinGameMessage.message = "Success! Loading...";
		networkView.RPC("RequestPlayerSetup",RPCMode.Server,Network.player,GameSettings.user.playerName,GameSettings.user.token);
	}
	
	[RPC]
	void SetReady(NetworkPlayer player, bool ready) {
		FindServerPlayer(player).ready = ready;
	}
	
	[RPC]
	void SetAllReady(bool ready) {
		foreach (ServerPlayer sp in players) {
			sp.ready = ready;
		}
	}
	
	[RPC]
	void AddPlayerToList(NetworkPlayer player, string name, int type) {
        int i = 0;
        while(true)
        {
            if(i == 0)
            {
                if (!players.Where(plr => plr.name.Equals(name)).Any())
                {
                    break;
                }
            }
            else
            {
                if (!players.Where(plr => plr.name.Equals(name + " (" + i + ")")).Any())
                {
                    break;
                }
            }
            i++;
        }
        if(i != 0)
        {
            name += " (" + i + ")";
        }
		players.Add(new ServerPlayer(player,name,(PlayerType)type));
	}
	
	[RPC]
	void RemovePlayerFromList(NetworkPlayer player) {
		RemovePlayerFromRace(player);
		ServerPlayer sp = FindServerPlayer(player);
		players.Remove(sp);
	}

	[RPC]
	void RemovePlayerFromRace(NetworkPlayer player) {
		ServerPlayer sp = FindServerPlayer(player);
		RaceController rc = GameObject.FindObjectOfType<RaceController>();
		if (rc != null) {
			rc.RemoveRacer(sp.racer);
		}
		if (sp.racer != null) {
			Destroy(sp.racer.gameObject);
		}
	}

	[RPC]
	void ChangeLevel(int level) {
		Application.LoadLevel(level);
	}

	[RPC]
	void SetHost(NetworkPlayer player) {
		host = player;
	}

	[RPC]
	void Kick(string reason) {
		MenuPopup popup = (MenuPopup)GameObject.Instantiate(popupObject);
		if (reason == "") {
			reason = "Unknown reason.";
		}
		popup.message = "Disconnected from server: " + reason;
		Disconnect();
	}

	#endregion
	#region Lobby

	[RPC]
	void SetInitLobbyOnLevelLoad() {
		initLobbyOnLevelLoad = true;
	}

	[RPC]
	void SetReadyOnLevelLoad() {
		readyOnLevelLoad = true;
	}

	[RPC]
	void GotoStageSelect() {
		RaceLobby lobby = FindObjectOfType<RaceLobby>();
		if (lobby != null) {
			lobby.GotoStageSelect();
		}
	}

	[RPC]
	void CancelStageSelect() {
		RaceLobby lobby = FindObjectOfType<RaceLobby>();
		if (lobby != null) {
			lobby.CancelStageSelect();
		}
	}

	[RPC]
	void LoadRace(string settings) {
		IsInLobby = false;
		RaceSetup rs = GameObject.FindObjectOfType<RaceSetup>();//Get the race setup object
		if (rs != null) {
			//ensure that the clientside settings are correct
			rs.settings = RaceSettings.ParseFromString(settings);
			rs.StartRace(false);
		} else {
			Debug.LogError("Tried to load race but no RaceSetup is present");
		}
	}

	[RPC]
	void JoinRaceInProgress(string settingsStr) {
		//When joining a game in progress the racesetup wont be there already, so create a new one
		IsInLobby = false;
		RaceSetup rs = (RaceSetup) Instantiate(raceSetupObject);
		rs.settings = RaceSettings.ParseFromString(settingsStr);
		rs.JoinGameInProgress(); //Make the racesetup run on level load
	}

	[RPC]
	void SetLobbyTitle(string name) {
		GameObject go = GameObject.Find("Lobbytitle");
		if (go != null) {
			go.guiText.text = name;
		}
	}

	[RPC]
	void UpdateRaceSettings(string settingsStr) {
		if (Network.isServer) return;
		RaceSetup rs = GameObject.FindObjectOfType<RaceSetup>();
		if (rs != null) {
			rs.settings = RaceSettings.ParseFromString(settingsStr);
		} else {
			Debug.LogError("Couldn't update race settings: RaceSetup not found.");
		}
	}

	[RPC]
	void SetCharacter(NetworkPlayer player, int character) {
		ServerPlayer sp = FindServerPlayer(player);
		sp.character = character;

		//Remove old character and spawn a new one
		if (IsInLobby && !initLobbyOnLevelLoad) {
			if (sp.racer != null) {
				Destroy(sp.racer.gameObject);
			}
			sp.racer = FindObjectOfType<RaceLobby>().SpawnLobbyBall(sp,sp.player == Network.player);
		}
	}

	#endregion
	#region Ingame

	[RPC]
	void SetSpectating(NetworkPlayer player, bool spec) {
		//Remember: once spectating you cannot go back
		FindServerPlayer(player).spectating = spec;
	}

	[RPC]
	void UpdatePlayerBall(NetworkPlayer player, Vector3 position, Quaternion rotation, Vector3 velocity, Vector3 angularVelocity) {
		if (Network.player == player) return; //Don't update ball if we own it
		ServerPlayer sp = FindServerPlayer(player);
		if (sp != null) {
			Racer r = sp.racer;
			if (r != null) {
				r.rigidbody.position = position;
				r.rigidbody.rotation = rotation;
				r.rigidbody.velocity = velocity;
				r.rigidbody.angularVelocity = angularVelocity;
			}
		}

	}

	[RPC]
	void UpdateAIBall(int ball, Vector3 position, Quaternion rotation, Vector3 velocity, Vector3 angularVelocity) {
		if (Network.isServer || aiBalls == null) return;
		Racer r = aiBalls[ball];
		if (r != null) {
			r.rigidbody.position = position;
			r.rigidbody.rotation = rotation;
			r.rigidbody.velocity = velocity;
			r.rigidbody.angularVelocity = angularVelocity;
		}
	}

	[RPC]
	void ReturnToLobby() {
		//Deletes RaceSetup and returns to lobby
		IsInLobby = true;
		initLobbyOnLevelLoad = true;
		aiBalls = null;
		networkView.RPC("SetReady",RPCMode.All,Network.player,false);
		//networkView.RPC ("SetCharacter",RPCMode.All,Network.player,0);
		networkView.RPC("SetSpectating",RPCMode.All,Network.player,false);
		Screen.lockCursor = false;
		Application.LoadLevel("Lobby");
	}

	[RPC]
	void SetGhost(NetworkPlayer player) {
		var p = FindServerPlayer(player);
		if (p.racer != null) {
			p.racer.gameObject.layer = LayerMask.NameToLayer("Racer Ghost");
		} else {
			Debug.LogWarning("Tried to enabled ghost mode on a player that isn't spawned.");
		}

	}

	[RPC]
	void SyncSign(int textureID) {
		RandomTexture rt = FindObjectOfType<RandomTexture>();
		if (rt != null && !Network.isServer) {
			rt.SetTexture(textureID);
		}
	}

	#endregion

}

[System.Serializable]
public class ServerPlayer {
	//Fields set on instantiation
	public string name = "missingno";
	public NetworkPlayer player;
	public PlayerType type = PlayerType.Normal;
	//Fields set locally
	public Racer racer; //The gameobject this player is racing with
	public bool ready = false;
	public bool spectating = false;
	public int character = 0; //Default to Sanic
	
	public ServerPlayer(NetworkPlayer player, string name, PlayerType type) {
		this.player = player;
		this.name = name;
		this.type = type;
	}
	
}