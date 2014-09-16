using UnityEngine;
using System.Collections;

public class Checkpoint : MonoBehaviour {

	public Renderer checkpointInverse;
	public Renderer checkpointMinimap;

	public Material matShown;
	public Material matHidden;

	public Texture2D texMinimapShown;
	public Texture2D texMinimapHidden;

	public LayerMask ballSpawningMask;

	void Start() {
		Hide ();
	}

	void OnDrawGizmosSelected() {
		Gizmos.color = Color.red;
		Gizmos.DrawSphere(GetRespawnPoint(),3);
	}

	public void Show() {
		renderer.material = matShown;
		//checkpointInverse.material.mainTexture = texShown;
		checkpointMinimap.material.mainTexture = texMinimapShown;
	}

	public void Hide() {
		renderer.material = matHidden;
		//checkpointInverse.material.mainTexture = texHidden;
		checkpointMinimap.material.mainTexture = texMinimapHidden;
	}

	public Vector3 GetRespawnPoint() {
		RaycastHit hit;
		Vector3 result = transform.position;
		if (Physics.Raycast(transform.position + Vector3.up*100,Vector3.down,out hit,200,ballSpawningMask)) {
			result = hit.point;
		}
		return result;
	}

}
