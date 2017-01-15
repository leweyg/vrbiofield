using UnityEngine;
using System.Collections;

public class BiofieldAppMain : MonoBehaviour {

	public bool TakeScreenshotOnS = false;

	void SetupExcersizeControls() {
		var book = GameObject.FindObjectOfType<BookAnimScript> ();
		var sched = GameObject.FindObjectOfType<ExcersizeSharedScheduler> ();

		book.OnPageChanged += ((int newPage) => {
			var ia = ((newPage/2) % sched.Activities.Length);
			var act = sched.Activities[ia];
			sched.UpdateCurrentActivity(act);
		});
	}

	// Use this for initialization
	void Start () {
		this.SetupExcersizeControls ();
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
