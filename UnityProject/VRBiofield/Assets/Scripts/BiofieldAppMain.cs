using UnityEngine;
using System.Collections;

public class BiofieldAppMain : MonoBehaviour {

	public bool TakeScreenshotOnS = false;

	// Use this for initialization
	void Start () {
	
	}

	private int ScreenshotCount = 0;
	
	// Update is called once per frame
	void Update () {
		if (TakeScreenshotOnS && Input.GetKeyUp (KeyCode.S)) {
			ScreenshotCount++;
			Application.CaptureScreenshot ("screenshot" + ScreenshotCount + ".png", 2);
		}
	}
}
