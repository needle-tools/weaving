using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Fody;
using Mono.Cecil;
using needle.Weaver;
using UnityEngine;
using UnityEngine.XR;

namespace Patch.InputDevices.Editor
{
	public class WeaveNeedlePatches : BaseModuleWeaver
	{
		public override void Execute()
		{
			var patches = UnityEditor.TypeCache.GetMethodsWithAttribute<NeedlePatch>().ToList();
			if(patches.Count <= 0) return;
			ModuleDefinition.ForEachMethod(m =>
			{
				var patch = TryGetPatch(m, patches);
				if (patch == null) return;
				// Ensure the method has a body (if it's a external method)
				m.AddExternalMethodBody();
				// Apply patch
				m.Write(patch, true);
			});
			
			ModuleDefinition.ForEachProperty(p =>
			{
				if (p.Name == "deviceId")
				{
					var patch = TryGetPatch(p, patches);
					if (patch == null) return;
					
					if(patch.ReturnType.FullName == p.PropertyType.FullName)
					{
						p.GetMethod.Write(patch, true);
					}
				}
			});
		}

		public override IEnumerable<string> GetAssembliesForScanning()
		{
			yield break;
		}

		private MethodInfo TryGetPatch<T>(T entry, List<MethodInfo> patches) where T : IMemberDefinition
		{
			var methodFullName = entry.DeclaringType.FullName + "." + entry.Name;
			foreach (var patch in patches)
			{
				var nps = patch.GetCustomAttributes(typeof(NeedlePatch), true) as NeedlePatch[];
				if(nps == null) continue;
				foreach (var np in nps)
				{
					if (np.FullName == methodFullName)
					{
						return patch;
					}

					if ((np.FullName + "." + patch.Name) == methodFullName)
					{
						return patch;
					}
				}
			}

			return null;
		}
	}
}