using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Rocks;
using UnityEngine;

namespace needle.Weaver
{
	public static class ImportHelper
	{
		public static void ResolvePatchReferences(this MethodDefinition targetMethod, MethodDefinition patch)
		{
			Debug.Log("Resolve " + patch + "\n" + targetMethod);
			Debug.Log("Generic instance? " + patch.IsGenericInstance);

			void LogGeneric(GenericParameter pr)
			{
				Debug.Log(pr + " HasConstraints? " + pr.HasConstraints + "" + "\n" +
				          string.Join("\n", pr.Constraints) + "\n" + pr.Module + "\n" + pr.ContainsGenericParameter);
			}
				
			
			if (patch.ContainsGenericParameter)
			{
				for (var index = 0; index < patch.GenericParameters.Count; index++)
				{
					var gn = patch.GenericParameters[index];
					LogGeneric(gn);
					foreach (var gn1 in gn.GenericParameters)
					{
						LogGeneric(gn1);
					}
				}
			}
			
			foreach (var inst in patch.Body.Instructions)
			{
				var op = inst.Operand;
				if (op == null) continue;
				if (op is MemberReference memberOperand)
				{
					Debug.Log("-----------");
					inst.Operand = targetMethod.ResolveAndImportGenericMember(memberOperand);
				}
			}
		}


		private static TypeReference Copy(this TypeReference tr, ModuleDefinition targetModule)
		{
			var instance = new TypeReference(tr.Namespace, tr.Name, targetModule, targetModule);
			

			if (tr.IsGenericInstance && tr is GenericInstanceType git)
			{
				instance = new GenericInstanceType(instance);
				
			}
			

			for (var index = 0; index < tr.GenericParameters.Count; index++)
			{
				var gn = tr.GenericParameters[index];
				var paramCopy = new GenericParameter(gn.Name, instance);
				if (instance.GenericParameters.Count <= index) instance.GenericParameters.Add(paramCopy);
				else  instance.GenericParameters[index] = paramCopy;
			}

			return instance;
		}

		private static MethodReference Copy(this MethodReference mr, ModuleDefinition targetModule)
		{
			var instance = new MethodReference(mr.Name, mr.ReturnType.Copy(targetModule), mr.DeclaringType.Copy(targetModule));

			if (mr.IsGenericInstance && mr is GenericInstanceMethod gim)
			{
				Debug.Log(mr + " - Element method " + gim.ElementMethod);
				instance = new GenericInstanceMethod(instance);
			}
			
			if (mr.HasParameters)
			{
				
			}

			if (mr.HasGenericParameters)
			{
			}
			
			return instance;
		}
		
		public static MemberReference ResolveAndImportGenericMember(this MethodDefinition targetMethod, MemberReference sourceMember)
		{
			if (sourceMember == null) return null;

			var targetModule = targetMethod.Module;
			
			Debug.Log(sourceMember + ": " + sourceMember.ContainsGenericParameter);
			

			var obj = sourceMember as object;
			switch (obj)
			{
				case FieldInfo fi:
					Debug.Log(fi);
					break;
				case FieldReference fr:
					Debug.Log(fr);
					break;
				case MethodBase mb:
					Debug.Log(mb);
					break;
				case MethodReference mr:
					mr = mr.Copy(targetModule);
					
					Debug.Log("METHOD REFERENCE: " +  mr + " - " + mr.IsGenericInstance + " - " + " - " + mr.HasGenericParameters + " - " +
					          mr.ContainsGenericParameter + " - " + mr.HasParameters + "\n" + string.Join("\n", mr.GenericParameters));
					
					for (var index = 0; index < mr.Parameters.Count; index++)
					{
						var gp = mr.Parameters[index];
						// var res = ResolveAndImportGenericMember(targetMethod, gp) as ParameterReference;
						Debug.Log(gp + " -> " + gp);
					}

					ResolveGenericParameters(mr);

					if (mr.ContainsGenericParameter)
					{
						var res = targetMethod.ResolveAndImportGenericMember(mr.DeclaringType);
						Debug.Log("resolved " + res);
					}
					

					break;
				case Type t:
					Debug.Log(t);
					break;
				case TypeReference tr:
					Debug.Log("TYPEREFERENCE + " + tr + " - " + tr.IsGenericInstance + " - " + tr.IsGenericParameter + " - " + tr.HasGenericParameters + " - " +
					          tr.ContainsGenericParameter + "\n" + string.Join("\n", tr.GenericParameters));
					ResolveGenericParameters(tr);

					break;
			}
			//
			//
			// return type;
			return sourceMember;
		}

