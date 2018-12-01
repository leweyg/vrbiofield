using UnityEngine;
using System.Collections;

public class BiofieldAppMain : MonoBehaviour {

	public bool TakeScreenshotOnS = false;
	private ExcersizeSharedScheduler Scheduler;
	private Abacus AbacusObj;

	void SetupExcersizeControls() {
		var book = GameObject.FindObjectOfType<BookAnimScript> ();
		Scheduler = (Scheduler!=null) ? Scheduler : GameObject.FindObjectOfType<ExcersizeSharedScheduler> ();
		AbacusObj = (AbacusObj!=null) ? AbacusObj : GameObject.FindObjectOfType<Abacus> ();

		var sched = this.Scheduler;
		book.OnPageChanged += ((int newPage) => {
			var ia = ((newPage/2) % sched.Activities.Length);
			var act = sched.Activities[ia];
			sched.UpdateCurrentActivity(act);
			ExcersizeAppState.main.AudioManager.ChangeTrack(null);
		});
	}

	// Use this for initialization
	void Start () {
		this.SetupExcersizeControls ();
	}

	private int ScreenshotCount = 0;

	void UpdateStuffEachFrame() {

		if ((this.AbacusObj != null) && (this.Scheduler != null)) {
			var ar = this.AbacusObj.AllRails;
			var br = this.Scheduler.Breath;
			var bpr = br.CurrentBreathsPerRep;
			float bt = 1.0f - ((Mathf.Cos (br.UnitTimeSinceStart  * Mathf.PI * 2.0f) * 0.5f) + 0.5f);

			// TODO: put biosensor "calmness" value here:
			//ar [0].SetBeadCountAndNumber(1, 0 to 1 value, ideally 0.5 is calm, 0 is too low, 1 is too high);

			ar [1].SetBeadCountAndNumber(1, bt);
			ar [2].SetBeadCountAndNumberABA (bpr, br.UnitTimeSinceStart);
			ar [3].SetBeadCountAndNumberABA (6, br.UnitTimeSinceStart / ((float)bpr) );
		}
	}
	
	// Update is called once per frame
	void Update () {
		
		this.UpdateStuffEachFrame ();

		// check for special things:
		if (TakeScreenshotOnS && Input.GetKeyUp (KeyCode.S)) {
			ScreenshotCount++;
			ScreenCapture.CaptureScreenshot ("screenshot" + ScreenshotCount + ".png", 2);
		}
	}
}
