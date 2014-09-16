using UnityEngine;
using System.Collections;
 
public class DontGoThroughThings : MonoBehaviour 
{
	public LayerMask layerMask; //make sure we aren't in this layer 
	public float skinWidth = 0.1f; //probably doesn't need to be changed 
 
	private float minimumExtent; 
	private float partialExtent; 
	private float sqrMinimumExtent; 
	private Vector3 previousPosition; 
	private Rigidbody myRigidbody; 
	bool respawnDeclared = false;
	int timer;
 
 
	//initialize values 
	void Awake() 
	{ 
	   myRigidbody = rigidbody; 
	   previousPosition = myRigidbody.position; 
	   minimumExtent = Mathf.Min(Mathf.Min(collider.bounds.extents.x, collider.bounds.extents.y), collider.bounds.extents.z); 
	   partialExtent = minimumExtent * (1.0f - skinWidth); 
	   sqrMinimumExtent = minimumExtent * minimumExtent; 
	} 
 
	void FixedUpdate() 
	{ 
		if (!respawnDeclared) {
		   //have we moved more than our minimum extent? 
		   Vector3 movementThisStep = myRigidbody.position - previousPosition; 
		   float movementSqrMagnitude = movementThisStep.sqrMagnitude;
	 
		   if (movementSqrMagnitude > sqrMinimumExtent) 
			{ 
		      float movementMagnitude = Mathf.Sqrt(movementSqrMagnitude);
		      RaycastHit hitInfo; 
	 
		      //check for obstructions we might have missed 
		      if (Physics.Raycast(previousPosition, movementThisStep, out hitInfo, movementMagnitude, layerMask.value)) 
		         myRigidbody.position = hitInfo.point - (movementThisStep/movementMagnitude)*partialExtent; 
		   } 
	 
		   
		} else {
			timer--;
			if (timer <= 0) {
				respawnDeclared = false;
			}
		}
		previousPosition = myRigidbody.position; 
	}
	
	public void DeclareRespawn() {
		//Call when respawning to avoid being pulled back
		respawnDeclared = true;
		timer=3;
	}
}