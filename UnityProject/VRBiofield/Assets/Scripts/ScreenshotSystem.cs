using UnityEngine;
using System.Collections;

public class ScreenshotSystem : MonoBehaviour {

	private int imageIndex = 0;

	// Use this for initialization
	void Start () {
	
	}
	[ContextMenu("Take Screenshot")]
	public void DoScreenshot() {			
		var shotName = "screenshot_" + (imageIndex++) + ".png";
		Application.CaptureScreenshot(shotName, 2);
		Debug.Log("Saved image to " + shotName);
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyUp(KeyCode.P)) {
			this.DoScreenshot ();
		}
	}
}
