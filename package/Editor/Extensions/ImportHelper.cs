using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;
using Mono.Cecil;
using Mono.Cecil.Rocks;
using Mono.Collections.Generic;
using UnityEngine;

namespace needle.Weaver
{
	public static class ImportHelper
	{
		public static void ResolvePatchReferences(this MethodDefinition targetMethod, MethodDefinition patch, ModuleDefinition module = null)
		{
			foreach (var inst in patch.Body.Instructions)
			{
				var op = inst.Operand;
				if (op == null) continue;
				if (op is MemberReference memberOperand)
				{
					inst.Operand = targetMethod.ResolveAndImportGenericMember(memberOperand, module);
				}
			}
		}

		public static object ResolveAndImportGenericMember(this MethodDefinition targetMethod, object operand, ModuleDefinition module = null)
		{
			if (operand == null) return null;

			var targetModule = module ?? targetMethod.Module;
			module = targetModule;
			if (targetModule == null) throw new Exception("Missing module for resolving: " + targetMethod);
			
			Log.Gray(operand.GetType() + " -> " + operand);

			var obj = operand as MemberReference;
			var declaringType = obj?.DeclaringType;
			if (declaringType != null)
			{
				// var resolvedDeclaringType = (TypeReference) targetMethod.ResolveAndImportGenericMember(declaringType);
				Log.Gray("DeclaringType is " + declaringType.GetType() + " = " + declaringType);
				// var tr = declaringType.Copy<TypeReference>(targetModule); //new TypeReference(declaringType.Namespace, declaringType.Name, targetModule, targetModule);
				var tr = ResolveAndImportGenericMember(targetMethod, declaringType, module) as TypeReference;
				obj.SetDeclaringType(tr);
			}

			obj = obj.Import(module);
			switch (operand)
			{
				default:
					Debug.LogWarning("Unhandled? " + obj?.GetType() + ", " + obj);
					break;
				
				case GenericParameter gp:
					// operand = targetModule.ImportReference(gp);
					break;
				
				// case GenericInstanceType git:
				// 	operand = targetModule.ImportReference(git);
				// 	break;
				
				case Type type:
					operand = module.ImportReference(type);
					break;
				case TypeReference tr:
					// operand = targetModule.ImportReference(tr);
					break;
				case FieldInfo fi:
					operand = module.ImportReference(fi);
					break;
				
				case FieldReference fr:
					operand = targetModule.ImportReference(fr);
					break;
				case MethodBase mb:
					operand = module.ImportReference(mb);
					break;
				case MethodReference mr:
					// operand = module.ImportReference(mr);
					break;
			}

			if (operand == null) throw new Exception("Lost member");
			if (obj != null && obj.Module == null) throw new Exception("Module is null for " + operand.GetType() + " - " + operand);
			// if(sourceMember.Module != targetModule) throw new Exception("Modules do not match: " + sourceMember.GetType() + " - " + sourceMember + "\nActual: " + sourceMember.Module + " \nExpected: " + targetModule);
			return operand;
		}

		private static void SetDeclaringType(this MemberReference tr, TypeReference dt)
		{
			if (dt == null) return;
			// handle setting declaring type not supported
			if (tr is MethodSpecification) return;
			if (tr is GenericInstanceType git)
				git.ElementType.DeclaringType = dt;
			else
				tr.DeclaringType = dt;
		}

		public class GenericParameterProviderWrapper : IGenericParameterProvider
		{
			public MetadataToken MetadataToken
			{
				get => mr.MetadataToken;
				set { throw new NotImplementedException(); }
			}

			public bool HasGenericParameters => prov.HasGenericParameters;
			public bool IsDefinition => mr.IsDefinition;
			public ModuleDefinition Module { get; private set; }
			public Collection<GenericParameter> GenericParameters => prov.GenericParameters;
			public GenericParameterType GenericParameterType => prov.GenericParameterType;

			private MemberReference mr;
			private IGenericParameterProvider prov;

			public GenericParameterProviderWrapper(MemberReference mr, ModuleDefinition module)
			{
				this.mr = mr;
				this.prov = mr as IGenericParameterProvider;
				this.Module = module;
			}
		}

