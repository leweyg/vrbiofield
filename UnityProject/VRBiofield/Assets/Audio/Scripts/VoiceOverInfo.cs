using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoiceOverInfo : MonoBehaviour {

	public string DisplayName = "Title";
	public AudioClip Clip;
	public TextAsset JsonEventData;
	public VOEventObj EventData = null;
	public bool IsPlayAtStart = false;
	public bool IsLoopable = false;
	public ExcersizeActivityBase RequiredExcersize = null;

	private bool mIsSetup = false;
	public void EnsureSetup() {
		if (mIsSetup)
			return;
		mIsSetup = true;

		var text = JsonEventData.text;
		EventData = JsonUtility.FromJson<VOEventObj> (text);

		//Debug.Log ("File=" + EventData.file);
		//Debug.Log ("Loaded " + EventData.events.Length + " events.");
	}


	[System.Serializable]
	public class VOEvent {
		public string name;
		public float time;

		public bool IsIn() {
			return (name == "in");
		}
	}

	[System.Serializable]
	public class VOEventObj {
		public string file;
		public VOEvent[] events;
	}


	// Use this for initialization
	void Start () {
		this.EnsureSetup ();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
