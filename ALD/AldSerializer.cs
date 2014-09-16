using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections;

namespace AmateurLabs.ALD {
	public static class AldSerializer {

		private static Type serializeAttributeType = typeof(AldSerializeAttribute);
		private static Type ignoreAttributeType = typeof(AldIgnoreAttribute);
		private static Type expandAttributeType = typeof(AldExpandAttribute);

		public static AldNode Serialize(object obj) {
			return Serialize(string.Empty, obj);
		}

		public static AldNode Serialize(string key, object obj) {
			AldNode data = new AldNode(key, string.Empty);
			Type type = obj.GetType();
			if (!type.IsValueType && !type.IsSealed) data.Type = TypeToName(type);
			PropertyInfo[] props = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			foreach (PropertyInfo prop in props) {
				if (!prop.IsDefined(serializeAttributeType, true)) continue;
				if (prop.IsDefined(ignoreAttributeType, true)) continue;
				object value = prop.GetValue(obj, null);
				data.Add(SerializeField(prop.Name, value));
			}
			FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			foreach (FieldInfo field in fields) {
				if (!field.IsPublic && !field.IsDefined(serializeAttributeType, true)) continue;
				if (field.IsDefined(ignoreAttributeType, true)) continue;
				object value = field.GetValue(obj);
				data.Add(SerializeField(field.Name, value));
			}
			return data;
		}

		public static AldNode SerializeField(string key, object obj) {
			AldNode data = new AldNode(key, string.Empty);
			if (obj == null) return data;
			Type type = obj.GetType();
			if (!type.IsValueType && !type.IsSealed) data.Type = TypeToName(type);
			bool expand = type.IsDefined(expandAttributeType, true);
			if (obj is IDictionary) {
				IDictionary dict = obj as IDictionary;
				Hashtable hashtable = new Hashtable(dict);
				foreach (DictionaryEntry pair in hashtable) {
					data.Add(SerializeField(pair.Key.ToString(), pair.Value));
				}
			}else if (obj is IList) {
				IList list = obj as IList;
				for (int i = 0; i < list.Count; i++) {
					data.Add(SerializeField(i.ToString(), list[i]));
				}
			} else if (obj is DateTime) {
				data.Value = obj.ToString();
			} else if (((AldSettings.AutoSerializeStructs || expand) && type.IsValueType && !type.IsPrimitive && !type.IsEnum) || (type.IsClass && expand)) {
				data = Serialize(key, obj);
			} else {
				data.Value = obj.ToString();
			}
			return data;
		}

		public static T Deserialize<T>(AldNode data) {
			return (T)Deserialize(typeof(T), data);
		}

		public static object Deserialize(Type type, AldNode data) {
			object obj = Activator.CreateInstance(type);
			PropertyInfo[] props = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			foreach (PropertyInfo prop in props) {
				if (!prop.IsDefined(serializeAttributeType, true)) continue;
				if (prop.IsDefined(ignoreAttributeType, true)) continue;
				if (data.ContainsKey(prop.Name)) {
					AldNode propData = data[prop.Name];
					Type pType = prop.PropertyType;
					prop.SetValue(obj, DeserializeField(pType, propData), null);
				}
			}
			FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			foreach (FieldInfo field in fields) {
				if (!field.IsPublic && !field.IsDefined(serializeAttributeType, true)) continue;
				if (field.IsDefined(ignoreAttributeType, true)) continue;
				if (data.ContainsKey(field.Name)) {
					AldNode fieldData = data[field.Name];
					Type fType = field.FieldType;
					field.SetValue(obj, DeserializeField(fType, fieldData));
				}
			}
			return obj;
		}

		public static object DeserializeField(Type type, AldNode data) {
			object obj = null;
			if (data.Type != string.Empty) type = NameToType(data.Type);
			bool expand = type.IsDefined(expandAttributeType, true);
			if (IsGenericList(type)) {
				Type genericArg = (type.GetGenericArguments().Length == 0) ? type.GetElementType() : type.GetGenericArguments()[0];
				IList list = (IList)Activator.CreateInstance(type, new object[] { data.ArrayLength });
				for (int i = 0; i < data.ArrayLength; i++) {
					if (list.IsFixedSize) list[i] = DeserializeField(genericArg, data[i]);
					else list.Insert(i, DeserializeField(genericArg, data[i]));
				}
				obj = list;
			} else if (IsGenericDictionary(type)) {
				Type genericArg0 = type.GetGenericArguments()[0];
				Type genericArg1 = type.GetGenericArguments()[1];
				IDictionary dict = (IDictionary)Activator.CreateInstance(type);
				foreach (AldNode node in data) {
					dict[((IConvertible)node.Key).ToType(genericArg0, null)] = DeserializeField(genericArg1, node);
				}
				obj = dict;
			} else if (!type.IsValueType && data.Value == string.Empty && data.ChildCount == 0) {
				obj = null;
			} else if (((AldSettings.AutoSerializeStructs || expand) && type.IsValueType && !type.IsPrimitive && !type.IsEnum) || (type.IsClass && expand)) {
				obj = Deserialize(type, data);
			} else if (type.IsEnum) {
				obj = Enum.Parse(type, data.Value);
			} else {
				obj = Convert.ChangeType(data.Value, type);
			}
			return obj;
		}

		private static bool IsGenericList(Type type) {
			foreach (Type i in type.GetInterfaces()) {
				if (i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IList<>)) return true;
			}
			return false;
		}

		private static bool IsGenericDictionary(Type type) {
			foreach (Type i in type.GetInterfaces()) {
				if (i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDictionary<,>)) return true;
			}
			return false;
		}

		private static string TypeToName(Type type) {
			if (type.IsGenericType && !type.IsGenericTypeDefinition) {
				string name = TypeToName(type.GetGenericTypeDefinition());
				name = name.Substring(0, name.IndexOf('`')) + "<";
				foreach (Type genParam in type.GetGenericArguments()) {
					name += TypeToName(genParam) + ",";
				}
				name = name.Substring(0, name.Length - 1) + ">";
				return name;
			}
			if (TypeNameDict.ContainsKey(type)) return TypeNameDict[type];
			return type.FullName;
		}

		private static Dictionary<Type, string> TypeNameDict = new Dictionary<Type,string>
		{
			{ typeof(List<>), "List`1" },
			{ typeof(Dictionary<,>), "Dict`2" },
		};
		
		private static Type NameToType(string name) {
			Type type = null;
			if (name.EndsWith(">")) {
				int off = name.IndexOf('<');
				string typeDefName = name.Substring(0, off);
				string[] genParamNames = name.Substring(off + 1, name.Length - 2 - off).Split(',');
				typeDefName += "`" + genParamNames.Length;
				type = NameToType(typeDefName);
				Type[] genParams = new Type[genParamNames.Length];
				for (int i = 0; i < genParamNames.Length; i++) {
					genParams[i] = NameToType(genParamNames[i]);
				}
				type = type.MakeGenericType(genParams);
				return type;
			}
			if (TypeNameDict.ContainsValue(name)) {
				return TypeNameDict.Where((pair) => { return pair.Value == name; }).First().Key;
			}
			type = Type.GetType(name);
			if (type != null) return type;
			foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies()) {
				type = asm.GetType(name);
				if (type != null) return type;
			}
			throw new ArgumentException("Couldn't find Type " + name + " in any loaded assembly");
		}
	}
}
