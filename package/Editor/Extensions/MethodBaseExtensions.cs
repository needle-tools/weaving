using System;
using System.Reflection;
using System.Text;

namespace needle.Weaver
{
	public static class MethodBaseExtensions
	{
		// from harmony GeneralExtensions.FullDescription, should match what cecil fullname outputs for method definitions
		// public static string FullName(this MethodBase member)
		// {
		// 	if (member == (MethodBase) null)
		// 		return "null";
		// 	var returnedType = AccessTools.GetReturnedType(member);
		// 	var stringBuilder = new StringBuilder();
		// 	// if (member.IsStatic)
		// 	// 	stringBuilder.Append("static ");
		// 	// if (member.IsAbstract)
		// 	// 	stringBuilder.Append("abstract ");
		// 	// if (member.IsVirtual)
		// 	// 	stringBuilder.Append("virtual ");
		// 	stringBuilder.Append(returnedType.FullName() + " ");
		// 	if (member.DeclaringType != (Type) null)
		// 		stringBuilder.Append(member.DeclaringType.FullName() + "::");
		// 	var str = member.GetParameters().Join(p => p.ParameterType.FullName());
		// 	stringBuilder.Append(member.Name + "(" + str + ")");
		// 	return stringBuilder.ToString();
		// }
		
		public static string FullName(this Type type)
		{
			if (type == (Type) null)
				return "null";
			string str1 = type.Namespace;
			if (!string.IsNullOrEmpty(str1))
				str1 += ".";
			string str2 = str1 + type.Name;
			if (type.IsGenericType)
			{
				string str3 = str2 + "<";
				foreach (Type genericArgument in type.GetGenericArguments())
				{
					if (!str3.EndsWith("<", StringComparison.Ordinal))
						str3 += ", ";
					str3 += genericArgument.FullName();
				}
				str2 = str3 + ">";
			}
			return str2;
		}
		
	}
}