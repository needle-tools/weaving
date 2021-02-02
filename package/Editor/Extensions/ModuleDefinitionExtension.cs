using System;
using System.Collections.Generic;
using Mono.Cecil;
using UnityEngine;

namespace needle.Weaver
{
	public static class ModuleDefinitionExtensions
	{
		public static void ForEachMethod(this ModuleDefinition module, Action<MethodDefinition> callback)
		{
			module.EnumerateWithNesting(t => t.Methods, callback);
		}
		
		public static void ForEachProperty(this ModuleDefinition module, Action<PropertyDefinition> callback)
		{
			module.EnumerateWithNesting(t => t.Properties, callback);
		}

		public static void EnumerateWithNesting<T>(this ModuleDefinition module, Func<TypeDefinition, IEnumerable<T>> types, Action<T> callback)
		{
			// recurse
			void Enumerate(TypeDefinition def)
			{
				foreach (var entry in types(def))
				{
					callback(entry);
				}
				
				// loop through all nested types
				foreach (var nt in def.NestedTypes)
				{
					Enumerate(nt);
				}
			}
			
			foreach(var type in module.Types)
			{
				Enumerate(type);
			}
		}

	}
}