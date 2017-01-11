using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LinesThroughPoints {

	public Vector3[] Points;
	public float[] LengthBefore;
	public float FullLength;

	public int Count {get { return this.Points.Length; } }

	public LinesThroughPoints(Vector3[] pnts) {
		this.Points = pnts;
		this.LengthBefore = new float[this.Points.Length+1];

		var runningLength = 0.0f;
		this.LengthBefore [0] = 0.0f;
		for (int i = 1; i < this.Count; i++) {
			var len = (pnts [i + 0] - pnts [i - 1]).magnitude;
			runningLength += len;
			this.LengthBefore [i] = runningLength;
		}
		this.LengthBefore [this.Count] = runningLength;
		this.FullLength = runningLength;
	}

	public Vector3 SampleAtLength(float len) {
		int beforeNdx = 0;
		for (int i = 1; i < this.Count; i++) {
			if (len <= this.LengthBefore [i]) {
				return Vector3.Lerp (
					this.Points [i - 1],
					this.Points [i],
					(len - this.LengthBefore [i - 1]) / (this.LengthBefore [i] - this.LengthBefore [i - 1])
				);
			}
		}
		return this.Points [this.Count - 1];
	}

	public Vector3 SampleAtUnitLength(float unitLen) {
		return this.SampleAtLength (unitLen * this.FullLength);
	}
}
