using UnityEngine;
using System.Collections;

public class PCControls : MonoBehaviour {

	public bool UseInEditor = false;
	public PCView ViewRight;
	public PCView ViewCenter;
	public PCView ViewLeft;
	private PCView[] AllViews;
	private PCView LatestView = null;

	// Use this for initialization
	void Start () {
		if (Application.isEditor && (!this.UseInEditor)) {
			this.enabled = false;
			return;
		}
		if (UnityEngine.VR.VRSettings.enabled) {
			this.enabled = false;
			return;
		}

		UseView (this.ViewRight);
		this.AllViews = new PCView[] {
			this.ViewLeft, this.ViewCenter, this.ViewRight
		};
	}

	public void UseView(PCView v) {
		this.LatestView = v;
		Camera.main.transform.position = v.transform.position;
		Camera.main.transform.rotation = v.transform.rotation;
		Camera.main.fieldOfView = v.FieldOfView;
	}

	public PCView FindClosestView() {
		PCView bestView = null;
		float bestScore = -100.0f;
		foreach (var v in this.AllViews) {
			if (v != null) {
				float d = Vector3.Dot (v.transform.forward, Camera.main.transform.forward);
				if (d > bestScore) {
					bestView = v;
					bestScore = d;
				}
			}
		}
		return bestView;
	}

	// Update is called once per frame
	void Update () {
		var bv = this.FindClosestView ();
		if ((bv != null) && (bv != this.LatestView)) {
			this.UseView (bv);
		}
		//Camera.main.fieldOfView = Mathf.Lerp (Camera.main.fieldOfView, this.LatestView.FieldOfView, 0.02f);
	}
}