		private static void ResolveGenericParameters(IGenericParameterProvider prov)
		{
			for (var index = 0; index < prov.GenericParameters.Count; index++)
			{
				var gp = prov.GenericParameters[index];
				Debug.Log("GenericParam: " + gp + " - " + string.Join("\n", gp.Constraints));
				if (gp.ContainsGenericParameter)
				{
					foreach (var inner in gp.GenericParameters)
					{
						ResolveGenericParameters(inner);
					}
				}
			}
		}
		
		
		public static TypeReference ResolveGenericParameters(this TypeReference tr, MethodDefinition method)
		{
			if (tr.IsGenericParameter)
			{
				Debug.LogWarning("TODO: resolve generics properly :)");
				return method.Parameters[0].ParameterType;
			}
		
			return tr;
		}
		
		// https://stackoverrun.com/de/q/1200700
		// public static TypeReference MakeGenericType (this TypeReference self, params TypeReference [] arguments)
		// {
		// 	if (self.GenericParameters.Count != arguments.Length)
		// 		throw new ArgumentException ();
		//
		// 	var instance = new GenericInstanceType (self);
		// 	foreach (var argument in arguments)
		// 		instance.GenericArguments.Add (argument);
		//
		// 	return instance;
		// }

		// https://stackoverrun.com/de/q/1200700
		// public static MethodReference MakeGeneric (this MethodReference self, params TypeReference [] arguments)
		// {
		// 	var reference = new MethodReference(self.Name,self.ReturnType) {
		// 		DeclaringType = self.DeclaringType,//.MakeGenericType (arguments),
		// 		HasThis = self.HasThis,
		// 		ExplicitThis = self.ExplicitThis,
		// 		CallingConvention = self.CallingConvention,
		// 	};
		//
		// 	foreach (var parameter in self.Parameters)
		// 		reference.Parameters.Add (new ParameterDefinition (parameter.ParameterType));
		//
		// 	foreach (var generic_parameter in self.GenericParameters)
		// 		reference.GenericParameters.Add (new GenericParameter (generic_parameter.Name, reference));
		//
		// 	return reference;
		// }
		
		
		// https://csharp.hotexamples.com/examples/Mono.Cecil/GenericParameter/-/php-genericparameter-class-examples.html

