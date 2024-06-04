using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if USE_FIREBASE
using Firebase;
using Firebase.Database;
using Firebase.Unity;

#if UNITY_EDITOR
using Firebase.Unity.Editor;
#endif
#endif

using System.Linq;


public class ChiVRtoFireBaseBridge : MonoBehaviour
{

    public static string EvnFirebaseRoot = "https://chivr-b0736.firebaseio.com/";
    public TextMesh StatusMesh;
    public VoiceOverEventManager VOManager;
    public string HomeId = "";
    public bool HomeIsReady = false;

	#if USE_FIREBASE
	#else
	public bool FirebaseMacroDisabled = true;
	#endif

    #if USE_FIREBASE
    private string mStatus = "";
    public string Status
    {
        get { return mStatus; }
        set
        {
            if (mStatus != value)
            {
                mStatus = value;
                if (StatusMesh)
                {
                    StatusMesh.text = value;
                }
            }
        }
    }

    private bool mStarted = false;
    public void EnsureStarted()
    {
        if (mStarted)
            return;
        mStarted = false;

        if (!VOManager)
        {
            VOManager = GameObject.FindObjectOfType<VoiceOverEventManager>();
        }

        if (Application.platform == RuntimePlatform.Android)
        {
            this.Status = "local";
            return;
        }

        this.Status = "Connecting...";
#if USE_FIREBASE
		FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
			var dependencyStatus = task.Result;
			if (dependencyStatus == DependencyStatus.Available)
			{
				EchoLine ("Initializing...");
				InitializeFirebase();
			}
			else
			{
				Debug.LogError(
					"Could not resolve all Firebase dependencies: " + dependencyStatus);
			}
		});
