using System;
using System.Linq;
using System.Collections;
using System.Reflection;

namespace AmateurLabs.ALD {
	public struct AldValue : IConvertible {
		private string _value;

		private AldValue(string val) {
			_value = val;
		}

		public override string ToString() {
			return _value;
		}

		public static implicit operator string(AldValue v) {
			return v._value;
		}

		public static implicit operator AldValue(string v) {
			return new AldValue(v);
		}

		public TypeCode GetTypeCode() {
			return TypeCode.Object;
		}

		public bool ToBoolean(IFormatProvider provider) {
			return bool.Parse(_value);
		}

		public static explicit operator bool(AldValue v) { return v.ToBoolean(null); }

		public byte ToByte(IFormatProvider provider) {
			return byte.Parse(_value);
		}

		public static explicit operator byte(AldValue v) { return v.ToByte(null); }

		public char ToChar(IFormatProvider provider) {
			return char.Parse(_value);
		}

		public static explicit operator char(AldValue v) { return v.ToChar(null); }

		public DateTime ToDateTime(IFormatProvider provider) {
			return DateTime.Parse(_value);
		}

		public static explicit operator DateTime(AldValue v) { return v.ToDateTime(null); }

		public decimal ToDecimal(IFormatProvider provider) {
			return decimal.Parse(_value);
		}

		public static explicit operator decimal(AldValue v) { return v.ToDecimal(null); }

		public double ToDouble(IFormatProvider provider) {
			return double.Parse(_value);
		}

		public static explicit operator double(AldValue v) { return v.ToDouble(null); }

		public short ToInt16(IFormatProvider provider) {
			return short.Parse(_value);
		}

		public static explicit operator short(AldValue v) { return v.ToInt16(null); }

		public int ToInt32(IFormatProvider provider) {
			return int.Parse(_value);
		}

		public static explicit operator int(AldValue v) { return v.ToInt32(null); }

		public long ToInt64(IFormatProvider provider) {
			return long.Parse(_value);
		}

		public static explicit operator long(AldValue v) { return v.ToInt64(null); }

		public sbyte ToSByte(IFormatProvider provider) {
			return sbyte.Parse(_value);
		}

		public static explicit operator sbyte(AldValue v) { return v.ToSByte(null); }

		public float ToSingle(IFormatProvider provider) {
			return float.Parse(_value);
		}

		public static explicit operator float(AldValue v) { return v.ToSingle(null); }

		public string ToString(IFormatProvider provider) {
			return _value;
		}

		public object ToType(Type conversionType, IFormatProvider provider) {
			try {
				return ((IConvertible)_value).ToType(conversionType, provider);
			} catch (InvalidCastException e) {
				//Type thisType = typeof(AldValue);
				//MethodInfo method = thisType.GetMethod("Cast");
				//method = method.MakeGenericMethod(conversionType);
				object obj = Cast(_value, conversionType);
				if (obj == null) throw e;
				return obj;
			}
		}

		/*public static T Cast<T>(object obj) {
			Type t = obj.GetType();
			MethodInfo method = Array.Find(typeof(T).GetMethods(BindingFlags.Static | BindingFlags.Public), delegate(MethodInfo inf) { return ((inf.Name == "op_Implicit") && (inf.ReturnType == typeof(T)) && (inf.GetParameters()[0].GetType() == t)); });
			//if (method == null) method = GetMethod(typeof(T), "op_Explicit", t, BindingFlags.Static | BindingFlags.Public);
			if (method == null) return (T)obj;
			return (T)method.Invoke(null, new object[] { obj });
		}*/

		public static object Cast(object obj, Type targetType) {
			if (targetType.IsAssignableFrom(obj.GetType()))
				return obj;
			BindingFlags pubStatBinding =
			BindingFlags.Public | BindingFlags.Static;
			Type originType = obj.GetType();
			String[] names = { "op_Implicit", "op_Explicit" };

			MethodInfo castMethod =
			targetType.GetMethods(pubStatBinding)
			.Union(originType.GetMethods(pubStatBinding))
			.FirstOrDefault(
			itm =>
			itm.ReturnType.Equals(targetType)
			&&
			itm.GetParameters().Length == 1
			&&
			itm.GetParameters()[0].ParameterType.IsAssignableFrom(originType)
			&&
			names.Contains(itm.Name)
			);
			if (null != castMethod)
				return castMethod.Invoke(null, new object[] { obj });
			else
				throw new InvalidOperationException(
				String.Format("No matching cast operator found from {0} to {1}.", originType.Name, targetType.Name));
		}

		public ushort ToUInt16(IFormatProvider provider) {
			return ushort.Parse(_value);
		}

		public uint ToUInt32(IFormatProvider provider) {
			return uint.Parse(_value);
		}

		public ulong ToUInt64(IFormatProvider provider) {
			return ulong.Parse(_value);
		}
	}
}