using Mono.Cecil;
using UnityEngine;

namespace needle.Weaver
{
	public static class MethodDefinitionDebugExtensions
	{
		public static void LogIL(this MethodDefinition method, string prefix)
		{
			prefix = !string.IsNullOrEmpty(prefix) ? ($"{prefix} {method?.Name}") : method?.Name;
			var ilString = method.CaptureILString();
			Debug.Log($"{prefix}\n{ilString}");
		}

		public static string CaptureILString(this MethodDefinition method)
		{	
			var inst = method?.Body?.Instructions;
			var variables = method?.Body?.Variables;
			if (inst != null)
			{
				var msg = string.Empty;
				if (variables != null)
				{
					msg += "Variables [" + variables.Count + "]\n";
					msg += string.Join("\n", variables);
					msg += "\n";
				}
				msg += "Instructions [" + inst.Count + "]\n";
				msg += string.Join("\n", inst);
				return msg;
			}
			Debug.Log($"{method?.FullName} has no body/instructions \nbody? {method?.Body != null}");
			return string.Empty;
		}
	}
}