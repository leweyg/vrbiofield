using UnityEngine;
using System.Collections;

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
			Application.CaptureScreenshot(shotName, 4);
			Debug.Log("Saved image to " + shotName);
		}
	}
}
