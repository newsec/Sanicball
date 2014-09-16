using UnityEngine;
using System.Collections;

public class SmoothMouseLook : MonoBehaviour {
	
	public bool canControl=true;
	public int smoothing=2;
	public int min=-85;
	public int max=85;

	float xtargetRotation=90;
	float ytargetRotation=0;
	float sensitivityMouse=3;
	float sensitivityKeyboard=10;

	// Use this for initialization
	void Start () {
		sensitivityMouse = GameSettings.sensitivityMouse;
		sensitivityKeyboard = GameSettings.sensitivityKeyboard;
		Screen.lockCursor = true;
	}
	
	// Update is called once per frame
	void Update () {

		//Don't lock the cursor if the player list is open
		PlayerList pl = FindObjectOfType<PlayerList>();
		bool success = true;
		if (pl != null) {
			if (!pl.Hidden)
				success = false;
		}

		if (Input.GetMouseButtonDown(0) && !GameSettings.keybinds.isTyping && success) {
			if (canControl)
				Screen.lockCursor = true;
		}

		/*if (Input.GetKeyDown(KeyCode.LeftAlt)) { Replaced by MouseUnlocker script that works anywhere
			Screen.lockCursor = false;
		}*/
		
		if (canControl) {
			if (Screen.lockCursor) {
				float yAxisMove=Input.GetAxis("Mouse Y")*sensitivityMouse; // how much has the mouse moved?
				ytargetRotation+=-yAxisMove; // what is the target angle of rotation
				
				float xAxisMove=Input.GetAxis("Mouse X")*sensitivityMouse; // how much has the mouse moved?
				xtargetRotation+=xAxisMove; // what is the target angle of rotation
			}

			//Keyboard controls
			if (GameSettings.keybinds.GetKey("cameraleft"))
				xtargetRotation-=10*sensitivityKeyboard*Time.deltaTime;
			if (GameSettings.keybinds.GetKey("cameraright"))
				xtargetRotation+=10*sensitivityKeyboard*Time.deltaTime;
			if (GameSettings.keybinds.GetKey("cameraup"))
				ytargetRotation-=10*sensitivityKeyboard*Time.deltaTime;
			if (GameSettings.keybinds.GetKey("cameradown"))
				ytargetRotation+=10*sensitivityKeyboard*Time.deltaTime;

			ytargetRotation=Mathf.Clamp(ytargetRotation,min,max);
			xtargetRotation=xtargetRotation % 360;
			ytargetRotation=ytargetRotation % 360;
			
			
			transform.localRotation=Quaternion.Lerp(transform.localRotation,Quaternion.Euler(0,xtargetRotation,ytargetRotation),Time.deltaTime*10/smoothing);
			//transform.localRotation=Quaternion.Lerp(transform.parent.rotation,Quaternion.Euler(0,xtargetRotation,0),Time.deltaTime*10/smoothing);
			}
	}
	
	public void SetTargetRotation(float xRot, float yRot) {
		xtargetRotation=xRot;
		ytargetRotation=yRot;
	}
	
	public float GetXTargetRotation() {
		return xtargetRotation;
	}
	
	public float GetYTargetRotation() {
		return ytargetRotation;
	}
	
	public Quaternion GetTargetDirection() {
		return Quaternion.Euler(0,xtargetRotation,ytargetRotation);
	}
}
