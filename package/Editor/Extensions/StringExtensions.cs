using System.IO;

namespace needle.Weaver
{
	public static class StringExtensions
	{
		public static string Gray(this string str)
		{
			return str.Color("#888888");
		}

		private static string Color(this string str, string hex)
		{
			return str.ApplyPerLine("<color=" + hex + ">", "</color>");
		}

		private static string ApplyPerLine(this string str, string before, string after)
		{
			var lines = new StringReader(str);
			str = string.Empty;
			while (true)
			{
				var line = lines.ReadLine();
				if (line == null) break;
				line = before + line + after;
				str += line + "\n";
			}
			return str;
		}
	}
}