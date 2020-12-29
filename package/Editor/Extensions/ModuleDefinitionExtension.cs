using System;
using Mono.Cecil;

namespace needle.Weaver
{
	public static class ModuleDefinitionExtensions
	{
		public static void ForEachMethod(this ModuleDefinition module, Action<MethodDefinition> callback)
		{
			foreach(var type in module.Types)
			{
				foreach (var entry in type.Methods)
				{
					callback(entry);
				}
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