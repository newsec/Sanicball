using UnityEngine;
using System.Collections;

public class MouseUnlocker : MonoBehaviour {

	// Use this for initialization
	void Start () {
		if (GameObject.FindObjectsOfType<MouseUnlocker>().Length > 1) {
			Destroy(this.gameObject);
		}
		DontDestroyOnLoad(this.gameObject);
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown(KeyCode.LeftAlt)) {
			Screen.lockCursor = false;
		}
	}
}
