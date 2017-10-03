using UnityEngine;
using System.Collections;

public class ExcersizeActivityInst : MonoBehaviour {

	public ExcersizeSharedScheduler.ActivityAvatar Avatar = ExcersizeSharedScheduler.ActivityAvatar.Guide;
	public ExcersizeActivityBase Activity { get; set; }
	public BodyLandmarks Body { get; set; }

	public ExcersizeSharedScheduler Context { get { return this.Activity.Context; } }
	public ExcersizeBreathController Breath { get { return this.Context.Breath; } }
	public bool IsInfoAvatar { get { return (this.Avatar == ExcersizeSharedScheduler.ActivityAvatar.Info); } }
	public bool IsGuideAvatar { get { return (this.Avatar == ExcersizeSharedScheduler.ActivityAvatar.Guide); } }

	public virtual void EnsureSetup() {
		if (this.Activity == null) {
			this.Activity = this.GetComponentInParent<ExcersizeActivityBase> ();
			this.Activity.EnsureSetup ();
			this.Activity.RegisterInstance (this);
		}
	}

	public virtual void ApplyBodyPositioning() {
		this.EnsureSetup ();
		this.Body.EnsureBodyPositioning ().ResetPositioning ();
	}

	public virtual Vector3 CalcVectorField(DynamicFieldModel model, Vector3 pos, out Color primaryColor) {
		primaryColor = Color.white;
		return Vector3.zero;
	}

	// Use this for initialization
	void Start () {
		this.EnsureSetup();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
