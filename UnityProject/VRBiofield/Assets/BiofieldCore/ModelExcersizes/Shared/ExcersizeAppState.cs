using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExcersizeAppState : MonoBehaviour {

	public ExcersizeSharedScheduler Shared = null;
	public AppState State { get; set; }
	public VoiceOverEventManager AudioManager = null;


	public class AppState {

		public List<MeridianState> Meridians = new List<MeridianState>();
		public event AppStateChangedEvent OnMeridianChanged;
		public MeridianPath.EMeridian HoverMeridian = MeridianPath.EMeridian.Unknown;



		public delegate void AppStateChangedEvent ();

		public void DoMeridianChanged() {
			if (OnMeridianChanged!=null) {
				OnMeridianChanged ();
			}
		}

		public MeridianState GetMeridianState(MeridianPath.EMeridian id) {
			foreach (var ms in this.Meridians) {
				if (ms.Id == id)
					return ms;
			}
			Debug.LogError ("Unknown meridian: " + id);
			return null;
		}

		public AppState() {
			foreach (var e in System.Enum.GetValues(typeof( MeridianPath.EMeridian)) ) {
				var id = (MeridianPath.EMeridian)e;
				Meridians.Add(new MeridianState(){Id=id});
			}
			this.GetMeridianState(MeridianPath.EMeridian.Heart).Direction = 1;
			this.GetMeridianState(MeridianPath.EMeridian.Kidney).Direction = -1;
		}
	}

	public class MeridianState {
		public MeridianPath.EMeridian Id;
		public int Direction = 0;
		public bool IsInfoHover = false;
	}

	private static ExcersizeAppState mMain = null;
	public static ExcersizeAppState main {
		get {
			if (mMain)
				return mMain;
			mMain = GameObject.FindObjectOfType<ExcersizeAppState> ();
			mMain.EnsureSetup ();
			Debug.Assert (mMain);
			return mMain;
		}
	}

	private bool mIsSetup = false;
	public void EnsureSetup() {
		if (mIsSetup)
			return;
		mIsSetup = true;
		mMain = this;

		State = new AppState ();

		if (!this.Shared) {
			this.Shared = GameObject.FindObjectOfType<ExcersizeSharedScheduler> ();
		}
		this.AudioManager = GameObject.FindObjectOfType<VoiceOverEventManager> ();
	}

	// Use this for initialization
	void Start () {
		this.EnsureSetup ();
	}

	// Update is called once per frame
	void Update () {

	}


}
