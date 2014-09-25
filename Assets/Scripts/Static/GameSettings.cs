using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class GameSettings {

	public static Keybinds keybinds = new Keybinds();
	public static User user = new User();
	public static float resolution;
	public static bool enableTrails = true;
	public static bool fullscreen;
	public static int aa;
	public static bool vsync;
	public static bool shadows = true;
	public static float sensitivityMouse = 3;
	public static float sensitivityKeyboard = 15;
	public static float volume = 1;
	public static bool music = true;
	public static bool sanicSpeedSong = true;
    public static List<ExternalSong> userPlaylist = new List<ExternalSong>();
    private static bool musicLoaded = false;
	
	public static void Init () {
		Load();
		Apply(false);
	}

	public static void Load() {
		keybinds.LoadFromPrefs();
		user.playerName = PlayerPrefs.GetString("Settings_PlayerName",user.playerName);
		if (user.playerName.Length > 32) {
			user.playerName.Remove(32);
		}
		user.token = PlayerPrefs.GetString("Settings_Token",user.token);
		//resolution = PlayerPrefs.GetFloat("Settings_Resolution",resolution);
		enableTrails = PlayerPrefsX.GetBool("Settings_EnableTrails",enableTrails);
		fullscreen = PlayerPrefsX.GetBool("Settings_Fullscreen",fullscreen);
		aa = PlayerPrefs.GetInt("Settings_AA",aa);
		vsync = PlayerPrefsX.GetBool("Settings_Vsync",vsync);
		shadows = PlayerPrefsX.GetBool("Settings_Shadows",shadows);
		sensitivityMouse = PlayerPrefs.GetFloat("Settings_SensitivityMouse",sensitivityMouse);
		sensitivityKeyboard = PlayerPrefs.GetFloat("Settings_SensitivityKeyboard",sensitivityKeyboard);
		volume = PlayerPrefs.GetFloat("Settings_Volume",volume);
		music = PlayerPrefsX.GetBool("Settings_Music",music);
		sanicSpeedSong = PlayerPrefsX.GetBool("Settings_SanicSpeedSong",sanicSpeedSong);
        string savedPlaylist = PlayerPrefs.GetString("User_Playlist", "");
        if (!string.IsNullOrEmpty(savedPlaylist) && !musicLoaded)
        {
            foreach(string s in savedPlaylist.Split('@'))
            {
                GameSettings.userPlaylist.Add(new ExternalSong(s));
            }
        }

        string disabledSongs = PlayerPrefs.GetString("User_DisabledSongs", "");
        if (!string.IsNullOrEmpty(disabledSongs) && !musicLoaded)
        {
            foreach (string s in disabledSongs.Split('@'))
            {
                foreach(Song song in Global.playlist)
                {
                    if(song.name.Equals(s))
                    {
                        song.enabled = false;
                    }
                }
            }
        }
        musicLoaded = true;

		//Get resolution
		for (int i=0;i<Screen.resolutions.Length;i++) {
			Resolution r = Screen.resolutions[i];
			if (Screen.width == r.width && Screen.height == r.height) {
				resolution = i;
				break;
			}
		}
	}

	public static void Save() {
		keybinds.SaveToPrefs();
		//PlayerPrefs.SetFloat("Settings_Resolution",resolution); Unity gets the latest resolution automagically
		PlayerPrefsX.SetBool("Settings_EnableTrails",enableTrails);
		PlayerPrefsX.SetBool("Settings_Fullscreen",fullscreen);
		PlayerPrefs.SetInt("Settings_AA",aa);
		PlayerPrefsX.SetBool("Settings_Vsync",vsync);
		PlayerPrefsX.SetBool("Settings_Shadows",shadows);
		PlayerPrefs.SetFloat("Settings_SensitivityMouse",sensitivityMouse);
		PlayerPrefs.SetFloat("Settings_SensitivityKeyboard",sensitivityKeyboard);
		PlayerPrefs.SetFloat("Settings_Volume",volume);
		PlayerPrefsX.SetBool("Settings_Music",music);
		PlayerPrefsX.SetBool("Settings_SanicSpeedSong",sanicSpeedSong);
        string toBeSavedPlaylist = "";
        foreach(ExternalSong s in userPlaylist)
        {
            toBeSavedPlaylist += s.filename + "@";
        }
        if(toBeSavedPlaylist.Length > 0)
        {
            // remove the last @
            toBeSavedPlaylist = toBeSavedPlaylist.Remove(toBeSavedPlaylist.Length - 1, 1);
        }
        PlayerPrefs.SetString("User_Playlist", toBeSavedPlaylist);
        string toBeSavedDisabledSongs = "";
        foreach (Song s in Global.playlist)
        {
            if (!s.enabled)
            {
                toBeSavedDisabledSongs += s.name + "@";
            }
        }
        if (toBeSavedDisabledSongs.Length > 0)
        {
            // remove the last @
            toBeSavedDisabledSongs = toBeSavedDisabledSongs.Remove(toBeSavedDisabledSongs.Length - 1, 1);
        }
        PlayerPrefs.SetString("User_DisabledSongs", toBeSavedDisabledSongs);
		PlayerPrefs.Save();
	}
	
	public static void Apply(bool changeWindow) {
		if (changeWindow) {
			//Resolution and fullscreen
			if ((int)resolution < Screen.resolutions.Length) {
				Resolution res = Screen.resolutions[(int)resolution];
				if (Screen.width != res.width || Screen.height != res.height || fullscreen != Screen.fullScreen)
					Screen.SetResolution(res.width,res.height,fullscreen);
			}
		}
		//AA
		QualitySettings.antiAliasing = aa;
		//Vsync
		if (vsync) {QualitySettings.vSyncCount=1;} else {QualitySettings.vSyncCount=0;}
		//Shadows
		GameObject dl = GameObject.Find ("Directional light");
		if (dl != null) {
			LightShadows ls;
			if (shadows) {ls=LightShadows.Hard;} else {ls=LightShadows.None;}
			dl.light.shadows = ls;
		}
		//Volume
		AudioListener.volume = volume;
		//Mute
		GameObject go = GameObject.Find("music");
		if (go != null)
			go.audio.mute = !music;
		//Camera.main.fieldOfView = fov;
	}

}

