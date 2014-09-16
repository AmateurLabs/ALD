using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AmateurLabs.ALD {
	public struct Vector {
		public float X, Y, Z, W;

		public override string ToString() {
			return "("+X+","+Y+","+Z+","+W+")";
		}

		public float this[int index] {
			get {
				if (index == 0) return X;
				else if (index == 1) return Y;
				else if (index == 2) return Z;
				else if (index == 3) return W;
				else throw new IndexOutOfRangeException("Index: " + index);
			}
			set {
				if (index == 0) X = value;
				else if (index == 1) Y = value;
				else if (index == 2) Z = value;
				else if (index == 3) W = value;
				else throw new IndexOutOfRangeException("Index: " + index);
			}
		}

		public Vector(params float[] values) {
			X=Y=Z=W=0;
			if (values.Length > 0) X = values[0];
			if (values.Length > 1) Y = values[1];
			if (values.Length > 2) Z = values[2];
			if (values.Length > 3) W = values[3];
		}

		public static Vector operator +(Vector a, Vector b) {
			return new Vector(a.X + b.X, a.Y + b.Y, a.Z + b.Z, a.W + b.W);
		}

		public static Vector operator -(Vector a, Vector b) {
			return new Vector(a.X - b.X, a.Y - b.Y, a.Z - b.Z, a.W - b.W);
		}

		public static Vector operator *(Vector a, Vector b) {
			return new Vector(a.X * b.X, a.Y * b.Y, a.Z * b.Z, a.W * b.W);
		}

		public static Vector operator /(Vector a, Vector b) {
			return new Vector(a.X / b.X, a.Y / b.Y, a.Z / b.Z, a.W / b.W);
		}

		public static Vector Cross(Vector v1, Vector v2) {
			Vector result = new Vector();
			result.X = (v1.Y * v2.Z) - (v1.Z * v2.Y);
			result.Y = (v1.Z * v2.X) - (v1.X * v2.Z);
			result.Z = (v1.X * v2.Y) - (v1.Y * v2.X);
			return result;
		}

		public static Vector Normalize(Vector v1) {
			Vector result = new Vector(v1.X, v1.Y, v1.Z, v1.W);
			float length = result.X * result.X + result.Y * result.Y + result.Z * result.Z;
			length = (float)Math.Sqrt(length);
			result.X /= length; result.Y /= length; result.Z /= length;
			return result;
		}
	}
}
