using UnityEngine;
using System.Collections;

public class IsInputConsumer : MonoBehaviour {

	public bool IsShowCursorOver = false;

	public delegate void FocusCusorEvent(FocusCursor ray);
	public event FocusCusorEvent FocusEntered;
	public event FocusCusorEvent FocusExited;
	public event FocusCusorEvent FocusSelected;
	public bool IsFocusedNow { get; private set; }
	public FocusCursor CursorOver { get; private set; }

	public void DoEvent_FocusEntered(FocusCursor ray) {
		IsFocusedNow = true;
		CursorOver = ray;
		if (FocusEntered!=null) {
			FocusEntered (ray);
		}
	}

	public void DoEvent_FocusExited(FocusCursor ray) {
		IsFocusedNow = false;
		CursorOver = null;
		if (FocusExited!=null) {
			FocusExited (ray);
		}
	}

	public void DoEvent_FocusSelected(FocusCursor ray) {
		if (FocusSelected!=null) {
			FocusSelected (ray);
		}
	}

	// Use this for initialization
	//void Start () {}
	
	// Update is called once per frame
	//void Update () {}
}
