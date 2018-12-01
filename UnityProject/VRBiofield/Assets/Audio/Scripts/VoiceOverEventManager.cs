using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoiceOverEventManager : MonoBehaviour {

	public AudioSource Player = null;
	public AudioSource BgAudio = null;
	public VoiceOverInfo CurrentTrack;
	private float PreviousTime = 0.0f;
	private VoiceOverInfo.VOEvent LatestEvent = null;
	private VoiceOverInfo.VOEvent NextEvent = null;
	private ExcersizeBreathController Breath;
	private bool FirstFrameAfterPause = true;
	public VoiceOverInfo[] AllTracks { get; private set; }
	public bool DEBUG_IsTicking = false;
	public delegate void TrackChangedEvent (VoiceOverInfo newTrackOrNull);
	public event TrackChangedEvent OnTrackChanged;

	private bool mIsSetup = false;
	public void EnsureSetup() {
		if (mIsSetup)
			return;
		mIsSetup = false;


		Breath = GameObject.FindObjectOfType<ExcersizeBreathController> ();

		if (Player == null) {
			Player = this.GetComponent<AudioSource> ();
		}

		this.AllTracks = this.GetComponentsInChildren<VoiceOverInfo> ();
		foreach (var t in this.AllTracks) {
			t.EnsureSetup ();
		}
		foreach (var t in this.AllTracks) {
			if (t.IsPlayAtStart) {
				this.ChangeTrack (t);
			}
		}
	}

	public bool IsBackgroundMusicPlaying {
		get { return this.BgAudio.isPlaying; }
		set {
			if (this.IsBackgroundMusicPlaying != value) {
				if (value) {
					this.BgAudio.Play ();
				} else {
					this.BgAudio.Stop ();
				}
				if (this.OnTrackChanged != null) {
					this.OnTrackChanged (this.CurrentTrack);
				}
			}
		}
	}

	public void ChangeTrack(VoiceOverInfo to) {
		if (this.CurrentTrack == to) {
			return;
		}
		this.CurrentTrack = to;
		if (to) {
			this.Player.clip = to.Clip;
			this.Player.time = 0.0f;
			this.Player.loop = to.IsLoopable;
			this.Player.Stop ();
			this.Player.Play ();

			if (to.RequiredExcersize) {
				ExcersizeAppState.main.Shared.UpdateCurrentActivity (to.RequiredExcersize);
			}
		} else {
			this.Player.Stop ();
			this.Player.clip = null;
			this.Breath.SetAnimationBreathEnd ();
		}
		if (this.OnTrackChanged != null) {
			this.OnTrackChanged (this.CurrentTrack);
		}
	}

	// Use this for initialization
	void Start () {
		this.EnsureSetup ();
	}
	
	// Update is called once per frame
	void Update () {
		
		if (!this.Player.isPlaying) {
			DEBUG_IsTicking = false;
			this.Breath.SetAnimationBreathEnd ();
			return;
		}
		if (!this.CurrentTrack) {
			DEBUG_IsTicking = false;
			this.Breath.SetAnimationBreathEnd ();
			return;
		}
		var curTime = this.Player.time;
		VoiceOverInfo.VOEvent foundNext = null;
		VoiceOverInfo.VOEvent foundFired = null;
		foreach (var e in this.CurrentTrack.EventData.events) {
			if ((PreviousTime < e.time) && (e.time <= curTime)) {
				foundFired = e;
			}
			else if (e.time > curTime) {
				foundNext = e;
				break;
			}
		}

		if ((this.NextEvent != foundNext)) {
			this.NextEvent = foundNext;
			if (foundNext != null) {
				this.Breath.SetAnimationBreathNext (foundNext.IsIn (), (foundNext.time - curTime));
				DEBUG_IsTicking = true;
			} else {
				this.Breath.SetAnimationBreathEnd ();
				DEBUG_IsTicking = false;
			}
		}

		if (foundFired != null) {
			this.LatestEvent = foundFired;
		} 

		PreviousTime = curTime;
	}
}
