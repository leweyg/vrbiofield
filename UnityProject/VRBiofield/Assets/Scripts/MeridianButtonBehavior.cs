using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeridianButtonBehavior : MonoBehaviour, MenuSimpleButton.IButtonReciever {

	public MeridianPath.EMeridian MeridianId;
	private MenuSimpleButton[] SubButtons = null;
	private ExcersizeSharedScheduler Excersizer = null;

	[TextArea] public string MainTitle = "Organ";
	[TextArea] public string SubText = "Details about it";
	[TextArea] public string ReleasesThese = "Old Chi";
	[TextArea] public string RegainThese = "Strong Chi";
	public int DayTimeStart;
	public bool StartInPM = false;

	// Use this for initialization
	void Start () {
		if (!Excersizer) {
			Excersizer = GameObject.FindObjectOfType<ExcersizeSharedScheduler> ();
		}
		ExcersizeAppState.main.EnsureSetup ();
		ExcersizeAppState.main.State.OnMeridianChanged += UpdateFromState;
		UpdateFromState ();
	}

	void UpdateFromState() {
		if (SubButtons == null) {
			SubButtons = this.GetComponentsInChildren<MenuSimpleButton> ();
		}
		var ms = ExcersizeAppState.main.State.GetMeridianState (this.MeridianId);
		foreach (var sb in this.SubButtons) {
			bool isActive = false;
			switch (sb.Action) {
			case MenuSimpleButton.StandardButton.ContextBalance:
				isActive = (ms.Direction == 0);
				break;
			case MenuSimpleButton.StandardButton.ContextIncrease:
				isActive = (ms.Direction > 0);
				break;
			case MenuSimpleButton.StandardButton.ContextDecrease:
				isActive = (ms.Direction < 0);
				break;
			}
			sb.IsMode2 = isActive;
		}
	}

	// Update is called once per frame
	public bool IsShowingKids = true;
	void Update () {
		bool isActive = false;
		if ((Excersizer.CurrentActivity) && (this.Excersizer.CurrentActivity.Instances.Count > 0) 
			&& (this.Excersizer.CurrentActivity.Instances[0] is MeridianOrgans)) {
			isActive = true;
		}
		if (this.IsShowingKids != isActive) {
			IsShowingKids = isActive;
			for (int ki = 0; ki < this.transform.childCount; ki++) {
				this.transform.GetChild (ki).gameObject.SetActive (isActive);
			}
			if (isActive) {
				this.UpdateFromState ();
			}
		}
	}

	#region IButtonReciever implementation

	public void ButtonHoverChanged (MenuSimpleButton.StandardButton bt, bool isHovered)
	{
		var appState = ExcersizeAppState.main.State;
		if (isHovered) {
			appState.HoverMeridian = this.MeridianId;
		} else {
			appState.HoverMeridian = MeridianPath.EMeridian.Unknown;
		}
	}

	public void ButtonSelected (MenuSimpleButton.StandardButton bt)
	{
		Debug.Log ("Meta button clicked: : " + bt);
		var appState = ExcersizeAppState.main.State;
		var dir = -2;
		switch (bt) {
		case MenuSimpleButton.StandardButton.ContextDecrease:
			dir = -1;
			break;
		case MenuSimpleButton.StandardButton.ContextBalance:
			dir = 0;
			break;
		case MenuSimpleButton.StandardButton.ContextIncrease:
			dir = 1;
			break;
		default:
			return; // ignore it
		}
		Debug.Log ("Meridian State changed: " + this.MeridianId + " to " + dir);
		appState.GetMeridianState (this.MeridianId).Direction = dir;
		appState.DoMeridianChanged ();
	}

	#endregion
}
