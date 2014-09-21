using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MenuLogin : MonoBehaviour {

	public GUISkin skin;

	public Intro introToStart;

	public int gameID;
	public string privateKey;

	string username = "";
	string token = "";
	bool rememberMe = false;

	string status = "";
	MenuMain menuMain;

	bool started = false;

	bool isStarting = true;
	bool isVerifying = false;
	bool isRemembering = false;

	LoginMethod method = LoginMethod.Undecided;

	// Use this for initialization
	void Start () {
		//Init the GJAPI and get the list of special players
		GJAPI.Init(gameID,privateKey,true,1); //Set this 'true' value to 'false' to disable GJ debug messages
		GJAPI.Data.Get("players");
		GJAPI.Data.GetCallback += LoadPlayerTypes;

		//Check the version of saved data and delete records if older (stupid, but gets rid of obsolete data)
		float savedVersion = PlayerPrefs.GetFloat("Version",-1f);
		if (savedVersion < GameVersion.AsFloat) {
			PlayerPrefs.DeleteAll();
			PlayerPrefs.SetFloat("Version",GameVersion.AsFloat);
			PlayerPrefs.Save();
			Debug.Log("Saved data is from an older game version! Removing everything.");

		}
	}

	void LoadPlayerTypes(string data) {
		//Parse and add all players to the specialUsers list
		if (string.IsNullOrEmpty(data)) { //Oh shit, the connection failed
			isStarting = false;
			GJAPI.Data.GetCallback -= LoadPlayerTypes;
			StartOfflineMode();
			return;
		}
		var specialUsers = new Dictionary<string, PlayerType>();
		string[] pairs = data.Split(';');
		foreach (string s in pairs) {
			string[] nameTypePair = s.Split('=');
			string nameStr = nameTypePair[0];
			string typeStr = nameTypePair[1];
			int typeInt = 0;
			if (int.TryParse(typeStr, out typeInt)) {
				typeInt += 2; //Add 2 to type to match with PlayerType enum
				specialUsers.Add(nameStr,(PlayerType)typeInt);
			}
		}
		PlayerTypeHandler.SetSpecialUsers(specialUsers);
		isStarting = false;
		GJAPI.Data.GetCallback -= LoadPlayerTypes;
		CheckIfSignedIn();
	}

	void CheckIfSignedIn() {
		GameSettings.user.playerName = PlayerPrefs.GetString("Settings_PlayerName","");
		GameSettings.user.token = PlayerPrefs.GetString("Settings_Token","");
		if (GameSettings.user.playerName.Length > 32) {
			GameSettings.user.playerName = GameSettings.user.playerName.Remove(32);
		}
		if (GameSettings.user.playerName.Trim() != "") {
			rememberMe = true;
			username = GameSettings.user.playerName;
			//Check if logged in to Game Jolt
			if (GameSettings.user.UseGJ) {
				Debug.Log("Name found with token, verifying");
				method = LoginMethod.GameJolt;
				username = GameSettings.user.playerName;
				token = GameSettings.user.token;
				isRemembering = true;
				isVerifying = true;
				GJAPI.Users.Verify(username,token);
				GJAPI.Users.VerifyCallback += LogInCallback;
			} else {
				Accept();
				Debug.Log("Name found with no token, starting as anon");
			}
		}
	}

	void StartOfflineMode() {
		Debug.Log("Offline mode!");
		method = LoginMethod.Anon;
		GameSettings.user.offlineMode = true;
		GameSettings.user.playerName = PlayerPrefs.GetString("Settings_PlayerName","");
		if (GameSettings.user.playerName.Length > 32) {
			GameSettings.user.playerName = GameSettings.user.playerName.Remove(32);
		}
		if (GameSettings.user.playerName.Trim() != "") {
			rememberMe = true;
			username = GameSettings.user.playerName;
			Accept();
			Debug.Log("Name found while in offline mode, starting as anon");
		}
	}
	
	// Update is called once per frame
	void OnGUI() {
		if (started) return;
		GUI.skin = skin;
		GUIStyle smallButton = GUI.skin.GetStyle("smallButton");

		int startX = Mathf.Max(0,Screen.width/2-300);
		int startY = Mathf.Max(0,Screen.height/2-150);

		Rect rect = new Rect(
			startX,
			startY,
			Mathf.Min(Screen.width - startX,600),
			Mathf.Min(Screen.height - startY,300)
			);

		GUILayout.BeginArea(rect);
		GUIStyle centerStyle = new GUIStyle(GUI.skin.label);
		centerStyle.alignment = TextAnchor.MiddleCenter;
		if (isStarting) {
			GUILayout.FlexibleSpace();
            Spinner.DrawAt(new Vector2(rect.width / 2 - 16, rect.height / 2 - 16 - 32));
            GUILayout.Space(32);
			GUILayout.Label("Starting up...",centerStyle);
			GUILayout.FlexibleSpace();
		} else if (isRemembering) { //Don't show the login form if remembering
			GUILayout.FlexibleSpace();
            Spinner.DrawAt(new Vector2(rect.width / 2 - 16, rect.height / 2 - 16 - 32));
            GUILayout.Space(32);
			GUILayout.Label("Logging in...",centerStyle);
			GUILayout.FlexibleSpace();
		} else {
			if (method == LoginMethod.Undecided) {
				GUILayout.Label("Welcome to Sanicball!");
				GUILayout.Label("Select an option below. Signing in with Game Jolt will in a future update let you submit your highscores online.");
				GUILayout.FlexibleSpace();
				GUILayout.BeginHorizontal();
				if (GUILayout.Button("Sign in with Game Jolt",GUI.skin.GetStyle("smallButtonGreen"))) {
					method = LoginMethod.GameJolt;
				}
				GUILayout.FlexibleSpace();
				if (GUILayout.Button("Stay anonymous",smallButton)) {
					method = LoginMethod.Anon;
				}
				GUILayout.EndHorizontal();
			}
			if (method == LoginMethod.GameJolt) {
				GUILayout.Label("GJ username: ");
				GUI.SetNextControlName("Focus");
				username = GUILayout.TextField(username,128);
				GUILayout.Label("User token: ");
				token = GUILayout.PasswordField(token,(char)0x25CF,128);
				rememberMe = GUILayout.Toggle(rememberMe,"Remember me");
				GUILayout.FlexibleSpace();
				GUILayout.BeginHorizontal();
				if ((GUILayout.Button("Log in",smallButton) || (Event.current.isKey && Event.current.keyCode == KeyCode.Return)) && !isVerifying) {
					status = "";
					if (username.Trim().Length <= 0) { //Has the user typed something in the username field?
						status = "You need to type an username.";
					} else if (token.Trim().Length <= 0) { //Has the user typed something in the token field?
						status = "You need to type in your token.";
					} else { //Log in with the Game Jolt API
						status = "@spinner/Logging in...";
						isVerifying = true;
						GJAPI.Users.Verify(username,token);
						GJAPI.Users.VerifyCallback += LogInCallback;
					}
				}
				GUILayout.FlexibleSpace();
				if (GUILayout.Button("Cancel",smallButton)) {
					method = LoginMethod.Undecided;
					status = "";
				}
				GUILayout.EndHorizontal();
			}
			if (method == LoginMethod.Anon) {
				if (GameSettings.user.offlineMode) {
					GUILayout.Label("Failed to connect to Game Jolt! However, you can still play multiplayer anonymously.");
				}
				GUILayout.Label("Type the name you want to use:");
				GUI.SetNextControlName("Focus");
				username = GUILayout.TextField(username,32);
				GUILayout.Label("Your name will be slightly grayed out in multiplayer to avoid impersonation.");
				rememberMe = GUILayout.Toggle(rememberMe,"Remember name");
				GUILayout.FlexibleSpace();
				GUILayout.BeginHorizontal();
				if (GUILayout.Button("Okay",smallButton) || (Event.current.isKey && Event.current.keyCode == KeyCode.Return)) {
					status = "";
					if (username.Trim().Length <= 0) {
					    status = "You need to type something.";
					} else {
						Accept();
					}
				}
				if (!GameSettings.user.offlineMode) {
					GUILayout.FlexibleSpace();
					if (GUILayout.Button("Cancel",smallButton)) {
						method = LoginMethod.Undecided;
						status = "";
					}
				}
				GUILayout.EndHorizontal();
			}
            if (status.StartsWith("@spinner/"))
            {
                Spinner.Draw(status.Replace("@spinner/", ""));
            }
            else
            {
                GUILayout.Label(status);
            }
		}
		GUILayout.EndArea();
	}

	void LogInCallback(bool success) {
		isVerifying = false;
		GJAPI.Users.VerifyCallback -= LogInCallback;
		if (success) {
			status = "Success!";
			GameSettings.user.token = token;
			Accept();

		} else {
			status = "Username or token is incorrect!";
			isRemembering = false; //If we were trying to verify saved info, disable isRemembering
		}
	}

	void Accept() {
		GameSettings.user.playerName = username;
		GameSettings.user.rememberInfo = rememberMe;
		GameSettings.user.Save();
		introToStart.started = true;
		started = true;
	}

	enum LoginMethod {
		Undecided,GameJolt,Anon
	}
}