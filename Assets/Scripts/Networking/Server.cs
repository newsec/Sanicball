using UnityEngine;
using System.Collections.Generic;

[RequireComponent (typeof(Client))]
[RequireComponent (typeof(NetworkView))]
public class Server : MonoBehaviour {

	public ServerSettings settings = new ServerSettings();
	bool isOnServerList = false;

	Client client;

	Dictionary<NetworkPlayer, float> connectionTimeout = new Dictionary<NetworkPlayer, float>();

	// Use this for initialization
	void Awake () {
		client = GetComponent<Client>();

		if (GameObject.FindObjectsOfType(typeof (Server)).Length > 1) {
			Destroy(this.gameObject);
		}
		DontDestroyOnLoad(this.gameObject);
		//GameObject.Find("Lobbytitle").guiText.text = settings.gameName;
	}

	void Update() {
		var tempDic = new Dictionary<NetworkPlayer, float>(connectionTimeout);
		foreach (KeyValuePair<NetworkPlayer,float> kvp in tempDic) {
			NetworkPlayer p = kvp.Key;
			connectionTimeout[p] -= Time.deltaTime;
			if (connectionTimeout[p] <= 0) {
				//Timeout
				SendServerMsg("A player tried to join but timed out.");
				networkView.RPC("Kick",p,"Connection timed out.");//Try kicking, then close connection
				Network.CloseConnection(p,true);
				connectionTimeout.Remove(p);
			}
		}
	}

	public NetworkConnectionError StartServer(bool addToServerList) {
		NetworkConnectionError error;
		Network.InitializeSecurity();
		error = Network.InitializeServer(settings.maxPlayers-1,settings.port,!Network.HavePublicAddress());
		//Get player type
		PlayerType myType = PlayerType.Anon;
		if (GameSettings.user.token != "") {
			myType = PlayerTypeHandler.GetPlayerType(GameSettings.user.playerName);
		}
		networkView.RPC("AddPlayerToList",RPCMode.All,
		                Network.player,
		                GameSettings.user.playerName,
		                (int)myType);
		networkView.RPC("SetHost",RPCMode.AllBuffered,Network.player); //Set host for all players joining
		if (addToServerList) {
			isOnServerList = true;
			RegisterServer("In lobby");
		}
		SendPublicMsg("Welcome to Sanicball!");
		return error;
	}

	public void RegisterServer(string status) {
		if (isOnServerList) {
			MasterServer.RegisterHost("sanicball",settings.gameName,status+";"+GameVersion.AsFloat+";"+GameVersion.AsString);
		}
	}

	public void KickPlayer (NetworkPlayer player, string reason) {
		networkView.RPC("Kick",player,reason);
		SendPublicMsg(client.FindServerPlayer(player).name + " was kicked by the host.");
	}

	void OnServerInitialized() {
		client.initLobbyOnLevelLoad = true;
		Application.LoadLevel("Lobby");
	}

	void OnPlayerConnected(NetworkPlayer player) {
		connectionTimeout.Add(player,10);
	}

	void OnPlayerDisconnected(NetworkPlayer player) {
		if (client.FindServerPlayer(player) != null) {
			SendPublicMsg(client.FindServerPlayer(player).name+" has left the game.");
			networkView.RPC("RemovePlayerFromList",RPCMode.All,player);
		}
	}

	public void SendPublicMsg(string text)  {
		networkView.RPC("SendChatMessage",RPCMode.All,"[System]",text,new Vector3(1,1,0));
	}

	public void SendServerMsg(string text) {
		Vector3 col = new Vector3(1,0.2f,0.2f);
		if (Network.isServer) {
			GetComponent<Chat>().SendChatMessage("[System]",text,col);
		} else {
			networkView.RPC("SendChatMessage",RPCMode.Server,"[System]",text,col);
		}
	}

	public void SendPrivateMsg(string text, NetworkPlayer target) {
		networkView.RPC("SendChatMessage",target,"[System]",text,new Vector3(0.8f,0.8f,0));
	}

