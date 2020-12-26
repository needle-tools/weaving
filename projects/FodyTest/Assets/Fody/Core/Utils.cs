using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using UnityEngine;

namespace Fody
{
	public static class Utils
	{
		public static void LogIL(this MethodDefinition method, string prefix)
		{
			var inst = method?.Body?.Instructions;
			if (inst == null) return;
			var msg = string.Join("\n", inst);
			Debug.Log($"{prefix} \n{method.Name} has {inst.Count} Instructions: \n{msg}");
		}
	}
}