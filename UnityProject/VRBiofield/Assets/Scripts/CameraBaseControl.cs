using UnityEngine;
using System.Collections;

public class CameraBaseControl : MonoBehaviour {

	public Transform CameraCorrector;
	public bool AutoResetView = false;
	public float AutoResetRadius = 0.5f;
	public bool KeyReset = true;
	public bool KeyExitFromEscape = false;

	public void EnsureSetup() {
		if (this.CameraCorrector == null) {
			this.CameraCorrector = this.transform.GetChild (0);
		}
	}

	[ContextMenu("Do Reset View Now")]
	public void DoResetViewNow() {
		this.ResetCameraOrientToBase();
	}

	public void ResetCameraOrientToBase() {
		var cam = Camera.main.transform;
		var ideal = this.transform;
		var control = this.CameraCorrector;

		control.localRotation = Quaternion.identity;
		control.localPosition = Vector3.zero;

		var rot = Quaternion.LookRotation (
			(new Vector3( cam.forward.x, 0, cam.forward.z )).normalized, 
			Vector3.up);

		control.localRotation = ideal.rotation * (Quaternion.Inverse (rot));
		control.localPosition = ideal.position - cam.position;
	}

	public static CameraBaseControl mainBase {
		get { return GetMainCameraBase (); }
	}

	private static CameraBaseControl CachedBaseControl = null;
	public static CameraBaseControl GetMainCameraBase() {
		if (CachedBaseControl == null) {
			CachedBaseControl = GameObject.FindObjectOfType<CameraBaseControl> ();
		}
		return CachedBaseControl;
	}


	// Use this for initialization
	void Start () {
		CachedBaseControl = this;
		this.EnsureSetup ();
	}
	
	// Update is called once per frame
	void Update () {
		if (AutoResetView) {
			var dist = (Camera.main.transform.position - this.transform.position).magnitude;
			if (dist > this.AutoResetRadius) {
				this.DoResetViewNow ();
			}
		}
		if (this.KeyReset && (Input.GetKeyUp (KeyCode.Space))) {
			this.DoResetViewNow ();
		}
		if (this.KeyExitFromEscape && (Input.GetKeyUp (KeyCode.Escape)) ){
			Application.Quit ();
		}
	}
}
