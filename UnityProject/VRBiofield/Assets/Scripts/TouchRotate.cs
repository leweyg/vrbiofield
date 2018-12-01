using UnityEngine;
using System.Collections;

public class TouchRotate : MonoBehaviour {

	Quaternion InitialRot;
	bool isMoving = false;
	Vector3 dragStart = Vector3.zero;

	// Use this for initialization
	void Start () {
		this.InitialRot = this.transform.rotation;
	}
	
	// Update is called once per frame
	void Update () {
		bool curMoving = UnityEngine.Input.GetMouseButton (0);

	}
}
