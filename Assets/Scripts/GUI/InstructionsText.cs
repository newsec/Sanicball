using UnityEngine;
using System.Collections;

public class InstructionsText : MonoBehaviour {

	public GUISkin skin;
	bool extended = false;
	Vector2 scrollPos;

	void OnGUI() {
		GUI.skin = skin;
		int left = 8;
		int top = (int)(Screen.height*0.4f);
		int width = 340;
		int height = Screen.height-top;
		GUILayout.BeginArea(new Rect(left,top,width,height));
		scrollPos = GUILayout.BeginScrollView(scrollPos);

		string gofast = GameSettings.keybinds.GetKeyCodeName(GameSettings.keybinds.Find("moveforward").keyCode)
			+ GameSettings.keybinds.GetKeyCodeName(GameSettings.keybinds.Find("moveleft").keyCode)
			+ GameSettings.keybinds.GetKeyCodeName(GameSettings.keybinds.Find("moveback").keyCode)
			+ GameSettings.keybinds.GetKeyCodeName(GameSettings.keybinds.Find("moveright").keyCode);
		gofast = gofast.ToUpper();
		string brakefast = GameSettings.keybinds.GetKeyCodeName(GameSettings.keybinds.Find("brake").keyCode).ToUpper();
		string jumpfast = GameSettings.keybinds.GetKeyCodeName(GameSettings.keybinds.Find("jump").keyCode).ToUpper();
		string respawnfast = GameSettings.keybinds.GetKeyCodeName(GameSettings.keybinds.Find("respawn").keyCode).ToUpper();
		string goslow = brakefast + "+" + gofast;
		string nextsong = GameSettings.keybinds.GetKeyCodeName(GameSettings.keybinds.Find("nextsong").keyCode).ToUpper();
		string menu = GameSettings.keybinds.GetKeyCodeName(GameSettings.keybinds.Find("menu").keyCode).ToUpper();
		string chat = GameSettings.keybinds.GetKeyCodeName(GameSettings.keybinds.Find("chat").keyCode).ToUpper();

		GUILayout.Label("INSTRICTIONS\n" +
		          gofast + ": go fast\n" +
		          brakefast + ": brake fast\n" +
		          jumpfast + ": jump fast\n" +
		          respawnfast + ": respawn fast"
		          );
		GUIStyle style = new GUIStyle(GUI.skin.button);
		style.fontSize = GUI.skin.label.fontSize;
		string bText = "Show full controls";
		if (extended) {
			bText = "Hide full controls";
			GUILayout.Label("MOUSE or ARROWS: control camera\n" +
				goslow + ": go slow\n" +
			    nextsong + ": next song\n" +
			    "ESCAPE or "+menu+": ingame menu\n" +
			    chat + ": use chat"
			                );
		}
		if (GUILayout.Button(bText,style,GUILayout.Width(250))) {
			extended = !extended;
		}
		GUILayout.EndScrollView();
		GUILayout.EndArea();

	}
}
