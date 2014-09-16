using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AmateurLabs.ALD {
	public class Color {
		public byte R;
		public byte G;
		public byte B;
		public byte A;

		public Color(byte r, byte g, byte b) {
			R = r;
			G = g;
			B = b;
		}

		public Color(byte r, byte g, byte b, byte a)
			: this(r, g, b) {
			A = a;
		}
	}
}
