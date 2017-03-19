using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


public class VolumeHeader {
	public readonly Int3 Size;
	public readonly int TotalCount;

	public VolumeHeader(Int3 sz) {
		this.Size = sz;
		this.TotalCount = sz.X * sz.Y * sz.Z;
	}

	public int CubicToLinear(Int3 pos) {
		return (pos.X + (pos.Y * Size.X) + (pos.Z * this.Size.X * this.Size.Y));
	}

	public Int3 LinearToCubic(int ndx) {
		return new Int3 (
			((ndx / (1)) % this.Size.X),
			((ndx / (this.Size.X)) % this.Size.Y),
			((ndx / (this.Size.X * this.Size.Y)) % this.Size.Z));
	}

	public bool IsSafe(Int3 ndx) {
		return ((ndx.X >= 0) && (ndx.Y >= 0) && (ndx.Z >= 0)
			&& (ndx.X < Size.X) && (ndx.Y < Size.Y) && (ndx.Z < Size.Z));
	}

	public Vector3 CubicToDecimalUnit(Int3 ndx) {
		return new Vector3 (((float)ndx.X) / ((float)Size.X - 1), 
			((float)ndx.Y) / ((float)Size.Y - 1), ((float)ndx.Z) / ((float)Size.Z - 1));
	}

}

public class VolumeBuffer<T> {
	public VolumeHeader Header;
	public T[] Array { get; private set; }

	public VolumeBuffer(Cubic<int> sz) {
		this.Header = new VolumeHeader (new Int3(sz));
		int count = sz.X * sz.Y * sz.Z;
		this.Array = new T[count];
	}

	public Cubic<int> Size { get { return this.Header.Size.AsCubic(); } }

	public Cubic<int> UnprojectIndex(int ndx) {
		return new Cubic<int> (
			((ndx / (1)) % this.Size.X),
			((ndx / (this.Size.X)) % this.Size.Y),
			((ndx / (this.Size.X * this.Size.Y)) % this.Size.Z));
	}

	public IEnumerable<Int3> AllIndices3() {
		for (int i=0; i<this.Array.Length; i++) {
			var pnt = this.UnprojectIndex(i);
			yield return new Int3(pnt);
		}
	}

	public bool IsSafe(Int3 pos) {
		return this.Header.IsSafe (pos);
	}

	public T this[Int3 ndx] {
		get {
			if (this.Header.IsSafe(ndx)) 
				return this.Array[this.Header.CubicToLinear(ndx)];
			return default(T);
		}
		set {
			if (this.Header.IsSafe(ndx))
				this.Array[this.Header.CubicToLinear(ndx)] = value;
			else {
				//throw new IndexOutOfRangeException();
			}
		}
	}

	public bool IsSafe(Cubic<int> pos) {
		return pos.Select2 (this.Size, (i,sz) => ((i >= 0) && (i < sz))).AllTrue ();
	}

	public bool IsEdge(Cubic<int> pos) {
		return pos.Select2 (this.Size, (i,sz) => ((i == 0) || ((i + 1) == sz))).AnyTrue();
	}

	public T Read(Int3 pos) {
		if (this.IsSafe (pos)) {
			return this.Array [this.Header.CubicToLinear(pos)];
		} else {
			return default(T);
		}
	}

	public T Read(Cubic<int> pos) {
		return this.Read (new Int3 (pos));
	}

	public void Write(Int3 pos, T val) {
		if (this.IsSafe (pos)) {
			this.Array [this.Header.CubicToLinear(pos)] = val;
		}
	}

	public void Write(Cubic<int> pos, T val) {
		this.Write (new Int3 (pos), val);
	}

	public T SampleSpan(Cubic<SpanI> span, T defVal, Func<T,T,T> combiner)
	{
		bool hasFirst = false;
		T res = defVal;
		for (int iz = span.Z.From; iz <= span.Z.To; iz++)
		{
			for (int iy = span.Y.From; iy <= span.Y.To; iy++)
			{
				for (int ix = span.X.From; ix <= span.X.To; ix++)
				{
					Cubic<int> curNdx = new Cubic<int>(ix, iy, iz);
					if (this.IsSafe(curNdx))
					{
						T val = this.Read(curNdx);
						if (hasFirst)
						{
							res = combiner(res, val);
						}
						else
						{
							hasFirst = true;
							res = val;
						}
					}
				}
			}
		}
		return res;
	}

	public int Length
	{
		get { 
			return this.Array.Length; 
		}
	}

	public void ClearAll(T val)
	{
		for (int i = 0; i < this.Array.Length; i++)
		{
			this.Array[i] = val;
		}
	}

	public void ClearEdges(T val)
	{
		for (int i = 0; i < this.Array.Length; i++)
		{
			var ndx = this.UnprojectIndex(i);
			if (this.IsEdge(ndx))
			{
				this.Array[i] = val;
			}
		}
	}
}

