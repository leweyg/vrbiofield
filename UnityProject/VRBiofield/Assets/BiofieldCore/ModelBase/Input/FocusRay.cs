using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FocusRay : MonoBehaviour {

	private bool IsMouseAiming = false;
	private GameObject MouseAimObject = null;
	public bool IsTestGazeOnly = false;

	public Transform FocusSource {
		get { 
			// TODO: support other focus sources, such as hand controller here
			if (IsMouseAiming) {
				return this.MouseAimObject.transform;
			}

			// default is HMD/camera orientation:
			return Camera.main.transform;
		}
	}

	public bool IsMouseOrTouch {
		get { return this.IsMouseAiming; }
	}

	public bool IsHeadGaze {
		get { return !this.IsMouseAiming; }
	}

	public Ray CurrentRay {
		get {
			return new Ray (
				this.FocusSource.position,
				this.FocusSource.forward);
		}
	}

	private static FocusRay mainInst = null;
	public static FocusRay main {
		get {
			if (mainInst != null)
				return mainInst;
			mainInst = GameObject.FindObjectOfType<FocusRay> ();
			Debug.Assert (mainInst != null, "Please add a 'FocusRay' object to the scene");
			return mainInst;
		}
	}


	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (UnityEngine.VR.VRSettings.enabled || IsTestGazeOnly) {
		} else if (Input.mousePosition != Vector3.zero) {
			this.IsMouseAiming = true;
			var r = Camera.main.ScreenPointToRay (Input.mousePosition);

			this.EnsureMouseAimObject ();
			this.MouseAimObject.transform.position = r.origin;
			this.MouseAimObject.transform.rotation = Quaternion.LookRotation (r.direction);
		} else {
			this.IsMouseAiming = false;
		}
	}

	private void EnsureMouseAimObject() {
		if (this.MouseAimObject != null) {
			return;
		}

		this.MouseAimObject = new GameObject ("Mouse Aim Object");
	}
}
