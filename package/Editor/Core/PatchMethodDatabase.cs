using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using UnityEditor;
using UnityEngine;

namespace needle.Weaver
{
	public static class PatchMethodDatabase
	{
		public static IReadOnlyList<Patch> AllPatchedMethods() => _allPatchedMethods.Value;
		public static int MarkedTypesCount => _typesWithAttribute.Value.Count;
		public static TypeCache.TypeCollection MarkedTypes => _typesWithAttribute.Value;

		/// <summary>
		/// Get a member marked with NeedlePatch that matches the signature of target
		/// </summary>
		/// <param name="target">the method to find a patch for</param>
		/// <param name="res">contains the patch method and the attribute</param>
		/// <typeparam name="TInfo">the member type to patch</typeparam>
		public static bool TryGetPatch<TInfo>(IMemberDefinition target, out (TInfo patch, NeedlePatch attribute) res)
			where TInfo : MemberInfo
		{
			var marked = MarkedTypes;
			var available = GetPatches<TInfo>(marked);
			res = TryFindPatchMember(target, available);
			return res.patch != null;
		}


		private static readonly Lazy<TypeCache.TypeCollection> _typesWithAttribute =
			new Lazy<TypeCache.TypeCollection>(TypeCache.GetTypesWithAttribute<NeedlePatch>);

		private static readonly Lazy<List<Patch>> _allPatchedMethods = new Lazy<List<Patch>>(FindPatches);

		/// <summary>
		/// get type names we want to patch - e.g. attribute [NeedlePatch(MyType)] on class: will return dict with { key:MyType, values:members of type T }
		/// </summary>
		private static Dictionary<NeedlePatch, (Type source, List<T> members, bool used)> GetPatches<T>(TypeCache.TypeCollection typesWithPatchAttribute)
			where T : MemberInfo
		{
			var res = new Dictionary<NeedlePatch, (Type source, List<T> members, bool used)>();
			foreach (var type in typesWithPatchAttribute)
			{
				var members = TypeAccessHelper.GetMembers<T>(type, false);
				// loop through attributes on class
				if (type.GetCustomAttributes(typeof(NeedlePatch), true) is NeedlePatch[] nps)
				{
					foreach (var np in nps)
					{
						// if any attribute has a full type name add all its members of typ T with that full name as the base class name to patch
						if (string.IsNullOrEmpty(np.FullName)) continue;
						foreach (var member in members)
						{
							// constructors need a own NeedlePatch attribute
							// TODO: only capture constructors that match exactly (constructors with argument and without are both captured currently)
							if (member is ConstructorInfo && member.GetCustomAttribute<NeedlePatch>() == null)
								continue;

							// add member
							if (!res.ContainsKey(np))
								res.Add(np, (type, new List<T>(), false));
							res[np].members.Add(member);
						}
					}
				}
			}

			return res;
		}

		private static (TMember patch, NeedlePatch attribute) TryFindPatchMember<TTarget, TMember>(TTarget target,
			Dictionary<NeedlePatch, (Type source, List<TMember> list, bool used)> dict)
			where TTarget : IMemberDefinition
			where TMember : MemberInfo
		{
			var entryFullName = target.DeclaringType.FullName + "." + target.Name;
			foreach (var kvp in dict)
			{
				// we baseName is the fullname of the class for the type we want to patch
				var patch = kvp.Key;
				var members = kvp.Value.list;

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
								return (member, patch);
							}

							if (np.FullName + "." + member.Name == entryFullName)
							{
								return (member, patch);
							}
						}
					}

					var name = patch.FullName + "." + member.Name;
					if (name == entryFullName)
					{
						return (member, patch);
					}
				}
			}

			// Debug.LogWarning("Did not find patch for " + entry);
			return default;
		}


		public class Patch
		{
			public string FullName { get; internal set; }
			public bool FoundTarget => Target != null;
			public MemberInfo Target { get; internal set; }
			public readonly List<MemberInfo> Sources = new List<MemberInfo>();
		}

		private static List<Patch> FindPatches()
		{
			var types = AppDomain.CurrentDomain.GetAssemblies()
				.SelectMany(a => a.GetTypes())
				.ToArray();

			var markedTypes = MarkedTypes;
			var patches = GetPatches<MemberInfo>(markedTypes);

			var found = new Dictionary<string, Patch>();

			foreach (var kvp in patches)
			{
				var info = kvp.Value;
				foreach (var member in info.members)
				{
					var baseName = kvp.Key.FullName;
					var fullname = baseName + "." + member.Name;
					var name = member.Name;
					if (name.StartsWith("get_") || name.StartsWith("set_"))
					{
						name = name.Substring(4);
						Debug.Log(name);
					}
					var target = types.FirstOrDefault(t => t.FullName == baseName);
					var targetMember = target?.GetMember(name, (BindingFlags)~0).FirstOrDefault();
					var key = baseName + "." + name;
					if (!found.ContainsKey(key))
					{
						found.Add(key, new Patch());
					}
					var p = found[key];

					Debug.Log("Found target for " + fullname + " -> " + targetMember);

					p.Target = targetMember;
					p.FullName = fullname;
					if(!p.Sources.Contains(member))
						p.Sources.Add(member);
				}
			}
			return found.Values.ToList();
		}
	}
}