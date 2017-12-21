using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimChiViewer : MonoBehaviour {

	public float PercentEthereal = 0.0f;
	public float PercentPhysical = 1.0f;
	public float PercentInIt = 0.0f;
	public Light MainSceneLight = null;
	public Color CurrentEtherealColor = Color.white;
	private float MainSceneLightInitial = 0.0f;
	private ApplyPostShader PostShader;

	public static SimChiViewer CacheMain() {
		SimChiViewer foundMain;
		foundMain = GameObject.FindObjectOfType<SimChiViewer> ();
		Debug.Assert (foundMain);
		return foundMain;
	}

	// Use this for initialization
	void Start () {
		MainSceneLightInitial = MainSceneLight.intensity;
		MainSceneLight.gameObject.SetActive (true);
		this.StartCoroutine (InternalFiber ());

		this.PostShader = GameObject.FindObjectOfType<ApplyPostShader> ();
		PostShader.ShaderMaterial = new Material (PostShader.ShaderMaterial);
	}

	public void UpdateViewerScene() {
		float blendPct = 1.7f;
		PercentPhysical = 1.0f - Mathf.Clamp01 (PercentInIt * blendPct);
		PercentEthereal = Mathf.Clamp01 (1.0f - ((1.0f - PercentInIt) * blendPct));
		
		MainSceneLight.intensity = PercentPhysical * MainSceneLightInitial;
		PostShader.ShaderMaterial.SetFloat ("_bwBlend", 1.0f - PercentPhysical);
		PostShader.ShaderMaterial.SetColor ("_TintColor", this.CurrentEtherealColor);
	}

	private bool SlowChangeUnit(ref float val, float rate) {
		val = Mathf.Clamp01 (val + (Time.deltaTime * rate));
		if (rate == 0.0f)
			return false;
		if (rate > 0.0f)
			return (val < 1.0f);
		else
			return (val > 0.0f);
	}

	IEnumerator InternalFiber() {
		while (true) {
			float rate = 1.0f / 4.0f;

			while (SlowChangeUnit (ref PercentInIt, rate)) {
				yield return null;
			}

			yield return new WaitForSeconds (1.0f);


			while (SlowChangeUnit (ref PercentInIt, -rate)) {
				yield return null;
			}

			yield return new WaitForSeconds (1.0f);
		}
	}
	
	// Update is called once per frame
	void Update () {
		UpdateViewerScene ();
	}
}
