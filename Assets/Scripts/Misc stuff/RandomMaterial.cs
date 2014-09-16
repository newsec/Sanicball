using UnityEngine;
using System.Collections;

public class RandomMaterial : MonoBehaviour {
	
	public Material[] materials;
	int current;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	public void Switch () {
		current++;
		if (current >= materials.Length)
			current = 0;
		renderer.material = materials[current];
	}
}
