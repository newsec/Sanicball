using UnityEngine;
using System.Collections;

public static class GUIX {

	public static void ShadowLabel (Rect rect, string text, GUIStyle style, int offset) {
		style = new GUIStyle(style);
		style.normal.textColor = Color.black;
		rect = new Rect(rect.x+offset,rect.y+offset,rect.width,rect.height);
		GUI.Label(rect,text,style);
	}
}
