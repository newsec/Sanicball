using UnityEngine;
using System.Collections;

public class RoadPathNode : MonoBehaviour {

	public float roadWidth = 10;

	public Vector3 LeftPoint {
		get {
			return transform.TransformPoint(Vector3.left*roadWidth/2);
		}
	}

	public Vector3 RightPoint {
		get {
			return transform.TransformPoint(Vector3.right*roadWidth/2);
		}
	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnDrawGizmos() {
		Gizmos.color = Color.magenta;
		Gizmos.DrawSphere(LeftPoint,2);
		Gizmos.DrawSphere(RightPoint,2);

		Matrix4x4 rotationMatrix = Matrix4x4.TRS(transform.position,transform.rotation,transform.lossyScale);
		Gizmos.matrix = rotationMatrix;
		Gizmos.DrawCube(Vector3.zero,new Vector3(roadWidth,1,1));
	}
}
