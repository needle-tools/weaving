using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using UnityEngine;

namespace needle.Weaver
{
	public static class MethodDefinitionExtensions
	{
		public static bool AddExternalMethodBody(this MethodDefinition method)
		{
			if (method == null) return false;
			if (method.HasBody) return false;
			
			method.IsManaged = true;
			method.IsIL = true;
			method.IsNative = false;
			method.PInvokeInfo = null;
			method.IsInternalCall = false;
			method.IsPInvokeImpl = false;
			method.NoInlining = true;
			method.Body = new MethodBody(method); 
			var processor = method.Body?.GetILProcessor();
			var mrt = method.MethodReturnType;
			if (mrt != null)
			{
				var rt = mrt.ReturnType;
				var tempVar = new VariableDefinition(rt);
				method.Body.Variables.Add(tempVar);
				switch (rt.Name)
				{
					case "String":
						processor.Emit(OpCodes.Ldstr, "");
						break;
					default:
						processor.Emit(OpCodes.Ldloc, tempVar);
						break;
					case "Void":
						break;
				}
			}
			processor.Append(Instruction.Create(OpCodes.Ret));
			return true;
		}
		
		
		// // from https://github.com/jbevain/cecil/blob/master/Test/Mono.Cecil.Tests/Extensions.cs
		public static TypeDefinition ToDefinition (this Type self)
		{
			var module = ModuleDefinition.ReadModule (new MemoryStream (File.ReadAllBytes (self.Module.FullyQualifiedName)));
			return (TypeDefinition) module.LookupToken (self.MetadataToken);
		}
		
		// // from https://github.com/jbevain/cecil/blob/master/Test/Mono.Cecil.Tests/Extensions.cs
		public static MethodDefinition ToDefinition (this System.Reflection.MethodBase method)
		{
			var declaring_type = method.DeclaringType.ToDefinition ();
			return (MethodDefinition) declaring_type.Module.LookupToken (method.MetadataToken);
		}
	}
}