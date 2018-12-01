using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeridianInfoBehavior : MonoBehaviour {

	public TextMesh MainLabel;
	public TextMesh DetailsLabel;
	public string ExampleLineLength = "This is like a really";
	public Color ColorRelease = Color.white;
	public Color ColorRegain = Color.white;

	private MeridianButtonBehavior[] MerButtons;
	private MeridianPath.EMeridian CurrentToShow = MeridianPath.EMeridian.Unknown;
	private bool IsShowingInfo = true;

	// Use this for initialization
	void Start () {
		this.MerButtons = this.transform.parent.GetComponentsInChildren<MeridianButtonBehavior> ();

		MainLabel.text = "";
		DetailsLabel.text = "";

		ExcersizeAppState.main.EnsureSetup ();
		ExcersizeAppState.main.State.OnMeridianChanged += () => {
			this.UpdateMeridianInfoText ();
		};
	}

	public static string WordWrapOneLine(string line, int maxLength) {
		var parts = line.Split (' ');
		var ans = "";
		int curLength = 0;
		foreach (var p in parts) {
			if (((curLength + p.Length) <= maxLength) || (curLength == 0)) {
				// should fit
			} else {
				// needs new line
				ans += "\n";
				curLength = 0;
			}
			ans += p + " ";
			curLength += p.Length + 1;
		}
		return ans;
	}

	public static string WordWrap(string manyLines, int maxLength) {
		var lines = manyLines.Split ('\n');
		var ans = "";
		foreach (var ln in lines) {
			ans += WordWrapOneLine (ln, maxLength) + "\n";
		}
		return ans.Trim();
	}

	public void UpdateMeridianInfoText() {
		var info = MerButtons [0];
		foreach (var i in MerButtons) {
			if (i.MeridianId == this.CurrentToShow)
				info = i;
		}

		this.MainLabel.text = info.MainTitle;

		var clrRelease = "<color=#" + ColorUtility.ToHtmlStringRGBA (this.ColorRelease) + ">";
		var clrRegain = "<color=#" + ColorUtility.ToHtmlStringRGBA (this.ColorRegain) + ">";

		var clrActive = "<color='grey'>";
		var clrDeactive = "<color='white'>";

		var ms = ExcersizeAppState.main.State.GetMeridianState (this.CurrentToShow);
		var clrTextRelease = ((ms.Direction < 0) ? clrDeactive : clrActive );
		var clrTextRegain = ((ms.Direction > 0) ? clrDeactive : clrActive );

		var intotext = clrRelease + "release:</color>" + clrTextRelease + "\n " + WordWrap(info.ReleasesThese.Replace ("\n", "\n "), this.ExampleLineLength.Length);
		intotext += "\n</color>" + clrRegain + "regain:</color>" + clrTextRegain + "\n " + WordWrap (info.RegainThese.Replace ("\n", "\n "), this.ExampleLineLength.Length) + "\n</color>";

		this.DetailsLabel.text = intotext; //WordWrap (intotext, this.ExampleLineLength.Length);
	}
	
	// Update is called once per frame
	void Update () {
		var kidsShowing = this.MerButtons [0].IsShowingKids;
		if (this.IsShowingInfo != kidsShowing) {
			this.IsShowingInfo = kidsShowing;
			for (int ci = 0; ci < this.transform.childCount; ci++) {
				this.transform.GetChild (ci).gameObject.SetActive (this.IsShowingInfo);
			}
		}

		var appState = ExcersizeAppState.main.State;
		var hm = appState.HoverMeridian;
		if (hm != MeridianPath.EMeridian.Unknown) {
			if (hm != this.CurrentToShow) {
				CurrentToShow = hm;
				UpdateMeridianInfoText ();
			}
		}
	}
}
