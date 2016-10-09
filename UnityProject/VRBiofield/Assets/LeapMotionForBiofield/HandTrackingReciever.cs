using UnityEngine;
using System.Collections;
using Leap;

public class HandTrackingReciever {

	public SimpleHandInfo LeftInfo = new SimpleHandInfo();
	public SimpleHandInfo RightInfo = new SimpleHandInfo();

	private Leap.Controller mController;
	private Matrix4x4 CachedLocalToWorld = Matrix4x4.identity;

	public class SimpleHandInfo {
		public bool IsTracked;
		public Leap.Vector LeapPos;
		public Leap.LeapTransform LeapTrans;
	};
		

	public void OnEnable() {
		if (this.mController != null)
			return;
		
		this.mController = new Controller ();
		this.mController.Connect += (object sender, ConnectionEventArgs e) => {
			Debug.Log("Leap Connected!");
		};
		this.mController.FrameReady += (object sender, FrameEventArgs e) => {
			//Debug.Log("Leap Frame Recieved");
			bool leftTracked = false;
			bool rightTracked = false;
			foreach (var h in e.frame.Hands) {
				if (h.IsLeft) {
					leftTracked = true;
					this.LeftInfo.IsTracked = true;
					this.LeftInfo.LeapPos = h.PalmPosition;
					this.LeftInfo.LeapTrans = h.Basis;
				}
				else {
					rightTracked = true;
					this.RightInfo.IsTracked = true;
					this.RightInfo.LeapPos = h.PalmPosition;
					this.RightInfo.LeapTrans = h.Basis;
				}
			}
			if (!leftTracked) this.LeftInfo.IsTracked = false;
			if (!rightTracked) this.RightInfo.IsTracked = false;
		};
	}

	public void OnDisable() {
		this.mController.Dispose ();
		this.mController = null;
	}
}
