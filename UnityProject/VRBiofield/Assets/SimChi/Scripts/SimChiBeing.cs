using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimChiBeing : MonoBehaviour {

	public GameObject MainShape;
	public GameObject BelowGround;
	public Light MainLight;
	private float MainLightInitialIntensity;

	private SimChiViewer Viewer;

	// Use this for initialization
	void Start () {
		Viewer = SimChiViewer.CacheMain ();

		MainLightInitialIntensity = MainLight.intensity;
	}

	public void UpdateFromViewer() {
		if (Viewer.PercentEthereal == 0.0f) {
			MainLight.enabled = false;

			if (BelowGround)
				BelowGround.SetActive (false);
		} else {
			MainLight.enabled = true;
			MainLight.intensity = MainLightInitialIntensity * Viewer.PercentEthereal;
			//MainLight.color = Viewer.CurrentEtherealColor;

			if (BelowGround)
				BelowGround.SetActive (true);
		}
	}
	
	// Update is called once per frame
	void Update () {
		this.UpdateFromViewer ();
	}
}
