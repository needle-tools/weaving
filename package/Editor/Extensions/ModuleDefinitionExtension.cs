using System;
using Mono.Cecil;
using UnityEngine;

namespace needle.Weaver
{
	public static class ModuleDefinitionExtensions
	{
		public static void ForEachMethod(this ModuleDefinition module, Action<MethodDefinition> callback)
		{
			for (var index = 0; index < module.Types.Count; index++)
			{
				var type = module.Types[index];
				foreach (var entry in type.Methods)
				{
					callback(entry);
				}

				// TODO: could we patch methods in other assemblies this way?
				// base type is a definition apparently
				// if (type.BaseType != null)
				// {
				// 	Debug.Log(type.BaseType + " - " + type.BaseType.IsDefinition);
				// }
			}
		}
		
		public static void ForEachProperty(this ModuleDefinition module, Action<PropertyDefinition> callback)
		{
			foreach(var type in module.Types)
			{
				foreach (var entry in type.Properties)
				{
					callback(entry);
				}
			}
		}

	}
}