using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ExcersizeBreathController : MonoBehaviour {

	public float TimePerBreath = 9.0f;
	public int CurrentBreathsPerRep = 7;

	public float TimeStarted { get; private set; }
	public float UnitTimeSinceStart { get; private set; }

	public bool AnimationBreathIsActive { get; set; }
	public float AnimationBreathPerSecond = 10;
	public bool AnimationBreathIsBreathingIn = false;
	public bool AnimationBreathIsNextAvailable = true;

	public bool IsUsingUserBreathRate { get; set; }
	public bool IsUserBreathingIn { get; set; }
	private float UserBreathEstTimePerBreath = 10.0f;
	private bool WasUserBreathingIn { get; set; }
	private List<float> UserRecentHalfBreathTimes = new List<float> ();
	private float UserTimeInHalfBreath = 0.0f;
	public float CurrentEstBreathDuration { get { 
			if (this.IsUsingUserBreathRate)
				return this.UserBreathEstTimePerBreath;
			else
				return this.TimePerBreath;
		} }


	public bool UseHeartBeats { get; set; }
	public float HeartBeatsPerMinute { get; set; }
	public float HeartBeatUnitTotalTime { get; private set; }

	public int BreathIndex { get { return ((int)UnitTimeSinceStart);
		} }

	public float UnitTimeInBreath { get { return (UnitTimeSinceStart - ((float)BreathIndex));
		}}

	public bool IsBreathingIn { get { 
			var timeFrac = this.UnitTimeInBreath;
			return (timeFrac < 0.5f);
		} }

	public static float Fraction(float f) {
		return (f - Mathf.Floor (f));
	}

	public float HeartBeatUnitAlpha { get { return UnitTo010( Fraction (HeartBeatUnitTotalTime) );
	} }

	public static float UnitTo010(float t) {
		var alpha = 1.0f - Mathf.Clamp01 (Mathf.Abs ((t - 0.5f) * 2.0f));
		return alpha;
	}

	public static float UnitTo010Smooth(float t) {
		var alpha = 1.0f - ((Mathf.Cos (t * Mathf.PI * 2.0f) * 0.5f) + 0.5f);
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
		this.HeartBeatsPerMinute = 70.0f;
	}

	// Use this for initialization
	void Start () {
		this.EnsureSetup ();
	}

	public void RestartTimer() {
		this.TimeStarted = Time.time;
		this.UnitTimeSinceStart = 1.0f;
		this.UpdateTimer ();
	}

	public float ApproachValue(float v, float delta, float mx) {
		var biasPct = 0.5f;
		var pctToGo = Mathf.Lerp( Mathf.Clamp01 (1.0f - (v / mx)), 1.0f, biasPct );
		var ans = v + (delta * pctToGo);
		ans = Mathf.Min (ans, mx - delta);
		return ans;
	}

	private float CalculateAverageBreathTime() {
		float sum = 0.0f;
		int count = 0;
		foreach (var t in this.UserRecentHalfBreathTimes) {
			sum += t; count++;
		}
		return (sum / ((float)count)) * 2.0f;
	}

	public void SetAnimationBreathNext(bool isIn, float timeUntilChange, bool hasNext = true) {
		AnimationBreathIsActive = true;
		AnimationBreathIsNextAvailable = hasNext;
		if (hasNext) {
			AnimationBreathIsBreathingIn = isIn;
			AnimationBreathPerSecond = timeUntilChange;
			//Debug.Log ("Set time breath: in=" + isIn + " in_s=" + timeUntilChange);
			UnitTimeSinceStart = (isIn ? 0.5f : 0.0f) + Mathf.Floor (UnitTimeSinceStart);
		}
	}

	public void SetAnimationBreathEnd() {
		AnimationBreathIsActive = false;
	}

	public float DEBUG_CT = 0.0f;

	private int mUpdatedFrameIndex = 0;
	public void EnsureUpdated() {
		if (Time.frameCount == this.mUpdatedFrameIndex) {
			return;
		}
		this.mUpdatedFrameIndex = Time.frameCount;

		this.UpdateTimer ();
	}

	public void UpdateTimer() {

		if (AnimationBreathIsActive) {
			var ct = Fraction (this.UnitTimeSinceStart);
			ct += (Time.deltaTime / this.AnimationBreathPerSecond) * 0.5f * 0.95f;
			if (AnimationBreathIsNextAvailable) {
				if (!this.AnimationBreathIsBreathingIn) {
					if (ct > 0.5f) {
						ct = 0.5f; //(ct - 0.5f);
					}
				} else {
					if (ct < 0.5f) {
						ct = 0.5f; // + (0.5f - ct);
					}
				}
			}
			DEBUG_CT = ct;
			this.UnitTimeSinceStart = Mathf.Floor (this.UnitTimeSinceStart) + ct;
		} else if (IsUsingUserBreathRate) {
			
			// Only use for biosensors:
			var ct = Fraction (this.UnitTimeSinceStart);
			if (this.WasUserBreathingIn != this.IsUserBreathingIn) {
				this.WasUserBreathingIn = this.IsUserBreathingIn;

				if (UserTimeInHalfBreath > 0.2f) {
					this.UserRecentHalfBreathTimes.Add (this.UserTimeInHalfBreath);
				}
				if (this.UserRecentHalfBreathTimes.Count >= 2) {
					this.UserBreathEstTimePerBreath = CalculateAverageBreathTime ();
					if (this.UserRecentHalfBreathTimes.Count > 4) {
						this.UserRecentHalfBreathTimes.Remove (0);
					}
				}
				this.UserTimeInHalfBreath = 0.0f;


				if (this.IsUserBreathingIn) {
					ct = Mathf.Min (ct, 1.0f);
					ct = (1.0f - ct) + 1.0f;
				} else {
					ct = Mathf.Min (ct, 0.5f);
					ct = (0.5f - ct) + 0.5f;
				}
			} else {
				UserTimeInHalfBreath += Time.deltaTime;
				float nextGoal = (this.IsUserBreathingIn ? 0.5f : 1.0f);
				ct = ApproachValue (ct, (Time.deltaTime / this.UserBreathEstTimePerBreath), nextGoal);
			}
			this.UnitTimeSinceStart = Mathf.Floor (this.UnitTimeSinceStart) + ct;

		} else {
			
			// Standard method: simple time:
			float ftime = ((Time.time - this.TimeStarted) / this.TimePerBreath);
			this.UnitTimeSinceStart = ftime;
		}

		this.HeartBeatUnitTotalTime += ((this.HeartBeatsPerMinute / 60.0f) * Time.deltaTime);
	}
	
	// Update is called once per frame
	void Update () {
		this.EnsureUpdated ();
	}
}
