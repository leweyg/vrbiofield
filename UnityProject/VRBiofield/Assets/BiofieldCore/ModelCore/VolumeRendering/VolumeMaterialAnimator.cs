using UnityEngine;
using System.Collections;

public class VolumeMaterialAnimator : MonoBehaviour {

	private Material mMaterial;

	// Use this for initialization
	void Start () {
	
		this.mMaterial = this.GetComponent<Renderer> ().material;

	}
	
	// Update is called once per frame
	void Update () {
		float t = UnityEngine.Time.time;
		t = (t - Mathf.Floor (t));
		mMaterial.SetColor ("_RandomSeedColor", new Color (t, t, t, t));
	}
}
