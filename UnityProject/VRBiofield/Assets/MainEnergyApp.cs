using UnityEngine;
using System.Collections;
using System.IO;

public class MainEnergyApp : MonoBehaviour {

	public GameObject Arrows;
	public GameObject Meshes;
	public GameObject Volume;

	// Use this for initialization
	void Start () {
	
	}

	void ShowOnly(GameObject gm) {
		Arrows.SetActive (false);
		Meshes.SetActive (false);
		Volume.SetActive (false);
		if (gm != null) {
			gm.SetActive (true);
		}
	}

	[ContextMenu("Export Flow Arrows to CSV")]
	void ExportVectorsToCSV() {
		var w2l = this.Meshes.GetComponentInChildren<MeshFilter>().transform.worldToLocalMatrix;
		var arrows = this.Arrows.gameObject.GetComponentsInChildren<FlowVertexNode> ();
		StreamWriter sw = new StreamWriter ("hand_flow_points.csv");
		sw.WriteLine ("X,Y,Z,dx,dy,dz");
		foreach (var a in arrows) {
			var lpos = w2l.MultiplyPoint (a.transform.position);
			var lfwd = w2l.MultiplyVector (a.transform.up).normalized;
			sw.WriteLine (
				"" + lpos.x + "," + lpos.y + "," + lpos.z + ","
				+ lfwd.x + "," + lfwd.y + "," + lfwd.z);
		}
		sw.Close ();
	}

	private int imageIndex = 0;
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyUp(KeyCode.Alpha1)) {
			ShowOnly(this.Meshes);
		}
		if (Input.GetKeyUp(KeyCode.Alpha2)) {
			ShowOnly(this.Arrows);
		}
		if (Input.GetKeyUp(KeyCode.Alpha3)) {
			ShowOnly(this.Volume);
		}
		if (Input.GetKeyUp(KeyCode.Alpha4)) {
			ShowOnly(null);
		}
		if (Input.GetKeyUp(KeyCode.P)) {
			var shotName = "screenshot_" + (imageIndex++) + ".png";
			Application.CaptureScreenshot(shotName, 2);
			Debug.Log("Saved image to " + shotName);
		}
	}
}
