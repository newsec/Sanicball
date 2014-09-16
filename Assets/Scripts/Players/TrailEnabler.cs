using UnityEngine;
using System.Collections;

[RequireComponent (typeof(TrailRenderer))]
public class TrailEnabler : MonoBehaviour {

	TrailRenderer trailRenderer;

	// Use this for initialization
	void Start () {
		trailRenderer = GetComponent<TrailRenderer>();
		trailRenderer.enabled = GameSettings.enableTrails;
	}

	void Update() {
		if (!trailRenderer.enabled) return;

		float spd = Mathf.Max(0,rigidbody.velocity.magnitude - 60);
		trailRenderer.time = Mathf.Clamp(spd/20,0,5);
		trailRenderer.startWidth = Mathf.Clamp(spd/80,0,0.8f);
		trailRenderer.material.mainTextureScale = new Vector2(trailRenderer.time*150,1);
		trailRenderer.material.mainTextureOffset = new Vector2((trailRenderer.material.mainTextureOffset.x - 2*Time.deltaTime) % 1,0);
	}
	
}
