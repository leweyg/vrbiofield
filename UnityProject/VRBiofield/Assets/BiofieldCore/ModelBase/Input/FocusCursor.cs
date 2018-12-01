using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FocusCursor : MonoBehaviour {

	public bool IsShowing { get; set; }
	private Renderer MyRenderer = null;
	private IsInputConsumer CurrentEntered = null;

	private IsInputConsumer ClickDownStartedOn = null;
	private bool ClickDownIs = false;
	public bool IsJustClicked = false;
	public bool IsClickPossible = false;

	// Use this for initialization
	void Start () {
		this.MyRenderer = this.gameObject.GetComponent<Renderer> ();
	}

	private bool UpdateIsClicking() {
		IsJustClicked = false;
		IsClickPossible = (!UnityEngine.XR.XRSettings.isDeviceActive);
		var curDown = (Input.GetMouseButton (0));
		if (curDown) {
			if (!ClickDownIs) {
				// on start:
				ClickDownStartedOn = this.CurrentEntered;
			}
		} else {
			if (ClickDownIs) {
				// on release:
				if (this.CurrentEntered == this.ClickDownStartedOn) {
					IsJustClicked = true;
				}
			}
			ClickDownStartedOn = null;
		}
		ClickDownIs = curDown;
		return IsJustClicked;
	}

	private void UpdateCurrentEntered(IsInputConsumer ic) {
		if (this.CurrentEntered == ic) {
			return;
		}

		if (CurrentEntered) {
			CurrentEntered.DoEvent_FocusExited (this);
			CurrentEntered = null;
		}

		CurrentEntered = ic;

		if (CurrentEntered) {
			CurrentEntered.DoEvent_FocusEntered (this);
		}
	}
	
	// Update is called once per frame
	void Update () {
		var fr = FocusRay.main;



		bool showMe = false;
		RaycastHit hitInfo;
		IsInputConsumer ic = null;
		if (Physics.Raycast (fr.CurrentRay, out hitInfo)) {
			if ((hitInfo.collider != null)) {
				ic = hitInfo.collider.gameObject.GetComponent<IsInputConsumer> ();
				if (ic != null) {
					showMe = ic.IsShowCursorOver;
					this.transform.position = hitInfo.point;
				}
			} 
		}
		this.UpdateCurrentEntered (ic);

		if (UpdateIsClicking ()) {
			if (this.CurrentEntered) {
				this.CurrentEntered.DoEvent_FocusSelected (this);
			}
		}

		this.IsShowing = showMe;
		this.MyRenderer.enabled = this.IsShowing;
	}
}
