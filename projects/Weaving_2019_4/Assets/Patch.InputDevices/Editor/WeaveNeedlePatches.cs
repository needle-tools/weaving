using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Fody;
using Mono.Cecil;
using needle.Weaver;
using UnityEngine;
using UnityEngine.XR;
using TypeCache = UnityEditor.TypeCache;

namespace Patch.InputDevices.Editor
{
	public class WeaveNeedlePatches : BaseModuleWeaver
	{
		public override void Execute()
		{
			// patch methods
			var patches = TypeCache.GetMethodsWithAttribute<NeedlePatch>().ToList();
			ModuleDefinition.ForEachMethod(def =>
			{
				var patch = TryGetPatch(def, patches);
				if (patch == null) return;
				// Ensure the method has a body (if it's a external method)
				def.AddExternalMethodBody();
				// Apply patch
				if(!def.Write(patch, true)) 
					Debug.LogWarning("Failed patching " + def);
			});
			
			// patch properties
			var propertyPatches = TypeCache.GetTypesWithAttribute<NeedlePatch>()
				.Where(t => t.IsClass)
				.SelectMany(TypeHelper.GetPropertiesWithAttribute<NeedlePatch>)
				.ToList();
			ModuleDefinition.ForEachProperty(def =>
			{
				var patch = TryGetPatch(def, propertyPatches);
				if (patch == null) return;
				// make sure property return type matches patch return type
				if(patch.PropertyType.FullName == def.PropertyType.FullName)
				{
					if(!def.GetMethod.Write(patch.GetMethod, true))
						Debug.LogWarning("Failed patching " + def);
				}
			});
		}

		public override IEnumerable<string> GetAssembliesForScanning()
		{
			yield break;
		}

		private TResult TryGetPatch<TEntry, TResult>(TEntry entry, IEnumerable<TResult> patches) where TEntry : IMemberDefinition where TResult : MemberInfo
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