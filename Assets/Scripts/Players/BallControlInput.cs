using UnityEngine;
using System.Collections;

[RequireComponent (typeof(BallControl))]
public class BallControlInput : MonoBehaviour {

	public bool createCamera = true;
	public CameraPivot cameraPivotObject;

	BallControl ballControl;

	SmoothMouseLook cameraPivot;

	Vector3 rawDirection;

	// Use this for initialization
	void Start () {
		ballControl = GetComponent<BallControl>();
		//Create player camera
		if (createCamera) {
			CameraPivot cpivot = (CameraPivot) Instantiate(cameraPivotObject);
			cpivot.name = "CameraPivot_"+this.gameObject.name;
			cpivot.target = ballControl.gameObject;
			cameraPivot = cpivot.GetComponent<SmoothMouseLook>();
		}
	}
	
	// Update is called once per frame
	void Update () {
		//GO FAST
		//Vector3 directionVector = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
		float weight = 0.5f;
		bool forward = GameSettings.keybinds.GetKey("moveforward");
		bool left = GameSettings.keybinds.GetKey("moveleft");
		bool back = GameSettings.keybinds.GetKey("moveback");
		bool right = GameSettings.keybinds.GetKey("moveright");
		Vector3 targetVector = Vector3.zero;
		if (left && !right) {
			targetVector = new Vector3(-1,0,targetVector.z);
		}
		if (right && !left) {
			targetVector = new Vector3(+1,0,targetVector.z);
		}
		if (forward && !back) {
			targetVector = new Vector3(targetVector.x,0,1);
		}
		if (back && !forward) {
			targetVector = new Vector3(targetVector.x,0,-1);
		}
		rawDirection = Vector3.MoveTowards(rawDirection,targetVector,weight);

		Vector3 directionVector = rawDirection;

		if (directionVector != Vector3.zero) { //Modify direction vector to be more controller-friendly (And normalize it)
			var directionLength = directionVector.magnitude;
			directionVector = directionVector / directionLength;
			directionLength = Mathf.Min(1, directionLength);
			directionLength = directionLength * directionLength;
			directionVector = directionVector * directionLength;
		}

		Quaternion cameraDir;
		if (cameraPivot != null)
			cameraDir = cameraPivot.GetTargetDirection();
		else
			cameraDir = Quaternion.Euler(0,90,0);
		directionVector = cameraDir * directionVector; //Multiply vector by camera rotation

		ballControl.directionVector = directionVector;
		//BRAKE FAST
		ballControl.brake = GameSettings.keybinds.GetKey("brake");
		//JUMP FAST
		if (GameSettings.keybinds.GetKeyDown("jump")) {
			ballControl.Jump();
		}
	}

	public SmoothMouseLook GetCameraPivot {
		get {return cameraPivot;}
	}
}
