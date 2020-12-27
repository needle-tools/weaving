using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using UnityEngine;

namespace needle.Weaver
{
	public static class MethodDefinitionExtensions
	{
		public static void LogIL(this MethodDefinition method, string prefix)
		{
			prefix = !string.IsNullOrEmpty(prefix) ? ($"{prefix} {method?.Name}") : method?.Name;
			var inst = method?.Body?.Instructions;
			if (inst == null)
			{
				Debug.Log($"{prefix} has no body/instructions \nbody? {method?.Body != null}");
				return;
			}
			var msg = string.Join("\n", inst);
			Debug.Log($"{prefix} has {inst.Count} Instructions: \n{msg}");
		}
	}
}