using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Rocks;

namespace needle.Weaver
{
	public static class TypeReferenceExtensions
	{
		public static TypeReference ResolveGenericParameters(this TypeReference tr, MethodDefinition method)
		{
			if (tr.IsGenericParameter)
			{

				return method.Parameters[0].ParameterType;
			}

			return tr;
		}
		
		public static IEnumerable<Type> AllGenericParameters( Type type )
        {
            if (type.IsGenericParameter)
                yield return type;
        
            if (!type.ContainsGenericParameters)
                yield break;
        
            foreach (var targ in type.GetGenericArguments())
                foreach (var gp in AllGenericParameters(targ))
                    yield return gp;
        }


		// public static TypeReference ResolveIfGeneric(this MemberReference member, TypeReference param)
  //               {
  //                   if (param.ContainsGenericParameter)
  //                       return member.ResolveGenericType(param);
  //        
  //                   return param;
  //               }
  //               
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
		// 		var nestedGeneric = (GenericInstanceType)param;
		// 		var args = nestedGeneric.GenericArguments.Select(ga => member.ResolveIfGeneric(ga)).ToArray();
		// 		return param.Module.Import(param.Resolve()).MakeGenericInstanceType(args);
		// 	}
  //
		// 	var gparam = param as GenericParameter;
		// 	if (gparam == null)
		// 		throw new Exception("Cannot resolve generic parameter");
  //
		// 	object resolvedMember = ((dynamic)member).Resolve();
		// 	object resolvedOwner = ((dynamic)gparam.Owner).Resolve();
  //
		// 	if (resolvedOwner == resolvedMember)
		// 	{
		// 		if (member is IGenericInstance)
		// 			return (member as IGenericInstance).GenericArguments[gparam.Position];
		// 		else
		// 			return ((IGenericParameterProvider)member).GenericParameters[gparam.Position];
		// 	}
		// 	else if (member.DeclaringType != null)
		// 		return member.DeclaringType.ResolveGenericType(gparam);
		// 	else
		// 		throw new Exception("Cannot resolve generic parameter");
		// }
		//
		// public static TypeReference ResolveGenericParameter(this TypeReference type, TypeReference parent)
		// {
		// 	if (!(parent is GenericInstanceType genericParent))
		// 		return type;
  //
		// 	if (type.IsGenericParameter)
		// 		return genericParent.GenericArguments[((GenericParameter) type).Position];
  //
		// 	if (type.IsArray)
		// 	{
		// 		if (type is ArrayType array)
		// 		{
		// 			array.ElementType.ResolveGenericParameter(parent);
		// 			return array;
		// 		}
		// 	}
  //
		// 	if (!type.IsGenericInstance)
		// 		return type;
  //
		// 	if (!(type is GenericInstanceType inst)) return null;
		// 	for (var i = 0; i < inst.GenericArguments.Count; i++)
		// 	{
		// 		if (!inst.GenericArguments[i].IsGenericParameter)
		// 			continue;
  //
		// 		if (inst.GenericArguments[i] is GenericParameter param) 
		// 			inst.GenericArguments[i] = genericParent.GenericArguments[param.Position];
		// 	}
  //
		// 	return inst;
		// }
	}
}