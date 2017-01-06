using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class PCControls : MonoBehaviour {

	public bool UseInEditor = false;
	public PCView ViewLeft;
	public PCView ViewCenter;
	public PCView ViewRight;
	public PCView ViewFarRight;

	private PCView[] AllViews;
	private PCView LatestView = null;
	private float MouseDownTime = 0.0f;

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
			this.ViewLeft, 
			this.ViewCenter, 
			this.ViewRight, this.ViewFarRight
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

	private bool LerpingIsActive = false;
	private Matrix4x4 LerpingStartTrans = Matrix4x4.identity;
	private float LerpingStartFOV = 90.0f;
	private float LerpingStartTime = 0.0f;
	private void UpdateLerping() {
		if (LerpingIsActive) {
			float lerpTime = 0.61f;
			float t = ((Time.time - LerpingStartTime) / lerpTime);
			if (t >= 1.0f) {
				t = 1.0f;
				LerpingIsActive = false;
				this.UseView (this.LatestView);
			} else {
				Camera.main.transform.position = Vector3.Lerp (
					LerpingStartTrans.MultiplyPoint (Vector3.zero),
					this.LatestView.transform.position, t);
				Camera.main.transform.rotation = Quaternion.Lerp (
					Quaternion.LookRotation (LerpingStartTrans.MultiplyVector (Vector3.forward), LerpingStartTrans.MultiplyVector (Vector3.up)),
					this.LatestView.transform.rotation, t);
				Camera.main.fieldOfView = Mathf.Lerp (this.LerpingStartFOV, this.LatestView.FieldOfView, t);
			}
		}
	}

	void PossibleTapClick() {

		RaycastHit hitInfo;
		if (Physics.Raycast (Camera.main.ScreenPointToRay (Input.mousePosition), out hitInfo)) {
			if ((hitInfo.collider != null) && (hitInfo.collider.gameObject.GetComponent<IsInputConsumer> () != null)) {
				return; // tapped on input system
			}
		}

		float mx = Input.mousePosition.x;
		bool isLeft = ((mx < (Screen.width / 2)));

		var curViewNdx = this.AllViews.ToList ().IndexOf (this.LatestView);
		var nextNdx = (isLeft ? (curViewNdx - 1) : (curViewNdx + 1));
		nextNdx = ((nextNdx + this.AllViews.Length) % this.AllViews.Length);
		this.LatestView = this.AllViews [nextNdx];
	}

	// Update is called once per frame
	void Update () {
		this.UpdateLerping ();
		var bv = this.FindClosestView ();
		if (Input.GetMouseButton (0)) {
			MouseDownTime += Time.deltaTime;
			Camera.main.fieldOfView = Mathf.Lerp (Camera.main.fieldOfView, bv.FieldOfView, 0.02f);
		} else {
			var startView = this.LatestView;

			float maxTapTime = 0.3f;

			if (LerpingIsActive) {
				return;
			} else if ((bv != null) && (bv != this.LatestView)) {
				this.LatestView = bv;
			} else if ((MouseDownTime > 0) && (MouseDownTime < maxTapTime)) {
				this.PossibleTapClick ();
			}

			if (startView != this.LatestView) {
				LerpingStartTime = Time.time;
				LerpingStartTrans = Camera.main.transform.localToWorldMatrix;
				LerpingStartFOV = Camera.main.fieldOfView;
				LerpingIsActive = true;
			}

			MouseDownTime = 0.0f;
		}
	}
}
