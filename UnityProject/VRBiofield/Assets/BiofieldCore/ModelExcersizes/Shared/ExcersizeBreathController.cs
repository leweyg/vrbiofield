using UnityEngine;
using System.Collections;

public class ExcersizeBreathController : MonoBehaviour {

	public float TimePerBreath = 9.0f;
	public int CurrentBreathsPerRep = 7;

	public float TimeStarted { get; private set; }
	public float UnitTimeSinceStart { get; private set; }

	public int BreathIndex { get { return ((int)UnitTimeSinceStart);
		} }

	public float UnitTimeInBreath { get { return (UnitTimeSinceStart - ((float)BreathIndex));
		}}

	public bool IsBreathingIn { get { 
			var timeFrac = this.UnitTimeInBreath;
			return (timeFrac < 0.5f);
		} }

	public static float UnitTo010(float t) {
		var alpha = 1.0f - Mathf.Clamp01 (Mathf.Abs ((t - 0.5f) * 2.0f));
		return alpha;
	}

	public float UnitTo010f(float t) {
		return UnitTo010 (t);
	}

	public float UnitBreathInPct { get { return UnitTo010 (this.UnitTimeInBreath);
		} }

	public float UnitFadeInPct { get { return Mathf.Clamp01 (this.UnitBreathInPct * 1.5f);
		} }


	private bool isSetup = false;
	public void EnsureSetup() {
		if (isSetup)
			return;
		isSetup = true;
		this.RestartTimer ();
	}

	// Use this for initialization
	void Start () {
		this.EnsureSetup ();
	}

	public void RestartTimer() {
		this.TimeStarted = Time.time;
		this.UpdateTimer ();
	}

	public void UpdateTimer() {
		float ftime = ((Time.time - this.TimeStarted) / this.TimePerBreath);
		this.UnitTimeSinceStart = ftime;
	}
	
	// Update is called once per frame
	void Update () {
		this.UpdateTimer ();
	}
}
