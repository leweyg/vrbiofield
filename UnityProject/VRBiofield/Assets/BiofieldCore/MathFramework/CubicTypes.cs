using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Cubic<T>
{
	private T[] mArray;

	public Cubic() {
		mArray = new T[3];
	}

	public Cubic(T a, T b, T c) {
		mArray = new T[]{ a, b, c };
	}

	public static Cubic<T> Create(T a, T b, T c) {
		return new Cubic<T> (a, b, c);
	}

	public static Cubic<T> CreateSame(T a) {
		return new Cubic<T> (a, a, a);
	}

	public T X {
		get { return mArray [0]; }
		set { mArray [0] = value; }
	}

	public T Y {
		get { return mArray [1]; }
		set { mArray [1] = value; }
	}

	public T Z {
		get { return mArray [2]; }
		set { mArray [2] = value; }
	}

	public Cubic<X> Select<X>(Func<T,X> selector) {
		return new Cubic<X> (selector (this.X), selector (this.Y), selector (this.Z));
	}

	public Cubic<Y> Select2<X,Y>(Cubic<X> other, Func<T,X,Y> selector) {
		return new Cubic<Y> (
			selector (this.X, other.X),
			selector (this.Y, other.Y),
			selector (this.Z, other.Z));
	}

	public T Aggregate(Func<T,T,T> combiner) {
		return combiner (combiner (this.X, this.Y), this.Z);
	}

	public override string ToString()
	{
		return "{" + this.Select(k => k.ToString()).Aggregate((a, b) => a + "," + b) + "}";
	}
}


public struct Int3 {
	public int X, Y, Z;

	public Int3(int a, int b, int c) {
		X = a; Y = b; Z = c;
	}
	public Int3(Cubic<int> v) : this(v.X, v.Y, v.Z) { }

	public Int3 Select(Func<int,int> f) {
		return new Int3 (f (X), f (Y), f (Z));
	}

	public Int3 SelectWith(Int3 v, Func<int,int,int> f) {
		return new Int3 (f (X, v.X), f (Y, v.Y), f (Z, v.Z));
	}

	public Int3 Add(Int3 v) {
		return this.SelectWith (v, (a,b) => (a + b));
	}

	public bool Any(Func<int,bool> f) {
		return f (X) || f (Y) || f (Z);
	}

	public Int3 ShiftDownOne() {
		return this.Select (k => (k >> 1));
	}

	public bool AnyZero() {
		return this.Any(k => (k == 0));
	}

	public Cubic<int> AsCubic() {
		return new Cubic<int> (X, Y, Z);
	}

	public Vector3 AsVector3() {
		return new Vector3 (X, Y, Z);
	}
}
