using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FocusCursor : MonoBehaviour {

	public bool IsShowing { get; set; }
	private Renderer MyRenderer = null;

	// Use this for initialization
	void Start () {
		this.MyRenderer = this.gameObject.GetComponent<Renderer> ();
	}
	
	// Update is called once per frame
	void Update () {
		var fr = FocusRay.main;

		bool showMe = false;
		RaycastHit hitInfo;
		if (Physics.Raycast (fr.CurrentRay, out hitInfo)) {
			if ((hitInfo.collider != null)) {
				var ic = hitInfo.collider.gameObject.GetComponent<IsInputConsumer> ();
				if (ic != null) {
					showMe = ic.IsShowCursorOver;
					this.transform.position = hitInfo.point;
				}
			}
		}

		this.IsShowing = showMe;
		this.MyRenderer.enabled = this.IsShowing;
	}
}
