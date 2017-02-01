using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyPositioning : MonoBehaviour {

	private Dictionary<string,BodyBoneInfo> Bones = new Dictionary<string, BodyBoneInfo>();
	private string BoneNamePrefix = "MaleBaseMesh:";

	class BodyBoneInfo {
		public string Name;
		public Transform Object;
		public Vector3 InitialLocalPos;
		public Quaternion InitialLocalRot;
	}

	public void ResetPositioning() {
		foreach (var b in this.Bones.Values) {
			b.Object.localPosition = b.InitialLocalPos;
			b.Object.localRotation = b.InitialLocalRot;
		}
	}

	public void CopyPositioningFrom(BodyPositioning other) {
		this.BuildBoneMap ();
		other.BuildBoneMap ();

		foreach (var b in other.Bones) {
			if (this.Bones.ContainsKey (b.Key)) {
				var t = this.Bones [b.Key];
				var f = b.Value;

				t.Object.localPosition = f.Object.localPosition;
				t.Object.localRotation = f.Object.localRotation;
			}
		}
	}

	void BuildBoneMap() {
		if (this.Bones.Count > 0)
			return;
		
		var wasActive = this.gameObject.activeInHierarchy;
		this.gameObject.SetActive (true);
		
		var all = this.gameObject.GetComponentsInChildren<Transform> ();
		//Debug.Log ("Searching " + all.Length + " transforms....");
		foreach (var t in all) {
			if (t.gameObject.name.StartsWith (this.BoneNamePrefix)) {
				var bi = new BodyBoneInfo ();
				bi.Name = t.gameObject.name;
				bi.Object = t;
				bi.InitialLocalPos = t.localPosition;
				bi.InitialLocalRot = t.localRotation;
				this.Bones.Add (bi.Name, bi);
			}
		}

		this.gameObject.SetActive (wasActive);
		//Debug.Log ("Found " + this.Bones.Count + " bones.");
	}

}
