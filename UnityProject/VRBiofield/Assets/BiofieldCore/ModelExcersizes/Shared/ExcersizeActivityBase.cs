using UnityEngine;
using System.Collections;

public class ExcersizeActivityBase : MonoBehaviour {

	public string ActivityName = "General Breath";
	public ExcersizeSharedScheduler Context { get; set; }

	public virtual void EnsureSetup() {
		if (this.Context == null) {
			this.Context = this.GetComponentInParent<ExcersizeSharedScheduler> ();
			this.Context.EnsureSetup ();
		}
	}

	public virtual void RegisterInstance(ExcersizeActivityInst inst) {
		this.EnsureSetup ();
		inst.Body = this.Context.GetBody (inst.Avatar);
		inst.Body.EnsureSetup ();
	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