		//
		// public static IGenericParameterProvider DoResolve(this MethodReference mr)
		// {
		// 	if (mr.IsGenericInstance)
		// 	{
		// 		var gi = mr as GenericInstanceMethod;
		// 		if (gi != null)
		// 			foreach (var ga in gi?.GenericArguments)
		// 			{
		// 				Debug.Log(ga);
		// 			}
		//
		// 		if (gi != null)
		// 			foreach (var ga in gi?.GenericParameters)
		// 				Debug.Log(ga);
		// 	}
		//
		// 	if (mr.ContainsGenericParameter)
		// 	{
		// 		Debug.Log("contains generic");
		// 		var dc = mr.DeclaringType;
		// 		if (dc.HasGenericParameters)
		// 		{
		// 			foreach (var ga in dc?.GenericParameters)
		// 				Debug.Log(ga);
		// 		}
		//
		// 		if (dc.IsGenericInstance && dc is GenericInstanceType git)
		// 		{
		// 			if (git.HasGenericArguments)
		// 			{
		// 				for (var index = 0; index < git.GenericArguments.Count; index++)
		// 				{
		// 					// try
		// 					// {
		// 					// 	var imp = mr.Module.ImportReference(git.DeclaringType);
		// 					// 	var tr = imp as GenericInstanceType;
		// 					//
		// 					// }
		// 					// catch (Exception e)
		// 					// {
		// 					// 	Debug.LogException(e);
		// 					// }
		// 					
		// 					// var arg = git.GenericArguments[index];
		// 					// Debug.Log(arg);
		// 					// if (arg is GenericParameter gp)
		// 					// {
		// 					// 	Debug.Log(gp.Constraints[0].ConstraintType);
		// 					// 	gp.Constraints[0] = new GenericParameterConstraint(mr.Module.ImportReference(gp.Constraints[0].ConstraintType));
		// 					// }
		// 					//
		// 					// git.GenericArguments[index] = mr.Module.ImportReference(arg);
		// 					return git;// git.GenericArguments[index];
		// 				}
		// 			}
		// 		}
		// 		
		// 	}
		// 	return null;
		// }
		//
		//
		//
		// public static IEnumerable<Type> AllGenericParameters(Type type)
		// {
		// 	if (type.IsGenericParameter)
		// 		yield return type;
		//
		// 	if (!type.ContainsGenericParameters)
		// 		yield break;
		//
		// 	foreach (var targ in type.GetGenericArguments())
		// 	foreach (var gp in AllGenericParameters(targ))
		// 		yield return gp;
		// }
		//
		//
		// public static TypeReference ResolveIfGeneric(this MemberReference member, TypeReference param)
		// {
		// 	if (param.ContainsGenericParameter)
		// 		return member.ResolveGenericType(param);
		//
		// 	return param;
		// }
		//
		// //               
		// public static TypeReference ResolveGenericType(this MemberReference member, TypeReference param)
		// {
		// 	if (!param.ContainsGenericParameter)
		// 		throw new Exception($"{param} is not generic!");
		//
		// 	if (param.IsByReference && param.ContainsGenericParameter)
		// 		return new ByReferenceType(member.ResolveGenericType(param.GetElementType()));
		//
		// 	if (param.IsGenericInstance)
		// 	{
		// 		var nestedGeneric = (GenericInstanceType) param;
		// 		var args = nestedGeneric.GenericArguments.Select(ga => member.ResolveIfGeneric(ga)).ToArray();
		// 		return param.Module.Import(param.Resolve()).MakeGenericInstanceType(args);
		// 	}
		//
		// 	var gparam = param as GenericParameter;
		// 	if (gparam == null)
		// 		throw new Exception("Cannot resolve generic parameter");
		//
		// 	object resolvedMember = ((dynamic) member).Resolve();
		// 	object resolvedOwner = ((dynamic) gparam.Owner).Resolve();
		//
		// 	if (resolvedOwner == resolvedMember)
		// 	{
		// 		if (member is IGenericInstance)
		// 			return (member as IGenericInstance).GenericArguments[gparam.Position];
		// 		else
		// 			return ((IGenericParameterProvider) member).GenericParameters[gparam.Position];
		// 	}
		// 	else if (member.DeclaringType != null)
		// 		return member.DeclaringType.ResolveGenericType(gparam);
		// 	else
		// 		throw new Exception("Cannot resolve generic parameter");
		// }
		//
		// //
		// // public static TypeReference ResolveGenericParameter(this TypeReference type, TypeReference parent)
		// // {
		// // 	if (!(parent is GenericInstanceType genericParent))
		// // 		return type;
		// //
		// // 	if (type.IsGenericParameter)
		// // 		return genericParent.GenericArguments[((GenericParameter) type).Position];
		// //
		// // 	if (type.IsArray)
		// // 	{
		// // 		if (type is ArrayType array)
		// // 		{
		// // 			array.ElementType.ResolveGenericParameter(parent);
		// // 			return array;
		// // 		}
		// // 	}
		// //
		// // 	if (!type.IsGenericInstance)
		// // 		return type;
		// //
		// // 	if (!(type is GenericInstanceType inst)) return null;
		// // 	for (var i = 0; i < inst.GenericArguments.Count; i++)
		// // 	{
		// // 		if (!inst.GenericArguments[i].IsGenericParameter)
		// // 			continue;
		// //
		// // 		if (inst.GenericArguments[i] is GenericParameter param) 
		// // 			inst.GenericArguments[i] = genericParent.GenericArguments[param.Position];
		// // 	}
		// //
		// // 	return inst;
		// // }
	}
}