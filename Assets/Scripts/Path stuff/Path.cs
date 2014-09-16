using UnityEngine;
using System.Collections;

public class Path : MonoBehaviour {
	
	public Color pathColor = Color.blue;
	Transform[] nodes;
	public bool hidePath = false;

	// Use this for initialization
	void Start () {
		nodes = new Transform[transform.childCount];
		for (int a = 0;a<transform.childCount;a++) {
			nodes[a] = transform.GetChild(a);
		}
	}
	
	public void SetNodes(Transform[] replacement) {
		nodes = replacement;
	}
	
	public Transform[] GetNodes() {
		return nodes;
	}
	
	void OnDrawGizmos () {
		if (nodes != null && !hidePath) {
			iTween.DrawPath(nodes,pathColor);
		}
	}
}
