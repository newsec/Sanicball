using UnityEngine;
using System.Collections;

public class BallControl : MonoBehaviour {
	
	public bool canControl;
 
	public BallStats stats;
	
	//public AudioClip bounceClip;

	[System.NonSerialized]
	public bool brake;
	[System.NonSerialized]
	public Vector3 directionVector;

	bool grounded = false;
	float groundedTimer = 0;
	//Vector3 jumpDirection = Vector3.up;

	// Use this for initialization
	void Start () {
        base.rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		if (canControl) {
			//int terrain = 
			if (directionVector != Vector3.zero)
				rigidbody.AddTorque((directionVector)*stats.rollSpeed);

			if (!grounded) {
				rigidbody.AddForce((Quaternion.Euler(0,-90,0)*directionVector)*stats.airSpeed);
			}
		} else
			brake = true;

		if (brake) {
			rigidbody.angularVelocity=Vector3.zero; //Force ball to brake by resetting angular velocity every update
		}

		if (grounded) {
			//rigidbody.AddForce(Vector3.down*stats.grip * (rigidbody.velocity.magnitude/400)); //Downwards gravity to increase grip
			//Debug.Log(stats.grip * Mathf.Pow(rigidbody.velocity.magnitude/100,2));
		}
	}
	
	void Update() {
		//float distToGround = collider.bounds.extents.y;
		//grounded = Physics.Raycast(transform.position, Vector3.down, distToGround+0.2f);
		/*int mask = 1 << 7;
		mask = ~mask;
		Debug.Log(System.Convert.ToString(mask,2));
		grounded = Physics.CheckSphere(transform.position,transform.localScale.x + 0.2f,mask);*/

		Transform rollSound = transform.FindChild("Rollsound");
		if (rollSound != null) {
			if (grounded) {
				float rollSpd = Mathf.Clamp(rigidbody.angularVelocity.magnitude / 230,0,16);
				rollSound.audio.pitch = Mathf.Max(rollSpd,0.8f);
				rollSound.audio.volume = Mathf.Min(rollSpd,1);
			} else {
				if (rollSound.audio.volume > 0) {
					rollSound.audio.volume = Mathf.Max(0,rollSound.audio.volume - 0.2f);
				}
			}
		}

		if (groundedTimer > 0) {
			groundedTimer = Mathf.Max(0,groundedTimer - Time.deltaTime);
			if (groundedTimer <= 0) {
				grounded = false;
			}
		}
	}

	public void Jump() {
		if (grounded && canControl) {
			rigidbody.AddForce(Vector3.up*stats.jumpHeight,ForceMode.Impulse);
			if (transform.FindChild("Jumpsound") != null) {
				transform.FindChild("Jumpsound").GetComponent<AudioSource>().Play();
			} else {
				Debug.Log("Jump sound failed to play");
			}
			grounded = false;
		}
	}

	void OnCollisionStay(Collision c) {
		grounded = true;
		groundedTimer = 0;
		//jumpDirection = c.contacts[0].normal;
	}

	void OnCollisionExit(Collision c) {
		groundedTimer = 0.08f;
	}
	
}