	[RPC]
	void RequestPlayerSetup(NetworkPlayer player, string name, string token) {
		if (!Network.isServer) return;
		Debug.Log("Recieved player setup request from '"+name+"'.");
		//Check if this player is still connected to the server. If a player cancels his connection late enough, the server will still spawn him.
		bool hasFoundPlayer = false;
		foreach (NetworkPlayer np in Network.connections) {
			if (np == player) {
				hasFoundPlayer = true;
				break;
			}
		}

		if (!hasFoundPlayer) {
			Debug.Log("Player wasn't found in list of connections! Aborting setup.");
			return;
		}

		//Is this an anon or a player?
		if (string.IsNullOrEmpty(token)) {
			//This is an anon, let him though
			Debug.Log(name + " is anonymous, request accepted. Setting up..");
			SetupPlayer(player,name,true);
		} else {
			//This one claims to be a signed in player. Confirm his identity
			Debug.Log(name + " is a Game Jolt user, comfirming identity..");
			GJAPI.Users.Verify(name,token);

			//Declare a delegate to use for callback
			GJUsersMethods._VerifyCallback verifyCallback = null;
			//Fill in the delegate
			verifyCallback = delegate (bool isLegit){
				if (isLegit) {
					Debug.Log("Identity for "+name+" confirmed!");
					SetupPlayer(player,name,false);
				} else {
					Debug.LogWarning("Identity for "+name+" DENIED!! Player kicked.");
					networkView.RPC("Kick",player,"The server failed to verify your Game Jolt account details.");
				}
				//Remove the delegate from the event now that it's used
				GJAPI.Users.VerifyCallback -= verifyCallback;
			};
			//Add the delegate to the event
			GJAPI.Users.VerifyCallback += verifyCallback;
		}
	}

	void SetupPlayer(NetworkPlayer player, string name, bool isAnon) {
		if (!Network.isServer) return;

		//Check if this player's version has been accepted
		if (connectionTimeout.ContainsKey(player)) {
			//This only happens when an older version joins a newer version. Therefore, Kick cannot be used. Slightly obsolete code.
			Network.CloseConnection(player,true);
			SendServerMsg("Player joined with wrong version and was kicked.");
			connectionTimeout.Remove(player);
		}
		
		//Set stuff on the newly connected player
		networkView.RPC("ChangeLevel",player,Application.loadedLevel);
		SendPublicMsg(name + " has joined the game.");
		
		//Add other players to new player's player list and do stuff with the new players
		foreach (ServerPlayer sp in client.GetPlayerList()) {
			networkView.RPC("AddPlayerToList",player,
			                sp.player,
			                sp.name,
			                (int)sp.type);
			networkView.RPC("SetReady",player,sp.player,sp.ready);
			networkView.RPC("SetCharacter",player,sp.player,sp.character);
			networkView.RPC("SetSpectating",player,sp.player,sp.spectating);
		}
		//Grab new player's type
		PlayerType pType = PlayerType.Anon;
		if (!isAnon) {
			pType = PlayerTypeHandler.GetPlayerType(name);
		}
		//Add new player to everyone's player list
		networkView.RPC("AddPlayerToList",RPCMode.All,
		                player,
		                name,
		                (int)pType);
		
		if (!client.IsInLobby) { //Means the game is in progress.
			//networkView.RPC("SetSpectating",RPCMode.All,player,true);
			SendPrivateMsg("You'll be able to race once the next round starts.",player);
			RaceController rc = GameObject.FindObjectOfType<RaceController>();
			networkView.RPC("JoinRaceInProgress",player,rc.settings.ToString());
		} else { //Means we're in the lobby.
			networkView.RPC("SetInitLobbyOnLevelLoad",player);
			//Sets the new player's character, forcing all players to spawn him into the lobby
			networkView.RPC("SetCharacter",RPCMode.All,player,0);
		}
		Debug.Log("Player "+name+" has been successfully set up on clients.");
	}

	[RPC]
	void RequestLobbyTitle(NetworkPlayer sender) {
		if (!Network.isServer) return;
		networkView.RPC("SetLobbyTitle",sender,settings.gameName);
	}

	[RPC]
	void CheckGameVersion(NetworkPlayer player, float version) {
		Debug.Log("Recieved request to check game version of player " + player);
		if (version != GameVersion.AsFloat) {
			if (version < 0.53f) {//Versions below 0.5.3 don't support Kick
				Network.CloseConnection(player,true);
			} else {
				networkView.RPC("Kick",player,"Wrong game version: Server is running " + GameVersion.AsString + ".");
			}
			SendServerMsg("Player joined with wrong version and was kicked.");
			Debug.Log("Wrong version, request denied.");
		} else {
			networkView.RPC("AcceptGameVersion",player);
			Debug.Log("Correct version, sending accept message");
		}
		connectionTimeout.Remove(player);
	}

}

public struct ServerSettings {
	public string gameName;
	public int port;
	public int maxPlayers;
	public bool showOnList;
}
