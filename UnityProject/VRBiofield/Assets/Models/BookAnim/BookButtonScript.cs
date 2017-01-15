using UnityEngine;
using System.Collections;

public class BookButtonScript : MonoBehaviour {

	public bool IsTurnForward;
	private BookAnimScript Book;
	private Collider MyCollider;
	private bool IsHovering = false;
	private MeshRenderer MyRenderer;

	// Use this for initialization
	void Start () {
		Book = this.GetComponentInParent<BookAnimScript> ();
		this.MyCollider = this.GetComponent<Collider> ();
		this.MyRenderer = this.GetComponent<MeshRenderer> ();
		this.UpdateIsHovering (false, true);
	}

	bool EffecientRayCheck(Ray r) {
		//if (Vector3.Dot ((this.transform.position - r.origin).normalized, r.direction.normalized) > 0.8f) 
		{
			RaycastHit info;
			if (this.MyCollider.Raycast (r, out info, 10000.0f)) {
				return true;
			}
		}
		return false;
	}

	Color cachedColor = Color.yellow;
	private void UpdateIsHovering(bool isOver, bool fullUpdate=false) {
		
		if ((this.IsHovering != isOver) || (fullUpdate)) {
			this.IsHovering = isOver;
		}
		var clr = Book.CanChangePageInDirection(this.IsTurnForward) ? Book.ButtonColorActive : Color.black;
		clr = (this.IsHovering ? clr : Book.ButtonColorPassive);
		if (this.cachedColor != clr) {
			this.cachedColor = clr;
			this.MyRenderer.material.color = clr;
		}

		if (this.IsHovering) {
			Book.SetIsHovering (true, this.IsTurnForward);
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButtonUp (0)) {
			var ray = Camera.main.ScreenPointToRay (Input.mousePosition);
			if (EffecientRayCheck (ray)) {
				Book.ChangePageSimple (IsTurnForward, true);
			}
		}
		var cam = Camera.main.transform;
		var camRay = new Ray (cam.position, cam.forward);
		if (EffecientRayCheck (camRay)) {
			this.UpdateIsHovering (true);
		} else {
			this.UpdateIsHovering (false);
		}
	}
}
