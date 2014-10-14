﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class MenuMain : MonoBehaviour {
	public GUISkin skin;

	public MenuPageStartGame startGame;
	public MenuPageJoinGame joinGame;
	public MenuPageOptions options;
    public MenuPageCustomization customization;

	public MenuPage page;
    public MenuPage lastActualPage;
	Vector2 scrollPos;
    Vector2 scrollPos2;

    KeyCode[] validKeyCodes;

    public float pageOffset = 0f;

	void Start() {
        GameSettings.Init();
		PlayerTypeHandler.Init();
		options.UpdateVarsGeneral();
		options.UpdateVarsProfile();
		validKeyCodes = (KeyCode[])System.Enum.GetValues(typeof(KeyCode));
	}

	void Update() {
        if(page != MenuPage.None)
        {
            pageOffset = Mathf.Lerp(pageOffset, 400f, Time.deltaTime * 25f);
        }
        else
        {
            pageOffset = Mathf.Lerp(pageOffset, 0f, Time.deltaTime * 25f);
        }
		if (Input.GetKeyDown(KeyCode.Escape)) {
			if (string.IsNullOrEmpty(options.keybindToChange)) {
				page = MenuPage.None;
			} else {
				options.keybindToChange = null;
			}
		}
		if (!string.IsNullOrEmpty(options.keybindToChange)) {
			GetAnyKeyForKeybinding();
		}
		if (options.resolution > Screen.resolutions.Length-1) {
			options.resolution = Screen.resolutions.Length-1;
		}
	}

	public void SetPage(MenuPage page) {
		this.page = page;
        if(page != MenuPage.None)
        {
            lastActualPage = page;
        }
	}

    void OnGUI()
    {
        GUI.depth = -100;
        string headerText = "";
        switch (lastActualPage)
        {
            case MenuPage.Credits:
                headerText = "Game credits";
                break;
            case MenuPage.JoinGame:
                headerText = "Join by IP";
                break;
            case MenuPage.Options:
                headerText = "Options";
                break;
            case MenuPage.Customization:
                headerText = "Customization";
                break;
            case MenuPage.StartGame:
                headerText = "Start server";
                break;
        }
        GUI.skin = skin;
        GUIStyle smallButton = GUI.skin.GetStyle("SmallButton");
        Rect pos;
        if (pageOffset > 0.01f)
        {
            pos = new Rect(
    Screen.width - 400 - pageOffset,
    0,
    400,
    Screen.height
    );

            GUI.Box(pos, "");
            GUILayout.BeginArea(pos);
            GUIStyle headerStyle = new GUIStyle(GUI.skin.label);
            headerStyle.fontSize = 52;
            headerStyle.fontStyle = FontStyle.Bold;
            GUILayout.Label(headerText, headerStyle);
            GUILayout.FlexibleSpace();
            scrollPos2 = GUILayout.BeginScrollView(scrollPos2);
            if (lastActualPage == MenuPage.StartGame)
            {
                startGame.DrawGUI();
            }

            if (lastActualPage == MenuPage.JoinGame)
            {
                joinGame.DrawGUI();
            }

            if (lastActualPage == MenuPage.Options)
            {
                options.DrawGUI();
            }

            if (lastActualPage == MenuPage.Customization)
            {
                customization.DrawGUI();
            }

            if (lastActualPage == MenuPage.Credits)
            {
                #region Credits
                GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
                titleStyle.fontSize = 24;
                titleStyle.fontStyle = FontStyle.Bold;
                GUIStyle creditsStyle = new GUIStyle(GUI.skin.label);
                creditsStyle.fontSize = 16;
                GUILayout.Label("Original game by BK-TN \n" +
                                "Current version made by uhm.. github?\n" +
                                "Contribute at: https://github.com/HannesMann/Sanicball", creditsStyle);
                GUILayout.Label("Graphics", titleStyle);
                GUILayout.Label("- GRASS TEXTURE: Solon001 on Deviantart\n" +
                                "Most other textures are either made by Unity or taken from royality-free texture sites.", creditsStyle);
                GUILayout.Label("Character faces", titleStyle);
                GUILayout.Label("- SANIC: Popular wallpaper - unknown origin\n" +
                                "- KNACKLES: Disappeared from internet somehow\n" +
                                "- TAELS: Mew087123 on Deviantart\n" +
                                "- AME ROES: o_opc on Reddit\n" +
                                "- SHEDEW: GalaxyTheHedgehog1 on Deviantart\n" +
                                "- ROGE DA BAT: tysonhesse on Deviantart\n" +
                                "- ASSPIO: drawn by me\n" +
                                "- BIG DA CAT: EdulokoX on Reddit\n" +
                                "- DR. AGGMEN: tysonhesse on Deviantart", creditsStyle);
                GUILayout.Label("Music", titleStyle);
                GUILayout.Label("- MENU: Chariots Of Fire theme song\n" +
                                "- LOBBY: Green Hill Zone from Sonic The Hedgehog" +
                                "- INGAME PLAYLIST\n" +
                                "- Sonic Generations - vs. Metal Sonic (Stardust Speedway Bad Future) JP [Generations Mix]\n" +
                                "- Deadmau5 - Infra Super Turbo Pig Cart Racer\n" +
                                "- Sonic Adventure 2 - City Escape\n" +
                                "- Tube and Berger - Straight Ahead (Instrumental)\n" +
                                "- Sonic R - Can you feel the Sunshine?\n" +
                                "- Sonic X theme song (Gotta go fast)\n" +
                                "- Sonic the Hedgehog CD - Sonic Boom\n" +
                                "- Daytona USA - Rolling start!\n" +
                                "- Sonic the Hedgehog CD - Intro (Toot Toot Sonic Warrior)\n" +
                                "- Ugly Baby - Pronto\n" +
                                "- Katamari Damacy - Main Theme"
                                , creditsStyle);
                #endregion
                if (GUILayout.Button("Back", smallButton))
                {
                    page = MenuPage.None;
                }
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }
        pos = new Rect(
            Screen.width - 400,
            0,
            400,
            Screen.height
            );

        GUI.Box(pos, "");
        GUILayout.BeginArea(pos);
        //Game version and slogan
        GUIStyle version = new GUIStyle(GUI.skin.label);
        version.fontSize = 52;
        version.fontStyle = FontStyle.Bold;
        GUILayout.Label(GameVersion.AsString, version);
        GUILayout.Label(GameVersion.Slogan);
        //Page header

        GUILayout.FlexibleSpace();
        scrollPos = GUILayout.BeginScrollView(scrollPos);
        if (GUILayout.Button("Start Race"))
        {
            page = MenuPage.StartGame;
            lastActualPage = MenuPage.StartGame;
            startGame.gameName = GameSettings.user.playerName + "'s Game";
        }
        if (GUILayout.Button("Join Race"))
        {
            GetComponent<ServerBrowser>().Toggle();
        }
        if (GUILayout.Button("Options"))
        {
            page = MenuPage.Options;
            lastActualPage = MenuPage.Options;
        }
        if (GUILayout.Button("Customization"))
        {
            page = MenuPage.Customization;
            lastActualPage = MenuPage.Customization;
        }
        if (GUILayout.Button("Credits"))
        {
            page = MenuPage.Credits;
            lastActualPage = MenuPage.Credits;
        }

        if (GUILayout.Button("Visit Website"))
        {
            if (Application.isWebPlayer)
            {
                Application.ExternalEval("window.open('http://www.sanicball.com','SANICBALL');");
            }
            else
            {
                Application.OpenURL("http://www.sanicball.com");
            }
        }
        if (!Application.isWebPlayer)
        {
            if (GUILayout.Button("Quit"))
            {
                Application.Quit();
            }
        }

        GUILayout.EndScrollView();
        GUILayout.FlexibleSpace();

        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = 12;
        int currentYear = Mathf.Max(DateTime.Today.Year, 2014);
        GUILayout.Label("Sanicball is a game by BK-TN and is not related to Sega or Sonic Team in any way. © " + currentYear + " a few rights reserved", style);

        GUILayout.EndArea();
    }

	void GetAnyKeyForKeybinding() {
		foreach (KeyCode kc in validKeyCodes) {
			if (Input.GetKeyDown(kc) && kc != KeyCode.Escape) {
				KeybindInfo info = GameSettings.keybinds.Find(options.keybindToChange);
				info.keyCode = kc;
				options.keybindToChange = null;
				return;
			}
		}
	}


}

public enum MenuPage {
	None, StartGame, JoinGame, Options, Customization, Credits
}

[System.Serializable]
public class MenuPageStartGame {
	public MenuMain menu;
	public Server serverObject;
	public string gameName = "My game";
	string port = "25000";
	float maxPlayers = 12;
	bool addToServerList = false;
	string message = " ";

	public void DrawGUI() {
		GUILayout.Label("Server name");
		gameName = GUILayout.TextField(gameName,64);
		GUILayout.Label("Server port (Default 25000)");
		port = GUILayout.TextField(port);
		GUILayout.Label("Max players connected: "+(int)maxPlayers);
		maxPlayers = GUILayout.HorizontalSlider(maxPlayers,1,64);
		addToServerList = GUILayout.Toggle(addToServerList,"Show in server browser");
		//GUILayout.Label("NAT Status: "+NatChecker.status);
        if (message.StartsWith("@spinner/"))
        {
            Spinner.Draw(message.Replace("@spinner/", ""));
        }
        else
        {
            GUILayout.Label(message);
        }
		if (GUILayout.Button ("Start") || (Event.current.isKey && Event.current.keyCode == KeyCode.Return)) {
			StartServer();
		}
		if (GUILayout.Button("Back",GUI.skin.GetStyle ("smallButton"))) {
			menu.SetPage(MenuPage.None);
		}
		//GUIStyle thingStyle = new GUIStyle(GUI.skin.label);
		//thingStyle.fontSize = 18;
		//GUILayout.Label("If you're behind a router, you'll want to enable port forwarding for the port you use. This doesn't matter if you're playing alone.",thingStyle);
	}
	
	public void StartServer() {
		message = "";
		//Check if the port is a number
		int realPort;
		if (!int.TryParse(port,out realPort)) {
			message = "Port needs to be a number!";
			return;
		}
		//Check if we're already trying to join a server
		if (GameObject.FindObjectOfType<Server>() != null) {
			message = "You're already trying to start or join a server!";
			return;
		}
		message = "@spinner/Starting server..";
		int realMaxPlayers = (int)maxPlayers;
		Server s = (Server)GameObject.Instantiate(serverObject);
		s.settings.gameName = gameName;
		s.settings.port = realPort;
		s.settings.maxPlayers = realMaxPlayers;
		//s.settings.showOnList = showOnList;
		NetworkConnectionError connectionError = s.StartServer(addToServerList);
		if (connectionError != NetworkConnectionError.NoError) { //We have an error!
			GameObject.Destroy(s.gameObject);
			switch(connectionError) {
			case NetworkConnectionError.CreateSocketOrThreadFailure:
				message = "This port is already in use.";
				break;
			default:
				message = "An error occurred ("+connectionError.ToString()+")";
				break;
			}
		}
	}

}

[System.Serializable]
public class MenuPageJoinGame {
	public MenuMain menu;
	public Client clientObject;
	
	string ip = "";
	string port = "25000";
	
	ConnectionStatus connectionStatus = new ConnectionStatus();

	public void DrawGUI() {
		GUILayout.Label("IP Adress:");
		ip = GUILayout.TextField(ip);
		GUILayout.Label("Port (Default 25000)");
		port = GUILayout.TextField(port);
        if (connectionStatus != null && connectionStatus.message != null)
        {
            if (connectionStatus.message.StartsWith("@spinner/"))
            {
                Spinner.Draw(connectionStatus.message.Replace("@spinner/", ""));
            }
            else
            {
                GUILayout.Label(connectionStatus.message);
            }
        }
		if (!connectionStatus.isConnecting) {
			if (GUILayout.Button ("Connect") || (Event.current.isKey && Event.current.keyCode == KeyCode.Return)) {
				Connect ();
			}
		} else {
			if (GUILayout.Button("Cancel")) {
				Cancel();
			}
		}
		
		if (GUILayout.Button ("Back",GUI.skin.GetStyle("smallButton"))) {
			menu.SetPage(MenuPage.None);
		}
	}
	
	public void Connect() {
		if (GameObject.FindObjectOfType<Client>() != null) return;
		int realPort;
		if (!int.TryParse(port,out realPort)) {
			connectionStatus.message = "Port needs to be a number!";
			return;
		}
		Client c = (Client)GameObject.Instantiate(clientObject);
		connectionStatus = c.Connect(ip,realPort);
	}

	public void Cancel() {
		GameObject.Destroy(GameObject.FindObjectOfType<Client>().gameObject);
		connectionStatus.message = "Cancelled.";
		connectionStatus.isConnecting = false;
		Network.Disconnect();
	}
	
}

[System.Serializable]
public class MenuPageOptions {
	public MenuMain menu;

	int page = 0;

	//general settings
	public float resolution;
	bool enableTrails;
	float sensitivityMouse;
	float sensitivityKeyboard;
	float volume = 1;
	bool music = true;
	bool sanicSpeedSong = true;
	bool fullscreen;
	bool vsync = false;
	bool shadows = true;
	int aa = 8;
	//keybinds
	public string keybindToChange = null;
	//profile
	string status = "";
	string anonName;
	string loginName = "";
	string loginToken = "";
	bool rememberMe = GameSettings.user.rememberInfo;

	bool isVerifying = false;

	public void DrawGUI() {
		GUIStyle smallButton = GUI.skin.GetStyle("smallButton");

		if (page == 0) {
			if (GUILayout.Button("General"))
				page = 1;
			if (GUILayout.Button("Keybinds"))
				page = 2;
			if (GUILayout.Button("Profile"))
				page = 3;
			if (GUILayout.Button("Back",smallButton))
				menu.SetPage(MenuPage.None);
		}

		if (page == 1) { //General settings
			enableTrails = GUILayout.Toggle(enableTrails,"Fancy ball trails");
			fullscreen = GUILayout.Toggle (fullscreen,"Fullscreen");
			vsync = GUILayout.Toggle (vsync,"Vsync");
			Resolution currentRes = Screen.resolutions[(int)resolution];
			GUILayout.Label("Resolution: " + currentRes.width + "x" + currentRes.height);
			resolution = GUILayout.HorizontalSlider(resolution,0,Screen.resolutions.Length-1);
			//GUILayout.Label ("Field of View: "+fov);
			//fov = (int)GUILayout.HorizontalSlider(fov,50,80);
			//AA buttons
			GUILayout.Label("Anti-aliasing: x"+aa);
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Off",smallButton)) {aa=0;}
			if (GUILayout.Button("x2",smallButton)) {aa=2;}
			if (GUILayout.Button("x4",smallButton)) {aa=4;}
			if (GUILayout.Button("x8",smallButton)) {aa=8;}
			GUILayout.EndHorizontal();
			//Dynamic shadows
			shadows = GUILayout.Toggle(shadows,"Dynamic Shedews");
			//Camera speed for mouse / keyboard seperately
			GUILayout.Label("Camera speed (Mouse): "+(float)decimal.Round((decimal)sensitivityMouse,1));
			sensitivityMouse = GUILayout.HorizontalSlider(sensitivityMouse,0.1f,10);
			GUILayout.Label("Camera speed (Keyboard): "+(float)decimal.Round((decimal)sensitivityKeyboard,1));
			sensitivityKeyboard = GUILayout.HorizontalSlider(sensitivityKeyboard,0.1f,30);
			//sound volume
			GUILayout.Label ("Sound volume: "+(float)decimal.Round((decimal)volume,2)*100+"%");
			volume = GUILayout.HorizontalSlider(volume,0,1);
			//Music toggle
			music = GUILayout.Toggle(music,"Music");
			//sanic speed song
			sanicSpeedSong = GUILayout.Toggle(sanicSpeedSong,"Alt. song when going fast");
			if (GUILayout.Button ("Apply")) {
				Apply ();
			}
			if (GUILayout.Button ("Back",smallButton)) {
				page = 0;
				UpdateVarsGeneral();
			}
		}
		if (page == 2) { //Keybind changer
			foreach (KeyValuePair<string,KeybindInfo> bind in GameSettings.keybinds.GetAllBinds()) {
				GUILayout.BeginHorizontal();
				GUILayout.Label(bind.Value.name,GUILayout.MaxWidth(200));
				GUILayout.FlexibleSpace();
				GUIStyle keycodeStyle = new GUIStyle(smallButton);
				keycodeStyle.alignment = TextAnchor.MiddleRight;
				if (GUILayout.Button(GameSettings.keybinds.GetKeyCodeName(bind.Value.keyCode),keycodeStyle,GUILayout.MaxWidth(200))) {
					keybindToChange = bind.Key;
				}
				GUILayout.EndHorizontal();
			}
			if (GUILayout.Button("Reset keybinds",smallButton)) {
				GameSettings.keybinds = new Keybinds();
			}
			if (string.IsNullOrEmpty(keybindToChange)) {
				if (GUILayout.Button("Apply")) {
					Apply();
				}
				if (GUILayout.Button("Back",smallButton)) {
					page = 0;
					GameSettings.keybinds.LoadFromPrefs();
				}
			} else {
				KeybindInfo info = GameSettings.keybinds.Find(keybindToChange);
				GUILayout.Label("Press a key to use for '"+info.name+"'. Escape to cancel.");
			}
		}
		if (page == 3) { //Profile
			if (GameSettings.user.UseGJ) {//Logged in menu
				GUILayout.Label("Signed in to Game Jolt as " + GameSettings.user.playerName);
				GUILayout.Label("'Remember me' is " + (GameSettings.user.rememberInfo ? "ON" : "OFF"));
				GUILayout.Label("Player status: " + PlayerTypeHandler.GetPlayerType(GameSettings.user.playerName));
				if (GUILayout.Button("Log out")) {
					loginToken = GameSettings.user.token;
					loginName = GameSettings.user.playerName;
					GameSettings.user.token = "";
					GameSettings.user.Save();
				}
			} else { //Anon menu
				GUILayout.Label("Playing anonymously as '"+GameSettings.user.playerName+"'");
				GUILayout.Label("'Remember me' is " + (GameSettings.user.rememberInfo ? "ON" : "OFF"));
				GUILayout.Label("Change your username:");
				anonName = GUILayout.TextField(anonName,32);
				if (GUILayout.Button("Change",smallButton)) {
					GameSettings.user.playerName = anonName;
				}
				if (!GameSettings.user.offlineMode) {
					if (GUILayout.Button("Log in with Game Jolt",GUI.skin.GetStyle("smallbuttongreen"))) {
						page = 4;
					}
				}

			}

			if (GUILayout.Button("Back",smallButton)) {
				page = 0;
				UpdateVarsProfile();
			}
		}
		if (page == 4) { //Log in
			GUILayout.Label("GJ username: ");
			GUI.SetNextControlName("Focus");
			loginName = GUILayout.TextField(loginName,128);
			GUILayout.Label("User token: ");
			loginToken = GUILayout.PasswordField(loginToken,(char)0x25CF,128);
			rememberMe = GUILayout.Toggle(rememberMe,"Remember me");
			if ((GUILayout.Button("Log in") || (Event.current.isKey && Event.current.keyCode == KeyCode.Return)) && !isVerifying) {
				status = "";
				if (loginName.Trim().Length <= 0) { //Has the user typed something in the username field?
					status = "You need to type an username.";
				} else if (loginToken.Trim().Length <= 0) { //Has the user typed something in the token field?
					status = "You need to type in your token.";
				} else { //Log in with the Game Jolt API
					status = "@spinner/Logging in...";
					isVerifying = true;
					GJAPI.Users.Verify(loginName,loginToken);
					GJAPI.Users.VerifyCallback += LogInCallback;
				}
			}
            if (status.StartsWith("@spinner/"))
            {
                Spinner.Draw(status.Replace("@spinner/", ""));
            }
            else
            {
                GUILayout.Label(status);
            }
			if (GUILayout.Button("Back",smallButton) && !isVerifying) {
				page = 3;
			}
		}
	}

	void LogInCallback(bool success) {
		isVerifying = false;
		GJAPI.Users.VerifyCallback -= LogInCallback;
		if (success) {
			status = "";
			GameSettings.user.playerName = loginName;
			GameSettings.user.token = loginToken;
			GameSettings.user.rememberInfo = rememberMe;
			GameSettings.user.Save();
			page = 3;
		} else {
			status = "Username or token is incorrect!";
		}
	}
	
	public void UpdateVarsGeneral() {
		resolution = GameSettings.resolution;
		enableTrails = GameSettings.enableTrails;
		sensitivityMouse = GameSettings.sensitivityMouse;
		sensitivityKeyboard = GameSettings.sensitivityKeyboard;
		volume = GameSettings.volume;
		music = GameSettings.music;
		sanicSpeedSong = GameSettings.sanicSpeedSong;
		fullscreen = GameSettings.fullscreen;
		vsync = GameSettings.vsync;
		shadows = GameSettings.shadows;
		aa = GameSettings.aa;
	}

	public void UpdateVarsProfile() {
		anonName = GameSettings.user.playerName;
	}

	public void Apply() {
		GameSettings.resolution = resolution;
		GameSettings.enableTrails = enableTrails;
		GameSettings.sensitivityMouse = sensitivityMouse;
		GameSettings.sensitivityKeyboard = sensitivityKeyboard;
		GameSettings.volume = volume;
		GameSettings.music = music;
		GameSettings.sanicSpeedSong = sanicSpeedSong;
		GameSettings.fullscreen = fullscreen;
		GameSettings.vsync = vsync;
		GameSettings.shadows = shadows;
		//Camera.None.fieldOfView = fov;
		//////mouse speed goes here
		GameSettings.aa = aa;
		GameSettings.Apply(true);
		GameSettings.Save();
	}
}

[System.Serializable]
public class MenuPageCustomization
{
    public MenuMain menu;

    int page = 0;

    private Vector2 scroll;

    public void DrawGUI()
    {
        GUIStyle smallButton = GUI.skin.GetStyle("smallButton");

        if (page == 0)
        {
            if (GUILayout.Button("Music"))
                page = 1;
            if (GUILayout.Button("Back", smallButton))
                menu.SetPage(MenuPage.None);
        }

        if (page == 1)
        {
            // music
            GUILayout.Space(10);
            scroll = GUILayout.BeginScrollView(scroll);
            GUIStyle style = new GUIStyle(GUI.skin.GetStyle("SmallButtonOdd")) { fontSize = 14, alignment = TextAnchor.MiddleRight };
            GUIStyle styleText = new GUIStyle(GUI.skin.GetStyle("LabelOdd")) { wordWrap = true, fontSize = 14, alignment = TextAnchor.MiddleLeft };
            if (GameSettings.userPlaylist.Count > 0)
            {
                GUILayout.Label("My music", new GUIStyle(GUI.skin.label) { wordWrap = true, fontSize = 18 });
                foreach (ExternalSong s in GameSettings.userPlaylist.ToArray())
                {
                    GUILayout.Space(1);
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(s.song.name, styleText);
                    if (GUILayout.Button("Remove", style, GUILayout.MaxWidth(style.CalcSize(new GUIContent("Remove")).x), GUILayout.ExpandHeight(true)))
                    {
                        s.Dispose();
                        GameSettings.userPlaylist.Remove(s);
                    }
                    GUILayout.EndHorizontal();
                    GUILayout.Space(1);
                }
                GUILayout.Space(10);
            }
            GUILayout.Label("Inbuilt music", new GUIStyle(GUI.skin.label) { wordWrap = true, fontSize = 18 });
            foreach (Song s in Global.playlist)
            {
                GUILayout.Space(1);
                GUILayout.BeginHorizontal();
                style = new GUIStyle(GUI.skin.GetStyle(s.enabled ? "SmallButtonOdd" : "SmallButtonDisabled")) { fontSize = 14, alignment = TextAnchor.MiddleRight };
                styleText = new GUIStyle(GUI.skin.GetStyle(s.enabled ? "LabelOdd" : "LabelDisabled")) { wordWrap = true, fontSize = 14, alignment = TextAnchor.MiddleLeft };
                GUILayout.Label(s.name, styleText);
                if (s.enabled)
                {
                    if (GUILayout.Button("Disable", style, GUILayout.MaxWidth(style.CalcSize(new GUIContent("Disable")).x), GUILayout.ExpandHeight(true)))
                    {
                        s.enabled = false;
                    }
                }
                else
                {
                    if (GUILayout.Button("Enable", style, GUILayout.MaxWidth(style.CalcSize(new GUIContent("Disable")).x), GUILayout.ExpandHeight(true)))
                    {
                        s.enabled = true;
                    }
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(1);
            }
            GUILayout.EndScrollView();
            GUILayout.Space(10);
            if (GUILayout.Button("Add", smallButton))
            {
                menu.StartCoroutine(AddMusic());
            }
            if (GUILayout.Button("Back", smallButton))
            {
                page = 0;
                GameSettings.Save();
            }
        }
    }

    private IEnumerator AddMusic()
    {
        if (Screen.fullScreen)
        {
            Screen.SetResolution(Screen.width, Screen.height, false);
            yield return new WaitForSeconds(0.5f);
        }
        System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();
        openFileDialog.Filter = "Sound Files (*.mp3, *.ogg)|*.mp3;*.ogg|All Files (*.*)|*.*";
        openFileDialog.Multiselect = true;
        if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            foreach (string s in openFileDialog.FileNames)
            {
                GameSettings.userPlaylist.Add(new ExternalSong(s));
            }
        }
        Screen.SetResolution(Screen.width, Screen.height, GameSettings.fullscreen);
        yield return null;
    }
}