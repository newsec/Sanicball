using UnityEngine;
using System.Collections;

public class DestroyOnWeb : MonoBehaviour {

	// Use this for initialization
	void Start () {
		if (Application.platform == RuntimePlatform.WindowsWebPlayer ||
			Application.platform == RuntimePlatform.OSXWebPlayer) {
			Destroy(this.gameObject);
		}
	}
}
