using UnityEngine;
using System.Collections.Generic;

public class DriftySmokeCreator : MonoBehaviour {

	public Object smokeObject;

	void Start() {
		GameObject o = (GameObject) Instantiate(smokeObject);
		o.GetComponent<DriftySmoke>().target = this.gameObject;
	}
	
}
