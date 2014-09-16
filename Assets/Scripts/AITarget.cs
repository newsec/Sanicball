using UnityEngine;
using System.Collections;

public class AITarget : MonoBehaviour {
	
	public float stupidness = 100;
	
	Vector2 pos;
	Vector2 velocity;
	Vector2 target;
	int timer;

	// Use this for initialization
	void Start () {
		pos = Vector2.zero;
		target = Random.insideUnitCircle*stupidness;
		timer = Random.Range (50,200);
	}
	
	void FixedUpdate () {
		/*velocity += new Vector2(
			Random.Range(-maxVelocityChange,maxVelocityChange),//x
			Random.Range(-maxVelocityChange,maxVelocityChange));//y
		transform.Translate(new Vector3(velocity.x,0,velocity.y));*/
		pos = Vector2.Lerp(pos,target,0.01f);

		timer--;
		if (timer<=0) {
			target = Random.insideUnitCircle*stupidness;
			timer = Random.Range(50,200);
		}
	}
	
	public Vector3 GetPos() {
		return transform.position + new Vector3(pos.x,0,pos.y);
	}
	
	void OnDrawGizmos() {
		Gizmos.color = Color.red;
		Gizmos.DrawSphere(transform.position + new Vector3(pos.x,0,pos.y),2);
		//Gizmos.color = Color.yellow;
		//Gizmos.DrawSphere(transform.position + new Vector3(target.x,0,target.y),2);
	}
}
