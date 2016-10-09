using UnityEngine;
using System.Collections;
using Leap;

public class BiofieldHandController : MonoBehaviour {

	public Transform LeftHandTransform;
	public Transform RightHandTransform;

	public Transform LeftHandEnergy;

	private static HandTrackingReciever mReciever;
	private Matrix4x4 CachedLocalToWorld = Matrix4x4.identity;

	void OnEnable() {
		if (mReciever == null) {
			mReciever = new HandTrackingReciever ();
		}
		mReciever.OnEnable ();
	}

	void OnDisable() {
		//mReciever.OnDisable ();
	}


	public Vector3 ToUnityBasic(Leap.Vector v) {
		var rawPos = new Vector3 (v.x, v.y, v.z);
		var lPos = rawPos * 0.001f;
		lPos.z *= -1.0f;
		var wPos = CachedLocalToWorld.MultiplyPoint (lPos);
		return wPos;
	}

	public Quaternion ToUnityRot(Leap.LeapTransform trans) {
		Matrix4x4 mat = Matrix4x4.identity;
		var center = ToUnityBasic(trans.TransformPoint (Vector.Zero));
		var axisFwd = ToUnityBasic(trans.TransformPoint (new Vector(0,1,0)));
		var axisUp = ToUnityBasic(trans.TransformPoint (new Vector(0,0,1)));
		return Quaternion.LookRotation ((axisFwd - center), axisUp - center);
	}


	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
		var CachedLocalToWorld = this.transform.localToWorldMatrix;
		var spaceCenter = CachedLocalToWorld.MultiplyPoint (Vector3.zero);

		Debug.DrawLine (spaceCenter, ToUnityBasic (mReciever.LeftInfo.LeapPos));
		Debug.DrawLine (spaceCenter, ToUnityBasic (mReciever.RightInfo.LeapPos));

		if (LeftHandTransform != null && mReciever.LeftInfo.IsTracked) {
			LeftHandTransform.position = ToUnityBasic (mReciever.LeftInfo.LeapPos);
			LeftHandTransform.rotation = ToUnityRot (mReciever.LeftInfo.LeapTrans);


		}
		if (RightHandTransform != null && mReciever.RightInfo.IsTracked) {
			RightHandTransform.position = ToUnityBasic (mReciever.RightInfo.LeapPos);
			RightHandTransform.rotation = ToUnityRot (mReciever.RightInfo.LeapTrans);

		}
	}
}
