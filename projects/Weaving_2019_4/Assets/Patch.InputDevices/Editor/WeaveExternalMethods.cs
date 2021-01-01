using System;
using System.Collections.Generic;
using needle.Weaver;
using UnityEngine;

namespace Fody.Weavers.InputDeviceWeaver
{
	public class WeaveExternalMethods : BaseModuleWeaver, IWithPriority
	{
		public int Priority => 100;
		
		public override void Execute()
		{
			int count = 0;
			var patchedMethods = "";
			foreach(var type in ModuleDefinition.Types)
			{
			    foreach (var method in type.Methods)
			    {
				    try
				    {
					    if (method.AddExternalMethodBody())
					    {
						    patchedMethods += method.FullName + "\n";
						    ++count;
					    }
				    }
				    catch (Exception e)
				    {
					    Debug.LogException(e);
				    }
			    }
			}
			Debug.Log("Patched " + count + " external methods in " + ModuleDefinition + ":\n" + patchedMethods);
		}

		public override IEnumerable<string> GetAssembliesForScanning()
		{
			yield break;
		}

	}
}