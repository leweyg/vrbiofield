using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MirrorHeadLook : MonoBehaviour {

	public Transform HeadTransform;
	public Transform MirrorTransform;
	private Matrix4x4 HeadOriginal;
	private Matrix4x4 CamOriginal;

	// Use this for initialization
	void Start () {
		if (!MirrorTransform) {
			MirrorTransform = this.transform;
		}

		this.HeadOriginal = (this.HeadTransform.localToWorldMatrix);
		this.CamOriginal = MirrorRotMatrix(Camera.main.transform.localToWorldMatrix);
	}

	Vector3 MirrorVector(Vector3 dir) {
		var res = dir;
		res.z *= -1.0f;
		return res;
	}

	Matrix4x4 MirrorRotMatrix(Matrix4x4 mtx) {
		var fwd =  mtx.MultiplyVector (Vector3.forward);
		var up = mtx.MultiplyVector (Vector3.up);
		return Matrix4x4.TRS(Vector3.zero, Quaternion.LookRotation (MirrorVector (fwd), MirrorVector (up)), Vector3.one );
	}

	// Update is called once per frame
	void Update () {
		var cam = Camera.main.transform;
		var rot = MirrorRotMatrix(cam.localToWorldMatrix);

		var delta = (rot * this.CamOriginal.inverse);
		var res = ( delta * this.HeadOriginal);

		this.HeadTransform.rotation = res.rotation;
	}
}
