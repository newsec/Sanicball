using UnityEngine;
using System.Collections;

public class DriftySmoke : MonoBehaviour {
	
	public GameObject target;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		if (target == null) {
			Destroy(this.gameObject);
			return;
		}
		AudioSource aSource = target.GetComponent<AudioSource>();
		
		float speed = target.rigidbody.velocity.magnitude;
		float rot = target.rigidbody.angularVelocity.magnitude/2;
		float angle = Vector3.Angle(target.rigidbody.velocity,Quaternion.Euler(0,-90,0)*target.rigidbody.angularVelocity);
		
		float distToGround = target.collider.bounds.extents.y;
		bool b = Physics.Raycast(target.transform.position, Vector3.down, distToGround+0.1f);
		
		if (((angle>50 && (rot>10 || speed>10)) || (rot>30 && speed<30)) && b) {
			particleSystem.Emit(target.transform.position-new Vector3(0,+0.5f,0),
				target.rigidbody.velocity/2 + Vector3.up*4,
				3,5,Color.white);
			if (aSource.volume<1) {aSource.volume=Mathf.Min(aSource.volume+0.5f,1);}
			//aSource.pitch = 0.8f+Mathf.Min(rot/400f,1.2f);
		} else {
			if (aSource.volume>0) {aSource.volume=Mathf.Max(aSource.volume-0.2f,0);}
		}
	}
}
