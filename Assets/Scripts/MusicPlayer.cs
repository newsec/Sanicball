using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class MusicPlayer : MonoBehaviour {
	public GUISkin skin;
	
	public Song[] playlist;
	public AudioClip mlgSong;
	public AudioSource fastSource;

	int currentSongID;
	bool isPlaying;

	[System.NonSerialized]
	public bool fastMode = false;

	string currentSongCredits;

	//Song credits
	float timer = 0;
	float slidePosition;
	float slidePositionMax = 20;
	
	void Start() {
		slidePosition = slidePositionMax;
		ShuffleSongs();
		audio.clip = playlist[0].clip;
		currentSongID = 0;
		isPlaying = audio.isPlaying;
		if (!GameSettings.music) {
			fastSource.Stop();
		}
	}
	void Update() {
		//If it's not playing but supposed to play, change song
		if ((!audio.isPlaying || GameSettings.keybinds.GetKeyDown("nextsong")) && isPlaying) {
			if (currentSongID<playlist.Length-1) {
				currentSongID++;
			} else {
				currentSongID = 0;
			}
			audio.clip = playlist[currentSongID].clip;
			slidePosition = slidePositionMax;
			Play ();
		}
		//Timer
		if (timer > 0) {
			timer -= Time.deltaTime;
		}

		if (fastMode && fastSource.volume < 1) {
			fastSource.volume = Mathf.Min(1,fastSource.volume + Time.deltaTime * 0.25f);
			audio.volume = 0.5f - fastSource.volume/2;
		}
		if (!fastMode && fastSource.volume > 0) {
			fastSource.volume = Mathf.Max(0,fastSource.volume - Time.deltaTime * 0.5f);
			audio.volume = 0.5f - fastSource.volume/2;
		}
		if (timer > 0) {
			slidePosition = Mathf.Lerp(slidePosition,0,Time.deltaTime);
		} else {
			slidePosition = Mathf.Lerp(slidePosition,slidePositionMax,Time.deltaTime);
		}
	}

	public void Play() {
		Play (playlist[currentSongID].name);
	}

	public void Play(string credits) {
		currentSongCredits = "Now playing: " + credits;
		if (FindObjectOfType<MlgMode>() != null) {//IS MLG MODE
			audio.clip = mlgSong;
			currentSongCredits = "Now playing: xXxSW3GST3PxXx";
			FindObjectOfType<MlgMode>().StartTheShit();//Start the wubs
		}
		isPlaying = true;
		if (!audio.mute) {
			timer = 8;
		}
		audio.Play();
	}
	public void Pause() {
		audio.Pause ();
		isPlaying = false;
	}

	void OnGUI() {
		if (slidePosition < slidePositionMax-0.1f) {
			GUI.skin = skin;
			GUIStyle style = new GUIStyle(GUI.skin.label);
			style.fontSize = 16;
			style.alignment = TextAnchor.MiddleRight;
			Rect rect = new Rect(0,Screen.height-30+slidePosition,Screen.width,30);

			//GUIX.ShadowLabel(rect,currentSongCredits,style,1);
			GUILayout.BeginArea(rect);
			GUILayout.FlexibleSpace (); //Push down
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace(); //Push to the right
			GUILayout.Label(currentSongCredits,GUI.skin.GetStyle("SoundCredits"),GUILayout.ExpandWidth(false));
			GUILayout.EndHorizontal();
			GUILayout.EndArea();
		}
	}

	void ShuffleSongs() {
		//Shuffle playlist using Fisher-Yates algorithm
		for (int i = playlist.Length;i > 1;i--) {
			int j = Random.Range(0,i);
			Song tmp = playlist[j];
			playlist[j] = playlist[i - 1];
			playlist[i - 1] = tmp;
		}
	}

}

[System.Serializable]
public class Song {
	public string name;
	public AudioClip clip;
}