#endif
    }

    public void EchoLine(string stmt)
    {
        Debug.Log("Firebase Log:" + stmt);
    }

    // Initialize the Firebase database:
    protected virtual void InitializeFirebase()
    {
#if USE_FIREBASE
		FirebaseApp app = FirebaseApp.DefaultInstance;
		// NOTE: You'll need to replace this url with your Firebase App's database
		// path in order for the database connection to work correctly in editor.
		app.Options.DatabaseUrl = new System.Uri( EvnFirebaseRoot );
#if UNITY_EDITOR
		app.SetEditorDatabaseUrl(EvnFirebaseRoot);
		if (app.Options.DatabaseUrl != null) app.SetEditorDatabaseUrl(app.Options.DatabaseUrl);
#endif
		StartListener();
#endif
	}

	public delegate void FirebaseCallback(DataSnapshot shot);

	public void GetPropertyUpdating(string path, FirebaseCallback callback) {
		try {
			FirebaseDatabase.DefaultInstance
				.GetReference(path)
				.ValueChanged += (object sender2, ValueChangedEventArgs e2) => {
				if (e2.DatabaseError != null)
				{
					Debug.LogError(e2.DatabaseError.Message);
					callback(null);
				} else {
					callback(e2.Snapshot);
				}
			};
		} catch(System.Exception ex) {
			Debug.Log ("Firebase: " + ex);
		}
	}

	public void SetProperty(string path, object val) {
		try {
			FirebaseDatabase.DefaultInstance
				.GetReference (path).SetValueAsync (val);
		}  catch(System.Exception ex) {
			Debug.Log ("Firebase: " + ex);
		}
	}

	public void GetListTopUpdating(string path, int count, FirebaseCallback callback) {
		try {
			FirebaseDatabase.DefaultInstance
				.GetReference(path).LimitToLast(count)
				.ValueChanged += (object sender2, ValueChangedEventArgs e2) => {
				if (e2.DatabaseError != null)
				{
					Debug.LogError(e2.DatabaseError.Message);
					callback(null);
				} else {
					callback(e2.Snapshot);
				}
			};
		}  catch(System.Exception ex) {
			Debug.Log ("Firebase: " + ex);
		}
	}

	public void PushListToGetKey(string path, object val, System.Action<string> gotKey) {
		try {
			var r = FirebaseDatabase.DefaultInstance
				.GetReference (path).Push ();
			r.SetValueAsync (val).ContinueWith(t => {
				if (gotKey != null) {
					gotKey(r.Key);
				}
			});
		}  catch(System.Exception ex) {
			Debug.Log ("Firebase: " + ex);
		}
	}

	public Dictionary<string,object> JOGetDict(object ob) {
		if (ob == null)
			return null;
		var ans = (ob as Dictionary<string,object>);
		if (ans == null) {
			var msg = "Not a dict<string,object>: " + ob.GetType ().FullName + " value:" + ob.ToString ();
			Debug.LogError (msg);
		}
		return ans;
	}

	public Dictionary<string,object> JOCreate(params object[] pairs) {
		var ans = new Dictionary<string, object> ();
		for (int i = 0; i < pairs.Length; i += 2) {
			ans.Add (pairs [i + 0].ToString (), pairs [i + 1]);
		}
		return ans;
	}

	public string StringOrDefault(string s, string def) {
		if (string.IsNullOrEmpty (s))
			return def;
		return s;
	}

	public string JOTime() {
		var tm = System.DateTime.Now;
		return tm.ToLongTimeString ();
	}

	public string JODisplayName() {
		var name = StringOrDefault (System.Environment.UserName, "User") + " on " + StringOrDefault (SystemInfo.deviceName, SystemInfo.graphicsDeviceName);

		return name;
	}

	protected void StartListener()
	{
		this.Status = "Connecting (p2)...";
		
		this.GetListTopUpdating ("/public/message_board/", 3, ((shot) => {
			if (shot != null) {
				var dict = JOGetDict(shot.Value);
				//EchoLine("Entries = " + dict.Keys.Aggregate((a,b)=>(a+"/"+b)));
				var val = dict.Values.Last();
				dict = JOGetDict(val);
				Status = ("Message: '" + dict["text"] + "' - " + dict["user"]);
				//EchoLine("ValueType=" + val.GetType().FullName + " = " + val.ToString());
			}
		}));

		var unityPrefKey = "chivr_firebase_homeid";
		if (string.IsNullOrEmpty(HomeId)) {
			if (PlayerPrefs.HasKey (unityPrefKey)) {
				HomeId = PlayerPrefs.GetString (unityPrefKey);
			}
		}
		if (string.IsNullOrEmpty (HomeId)) {
			Status = "Requesting network home...";
			var home = JOCreate (
					"info", JOCreate ("display_name", JODisplayName()),
		           "status", JOCreate ("summary", "Created", "time", JOTime ()));
			PushListToGetKey ("/public/homes/", home, (key) => {
				HomeId = key;
				PlayerPrefs.SetString (unityPrefKey, HomeId);
				Status = "Got a home: " + HomeId;
				StartWithHomeListener();
			});
		} else {
			StartWithHomeListener ();
		}
	}

	private void DoAudioCommand(string msg) {
		switch (msg) {
		case "audio_intro":
		case "audio_chant":
			{
				bool isIntro = (msg == "audio_intro");
				var found = this.VOManager.CurrentTrack; found = null;
				foreach (var vo in this.VOManager.AllTracks) {
					if (vo.IsPlayAtStart == isIntro) {
						found = vo;
						break;
					}
				}
				this.VOManager.ChangeTrack (found);
			}
			break;
		case "audio_music_on":
		case "audio_music_off":
			{
				bool isOn = (msg == "audio_music_on");
				this.VOManager.IsBackgroundMusicPlaying = isOn;
			}
			break;
		case "audio_stop":
			{
				this.VOManager.ChangeTrack (null);
				this.VOManager.IsBackgroundMusicPlaying = false;
			}
			break;
		}
	}

	private void DoCommand(string msg, object wholeCommand) {
		if (msg.StartsWith ("audio_")) {
			DoAudioCommand (msg);
		} else {
			Status = "Unknown command: '" + msg + "'";
		}
	}

	private void StartWithHomeListener() {
		HomeIsReady = true;
		
		// first let's say that we have a device:
		{
			var latest = JOCreate (
				             "home", HomeId,
				             "display_name", JODisplayName (),
				             "time", JOTime (),
				             "claimed", false);
			this.PushListToGetKey ("/public/latest/", latest, null);
		}

		var homeRoot = "/public/homes/" + HomeId;

		// Then let's update our summary:
		this.SetProperty(homeRoot + "/status", JOCreate("summary", "Running", "time", JOTime()));


		// now let's listen for messages and clear them:
		var cmdsHome = homeRoot + "/commands/";
		this.GetListTopUpdating (cmdsHome, 10, (cmds) => {
			var lst = JOGetDict( cmds.Value );
			if (lst != null) {
				foreach (var k in lst.Keys) {
					var cmdInfo = JOGetDict(lst[k]);
					var cmd = cmdInfo["command"].ToString();
					var isDone = (bool)(cmdInfo["isdone"]);
					if (!isDone) {
						cmdInfo["isdone"] = true;

						Status = "Got Command: " + cmd;

						DoCommand(cmd, cmdInfo);

						// mark it as done:
						SetProperty(cmdsHome + k + "/isdone", true );
					}
				}
			}
		});

		var meridiansHome = homeRoot + "/state/meridians/";
		this.GetPropertyUpdating (meridiansHome, (sn) => {
			var js = ((sn != null) ? sn.Value : null);
			if (js != null) {
				
				var mers = JOGetDict(js);
				foreach (var merName in mers.Keys) {
					var val = JOGetDict(mers[merName]);
					bool madeChange = false;
					ExcersizeAppState.main.EnsureSetup();
					var st = ExcersizeAppState.main.State;
					foreach (var m in st.Meridians) {
						if (m.Id.ToString().ToLower() == merName.ToLower()) {
							var cd = int.Parse( val["dir"].ToString() );
							if (m.Direction != cd) {
								m.Direction = cd;
								madeChange = true;
							}
						}
					}
					if (madeChange) {
						Status = "Updated meridians.";
						recieveMeridianChanges = false;
						st.DoMeridianChanged();
						recieveMeridianChanges = true;
					}
				}
			}
		});
		ExcersizeAppState.main.EnsureSetup ();
		ExcersizeAppState.main.State.OnMeridianChanged += (() => {
			if (!recieveMeridianChanges) return;
			var latest = JOCreate();
			foreach (var m in ExcersizeAppState.main.State.Meridians) {
				latest.Add(m.Id.ToString().ToLower(), JOCreate("dir", m.Direction));
			}
			SetProperty(meridiansHome, latest);
		});
	}
	private bool recieveMeridianChanges = true;

	// Use this for initialization
	void Start () {
		this.EnsureStarted ();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

#endif
}
