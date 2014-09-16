using UnityEngine;
using System.Collections;

[RequireComponent (typeof(BallControl))]
public class BallControlAI : MonoBehaviour {

	public AITarget aiTargetPrefab;
	public int stupidness = 30;
	public Path pathToFollow;

	BallControl ballControl;
	AITarget target;
	
	public AITarget GetTarget() {
		return target;
	}

	// Use this for initialization
	void Start () {
		ballControl = GetComponent<BallControl>();
		//PathToFollow
		pathToFollow = GameObject.FindWithTag("AIPath").GetComponent<Path>();
		//PathFollower
		AITarget pFollower = (AITarget) Instantiate(aiTargetPrefab);
		pFollower.GetComponent<PathFollower>().path = pathToFollow;
		pFollower.stupidness = stupidness;
		pFollower.GetComponent<Patroller>().target = this.gameObject;
		target = pFollower;
	}
	
	// Update is called once per frame
	void Update () {
		ballControl.brake = false;
		Quaternion moveDir = Quaternion.LookRotation(target.GetPos() - transform.position);
		Vector3 moveDir2 = moveDir.eulerAngles;
		moveDir = Quaternion.Euler(0,moveDir2.y,moveDir2.z);
		ballControl.directionVector = Quaternion.Euler(0,90,0)*moveDir*Vector3.forward;
		//Debug.DrawRay(transform.position,moveDir*directionVector*100);
		//rigidbody.AddTorque((Quaternion.Euler(0,90,0)*moveDir*directionVector)*rollSpeed);
	}
	
	public void TriggerJump() {
		ballControl.Jump();
	}
	
}
