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

		public static MemberReference ResolveAndImportGenericMember(this MethodDefinition targetMethod, MemberReference sourceMember, ModuleDefinition module = null)
		{
			if (sourceMember == null) return null;

			var targetModule = module ?? targetMethod.Module;
			if (targetModule == null) throw new Exception("Missing module for resolving: " + targetMethod);
			
			Log.Gray(sourceMember.GetType() + " -> " + sourceMember);

			var obj = sourceMember;
			
			var declaringType = obj.DeclaringType;
			if (declaringType != null)
			{
				try
				{
					// var resolvedDeclaringType = (TypeReference) targetMethod.ResolveAndImportGenericMember(declaringType);
					Log.Gray("Declaring type: " + declaringType);
					var tr = declaringType.Copy<TypeReference>(targetModule); //new TypeReference(declaringType.Namespace, declaringType.Name, targetModule, targetModule);
					var importedType = targetModule.ImportReference(tr);
					obj.SetDeclaringType(importedType);
				}
				catch (Exception io)
				{
					Debug.LogWarning("Set Declaring type " + declaringType.GetType() + " / " + declaringType + " -> " + io);
				}
			}
			
			switch (obj)
			{
				case GenericParameter gp:
					sourceMember = targetModule.ImportReference(gp.Copy<TypeReference>(targetModule));
					break;
				
				case GenericInstanceType git:
					var gitCopy = new GenericInstanceType(git);
					sourceMember = targetModule.ImportReference(git.Copy(targetModule, gitCopy));
					break;
				
				case TypeDefinition td:
					var bt = td.BaseType.Copy<TypeReference>(targetModule);
					bt = targetModule.ImportReference(bt);
					sourceMember = new TypeDefinition(td.Namespace, td.Name, td.Attributes, bt);
					break;
				case TypeReference tr:
					sourceMember = targetModule.ImportReference(tr.Copy<TypeReference>(targetModule));
					break;
				
				case FieldDefinition fd:
					var typeReference = fd.FieldType.Copy<TypeReference>(targetModule);
					typeReference = targetModule.ImportReference(typeReference);
					typeReference.SetDeclaringType(fd.FieldType.DeclaringType);
					var fieldCopy = new FieldDefinition(fd.Name, fd.Attributes, typeReference);
					fieldCopy.SetDeclaringType(obj.DeclaringType);
					sourceMember = targetModule.ImportReference(fieldCopy);
					break;
					
				case FieldReference fr:
					var fieldType = fr.FieldType.Copy<TypeReference>(targetModule);
					fieldType = targetModule.ImportReference(fieldType);
					fieldType.SetDeclaringType(fr.FieldType.DeclaringType);
					var fieldReference = new FieldReference(fr.Name, fieldType);
					fieldReference.DeclaringType = obj.DeclaringType;
					sourceMember = targetModule.ImportReference(fieldReference);
					break;
				
				case MethodDefinition md:
					Debug.Log("METHOD DEF " + md);
					// var rt = targetModule.ImportReference(md.ReturnType.Copy<TypeReference>(targetModule));
					var copy = md.Copy(targetModule);
					// sourceMember = methodReference;
					// var bst = md.DeclaringType.BaseType;
					// var copy = new MethodDefinition(md.Name, md.Attributes, methodReference.ReturnType);
					// copy.DeclaringType = new TypeDefinition(md.DeclaringType.Name, md.DeclaringType.Name, md.DeclaringType.Attributes,
					// 	targetModule.ImportReference(bst));
					// copy.ResolvePatchReferences(md, md.Module);
					// sourceMember = copy;
					sourceMember = targetModule.ImportReference(copy);
					break;
				case MethodReference mr:
					sourceMember = targetModule.ImportReference(mr.Copy(targetModule));
					break;
			}
			
			return sourceMember;
		}

		public static MemberReference SetDeclaringType(this MemberReference tr, TypeReference dt)
		{
			if (tr is GenericInstanceType git)
				git.ElementType.DeclaringType = dt;
			else
				tr.DeclaringType = dt;
			return tr;
		}

		public static T Copy<T>(this TypeReference tr, ModuleDefinition targetModule, T instance = null) where T: TypeReference
		{
			if(instance == null)
				instance = (T) new TypeReference(tr.Namespace, tr.Name, targetModule, targetModule);
			
			if (tr.IsGenericInstance && tr is GenericInstanceType git)
			{
				if(!(instance is GenericInstanceType))
					instance = new GenericInstanceType(instance) as T;
				var git_instance = instance as GenericInstanceType;
				for (var index = 0; index < git.GenericArguments.Count; index++)
				{
					var arg = git.GenericArguments[index];
					var paramCopy = arg.Copy<TypeReference>(targetModule);
					if (git_instance.GenericArguments.Count <= index) git_instance.GenericArguments.Add(paramCopy);
					else git_instance.GenericArguments[index] = paramCopy;
				}
			}

			if (tr.IsGenericParameter && tr is GenericParameter gp)
			{
				// if(!(instance is GenericParameter))
				// 	instance = new GenericParameter(instance.Name, instance) as T;
				var pInstance = gp;// instance as GenericParameter;
				for (var index = 0; index < gp.Constraints.Count; index++)
				{
					var constraint = gp.Constraints[index];
					if (pInstance.Constraints.Count <= index) pInstance.Constraints.Add(constraint);
					else pInstance.Constraints[index] = constraint;
				}
			}

			for (var index = 0; index < tr.GenericParameters.Count; index++)
			{
				var gn = tr.GenericParameters[index];
				var paramCopy = gn.Copy(targetModule, new GenericParameter(gn.Name, instance));
				if (instance.GenericParameters.Count <= index) instance.GenericParameters.Add(paramCopy);
				else instance.GenericParameters[index] = paramCopy;
			}
			
			instance.IsValueType = tr.IsValueType;
			return instance;
		}

		private static MethodReference Copy(this MethodReference mr, ModuleDefinition targetModule)
		{
			var instance = new MethodReference(mr.Name, mr.ReturnType.Copy<TypeReference>(targetModule), mr.DeclaringType.Copy<TypeReference>(targetModule));

			if (mr.IsGenericInstance && mr is GenericInstanceMethod gim)
			{
				instance = new GenericInstanceMethod(instance);
			}
			
			if (mr.HasParameters)
			{
				for (var i = 0; i < mr.Parameters.Count; i++)
				{
					var param = mr.Parameters[i];
					var pt = param.ParameterType.Copy<TypeReference>(targetModule);
					pt = targetModule.ImportReference(pt);
					var paramCopy = new ParameterDefinition(param.Name, param.Attributes, pt);
					Debug.Log(paramCopy + " = " + paramCopy.ParameterType);
					if (instance.Parameters.Count <= i) instance.Parameters.Add(paramCopy);
					else instance.Parameters[i] = paramCopy;
				}
			}

			for (var index = 0; index < instance.GenericParameters.Count; index++)
			{
				var gn = instance.GenericParameters[index];
				var paramCopy = gn.Copy(targetModule, new GenericParameter(gn.Name, instance));
				if (instance.GenericParameters.Count <= index) instance.GenericParameters.Add(paramCopy);
				else instance.GenericParameters[index] = paramCopy;
			}

			if (instance.ReturnType != null)
			{
				var rf = instance.ReturnType.Copy<TypeReference>(targetModule);
				instance.ReturnType = rf;
			}
			
			return instance;
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