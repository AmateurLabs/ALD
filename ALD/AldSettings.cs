using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AmateurLabs.ALD {
	public static class AldSettings {
		public static char IndentCharacter = ' ';
		public static int IndentCount = 2;
		public static string LineSeperator = "\n";
		public static string KeyValueSeperator = "=";
		public static string KeyTypeSeperator = ":";
		public static string AutoArrayKey = "-";
		public static bool AutoSerializeStructs = true;
	}
}
