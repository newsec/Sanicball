using UnityEngine;
using System.Collections;

public class SpawnPoint : MonoBehaviour {

	public int ballCount = 8;
	public float ballSize = 1;
	public int columnWidth = 10;

	public LayerMask ballSpawningMask;

	// Use this for initialization
	void Start () {
		//Disable the arrow gizmo
		renderer.enabled = false;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnDrawGizmosSelected() {
		Gizmos.color = new Color(0.2f,0.6f,1);
		columnWidth = (int)Mathf.Max(1,columnWidth);
		for (int i=0;i<ballCount;i++) {
			Gizmos.DrawSphere(GetSpawnPoint(i,ballSize/2f),ballSize/2f);
		}
	}

	public Vector3 GetSpawnPoint(int position, float offsetY) {
		//Get the row of the ball
		int row = position / columnWidth;

		Vector3 dir;
		if (position % 2 == 0) {
			dir = Vector3.right * ((position % columnWidth) / 2 + 0.5f) * 2;
		} else {
			dir = Vector3.left * ((position % columnWidth) / 2 + 0.5f) * 2;
		}
		dir += (Vector3.back*2f)*row;
		
		RaycastHit hit;
		if (Physics.Raycast(transform.TransformPoint(dir + Vector3.up*100),Vector3.down,out hit,200,ballSpawningMask)) {
			dir = transform.InverseTransformPoint(hit.point);
		}
		return transform.TransformPoint(dir) + Vector3.up * offsetY;
	}
}
