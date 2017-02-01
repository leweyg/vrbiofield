using UnityEngine;
using System.Collections.Generic;

public class ExcersizeActivityBase : MonoBehaviour {

	public string ActivityName = "General Breath";
	public ExcersizeSharedScheduler Context { get; set; }
	private List<ExcersizeActivityInst> Instances = new List<ExcersizeActivityInst> ();

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
		this.Instances.Add (inst);
	}

	public virtual void ApplyState() {
		foreach (var i in this.Instances) {
			i.ApplyBodyPositioning ();
		}
	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