public abstract class VolumeBufferUtil
{
	//	public static IEnumerable<Int3> AllIndices_pXpYpZ(VolumeHeader header) {
	//		for (int i=0; i<header.TotalCount; i++) {
	//			var pnt = header.LinearToCubic(i);
	//			yield return new Int3(pnt);
	//		}
	//
	//	}

	public static void ConvertOpacityToDistanceBuffer(VolumeBuffer<float> opac) 
	{
		//		VolumeBuffer<Int3> nearest = new VolumeBuffer<Int3> (opac.Size);
		//		Int3 prevPos = new Cubic<int> (0, 0, 0);
		//		float prevOpac = opac.Read (prevPos);
		//		foreach (var i in AllIndices_pXpYpZ(opac.Header)) {
		//			var curOpac = opac.Read(i);
		//			if (curOpac >= prevOpac) {
		//				prevPos = i;
		//				prevOpac = curOpac;
		//				nearest.Write(i,i);
		//			}
		//			else {
		//				nearest.Write(i, prevPos);
		//			} 
		//		}

	}
}


public class VolumeBufferMip<T> 
{
	public VolumeBuffer<T>[] Mips;

	public VolumeBufferMip(Int3 size) {
		int h = MipHeight (size.X);
		Mips = new VolumeBuffer<T>[h];
		var curSize = size;
		for (int i=0; i<h; i++) {
			Mips[i] = new VolumeBuffer<T>(curSize.AsCubic());
			curSize = curSize.ShiftDownOne();
		}
	}

	public void SpreadValuesUp(int mip, Func<T,T,T> comb) {
		if (mip + 1 >= this.Mips.Length)
			return;
		var big = this.Mips [mip];
		var sml = this.Mips [mip + 1];
		var h2 = new VolumeHeader (new Int3 (2, 2, 2));
		foreach (var si in sml.AllIndices3()) {
			var res = big.Read(si.AsCubic());
			for (int n=1; n<h2.TotalCount; n++) {
				var ni = si.Add(h2.LinearToCubic(n));
				var sec = big.Read(ni.AsCubic());
				res = comb(res,sec);
			}
			sml.Write(si.AsCubic(),res);
		}
	}

	public void SpreadValuesUp(Func<T,T,T> comb) {
		for (int i=1; i<Mips.Length; i++) {
			this.SpreadValuesUp(i, comb);
		}
	}

	public static int MipHeight(int sz) {
		int c = 0;
		while (sz > 0) {
			sz = (sz >> 1);
			c++;
		}
		return c;
	}


	public VolumeBuffer<T> GetMip(int i) {
		return this.Mips [i];
	}
}

public class VolumeBufferFile
{
	public static string CacheFilePath(string baseName, Cubic<int> sz)
	{
		baseName = baseName.Replace(" ", "_");
		string szName = "_" + sz.X + "_" + sz.Y + "_" + sz.Z;
		var path = UnityEngine.Application.dataPath + "/SavedVolumes/" + baseName + szName + ".Colors";
		return path;
	}

	public static bool CacheFileExists(string filepath)
	{
		return System.IO.File.Exists(filepath);
	}

	public static void SaveColorVolToFile(VolumeBuffer<Color> vol, string filename)
	{
		System.IO.BinaryWriter bw = new System.IO.BinaryWriter(new System.IO.FileStream(filename, System.IO.FileMode.Create));
		for (int i = 0; i < vol.Array.Length; i++)
		{
			var c = vol.Array[i];
			bw.Write(c.r);
			bw.Write(c.g);
			bw.Write(c.b);
			bw.Write(c.a);
		}
		bw.Close();
	}

	public static VolumeBuffer<Color> ReadVolFromFile(Cubic<int> size, string filename)
	{
		System.IO.BinaryReader br = new System.IO.BinaryReader(new System.IO.FileStream(filename, System.IO.FileMode.Open));
		VolumeBuffer<Color> ans = new VolumeBuffer<Color>(size);
		for (int i = 0; i < ans.Array.Length; i++)
		{
			Color c = new Color();
			c.r = br.ReadSingle();
			c.g = br.ReadSingle();
			c.b = br.ReadSingle();
			c.a = br.ReadSingle();
			ans.Array[i] = c;
		}
		br.Close();
		return ans;
	}

	public static void SaveFloatVolToFile(VolumeBuffer<float> vol, string filename)
	{
		System.IO.BinaryWriter bw = new System.IO.BinaryWriter(new System.IO.FileStream(filename, System.IO.FileMode.Create));
		for (int i = 0; i < vol.Array.Length; i++)
		{
			var c = vol.Array[i];
			bw.Write(c);
		}
		bw.Close();
	}

	public static VolumeBuffer<float> ReadFloatVolFromFile(Cubic<int> size, string filename)
	{
		System.IO.BinaryReader br = new System.IO.BinaryReader(new System.IO.FileStream(filename, System.IO.FileMode.Open));
		VolumeBuffer<float> ans = new VolumeBuffer<float>(size);
		for (int i = 0; i < ans.Array.Length; i++)
		{
			float c = br.ReadSingle();
			ans.Array[i] = c;
		}
		br.Close();
		return ans;
	}
}
