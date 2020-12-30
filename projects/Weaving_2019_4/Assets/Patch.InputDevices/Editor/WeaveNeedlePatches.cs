using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Fody;
using HarmonyLib;
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
			var marked = TypeCache.GetTypesWithAttribute<NeedlePatch>();
			var failed = "";
			var cnt = 0;

			var constructors = Resolve<ConstructorInfo>(marked);
			ModuleDefinition.ForEachMethod(def =>
			{
				if (!def.IsConstructor) return;
				var patch = TryFindPatchMember(def, constructors);
				if (patch == null) return;
				// Ensure the method has a body (if it's a external method)
				def.AddExternalMethodBody();
				// Apply patch
				if (!def.Write(patch, true)) failed += (cnt++) + "Failed patching " + def + "\n";
			});
			if(cnt > 0)
				Debug.LogWarning("Could not patch " + cnt + " methods\n" + failed);
			
			var methods = Resolve<MethodInfo>(marked);
			ModuleDefinition.ForEachMethod(def =>
			{
				var patch = TryFindPatchMember(def, methods);
				if (patch == null) return;
				// Ensure the method has a body (if it's a external method)
				def.AddExternalMethodBody();
				// Apply patch
				if (!def.Write(patch, true)) failed += (cnt++) + "Failed patching " + def + "\n";
			});
			if(cnt > 0)
				Debug.LogWarning("Could not patch " + cnt + " methods\n" + failed);
			
			// patch properties
			var properties = Resolve<PropertyInfo>(marked);
			ModuleDefinition.ForEachProperty(def =>
			{
				var patch = TryFindPatchMember(def, properties);
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

		/// <summary>
		/// get type names we want to patch - e.g. attribute [NeedlePatch(MyType)] on class: will return dict with { key:MyType, values:members of type T }
		/// </summary>
		private static Dictionary<NeedlePatch, List<T>> Resolve<T>(TypeCache.TypeCollection typesWithPatchAttribute) 
			where T : MemberInfo
		{
			var res = new Dictionary<NeedlePatch, List<T>>();
			foreach (var type in typesWithPatchAttribute)
			{
				var methods = TypeAccessHelper.GetMembers<T>(type, false);
				// loop through attributes on class
				if (type.GetCustomAttributes(typeof(NeedlePatch), true) is NeedlePatch[] nps)
				{
					foreach (var np in nps)
					{
						// if any attribute has a full type name add all its members of typ T with that full name as the base class name to patch
						if (string.IsNullOrEmpty(np.FullName)) continue;
						foreach (var method in methods)
						{
							// constructors need a own NeedlePatch attribute
							if (method is ConstructorInfo && method.GetCustomAttribute<NeedlePatch>() == null)
								continue;
							
							// add member
							if(!res.ContainsKey(np))
								res.Add(np, new List<T>());
							res[np].Add(method);
						}
					}
				}
			}

			return res;
		}
		
		private static TMember TryFindPatchMember<TTarget, TMember>(TTarget target, Dictionary<NeedlePatch, List<TMember>> dict) 
			where TTarget : IMemberDefinition 
			where TMember : MemberInfo
		{
			var entryFullName = target.DeclaringType.FullName + "." + target.Name;
			foreach (var kvp in dict)
			{
				// we baseName is the fullname of the class for the type we want to patch
				var patch = kvp.Key;
				var members = kvp.Value;
				
				foreach (var member in members)
				{
					// members can still override their parents target when assigned with another NeedlePatch attribute
					if (member.GetCustomAttributes(typeof(NeedlePatch), true) is NeedlePatch[] nps)
					{
						foreach (var np in nps)
						{
							// var fn = member.ReflectedType;
							// np.ResolveFullNameFromParentIfNull(fn, member.Name);

							if (string.IsNullOrEmpty(np.FullName)) continue;

							if (np.FullName == entryFullName)
							{
								return member;
							}

							if (np.FullName + "." + member.Name == entryFullName)
							{
								return member;
							}
						}
					}
					
					var name = patch.FullName + "." + member.Name;
					if (name == entryFullName)
					{
						return member;
					}

				}
			}

			// Debug.LogWarning("Did not find patch for " + entry);
			return null;
		}
	
		
	}
}