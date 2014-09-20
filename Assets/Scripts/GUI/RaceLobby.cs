using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class RaceLobby : MonoBehaviour {
	public GUISkin skin;

	public Racer ballPlayerLobbyPrefab;
	public Racer ballRemoteLobbyPrefab;

	public LobbyCamera lobbyCamera;

	bool raceSettingsVisible = false;
	bool showAISettings = false;
	bool stageSelect = false;

	bool ready = false;
	float forceStartTimer = 0;

	int character = 0;
	int totalCharacters;
	int superSanicID = -1;

	Vector2 playerListScroll;
	Vector2 raceSettingsScroll;
	Vector2 stageSelectScroll;
	
	RaceSetup raceSetup;
	RaceSettings tempSettings;

	float raceSettingsPos = 0;

	float tempLapsF;
	float tempAIBallsF;
	string tempLapsStr = "2";
	string tempAIBallsStr = "7";
	float tempAIStupidnessF = 30;
	bool unlockFields = false;

	bool settingsEqual = true;

	float gameStartTimer = 5;
	bool gameStart = false;

	float[] stageRecords;

	Client client;

	void Start() {
		if (Network.isServer) {
			raceSettingsVisible = true;
		}

		client = FindObjectOfType<Client>();
		if (GameSettings.music) {
			audio.Play();
		}
		if (client == null) Destroy(this.gameObject);
		raceSetup = FindObjectOfType<RaceSetup>();
		//Load race settings from playerprefs
		string settingsString = PlayerPrefs.GetString("RaceSettings",raceSetup.settings.ToString());
		raceSetup.settings = RaceSettings.ParseFromString(settingsString);
		//Set some things from the race settings
		totalCharacters = raceSetup.characters.Length;
        for (int i = 0; i < totalCharacters; i++ )
        {
            if(raceSetup.characters[i].name.ToLower().Equals("super sanic"))
            {
                superSanicID = i;
            }
        }
        tempSettings = new RaceSettings(raceSetup.settings);
		tempLapsF = tempSettings.laps;
		tempAIBallsF = tempSettings.aiBallCount;
		tempAIStupidnessF = tempSettings.aiStupidness;
		tempLapsStr = tempSettings.laps.ToString();
		tempAIBallsStr = tempSettings.aiBallCount.ToString();

		character = client.FindServerPlayer(Network.player).character;
		//Set stage records
		stageRecords = new float[raceSetup.stages.Length];
		for (int i=0;i<stageRecords.Length;i++) {
			Stage s = raceSetup.stages[i];
			stageRecords[i] = PlayerPrefs.GetFloat(s.sceneName+"_lap",-1);
		}

		GameSettings.Apply(false);
	}

	void Update() {
		if (gameStart) {
			gameStartTimer = Mathf.Max(0,gameStartTimer - Time.deltaTime);
			if (gameStartTimer <= 0 && Network.isServer) {
				client.networkView.RPC("GotoStageSelect",RPCMode.All);
			}
		}

		if (forceStartTimer > 0) {
			forceStartTimer = Mathf.Max(0,forceStartTimer - Time.deltaTime);
		}

		if (character == superSanicID && !raceSetup.settings.allowSuperSanic) {
			character = 0;
			client.networkView.RPC("SetCharacter",RPCMode.All,Network.player,0);
		}

		//Check if temp settings equal race settings
		settingsEqual = Network.isServer && tempSettings.Equals(raceSetup.settings);

		int charSelectWidth = Mathf.Min(320,Screen.width - 80 - 280*2);
		Rect charSelectRect = new Rect(Screen.width/2-charSelectWidth/2,Screen.height-92,charSelectWidth,92);
		
		lobbyCamera.zoomAtPlayer = charSelectRect.Contains(new Vector2(Input.mousePosition.x,Screen.height - Input.mousePosition.y));

		//Slide race settings up and down
		if (raceSettingsVisible) {
			raceSettingsPos = Mathf.Lerp(raceSettingsPos,400,Time.deltaTime*10);
		} else {
			raceSettingsPos = Mathf.Lerp(raceSettingsPos,0,Time.deltaTime*10);
		}
	}

	public void StartCountdown() {
		gameStart = true;
	}

	public void StopCountdown() {
		gameStart = false;
		gameStartTimer = 5;
	}

	public void GotoStageSelect() {
		gameStart = false;
		gameStartTimer = 5;
		stageSelect = true;
	}

	public void CancelStageSelect() {
		stageSelect = false;
	}

	string GetCharacterName(int character) {
		return raceSetup.characters[character].name.ToUpper();
	}

	public Racer SpawnLobbyBall(ServerPlayer sp, bool isLocal) {
		Character c = FindObjectOfType<RaceSetup>().characters[sp.character]; //Get character for player
		Object o;
		if (sp.player == Network.player)
			o = ballPlayerLobbyPrefab;
		else
			o = ballRemoteLobbyPrefab;
		Racer r = (Racer)Instantiate(o,new Vector3(0,8,-4),Quaternion.Euler(0,90,25));
		r.name = o.name + "-" + sp.name;
		//Set character traits
		r.racerName = sp.name;
		r.renderer.material = c.material;
		r.mapIconMaterial = c.minimapIcon;
		r.GetComponent<TrailRenderer>().material = c.trail;
		BallControl bc = r.GetComponent<BallControl>();
		if (c.alternativeMesh != null) {
			r.GetComponent<MeshFilter>().mesh = c.alternativeMesh;
		}
		if (bc != null)
			bc.stats = c.stats;
		r.transform.localScale = new Vector3(c.ballSize,c.ballSize,c.ballSize);
		if (o == ballRemoteLobbyPrefab) {
			GUIText label = r.transform.FindChild("ObjectLabel").guiText;
			label.text = sp.name;
			label.color = PlayerTypeHandler.GetPlayerColor(sp.type);
		}
		return r;
		//Do more stuff
		//r.RaceController = raceController;
		//r.totalLaps = (int)settings.laps;
	}

	void OnGUI() {
		GUI.skin = skin;
		GUIStyle smallButton = GUI.skin.GetStyle("SmallButton");
		GUIStyle smallText = new GUIStyle(GUI.skin.label);
		smallText.fontSize = 14;

		if (!stageSelect) {
			#region Character selection
			int charSelectWidth = Mathf.Min(400,Screen.width - 280*2);
			Rect charSelectRect = new Rect(Screen.width/2-charSelectWidth/2,Screen.height-92,charSelectWidth,92);
			GUI.Box(charSelectRect,"");
			GUILayout.BeginArea(charSelectRect);
			GUILayout.BeginHorizontal();

			if (GUILayout.Button("",GUI.skin.GetStyle("BigArrowLeft")) && !ready) {
				character--;
				if (character < 0)
					character += totalCharacters;
				if (!raceSetup.settings.allowSuperSanic && character == superSanicID) {
					character = superSanicID - 1;
				}

				client.networkView.RPC("SetCharacter",RPCMode.All,Network.player,character);
			}

			GUILayout.BeginVertical();
			GUIStyle centeredLabel = GUI.skin.GetStyle("CenteredLabel");
			string pickCharacterText = ready ? "Character locked" : "Pick character";
			GUILayout.Label(pickCharacterText,centeredLabel);
			GUIStyle nameStyle = new GUIStyle(centeredLabel);
			if (Screen.width - 280*2 < 400) {
				nameStyle.fontSize = 24;
				nameStyle.fontStyle = FontStyle.Bold;
			} else {
				nameStyle.fontSize = 46;
			}
			GUILayout.FlexibleSpace();
			GUILayout.Label(GetCharacterName(character),nameStyle);
			GUILayout.FlexibleSpace();
			GUILayout.EndVertical();

			if (GUILayout.Button("",GUI.skin.GetStyle("BigArrowRight")) && !ready) {
				character++;
				if (character >= totalCharacters)
					character-=totalCharacters;
				client.networkView.RPC("SetCharacter",RPCMode.All,Network.player,character);
			}

			GUILayout.EndHorizontal();
			GUILayout.EndArea();
			#endregion

			#region Race settings
			//Main settings panel
			Rect settingsRect = new Rect(Screen.width-raceSettingsPos,33,400,Screen.height-260-33);
			GUI.Box (settingsRect,"");//Placing the Box inside the GUILayout gives strange results
			GUILayout.BeginArea(settingsRect);
				raceSettingsScroll = GUILayout.BeginScrollView(raceSettingsScroll);
				//Server panel
				if (Network.isServer) {
					if (!showAISettings) {
						if (unlockFields) {//Unlocked fields
							GUILayout.Label("Laps: " + tempSettings.laps);
							tempLapsStr = GUILayout.TextField(tempLapsStr);
							GUILayout.Label("AI Balls: " + tempSettings.aiBallCount);
							tempAIBallsStr = GUILayout.TextField(tempAIBallsStr);

							GUILayout.Label("Note that high amounts of AI balls can cause crazy lagspikes and sometimes cause Sanicball to stop responding.",smallText);
							//apply settings
							int lapsInt;
							if (!int.TryParse(tempLapsStr,out lapsInt)) {
								GUILayout.Label("- Laps has to be a number");
							} else {
								if (lapsInt < 1) {
									GUILayout.Label("- Laps has to be above 0");
								} else {
									tempSettings.laps = lapsInt;
								}
							}
							int aiBallsInt;
							if (!int.TryParse(tempAIBallsStr,out aiBallsInt)) {
								GUILayout.Label("- AI Balls has to be a number");
							} else { //Locked fields
								if (aiBallsInt < 0) {
									GUILayout.Label("- AI Balls has to be 0 or above");
								} else {
									tempSettings.aiBallCount = aiBallsInt;
								}
								if (aiBallsInt >= 1000) {
									GUILayout.Label("- Prepare your anus.");
								}
							}
						} else {
							GUILayout.Label("Number of laps: " + tempSettings.laps);
							tempLapsF = GUILayout.HorizontalSlider(tempLapsF,1,6);
							GUILayout.Label("AI Balls: " + tempSettings.aiBallCount);
							tempAIBallsF = GUILayout.HorizontalSlider(tempAIBallsF,0,30);
							
							//apply settings
							tempSettings.laps = (int)tempLapsF;
							tempSettings.aiBallCount = (int)tempAIBallsF;
						}
						unlockFields = GUILayout.Toggle(unlockFields,"Unlock fields");
						if (GUILayout.Button("Go to AI settings",smallButton)) {
							showAISettings = true;
						}
						tempSettings.allowSuperSanic = GUILayout.Toggle(tempSettings.allowSuperSanic,"Allow Super Sanic");

					} else {
						//AI settings
						if (GUILayout.Button("Back",smallButton)) {
							showAISettings = false;
						}
						string AIStupidnessname = tempSettings.aiStupidness.ToString();
						switch (tempSettings.aiStupidness) {
						case 0:
							AIStupidnessname = "Clones";
							break;
						case 30:
							AIStupidnessname = "Default";
							break;
						case 100:
							AIStupidnessname = "Retarded";
							break;
						}
						GUILayout.Label("AI Stupidness: "+AIStupidnessname);
						tempAIStupidnessF = GUILayout.HorizontalSlider(tempAIStupidnessF,0,100);

						tempSettings.aiStupidness = (int)tempAIStupidnessF;
						if (tempSettings.aiBallCount > tempSettings.aiCharacters.Count) {
							//Build an array of knackles to add to the list of AI characters
							int[] newBalls = new int[tempSettings.aiBallCount - tempSettings.aiCharacters.Count];
							for (int i=0;i<newBalls.Length;i++) {
								newBalls[i] = 1;
							}
							tempSettings.aiCharacters.AddRange(newBalls);
						}

						for (int i=0;i<Mathf.Min(tempSettings.aiBallCount,64);i++) {//AI Ball list
							GUIStyle labelStyle = GUI.skin.label;
							GUIStyle buttonStyle = smallButton;
							if (i % 2 == 0) {
								labelStyle = GUI.skin.GetStyle("LabelOdd");
								buttonStyle = GUI.skin.GetStyle("SmallButtonOdd");
							}
							int aiChar = 1; //Default to Knackles (just to be sure)
							if (i < tempSettings.aiCharacters.Count) { //If inside the AI character array
								aiChar = (int)tempSettings.aiCharacters[i];
							}
							GUILayout.BeginHorizontal();
							GUILayout.Label("AI "+(i+1)+": ",labelStyle,GUILayout.Width(85));
							if (GUILayout.Button("<",buttonStyle,GUILayout.Width(40))) {
								int c = 1;
								if (i < tempSettings.aiCharacters.Count)
									c = tempSettings.aiCharacters[i];
								c--;
								if (c < 0)
									c = totalCharacters-1;
								tempSettings.aiCharacters[i] = c;
							}
							GUILayout.Label(GetCharacterName(aiChar),labelStyle);
							if (GUILayout.Button(">",buttonStyle,GUILayout.Width(40))) {
								int c = 1;
								if (i < tempSettings.aiCharacters.Count)
									c = tempSettings.aiCharacters[i];
								c++;
								if (c >= totalCharacters)
									c = 0;
								tempSettings.aiCharacters[i] = c;
							}
							GUILayout.EndHorizontal();
						}
						if (tempSettings.aiBallCount > 64) {
							string str = "AI 65";
							if (tempSettings.aiBallCount > 65)
								str = "AI 65-" + (tempSettings.aiBallCount+1);
							GUILayout.Label(str + ": "+GetCharacterName(tempSettings.aiCharacters[63]));
						}
						if (GUILayout.Button("Set to random characters",smallButton)) {
							for (int i=0;i<tempSettings.aiCharacters.Count;i++) {
								tempSettings.aiCharacters[i] = Random.Range(0,raceSetup.characters.Length);
							}
						}
						if (GUILayout.Button("Set to default characters",smallButton)) {
							tempSettings.aiCharacters = new List<int>(new int[] {1,2,3,4,5,6,7,8});
						}
						GUILayout.Label("Set all:");
						for (int i=0;i<totalCharacters;i++) {
							if (GUILayout.Button(GetCharacterName(i),smallButton)) {
								SetAllAICharacters(i);
							}
						}
					}

				} else { //Client panel
					RaceSettings rs = raceSetup.settings;
					if (!showAISettings) {
						GUILayout.Label("Number of laps: " + rs.laps);
						GUILayout.Label("AI Balls: " + rs.aiBallCount);
						GUILayout.Label("Super sanic is " + (rs.allowSuperSanic ? "ALLOWED" : "BANNED"));
						if (GUILayout.Button("Show AI settings",smallButton)) {
							showAISettings = true;
						}
					} else {
						if (GUILayout.Button("Back",smallButton)) {
							showAISettings = false;
						}
						GUILayout.Label("AI stupidness: " + rs.aiStupidness);
						GUILayout.Label("AI characters:");
						for (int i=0;i<Mathf.Min(rs.aiBallCount,64);i++) {//AI Ball list
							GUIStyle labelStyle = GUI.skin.label;
							if (i % 2 == 0) {
								labelStyle = GUI.skin.GetStyle("LabelOdd");
							}
							int aiChar = 1; //Default to Knackles (just to be sure)
							if (i < rs.aiCharacters.Count) { //If inside the AI character array
								aiChar = (int)rs.aiCharacters[i];
							}
							GUILayout.BeginHorizontal();
							GUILayout.Label("AI "+(i+1)+": ",labelStyle,GUILayout.Width(85));
							GUILayout.Label(GetCharacterName(aiChar),labelStyle);
							GUILayout.EndHorizontal();
						}
						if (rs.aiBallCount > 64) {
							string str = "AI 65";
							if (rs.aiBallCount > 65)
								str = "AI 65-" + (rs.aiBallCount+1);
							GUILayout.Label(str + ": "+GetCharacterName(rs.aiCharacters[63]));
						}

					}
				}
				GUILayout.EndScrollView();

				GUILayout.FlexibleSpace();
				if (!settingsEqual && Network.isServer) {
					GUILayout.Label("Settings haven't been saved! Remember to save before you start the game.",smallText);
					GUILayout.BeginHorizontal();
					if (GUILayout.Button("Save")) {
						raceSetup.settings = new RaceSettings(tempSettings);
						client.networkView.RPC("UpdateRaceSettings",RPCMode.Others,tempSettings.ToString());
						FindObjectOfType<Server>().SendPublicMsg("The race settings have been updated.");
						PlayerPrefs.SetString("RaceSettings",raceSetup.settings.ToString());
						PlayerPrefs.Save();
					}
					if (GUILayout.Button("Revert")) {
						tempSettings = new RaceSettings(raceSetup.settings);
						tempAIBallsF = tempSettings.aiBallCount;
						tempLapsF = tempSettings.laps;
						tempAIStupidnessF = tempSettings.aiStupidness;
					}
					GUILayout.EndHorizontal();
				}
			GUILayout.EndArea();
			//Show/hide button
			Rect showHideRect = new Rect(Screen.width-400,0,400,33);
			GUILayout.BeginArea(showHideRect);
			if (!raceSettingsVisible) {
				string showRaceSettingsText = "Show race settings";
				if (Network.isServer) {
					if (settingsEqual) {
						showRaceSettingsText = "Change race settings";
					} else {
						showRaceSettingsText = "Settings not saved!";
					}
				}
				if (GUILayout.Button(showRaceSettingsText,smallButton)) {
					raceSettingsVisible = true;
				}
			} else {
				if (GUILayout.Button("Hide race settings",smallButton)) {
					raceSettingsVisible = false;
				}
			}
			GUILayout.EndArea();
			#endregion

			#region Ready/Start race

			Rect cornerRect = new Rect(Screen.width-280,Screen.height-92,280,92);
			GUI.Box(cornerRect,"");

			List<ServerPlayer> players = client.GetPlayerList();

			if (Network.isServer && players.Count > 1) {
				if (forceStartTimer <= 0) {
					if (GUI.Button(new Rect(Screen.width-280,Screen.height-92-34,280,34),"Force start",smallButton)) {
						forceStartTimer = 5;
					}
				} else {
					if (GUI.Button(new Rect(Screen.width-280,Screen.height-92-34,280,34),"Are you sure?",smallButton)) {
						client.networkView.RPC("GotoStageSelect",RPCMode.All);
					}
				}
			}

			GUILayout.BeginArea(cornerRect);
			if (players.Count > 1) {
				if (!ready) {
					if (GUILayout.Button("Ready!")) {
						ready=true;
						client.networkView.RPC("SetReady",RPCMode.All,Network.player,true);
					}
				} else {
					if (GUILayout.Button("Cancel")) {
						ready=false;
						client.networkView.RPC("SetReady",RPCMode.All,Network.player,false);
					}
				}
				//Get amount of players ready
				int playersReady = 0;
				foreach (ServerPlayer sp in players) {
					if (sp.ready)
						playersReady++;
				}
				if (playersReady < players.Count) {
					if (gameStart) {
						StopCountdown();
					}
					GUILayout.Label(playersReady + "/" + players.Count + " players ready");
				} else {
					if (!gameStart) {
						StartCountdown();
					}
					if (gameStartTimer > 0) {
						GUILayout.Label("Game starts in "+Mathf.Ceil(gameStartTimer)+" seconds");
					} else {
						GUILayout.Label("Just a moment...");
					}
				}
			} else {
				if (GUILayout.Button("Continue")) {
					client.networkView.RPC("GotoStageSelect",RPCMode.All);
				}
				GUILayout.Label("To stage select");
			}

			GUILayout.EndArea();
			#endregion
		} else {
			#region Stage Select
			GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
			titleStyle.fontSize = GUI.skin.button.fontSize;
			titleStyle.alignment = TextAnchor.UpperCenter;

			if (Network.isServer) {
				int width = Mathf.Min(740,Screen.width);
				int height = Screen.height - 92*2;

				Rect stageSelectRect = new Rect(Screen.width/2-width/2,Screen.height/2-height/2,width,height);

				GUIStyle recordStyle = new GUIStyle(GUI.skin.label);
				recordStyle.alignment = TextAnchor.UpperCenter;

				GUI.Box(stageSelectRect,"");
				GUILayout.BeginArea(stageSelectRect);

				GUILayout.Label("Stage select",titleStyle);
				stageSelectScroll = GUILayout.BeginScrollView(stageSelectScroll);
				//GUILayout.BeginHorizontal(GUILayout.Width(350));
				foreach (Stage s in raceSetup.stages) {
					GUILayout.BeginHorizontal();
					GUILayout.Box(s.picture,GUILayout.Width (350),GUILayout.Height(233));
					GUILayout.BeginVertical();
					if (GUILayout.Button(s.name)) {
						raceSetup.settings.stage = s.id;
						raceSetup.LoadRace();
					};
                    GUILayout.Label("Made by " + s.author);
					if (stageRecords[s.id] != -1) {
						GUILayout.Label("Lap record: "+Timing.GetTimeString(stageRecords[s.id]));
					} else {
						GUILayout.Label("No records yet");
					}
					GUILayout.EndVertical();

					GUILayout.EndHorizontal();
				}
				GUILayout.EndScrollView();
				GUILayout.EndArea();
			} else {
                if (!ready)
                {
                    ready = true;
                    client.networkView.RPC("SetReady", RPCMode.All, Network.player, true);
                }
				GUI.Label(new Rect(0,100,Screen.width,100),"Host is selecting stage...",titleStyle);
			}

			#endregion
		}

		Rect backToMenuRect = new Rect(0,Screen.height-92,280,92);

		GUI.Box(backToMenuRect,"");
		GUILayout.BeginArea(backToMenuRect);
		if (Network.isServer && stageSelect) {
			if (GUILayout.Button("Cancel")) {
				client.networkView.RPC("CancelStageSelect",RPCMode.All);
				client.networkView.RPC("SetAllReady",RPCMode.All,false);
			}
			GUILayout.Label("Back to settings");
		} else {
			if (GUILayout.Button("Disconnect")) {
				BackToMenu();
			}
			if (Network.isServer) {
				GUILayout.Label("Server will close");
			} else {
				GUILayout.Label("Back to menu");
			}
		}
		GUILayout.EndArea();
	}

	void SetAllAICharacters(int newChar) {
		for (int i=0;i<tempSettings.aiCharacters.Count;i++) {
			tempSettings.aiCharacters[i] = newChar;
		}
	}

	void BackToMenu() {
		if (Network.isServer) {
			//Kick off players properly
			foreach (ServerPlayer sp in client.GetPlayerList()) {
				if (sp.player != Network.player) {
					client.networkView.RPC("Kick",sp.player,"Server is shutting down.");
				}
			}
		}
		Destroy(FindObjectOfType<RaceSetup>().gameObject);
		if (client != null) {
			client.Disconnect();
		}
		Application.LoadLevel("Menu");
	}
}
