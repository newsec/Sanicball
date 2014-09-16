using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class RaceController : MonoBehaviour {
	public GUISkin skin;

	public RaceSettings settings;
	//public int laps;
	public Checkpoint[] checkpoints;
	public SpawnPoint spawnPoint;
	
	public AudioClip countdown1;
	public AudioClip countdown2;
	string countdownText = "";
	int countdownFontSize = 60;

	//int[] positions;
	List<Racer> racers = new List<Racer>();
	int countdown = 0;
	int timer = 100;
	int racerPos = 1;

	public void SetRacers(Racer[] newracers) {
		foreach (Racer r in newracers) {
			racers.Add(r);
		}
	}

	public Racer[] GetRacers() {
		Racer[] result = new Racer[racers.Count];
		int i = 0;
		foreach (Racer r in racers) {
			result[i] = r;
			i++;
		}
		return result;
	}

	public void RemoveRacer(Racer r) {
		racers.Remove(r);
	}

	public void StartRace () {
		//positions = new int[racers.Length];
		countdownText = "ON YOUR MARKS";
		countdown = 5;
	}
	
	// Update is called once per frame
	void Update() {
		//Update racer positions
		foreach (Racer r in racers) {
			if (r.finished) {
				r.progress = int.MaxValue - r.position;
			} else {
				int lap = r.Lap;
				int checkpoint = r.currentCheckpointID;
				float distance = Vector3.Distance(r.transform.position,r.nextCheckpoint.transform.position)//Distance from racer to checkpoint
					/ Vector3.Distance(r.currentCheckpoint.transform.position,r.nextCheckpoint.transform.position);//Divided by distance between current and next checkpoints
				distance = 1 - distance;

				float progress = lap*1000 + checkpoint*10 + distance;
				r.progress = progress;
			}
		}
		var tempRacers = racers.OrderByDescending(x => x.progress).ToList();//Use a temp array to avoid shifting around the positions of the players

		for (int i=0;i<tempRacers.Count;i++) {
			tempRacers[i].position = i+1;
		}
	}

	public int FinishRacer(Racer racer) {
		return racerPos++;
	}
	
	void FixedUpdate () {
		if (countdown>0) {
			timer--;
			if (timer<=0) {
				countdown--;
				switch(countdown) {
				case 4:
					countdownText = "READY";
					countdownFontSize = 70;
					AudioSource.PlayClipAtPoint(countdown1,transform.position);
					break;
				case 3:
					countdownText = "STEADY";
					countdownFontSize = 80;
					AudioSource.PlayClipAtPoint(countdown1,transform.position);
					break;
				case 2:
					countdownText = "GET SET";
					countdownFontSize = 100;
					AudioSource.PlayClipAtPoint(countdown1,transform.position);
					break;
				case 1:
					countdownText = "GO FAST";
					countdownFontSize = 120;
					AudioSource.PlayClipAtPoint(countdown2,transform.position);
					GameObject.Find ("music").GetComponent<MusicPlayer>().Play();
					foreach (Racer r in racers) {
						r.StartTimer();
						if (r.GetComponent<BallControl>() != null) {r.GetComponent<BallControl>().canControl=true;}
					}
					break;
				case 0:
					countdownText = "";
					break;
				}
				timer=50;
				if (countdown == 1) timer = 100;
			}
		}
	}

	void OnGUI() {
		if (countdownText != "") {
			GUI.skin = skin;
			GUIStyle style = new GUIStyle(GUI.skin.label);
			style.alignment = TextAnchor.MiddleCenter;
			style.fontSize = countdownFontSize;
			style.fontStyle = FontStyle.Bold;
			Rect rect = new Rect(0,0,Screen.width,400);

			GUIX.ShadowLabel(rect,countdownText,style,4);
			GUI.Label(rect,countdownText,style);
		}
	}
	
}
