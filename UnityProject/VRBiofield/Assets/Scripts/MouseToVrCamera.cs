using UnityEngine;
using System.Collections;

public class MouseToVrCamera : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}

	private bool HasPrevious;
	private Vector3 PreviousMouse;
	private float RotateSpeed = 0.1f;
	private float RotateX = 0.0f;
	private float RotateY = 0.0f;
	
	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButton (0)) {
			var curPos = Input.mousePosition;
			if (HasPrevious) {
				var delta = (curPos - PreviousMouse);
				this.RotateX = delta.x * RotateSpeed * -1.0f;
				this.RotateY = delta.y * RotateSpeed;
				//this.transform.rotation = Quaternion.identity;
				//this.transform.RotateAround (this.transform.position, Vector3.right, this.RotateY);
				//this.transform.RotateAround (this.transform.position, Vector3.up, this.RotateX);
				this.transform.RotateAround (this.transform.position, this.transform.right, this.RotateY);
				this.transform.RotateAround (this.transform.position, Vector3.up, this.RotateX);
			}
			PreviousMouse = curPos;
			HasPrevious = true;
		} else {
			HasPrevious = false;
		}
	}
}
