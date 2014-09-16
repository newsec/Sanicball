using UnityEngine;
using System;
using System.Collections;

public class Racer : MonoBehaviour {

	public bool isInLobby = false;
	
	public string racerName;
	public int totalLaps;
	//Prefab settings
	public AudioClip checkpointSound;
	public MinimapIcon mapIcon;
	public Material mapIconMaterial;
	//Checkpoints
	public Checkpoint currentCheckpoint;
	public int currentCheckpointID = 0;
	public Checkpoint nextCheckpoint;
	//Position in race
	public float progress;
	public int position;
	public bool finished = false;

	bool isPlayer = false;
	int lap = 1;
	//int rangs = 0;
	float lastCheckpointPosition;

	//Time
	bool timerOn = false;
	float lapTime = 0;
	float raceTime = 0;
	float[] checkpointTimes;

	float aiRespawnTime = 0;
	float aiMaxRespawnTime = 40;

	RaceController raceController;
	
	public int Lap {get {return lap;}}

	public float RaceTime{get {return raceTime;}}
	
	//public int Rangs {get; set;}

	void Start() {
		raceController = FindObjectOfType<RaceController>();

		if (GetComponent<BallControlInput>() != null) {
			isPlayer = true;
		}
		if (GetComponent<BallControlAI>() != null) {//If ball is AI
			aiRespawnTime = aiMaxRespawnTime; //Auto respawn after 20 secs of not passing a checkpoint.
		}
		if (isInLobby) return;

		if (raceController != null) {
			currentCheckpoint = raceController.checkpoints[0];
			nextCheckpoint = raceController.checkpoints[1];
			checkpointTimes = new float[raceController.checkpoints.Length];
			if (isPlayer)
				nextCheckpoint.Show();
		}

		AlignCameraWithCheckpoint();
		//Debug.Log (currentCheckpoint.gameObject.name);
		GameObject mapCamera = GameObject.Find("CameraMap");
		MinimapIcon micon = (MinimapIcon)Instantiate(mapIcon,new Vector3(0,mapCamera.transform.position.y - 10,0),Quaternion.Euler(90,90,0));
		micon.renderer.material = mapIconMaterial;
		micon.objectToFollow = this.gameObject;
		micon.mapCamera = mapCamera;
	}

	void OnTriggerEnter(Collider other) {
		Checkpoint otherCheckpoint = other.GetComponent<Checkpoint>();

		if (otherCheckpoint != null) {
			if (otherCheckpoint == nextCheckpoint && !finished) {

				checkpointTimes[currentCheckpointID] = lapTime;

				//DO CHECKPOINT STUFF
				bool isLastCheckpoint = otherCheckpoint == raceController.checkpoints[raceController.checkpoints.Length-1];
				bool isFinishLine = otherCheckpoint == raceController.checkpoints[0];
				
				currentCheckpoint = otherCheckpoint;
				
				if (isFinishLine) {
					currentCheckpointID = 0;
				} else {
					currentCheckpointID++;
				}
				if (isLastCheckpoint) {
					nextCheckpoint = raceController.checkpoints[0];
				} else {
					nextCheckpoint = raceController.checkpoints[currentCheckpointID+1];
				}
				
				if (checkpointSound != null && isPlayer) {AudioSource.PlayClipAtPoint(checkpointSound,transform.position,0.5f);}
				if (GetComponent<BallControlAI>() != null) {
					PathFollower target = GetComponent<BallControlAI>().GetTarget().GetComponent<PathFollower>();
					lastCheckpointPosition = target.GetPositionPercentile();
				}
				
				if (GetComponent<BallControlAI>() != null) {//If ball is AI
					aiRespawnTime = aiMaxRespawnTime; //Auto respawn after 20 secs of not passing a checkpoint.
				}
				//Check if finish line
				if (!isFinishLine) {//IF NOT FINISH LINE
					if (isPlayer) {
						ShowCheckpointTime(false);
					}
				} else {//IF FINISH LINE
					if (isPlayer) {
						ShowCheckpointTime(true);
					}
					checkpointTimes = new float[raceController.checkpoints.Length];
					if (lap < totalLaps) { //IF ANY LAPS ARE LEFT
						lap++;
						lapTime = 0;
					} else { // IF RACE IS FINISHED
						finished = true;
						timerOn = false;
						GameObject.Find("RaceController").GetComponent<RaceController>().FinishRacer(this);
						if (GetComponent<BallControl>() != null) {
							if (GetComponent<BallControlAI>() != null) {
								GetComponent<BallControl>().canControl = false;//Stop AI balls
							}
							//Disable collision with other balls
							FindObjectOfType<NetworkView>().RPC("SetGhost",RPCMode.All,Network.player);
						}
					}
				}

				if (isPlayer) {
					currentCheckpoint.Hide();
					if (!finished)
						nextCheckpoint.Show();
				}
			}	
		}
		if (other.GetComponent<TriggerRespawn>() != null) {
			Respawn();
		}
		
	}

