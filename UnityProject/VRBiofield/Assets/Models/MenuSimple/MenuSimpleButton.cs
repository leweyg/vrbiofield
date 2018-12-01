using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(IsInputConsumer))]
public class MenuSimpleButton : MonoBehaviour {

	public StandardButton Action = StandardButton.None;
	private Material MatInst;
	private float TimeStarted = 0.0f;
	private IsInputConsumer Inputter;
	private float SelectPct = 0.0f;
	private IButtonReciever ParentReciever;
	private Color DefaultColor, Default2Color, BasicColor;

	private bool isMode2 = false;
	public bool IsMode2 {
		get { return isMode2; }
		set {
			if (value == isMode2) {
				return;
			}
			this.EnsureSetup ();
			isMode2 = value;
			BasicColor = (isMode2 ? Default2Color : DefaultColor);
			if (this.MatInst) {
				this.MatInst.SetColor ("_BasicColor", this.BasicColor);
			}
		}
	}

	public enum StandardButton {
		None,
		PlayOrPause,
		Next, 
		Previous,
		ContextSelect,
		ContextDecrease,
		ContextBalance,
		ContextIncrease,

	};

	public interface IButtonReciever {
		void ButtonHoverChanged(StandardButton bt, bool isHovered);
		void ButtonSelected (StandardButton bt);
	}

	private bool mIsSetup = false;
	public void EnsureSetup() {
		if (mIsSetup)
			return;
		mIsSetup = true;

		var ic = this.GetComponent<IsInputConsumer> ();
		if (!ic) {
			ic = this.gameObject.AddComponent<IsInputConsumer> ();
		}
		ic.IsShowCursorOver = true;
		Inputter = ic;
		if (ParentReciever == null) {
			ParentReciever = this.gameObject.GetComponentInParent<IButtonReciever> ();
		}

		var mr = this.GetComponent<MeshRenderer> ();
		if (mr) {
			mr.material.SetFloat ("CreateInstance", 1.0f);
			this.MatInst = mr.material;
			this.DefaultColor =  this.MatInst.GetColor ("_DefaultColor");
			this.Default2Color = this.MatInst.GetColor ("_Default2Color");
			this.BasicColor = this.DefaultColor;
			this.MatInst.SetColor ("_BasicColor", this.BasicColor);
		}

		ic.FocusEntered += (FocusCursor ray) => {
			this.MatInst.SetFloat("_HighlightPct", 1.0f);
			this.TimeStarted = Time.time;
			SelectPct = 0.0f;
			if (ParentReciever != null) {
				ParentReciever.ButtonHoverChanged(this.Action, true);
			}
		};
		ic.FocusExited += (FocusCursor ray) => {
			this.MatInst.SetFloat("_HighlightPct", 0.0f);
			this.MatInst.SetFloat("_SelectionPct", 0.0f);
			SelectPct = 0.0f;
			if (ParentReciever != null) {
				ParentReciever.ButtonHoverChanged(this.Action, false);
			}
		};
		ic.FocusSelected += (FocusCursor ray) => {
			ButtonDoSelected();
		};
	}

	void ButtonDoSelected() {
		Debug.Log ("Button clicked");
		this.MatInst.SetFloat("_HighlightPct", 0.0f);
		this.IsMode2 = !this.isMode2;
		if (ParentReciever != null) {
			ParentReciever.ButtonSelected(this.Action);
		}
	}

	// Use this for initialization
	void Start () {
		this.EnsureSetup ();
	}
	
	// Update is called once per frame
	void Update () {
		if (Inputter.CursorOver && !Inputter.CursorOver.IsClickPossible) {
			var prevPct = SelectPct;
			SelectPct = Mathf.Clamp01 (((Time.time - this.TimeStarted) - 0.5f) / 1.0f);
			if (SelectPct != 1.0f) {
				this.MatInst.SetFloat("_SelectionPct", this.SelectPct);
			} else {
				this.MatInst.SetFloat ("_SelectionPct", 0.0f);
				if (prevPct < 1.0f) {
					this.ButtonDoSelected ();
				}
			}
		}
	}
}
