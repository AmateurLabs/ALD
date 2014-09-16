using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AmateurLabs.ALD {
	public class Mesh {
		public string Name;
		public Vector[] Vertices;
		public Vector[] Normals;
		public Color[] Colors;
		public uint[] Triangles;

		public static Mesh Load(string path) {
			Mesh mesh = new Mesh();
			mesh.Name = System.IO.Path.GetFileNameWithoutExtension(path);

			Vector[] vertices = new Vector[0];
			Color[] colors = new Color[0];
			uint[] triangles = new uint[0];

			uint v = 0; //The Vertices index
			uint c = 0; //The Colors index
			uint t = 0; //The Triangles index
			uint j = 0; //The current index to offset triangles by

			string[] lines = System.IO.File.ReadAllLines(path);
			int vertCount = 0;
			int triCount = 0;
			foreach (string line in lines) {
				if (line.StartsWith("v")) {
					vertCount++;
				}
				if (line.StartsWith("t")) {
					triCount += 3;
				}
			}
			vertices = new Vector[vertCount];
			colors = new Color[vertCount];
			triangles = new uint[triCount];
			for (int i = 0; i < lines.Length; i++) {
				string line = lines[i];
				if (line == "") continue;
				string[] bits = line.Split(" ".ToCharArray());
				if (bits[0] == "v") {
					vertices[v] = new Vector(float.Parse(bits[1]), float.Parse(bits[2]), float.Parse(bits[3]));
					v++;
				} else if (bits[0] == "c") {
					colors[c] = new Color(byte.Parse(bits[1]), byte.Parse(bits[2]), byte.Parse(bits[3]), ((bits.Length > 4) ? byte.Parse(bits[4]) : (byte)255));
					c++;
				} else if (bits[0] == "t") {
					triangles[t + 0] = uint.Parse(bits[1]) + j;
					triangles[t + 1] = uint.Parse(bits[2]) + j;
					triangles[t + 2] = uint.Parse(bits[3]) + j;
					t += 3;
				} else if (bits[0] == "#") {
					j = v;
				}
			}
			mesh.Vertices = vertices;
			mesh.Colors = colors;
			mesh.Triangles = triangles;
			mesh.CalculateNormals();
			return mesh;
		}

		public static void Save(string path, Mesh mesh) {
			Vector[] vertices = mesh.Vertices;
			Color[] colors = mesh.Colors;
			uint[] triangles = mesh.Triangles;
			int vcount = vertices.Length;
			int tcount = triangles.Length;
			List<string> lines = new List<string>(vcount + tcount);
			for (int i = 0; i < vertices.Length; i++) {
				Vector vert = vertices[i];
				Color col = colors[i];
				lines.Add("v " + vert.X + " " + vert.Y + " " + vert.Z);
				lines.Add("c " + col.R + " " + col.G + " " + col.B + ((col.A != 255) ? (" " + col.A) : ""));
			}
			for (int i = 0; i < triangles.Length; i += 3) {
				lines.Add("t " + triangles[i + 0] + " " + triangles[i + 1] + " " + triangles[i + 2]);
			}
			System.IO.File.WriteAllLines(path, lines.ToArray());
		}

		public void CalculateNormals() {
			List<uint> indicies = new List<uint>(Triangles);
			List<Vector> vertices = new List<Vector>(Vertices);
			List<Vector> normals = new List<Vector>(Vertices);
			for (int i = 0; i < indicies.Count; i += 3) {
				Vector v0 = vertices[(int)indicies[i]];
				Vector v1 = vertices[(int)indicies[i + 1]];
				Vector v2 = vertices[(int)indicies[i + 2]];

				Vector normal = Vector.Normalize(Vector.Cross(v2 - v0, v1 - v0));

				normals[(int)indicies[i]] += normal;
				normals[(int)indicies[i + 1]] += normal;
				normals[(int)indicies[i + 2]] += normal;
			}
			for (int i = 0; i < Vertices.Length; i++) {
				normals[i] = Vector.Normalize(normals[i]);
			}
			Normals = normals.ToArray();
		}
	}
}
