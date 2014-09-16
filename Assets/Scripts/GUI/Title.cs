using UnityEngine;
using System.Collections;

public class Title : MonoBehaviour {

	public Texture2D titleTexture;

	void OnGUI() {
		GUIStyle style = new GUIStyle();
		style.normal.background = titleTexture;
		int width = Screen.width - 400;
		float height = width * 0.33f;
		GUI.Box(new Rect(0,-height*0.2f,width,height),"",style);
	}
}
