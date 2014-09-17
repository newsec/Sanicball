using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// This was changed to a file so that it wouldn't conflict with normal Sanicball.
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

    public static XmlConfiguration settings;
	
	public static void Init () {
        settings = new XmlConfiguration("Settings", Application.persistentDataPath);
        settings.AddSetting("player-name", user.playerName);
        settings.AddSetting("token", user.token);
        settings.AddSetting("enable-trails", enableTrails);
        settings.AddSetting("fullscreen", fullscreen);
        settings.AddSetting("aa-level", aa);
        settings.AddSetting("vsync", vsync);
        settings.AddSetting("shadows", shadows);
        settings.AddSetting("sensitivity-mouse", sensitivityMouse);
        settings.AddSetting("sensitivity-keyboard", sensitivityKeyboard);
        settings.AddSetting("volume", volume);
        settings.AddSetting("music", music);
        settings.AddSetting("sanic-speed-song", sanicSpeedSong);
        settings.FinishedAddingSettings();
		Apply(false);
	}

	public static void Load() {
		keybinds.LoadFromPrefs();
		user.playerName = settings.GetSetting<string>("player-name");
		if (user.playerName.Length > 32) {
			user.playerName.Remove(32);
		}
        user.token = settings.GetSetting<string>("token");
        enableTrails = settings.GetSetting<bool>("enable-trails");
        fullscreen = settings.GetSetting<bool>("fullscreen");
        aa = settings.GetSetting<int>("aa-level");
        vsync = settings.GetSetting<bool>("vsync");
        shadows = settings.GetSetting<bool>("shadows");
        sensitivityMouse = settings.GetSetting<float>("sensitivity-mouse");
        sensitivityKeyboard = settings.GetSetting<float>("sensitivity-keyboard");
        volume = settings.GetSetting<float>("volume");
        music = settings.GetSetting<bool>("music");
        sanicSpeedSong = settings.GetSetting<bool>("sanic-speed-song");

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
        settings.SetValue("enable-trails", enableTrails);
        settings.SetValue("fullscreen", fullscreen);
        settings.SetValue("aa-level", aa);
        settings.SetValue("vsync", vsync);
        settings.SetValue("shadows", shadows);
        settings.SetValue("sensitivity-mouse", sensitivityMouse);
        settings.SetValue("sensitivity-keyboard", sensitivityKeyboard);
        settings.SetValue("volume", volume);
        settings.SetValue("music", music);
        settings.SetValue("sanic-speed-song", sanicSpeedSong);
        settings.Save();
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