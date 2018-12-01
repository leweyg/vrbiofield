using UnityEngine;
using System.Collections;

public class WASDController : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}

	public float MotionSpeed = 1.0f;
	public float RotateSpeed = 50.0f;
	public float UpCorrectSpeed = 50.0f;
	private Vector3 mLatestMouse = Vector3.zero;
	private bool mHasMouse = false;

	public void ResetCamera() {
	}
	
	// Update is called once per frame
	void Update () {
		bool anyKey = false;
		Vector3 unitMotion = new Vector3 (0, 0, 0);
		if (UnityEngine.Input.GetKey (KeyCode.W)) {
			anyKey = true;
			unitMotion += this.transform.forward;
		}
		if (UnityEngine.Input.GetKey (KeyCode.S)) {
			anyKey = true;
			unitMotion -= this.transform.forward;
		}
		if (UnityEngine.Input.GetKey (KeyCode.A)) {
			anyKey = true;
			unitMotion -= this.transform.right;
		}
		if (UnityEngine.Input.GetKey (KeyCode.D)) {
			anyKey = true;
			unitMotion += this.transform.right;
		}
		if (UnityEngine.Input.GetKey (KeyCode.Q)) {
			anyKey = true;
			unitMotion += this.transform.up;
		}
		if (UnityEngine.Input.GetKey (KeyCode.Z)) {
			anyKey = true;
			unitMotion -= this.transform.up;
		}

		if (anyKey && (unitMotion != Vector3.zero)) {
			var localMotion = this.transform.worldToLocalMatrix * unitMotion ;
			this.transform.Translate( localMotion * ( UnityEngine.Time.deltaTime * MotionSpeed ) );
		}


		if (UnityEngine.Input.GetMouseButton (0)) {
			if (this.mHasMouse) {
				var delta = UnityEngine.Input.mousePosition - this.mLatestMouse;
				delta = (delta * (RotateSpeed / Mathf.Min (Screen.width, Screen.height)));
				this.transform.Rotate (new Vector3 (-delta.y, delta.x, 0));

				// now fix the up vector:
				this.transform.rotation = Quaternion.LookRotation(this.transform.forward);
				//this.transform.up = new Vector3(0,1,0);
			}
			this.mHasMouse = true;
			this.mLatestMouse = UnityEngine.Input.mousePosition;
		} else {
			this.mHasMouse = false;
		}
	}
}
