using UnityEngine;
using System.Collections;

public class RandomTexture : MonoBehaviour {
	
	public Texture[] textures;
	int current;
	
	void Start () {
		SwitchTexture();
	}

	public int GetCurrentTexture() {
		return current;
	}

	public void SetTexture(int i) {
		renderer.material.mainTexture = textures[i];
		current = i;
	}

	void SwitchTexture() {
		int m = Random.Range(0,textures.Length);
		renderer.material.mainTexture = textures[m];
		current = m;
	}
}
