using UnityEngine;
using System.Collections.Generic;
using System.Text;

[System.Serializable]
public class Stage {
	public string name;
	public int id;
	public string sceneName;
	public Texture2D picture;
}

[System.Serializable]
public class Character {
	public string name;
	public BallStats stats;
	public Material material;
	public Material minimapIcon;
	public Material trail;
	public float ballSize = 1;
	public Mesh alternativeMesh = null;
}

public class RaceSetup : MonoBehaviour {
	public GUISkin skin;
	//Race settings
	public RaceSettings settings = new RaceSettings();

	//Things changed in inspector for now
	public RaceController raceController;
	public Character[] characters;
	public Stage[] stages;

	
	public Racer playerBallPrefab;
	public Racer remoteBallPrefab;
	public Racer aiBallPrefab;

	public MlgMode mlgModeObject;

	bool startOnLevelLoad = false;
	bool waitingForPlayers = false;
	float waitingForPlayersTimeout = 12;

	bool joinGameInProgress = false;

	Client client;

	void Start() {
		DontDestroyOnLoad(this.gameObject);
		client = GameObject.FindObjectOfType<Client>();
	}

	void FixedUpdate() {
		if (waitingForPlayers && Network.isServer) {
			waitingForPlayersTimeout = Mathf.Max(0,waitingForPlayersTimeout - Time.deltaTime);
			if (waitingForPlayersTimeout <= 0) {
				//Kick players not in yet
				foreach(ServerPlayer sp in client.GetPlayerList()) {
					if (!sp.ready) {
						client.networkView.RPC("Kick",sp.player,"Timed out on level load. (Did you alt+tab out of fullscreen?)");
						Network.CloseConnection(sp.player,true);
					}
				}
				client.networkView.RPC("SetAllReady",RPCMode.All,true);
			}
			if (client.EveryoneIsReady()) {
				waitingForPlayers = false;
				client.networkView.RPC("LoadRace",RPCMode.All,settings.ToString());
				RandomTexture rt = FindObjectOfType<RandomTexture>();
				if (rt != null) {
					client.networkView.RPC("SyncSign",RPCMode.Others,rt.GetCurrentTexture());
				}
			}
		}
	}

	void OnGUI() {
		if (waitingForPlayers) {
			GUI.skin = skin;
			GUIStyle style = new GUIStyle(GUI.skin.label);
			style.fontSize = 50;
			style.alignment = TextAnchor.MiddleCenter;
			Rect rect = new Rect(0,0,Screen.width,400);

			var players = client.GetPlayerList();
			int playersNotReady = 0;
			foreach (ServerPlayer p in players) {
				if (!p.ready) playersNotReady++;
			}

			string playertxt = "players";
			if (playersNotReady == 1) 
				playertxt = "player";

			string text = "Waiting for " + playersNotReady + " " + playertxt + "\n(Timeout in "+Mathf.Ceil(waitingForPlayersTimeout)+" seconds)";
			if (playersNotReady != 1) {
				text += "s";//Plural
			}
			
			GUIX.ShadowLabel(rect,text,style,3);
			GUI.Label(rect,text,style);
		}
	}

	public void JoinGameInProgress() {
		//Causes the RaceSetup to trigger on level load. Used when connecting to a game already in progress
		joinGameInProgress = true;
	}

	public void LoadRace() {
		NetworkView nw = GameObject.FindObjectOfType<NetworkView>();
		nw.RPC("SetAllReady",RPCMode.All,false);
		nw.RPC("SetReadyOnLevelLoad",RPCMode.All);
		//settings.aiBallCount = Mathf.Floor(settings.aiBallCount);
		Application.LoadLevel(stages[settings.stage].sceneName);
		startOnLevelLoad = true;
	}

	public void StartRace(bool raceIsInProgress) {
		raceController = GameObject.Find ("RaceController").GetComponent<RaceController>();

		CreateRacers(raceController);
		//Set up the race controller with settings from the menu
		raceController.settings = settings;
		CameraOverview overviewCam = FindObjectOfType<CameraOverview>();
		if (overviewCam != null) {
			overviewCam.camera.enabled = false;
			overviewCam.GetComponent<AudioListener>().enabled = false;
		}
		if (!raceIsInProgress) {
			raceController.StartRace();
		}
		//GameObject.Find ("CameraMap").GetComponent<MapCamera>().targetObject = GameObject.Find ("CameraPivot_BallPlayer_Sanic(Clone)");
		GameObject.Find ("CameraMap").camera.enabled = true;
		GameSettings.Apply(false);
		if (raceIsInProgress) {
			GameObject.FindObjectOfType<Client>().GoToSpectating();
			GameObject.FindObjectOfType<MenuPause>().spectating = true;
		}
		Destroy(this.gameObject);
	}

