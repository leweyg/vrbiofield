using UnityEngine;
using System.Collections;

public class ExcersizeSharedScheduler : MonoBehaviour {

	public ExcersizeBreathController Breath { get; set; }
	public ExcersizeActivityBase CurrentActivity;
	public ExcersizeActivityBase[] Activities;
	public BodyLandmarks AvatarGuide, AvatarInfo, AvatarUser;

	public enum ActivityAvatar {
		Guide,
		Info,
		User
	};

	public void EnsureSetup() {
		if (this.Breath == null) {
			this.Breath = this.gameObject.GetComponent<ExcersizeBreathController> ();
		}
		if (this.Breath == null) {
			this.Breath = GameObject.FindObjectOfType<ExcersizeBreathController> ();
			Debug.Assert (this.Breath != null);
		}
		this.Breath.EnsureSetup ();
	}

	public BodyLandmarks GetBody(ActivityAvatar av) {
		BodyLandmarks body = null;
		switch (av) {
		case ActivityAvatar.Guide:
			body = this.AvatarGuide;
			break;
		case ActivityAvatar.Info:
			body = this.AvatarInfo;
			break;
		case ActivityAvatar.User:
			body = this.AvatarUser;
			break;
			default:
			throw new System.ArgumentException("Unknown avatar: " + av);
		}
		if (body == null) {
			throw new System.ArgumentException ("Shared avatar not set: " + av);
		}
		return body;
	}

	private ExcersizeActivityBase cachedActivity = null;
	public void UpdateCurrentActivity(ExcersizeActivityBase act) {
		this.cachedActivity = act;
		this.CurrentActivity = act;
		//Debug.Log("Changing activity to: " + ((act!=null)?act.ActivityName : "NULL"));
		foreach (var ob in this.Activities) {
			if (ob != act) {
				ob.gameObject.SetActive (false);
			}
		}
		if (act != null) {
			act.gameObject.SetActive (true);
		}
	}

	// Use this for initialization
	void Start () {
		this.EnsureSetup ();
		this.UpdateCurrentActivity (this.CurrentActivity);
	}
	
	// Update is called once per frame
	void Update () {
		if (this.CurrentActivity != this.cachedActivity) {
			this.UpdateCurrentActivity (this.CurrentActivity);
		}
	}
}
