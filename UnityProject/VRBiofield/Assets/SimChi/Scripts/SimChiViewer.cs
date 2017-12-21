using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimChiViewer : MonoBehaviour {

	public float PercentEthereal = 0.0f;
	public float PercentPhysical = 1.0f;
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
		this.StartCoroutine (InternalFiber ());

		this.PostShader = GameObject.FindObjectOfType<ApplyPostShader> ();
		PostShader.ShaderMaterial = new Material (PostShader.ShaderMaterial);
	}

	public void UpdateViewerScene() {
		MainSceneLight.intensity = PercentPhysical * MainSceneLightInitial;
		PostShader.ShaderMaterial.SetFloat ("_bwBlend", PercentEthereal);
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
			float rate = 1.0f / 2.0f;

			while (SlowChangeUnit(ref PercentPhysical, -rate)){
				yield return null;
			}

			while (SlowChangeUnit(ref PercentEthereal, rate)) {
				yield return null;
			}

			yield return new WaitForSeconds (1.0f);

			while (SlowChangeUnit(ref PercentEthereal, -rate)){
				yield return null;
			}
			while (SlowChangeUnit(ref PercentPhysical, rate)){
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
