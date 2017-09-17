using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateCameraAround : MonoBehaviour {

	public Transform ToRotate = null;
	public Transform RotateAroundThis = null;
	private Vector3 mousePrev;
	private bool mouseHeld = false;

	// Use this for initialization
	void Start () {
		if (this.ToRotate == null) {
			this.ToRotate = Camera.main.transform;
		}
	}
	
	// Update is called once per frame
	void Update () {
		var isMouse = Input.GetMouseButton (0);
		if (isMouse) {
			var curMouse = Input.mousePosition;
			if (mouseHeld) {
				var delta = (curMouse - mousePrev);
				ToRotate.RotateAround (this.RotateAroundThis.position, Vector3.up, delta.x * 2.0f);
			}
			mousePrev = curMouse;
			mouseHeld = true;
		} else if (mouseHeld) {
			mouseHeld = false;
		}
	}
}