		private static T Import<T>(this T tr, ModuleDefinition targetModule) where T : MemberReference
		{
			// var instance = new TypeReference(tr.Namespace, tr.Name, targetModule, targetModule);

			Log.Gray("Import " + tr + " -> " + tr.GetType());
			if (tr is TypeReference typeReference)
			{
				for (var index = 0; index < typeReference.GenericParameters.Count; index++)
				{
					var genericParameter = typeReference.GenericParameters[index];
					var paramCopy = genericParameter.Import(targetModule);
					typeReference.GenericParameters[index] = paramCopy;
					if (paramCopy?.Position <= -1) throw new NotSupportedException(genericParameter.ToString());
				}
				
				if (typeReference.IsGenericInstance && tr is GenericInstanceType git)
				{
					for (var index = 0; index < git.GenericArguments.Count; index++)
					{
						var arg = git.GenericArguments[index];
						// var context = new GenericParameterProviderWrapper(arg, targetModule);
						var copy = arg.Import(targetModule);
						// copy = targetModule.ImportReference(copy);
						git.GenericArguments[index] = copy;
					}
				}

				if (typeReference.IsGenericParameter && tr is GenericParameter gp)
				{
					for (var index = 0; index < gp.Constraints.Count; index++)
					{
						var constraint = gp.Constraints[index];
						var ct = constraint.ConstraintType;
						Log.Gray(constraint);
						// var context = new GenericParameterProviderWrapper(ct, targetModule);
						constraint.ConstraintType = targetModule.ImportReference(ct.Import(targetModule));
						gp.Constraints[index] = constraint;
					}
				}
			}
			
			
			if (tr is MethodReference methodReference)
			{
				if (methodReference.HasParameters)
				{
					for (var index = 0; index < methodReference.Parameters.Count; index++)
					{
						var param = methodReference.Parameters[index];
						param.ParameterType = param.ParameterType.Import(targetModule);
					}
				}
			}
			
			return tr;
		}

		// private static MethodReference Copy(this MethodReference mr, ModuleDefinition targetModule)
		// {
		// 	var instance = new MethodReference(mr.Name, mr.ReturnType.Import(targetModule), mr.DeclaringType.Import(targetModule));
		//
		// 	if (mr.IsGenericInstance && mr is GenericInstanceMethod gim)
		// 	{
		// 		instance = new GenericInstanceMethod(instance);
		// 	}
		// 	
		// 	if (mr.HasParameters)
		// 	{
		// 		for (var i = 0; i < mr.Parameters.Count; i++)
		// 		{
		// 			var param = mr.Parameters[i];
		// 			Debug.Log(param);
		// 			// var pt = param.ParameterType;//.Copy(targetModule);
		// 			// pt = targetModule.ImportReference(pt);
		// 			param.ParameterType = targetModule.ImportReference(param.ParameterType.Import(targetModule));
		// 			// var paramCopy = new ParameterDefinition(param.Name, param.Attributes, pt);
		// 			// if (instance.Parameters.Count <= i) instance.Parameters.Add(paramCopy);
		// 			// else instance.Parameters[i] = paramCopy;
		// 		}
		// 	}
		//
		// 	for (var index = 0; index < instance.GenericParameters.Count; index++)
		// 	{
		// 		var gn = instance.GenericParameters[index];
		// 		var paramCopy = gn.Import(targetModule) as GenericParameter;
		// 		if (paramCopy?.Position <= -1) throw new NotSupportedException(gn.ToString());
		// 		if (instance.GenericParameters.Count <= index) instance.GenericParameters.Add(paramCopy);
		// 		else instance.GenericParameters[index] = paramCopy;
		// 	}
		//
		// 	if (instance.ReturnType != null)
		// 	{
		// 		var rf = instance.ReturnType.Import(targetModule);
		// 		instance.ReturnType = rf;
		// 	}
		// 	
		// 	return instance;
		// }

		// private static void ResolveGenericParameters(IGenericParameterProvider prov)
		// {
		// 	for (var index = 0; index < prov.GenericParameters.Count; index++)
		// 	{
		// 		var gp = prov.GenericParameters[index];
		// 		Debug.Log("GenericParam: " + gp + " - " + string.Join("\n", gp.Constraints));
		// 		if (gp.ContainsGenericParameter)
		// 		{
		// 			foreach (var inner in gp.GenericParameters)
		// 			{
		// 				ResolveGenericParameters(inner);
		// 			}
		// 		}
		// 	}
		// }
		
		
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