	void DoCheckpointStuff(Checkpoint otherCheckpoint) {
		

	}

	void ShowCheckpointTime(bool isFinishLine) {
		bool checkTimeDiff = true;
		string checkpointTimeString = Timing.GetTimeString(lapTime);
		string lapString = "";
		string diffString = "";
		Color diffStringColor = Color.white;
		if (isFinishLine) {
			if (lap < totalLaps) {
				lapString = "LAP " + (lap+1);
			} else {
				lapString = "RACE FINISHED";
			}
			float lapRecord = PlayerPrefs.GetFloat(Application.loadedLevelName+"_lap",float.MaxValue);
			if (lapTime < lapRecord) {
				diffStringColor = Color.blue;
				diffString = "New lap record!";
				if (FindObjectOfType<MlgMode>() != null) {
					diffString = "420 BLAZE IT FGT";
				}
				checkTimeDiff = false;
				if (FindObjectOfType<MlgMode>() == null) { //Don't save records in MLG mode
					PlayerPrefs.SetFloat(Application.loadedLevelName+"_lap",lapTime);
					for (int i=0;i<checkpointTimes.Length;i++) {
						PlayerPrefs.SetFloat(Application.loadedLevelName+"_checkpoint"+i,checkpointTimes[i]);
					}
					PlayerPrefs.Save();
				}
			}
		}
		if (checkTimeDiff) {
			//Get checkpoint time diff
			int lastCheckpointID = currentCheckpointID - 1;
			if (lastCheckpointID < 0) {
				lastCheckpointID = raceController.checkpoints.Length-1;
			}
			float checkpointRecord = PlayerPrefs.GetFloat(Application.loadedLevelName+"_checkpoint"+lastCheckpointID,-1);
			if (checkpointRecord != -1) {
				float timeDiff = lapTime - checkpointRecord;
				string diffStringPrefix = "";
				if (timeDiff <= 0) {
					diffStringColor = Color.blue;
				} else {
					diffStringPrefix = "+";
					diffStringColor = Color.red;
				}
				diffString += diffStringPrefix + Timing.GetTimeString(timeDiff);
			}
		}
		GetComponent<RaceUI>().Checkpoint(checkpointTimeString,diffString,lapString,diffStringColor);
	}
	
	void Update() {
		if (isInLobby) return;
		if (GameSettings.keybinds.GetKeyDown("respawn") && isPlayer) {
			Respawn();
		}
		//Timers
		if (timerOn) {
			raceTime += Time.deltaTime;
			lapTime += Time.deltaTime;
		}
		if (aiRespawnTime > 0) {
			aiRespawnTime -= Time.deltaTime;
			if (aiRespawnTime <= 0) {
				Respawn();
				aiRespawnTime = aiMaxRespawnTime;
			}
		}

	}
	
	public void Respawn() {
		if (currentCheckpoint != null && GetComponent<BallControl>().canControl) {
			transform.position = currentCheckpoint.GetRespawnPoint() + Vector3.up * collider.bounds.extents.y;
			rigidbody.angularVelocity = Vector3.zero;
			rigidbody.velocity = Vector3.zero;
			AlignCameraWithCheckpoint();
			if (GetComponent<BallControlAI>() != null) {
				PathFollower target = GetComponent<BallControlAI>().GetTarget().GetComponent<PathFollower>();
				target.position = lastCheckpointPosition;
			}
			if (GetComponent<DontGoThroughThings>() != null) {
				GetComponent<DontGoThroughThings>().DeclareRespawn();
			}
		}
	}

	void AlignCameraWithCheckpoint() {
		if (GetComponent<BallControlInput>() != null) {
			Quaternion ve = Quaternion.Euler(0,90,0) * currentCheckpoint.transform.rotation;
			GetComponent<BallControlInput>().GetCameraPivot.SetTargetRotation(ve.eulerAngles.y,ve.eulerAngles.z);
		}
	}

	public void StartTimer() {
		timerOn = true;
	}

	public void StopTimer() {
		timerOn = false;
	}

	public string GetPositionString () {
		string suffix = "th";
		if (position % 10 == 1 && position % 100 != 11) {
			suffix = "st";
		}
		if (position % 10 == 2 && position % 100 != 12) {
			suffix = "nd";
		}
		if (position % 10 == 3 && position % 100 != 13) {
			suffix = "rd";
		}
		return position + suffix;
	}
	
}