public class User {
	public string playerName = "unknown";
	public string token = "";
	public bool rememberInfo = false;
	public bool offlineMode = false;

	/// <summary>
	/// Returns true if the user token is set.
	/// </summary>
	public bool UseGJ {
		get {
			return token != string.Empty;
		}
	}

	/// <summary>
	/// Saves user data if "Remember me" was checked
	/// </summary>
	public void Save() {
		if (rememberInfo) {
			PlayerPrefs.SetString("Settings_PlayerName",playerName);
			PlayerPrefs.SetString("Settings_Token",token);
		} else {
			PlayerPrefs.DeleteKey("Settings_PlayerName");
			PlayerPrefs.DeleteKey("Settings_Token");
		}
	}
}

public class Keybinds {
	private Dictionary<string,KeybindInfo> binds = new Dictionary<string,KeybindInfo>();

	public bool isTyping = false;

	public Keybinds() {
		binds.Add("moveforward", new KeybindInfo("Roll Forward",KeyCode.W,true));
		binds.Add("moveleft",new KeybindInfo("Roll Left",KeyCode.A,true));
		binds.Add("moveback",new KeybindInfo("Roll Backwards",KeyCode.S,true));
		binds.Add("moveright",new KeybindInfo("Roll Right",KeyCode.D,true));

		binds.Add("cameraup",new KeybindInfo("Camera Up",KeyCode.UpArrow,false));
		binds.Add("cameraleft",new KeybindInfo("Camera Left",KeyCode.LeftArrow,false));
		binds.Add("cameradown",new KeybindInfo("Camera Down",KeyCode.DownArrow,false));
		binds.Add("cameraright",new KeybindInfo("Camera Right",KeyCode.RightArrow,false));

		binds.Add("brake",new KeybindInfo("Brake",KeyCode.LeftShift,true));
		binds.Add("jump",new KeybindInfo("Jump",KeyCode.Space,true));
		binds.Add("respawn",new KeybindInfo("Respawn",KeyCode.Backspace,true));
		binds.Add("nextsong",new KeybindInfo("Next Song",KeyCode.N,true));
		binds.Add("menu",new KeybindInfo("Menu",KeyCode.P,false));
		binds.Add("chat",new KeybindInfo("Chat",KeyCode.Return,false));
	}

	public Dictionary<string,KeybindInfo> GetAllBinds() {
		return binds;
	}

	public KeybindInfo Find(string name) {
		KeybindInfo output = null;
		if (!binds.TryGetValue(name,out output)) {
			Debug.LogError("Key '"+name+"' doesn't exist in keybinds.");
		}
		return output;
	}

	public bool GetKey(string name) {
		if (isTyping && Find(name).disableWhenTyping) return false;
		return Input.GetKey(Find(name).keyCode);
	}

	public bool GetKeyDown(string name) {
		if (isTyping && Find(name).disableWhenTyping) return false;
		return Input.GetKeyDown(Find(name).keyCode);
	}

	public bool GetKeyUp(string name) {
		if (isTyping && Find(name).disableWhenTyping) return false;
		return Input.GetKeyUp(Find(name).keyCode);
	}

	public void LoadFromPrefs() {
		foreach (var bind in binds) {
			bind.Value.keyCode = (KeyCode) PlayerPrefs.GetInt("Keybinds_"+bind.Key,(int)bind.Value.keyCode);
		}
	}

	public void SaveToPrefs() {
		foreach (var bind in binds) {
			PlayerPrefs.SetInt("Keybinds_"+bind.Key,(int)bind.Value.keyCode);
		}
	}

	public string GetKeyCodeName(KeyCode kc) {
		switch(kc) {
		case KeyCode.Alpha0:
			return "0";
		case KeyCode.Alpha1:
			return "1";
		case KeyCode.Alpha2:
			return "2";
		case KeyCode.Alpha3:
			return "3";
		case KeyCode.Alpha4:
			return "4";
		case KeyCode.Alpha5:
			return "5";
		case KeyCode.Alpha6:
			return "6";
		case KeyCode.Alpha7:
			return "7";
		case KeyCode.Alpha8:
			return "8";
		case KeyCode.Alpha9:
			return "9";
		case KeyCode.LeftAlt:
			return "Alt";
		case KeyCode.LeftShift:
			return "Shift";
		case KeyCode.Return:
			return "Enter";
		default:
			return kc.ToString();
		}
	}
}

public class KeybindInfo {
	public string name;
	public KeyCode keyCode;
	public bool disableWhenTyping;

	public KeybindInfo(string name, KeyCode keyCode, bool disableWhenTyping) {
		this.name = name;
		this.keyCode = keyCode;
		this.disableWhenTyping = disableWhenTyping;
	}
	
}