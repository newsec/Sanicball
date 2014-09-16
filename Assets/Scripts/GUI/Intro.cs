using UnityEngine;
using System.Collections;

public class Intro : MonoBehaviour {

	int curImg = 0;
	public bool started = false;
	public GUITexture[] images;
	public int imgCount = 3;
	public float imgTime = 0.2f;
	public float fadeTime = 0.05f;

	float timer = 0;

	bool isOn = false;
	bool isFadeOut = false;

	// Use this for initialization
	void Start () {
		timer = imgTime;
	}
	
	// Update is called once per frame
	void Update () {
		if (!started) return;
		if (isOn) {
			timer-=Time.deltaTime;
			if (timer <= 0) {
				isOn = false; //Stop the timer
				isFadeOut = true;
			}
		} else {
			//Fade in or out
			if (isFadeOut) {
				float a = images[curImg].color.a;
				a -= fadeTime*Time.deltaTime;
				images[curImg].color = new Color(0.5f,0.5f,0.5f,a);
				if (a <= 0) {
					NextImage();
					isFadeOut = false;
				}
			} else {
				float a = images[curImg].color.a;
				a += fadeTime*Time.deltaTime;
				images[curImg].color = new Color(0.5f,0.5f,0.5f,a);
				if (a >= 0.5f) {
					isOn = true;
				}
			}
		}
		if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(0)) {
			GoToMenu();
		}
	}

	void NextImage() {
		if (curImg >= imgCount-1) {
			GoToMenu();
			return;
		}
		images[curImg].enabled = false;
		curImg++;
		images[curImg].enabled = true;
		images[curImg].color = new Color(0.5f,0.5f,0.5f,0);
		timer += imgTime;
	}

	void GoToMenu () {
		Application.LoadLevel("Menu");
	}
}
