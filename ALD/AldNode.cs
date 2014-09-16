using System;
using System.Collections;
using System.Collections.Generic;

namespace AmateurLabs.ALD {
	public sealed class AldNode {
		public static AldNode Blank = new AldNode();
		private readonly Dictionary<string, AldNode> _NodeDict = new Dictionary<string, AldNode>();

		private string _Key = string.Empty;

		public string Key {
			get {
				return _Key;
			}
			set {
				AldNode parent = Parent;
				if (parent != null) Parent = null;
				_Key = value;
				if (parent != null) Parent = parent;
			}
		}

		private AldValue _Value = string.Empty;

		public AldValue Value {
			get {
				return _Value;
			}
			set {
				_Value = value;
			}
		}

		private AldNode _Parent = null;

		public AldNode Parent {
			get {
				return _Parent;
			}
			set {
				if (_Parent != null) _Parent._NodeDict.Remove(_Key);
				_AutoIndexed = false;
				_Parent = value;
				if (_Parent != null) {
					_Parent._NodeDict[_Key] = this;
					int index = 0;
					if (int.TryParse(_Key, out index) && (index == 0 || _Parent[index - 1] != null)) {
						_AutoIndexed = true;
						_Parent._ArrayLength = Math.Max(index + 1, _Parent._ArrayLength);
					}
				}
			}
		}

		private string _Type = string.Empty;

		public string Type {
			get { return _Type; }
			set { _Type = value; }
		}

		public int Depth {
			get {
				if (Parent != null) return Parent.Depth + 1;
				else return 0;
			}
		}

		public int ChildCount {
			get {
				return _NodeDict.Count;
			}
		}

		private bool _AutoIndexed;

		private int _ArrayLength = 0;

		public int ArrayLength {
			get {
				return _ArrayLength;
			}
		}

		public AldNode() {

		}

		public AldNode(string key) {
			Key = key;
		}

		public AldNode(string key, string val) {
			Key = key;
			Value = val;
		}

		public AldNode(string key, string type, string val) {
			Key = key;
			Value = val;
			Type = type;
		}

		public AldNode Add(AldNode node) {
			node.Parent = this;
			return node;
		}

		public AldNode Remove(AldNode node) {
			node.Parent = null;
			return node;
		}

		public bool Contains(AldNode node) {
			return _NodeDict.ContainsValue(node);
		}

		public bool ContainsKey(string key) {
			return _NodeDict.ContainsKey(key);
		}

		public AldNode this[int index] {
			get {
				return this[index.ToString()];
			}
			set {
				this[index.ToString()] = value;
			}
		}

		public AldNode this[string index] {
			get {
				if (!ContainsKey(index)) return null;
				return _NodeDict[index];
			}
			set {
				if (ContainsKey(index)) _NodeDict[index].Parent = null;
				if (value != null) {
					value.Key = index;
					value.Parent = this;
				}
			}
		}

		public IEnumerator<AldNode> GetEnumerator() {
			return _NodeDict.Values.GetEnumerator();
		}

		public override string ToString() {
			return "{" + Key + ((Value != string.Empty) ? AldSettings.KeyValueSeperator + Value : string.Empty) + "}";
		}

		public string Export() {
			string output = string.Empty;
			foreach (AldNode node in _NodeDict.Values) {
				output += new string(AldSettings.IndentCharacter, Depth * AldSettings.IndentCount);
				string key = (node._AutoIndexed) ? AldSettings.AutoArrayKey : node.Key;
				if (node.Type != string.Empty) key += AldSettings.KeyTypeSeperator + node.Type;
				if (node.Value == string.Empty) output += key;
				else output += key + AldSettings.KeyValueSeperator + node.Value;
				output += "\n";
				if (node.ChildCount > 0) output += node.Export();
			}
			return output;
		}

		public static AldNode Import(string input) {
			AldNode node = new AldNode();
			input = input.Replace(new string(AldSettings.IndentCharacter, AldSettings.IndentCount), string.Empty);
			if (input.Contains(AldSettings.KeyValueSeperator)) {
				string[] bits = input.Split(new string[] { AldSettings.KeyValueSeperator }, StringSplitOptions.None);
				node.Key = bits[0];
				node.Value = bits[1];
			} else {
				node.Key = input;
				node.Value = string.Empty;
			}
			if (node.Key.Contains(AldSettings.KeyTypeSeperator)) {
				string[] bits = node.Key.Split(new string[] { AldSettings.KeyTypeSeperator }, StringSplitOptions.None);
				node.Key = bits[0];
				node.Type = bits[1];
			}
			return node;
		}

		public static AldNode ParseFile(string path) {
			string input = System.IO.File.ReadAllText(path);
			return ParseString(input);
		}

		public static AldNode ParseString(string input) {
			AldNode root = new AldNode();
			input = input.Replace(AldSettings.LineSeperator, "\n");
			input = input.Replace("\r\n", "\n");
			string[] lines = input.Split('\n');
			root.ParseLines(lines, 0);
			return root;
		}

		public int ParseLines(string[] lines, int i) {
			AldNode n = null;
			int autoIndex = 0;
			while (i < lines.Length) {
				if (lines[i] == string.Empty) {
					i++;
					continue;
				}
				int lineDepth = StringDepth(lines[i]);
				if (lineDepth == Depth + 1) {
					n = Import(lines[i]);
					if (n.Key == AldSettings.AutoArrayKey) {
						n.Key = autoIndex.ToString();
						autoIndex++;
					}
					n.Parent = this;
				} else if (lineDepth > Depth + 1) {
					if (n != null)
						i = n.ParseLines(lines, i) - 1;
				} else {
					break;
				}
				i++;
			}
			_ArrayLength = Math.Max(_ArrayLength, autoIndex);
			return i;
		}

		private static int StringDepth(string line) {
			//if (!line.StartsWith("" + new String(AldSettings.IndentCharacter, AldSettings.IndentCount))) return 0;
			int depth = 0;
			while (line.StartsWith(new String(AldSettings.IndentCharacter, AldSettings.IndentCount * depth))) {
				depth++;
			}
			return depth;
		}
	}
}