	void CreateRacers(RaceController raceController) {
		Racer[] racers = new Racer[client.GetPlayerList().Count + (int)settings.aiBallCount];
		
		int position = 0;
		int row = 0;
		foreach (ServerPlayer sp in client.GetPlayerList()) {
			if (sp.spectating) continue; //Don't spawn spectators

			Character c = characters[sp.character]; //Get character for player
			Object o;
			if (sp.player == Network.player) {
				o = playerBallPrefab;
				//Is mlg mode?
				if (c.name == "Super Sanic") {
					Instantiate(mlgModeObject);
				}
			} else {
				o = remoteBallPrefab;
			}
			Racer r = (Racer)Instantiate(o,raceController.spawnPoint.GetSpawnPoint(position,c.ballSize/2),
			                             raceController.spawnPoint.transform.rotation * Quaternion.Euler(0,-90,0));
			r.name = o.name + "-" + sp.name;
			//Set character traits
			r.racerName = sp.name;
			r.renderer.material = c.material;
			r.mapIconMaterial = c.minimapIcon;
			r.GetComponent<TrailRenderer>().material = c.trail;
			if (c.alternativeMesh != null) {
				r.GetComponent<MeshFilter>().mesh = c.alternativeMesh;
				/*Destroy(r.collider); set collision mesh too
				MeshCollider mc = r.gameObject.AddComponent<MeshCollider>();
				mc.sharedMesh = c.alternativeMesh;
				mc.convex = true;
				mc.smoothSphereCollisions = true;*/
			}
			BallControl bc = r.GetComponent<BallControl>();
			if (bc != null) {
				bc.stats = c.stats;
			}
			r.transform.localScale = new Vector3(c.ballSize,c.ballSize,c.ballSize);
			if (o == remoteBallPrefab) {
				GUIText label = r.transform.FindChild("ObjectLabel").guiText;
				label.text = sp.name;
				label.color = PlayerTypeHandler.GetPlayerColor(sp.type);
			}
			//Do more stuff
			r.totalLaps = (int)settings.laps;
			//Add racer to list
			sp.racer = r;
			racers[position] = r;
			position++;
		}
		//Create AI balls
		int aiPos = 0;
		Racer[] balllist = new Racer[(int)settings.aiBallCount];
		for (int i=0;i<settings.aiBallCount;i++) {
			//TO BE CHANGED (COPYPASTE IS BAD)
			//Calc position to put racer at
			Vector3 dir;
			if (position % 11 == 10) {
				row ++;
			}
			if (position % 2 == 0) {
				dir = Vector3.right * ((position%10)/2+1) * 2;
			} else {
				dir = Vector3.left * ((position%10)/2) * 2;
			}
			dir += (Vector3.back*1.4f)*row;
			Character c;
			if (aiPos < settings.aiCharacters.Count) {
				c = characters[(int)settings.aiCharacters[aiPos]]; //Get character for player
			} else {
				c = characters[1]; //Default to knackles
			}
			Object o;
			if (Network.isServer)
				o = aiBallPrefab;
			else
				o = remoteBallPrefab;
			Racer r = (Racer)Instantiate(o,raceController.spawnPoint.GetSpawnPoint(position,c.ballSize/2),
			                             raceController.spawnPoint.transform.rotation * Quaternion.Euler(0,-90,0));
			r.name = o.name + "-" + c.name;
			//Set character traits
			r.racerName = c.name;
			r.renderer.material = c.material;
			r.mapIconMaterial = c.minimapIcon;
			r.GetComponent<TrailRenderer>().material = c.trail;
			if (c.alternativeMesh != null) {
				r.GetComponent<MeshFilter>().mesh = c.alternativeMesh;
			}
			BallControl bc = r.GetComponent<BallControl>();
			if (bc != null) {
				bc.stats = c.stats;
			}
			r.transform.localScale = new Vector3(c.ballSize,c.ballSize,c.ballSize);
			//Do more stuff
			r.totalLaps = (int)settings.laps;
			//Add racer to list
			balllist[aiPos] = r; //Set aiball for client
			racers[position] = r;
			position++;
			aiPos++;
		}
		client.SetAIBalls(balllist);
		raceController.SetRacers(racers);
	}
	
	void OnLevelWasLoaded(int level) {
		if (startOnLevelLoad) {
			waitingForPlayers = true;
			if (Network.isServer) {
				startOnLevelLoad = false;
				GameObject.FindObjectOfType<NetworkView>().RPC("ChangeLevel",RPCMode.All,Application.loadedLevel);
			}
		}
		if (joinGameInProgress) {
			//Debug.Log("Joining game in progress!");
			StartRace(true);
			joinGameInProgress = false;
		}
	}
}
