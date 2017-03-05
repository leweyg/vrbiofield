#define USE_BIOSENSOR_HW 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BioSensorHost : MonoBehaviour {

	public ExcersizeSharedScheduler ExcersizeSystem;
	public Abacus DebugAbacus;
	public bool TestHeartRateNow = false;
	public bool TestUserBreathTiming = false;
	public bool UserBreathingIn = false;
	public float READ_UserBreathTime = 0.0f;

	void Start () {
		ExcersizeSystem.EnsureSetup ();

		// TODO: BioSensor setup stuff
	}
	
	// Update is called once per frame
	void Update () {
		#if USE_BIOSENSOR_HW



		// TODO: Put BioSensor reading stuff here:
		bool sensorInited = TestHeartRateNow;
		float heartBeatsPerMinute = 70.0f;
		bool debugUnitCalmnessValue = false;
		float unitCalmnessValue = 0.75f;



		// Now install those values into the system:
		var br = this.ExcersizeSystem.Breath;
		if (sensorInited) {
			br.UseHeartBeats = true;
			br.HeartBeatsPerMinute = heartBeatsPerMinute;
			// NOTE: above will cause the chakra visualization to pulsate at this rate
		} else {
			br.UseHeartBeats = false;
		}
		if (TestUserBreathTiming) {
			br.IsUsingUserBreathRate = true;
			br.IsUserBreathingIn = this.UserBreathingIn;
		} else {
			br.IsUsingUserBreathRate = false;
		}
		this.READ_UserBreathTime = br.CurrentEstBreathDuration;
		if (debugUnitCalmnessValue) {
			var ar = this.DebugAbacus.AllRails[0];
			ar.SetBeadCountAndNumber(1, unitCalmnessValue);
			ar.TextDisplayUnitScalar = 100.0f; // scale value up
			ar.ForceTextDisplay = true;
		}
		#endif
	}
}
