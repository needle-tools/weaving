using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace needle.Weaver
{
	public static class TypeAccessHelper
	{
		public static IReadOnlyList<T> GetMembers<T>(Type type, bool includeBases) where T : MemberInfo
		{
			var list = new List<T>();
			InternalCollectTypes(type, list, includeBases);
			return list;
		}
		
		private static void InternalCollectTypes<TResult>(Type current, ICollection<TResult> list, bool includeBases, Type attributeType = null) 
			where TResult : MemberInfo
		{
			while (current != null)
			{
				const BindingFlags flags = (BindingFlags) ~0;
				void CollectIfTypeIsAssignable<T>(Func<BindingFlags, T[]> get) where T : MemberInfo
				{
					if (typeof(TResult).IsAssignableFrom(typeof(T)))
					{
						var found = get(flags);
						foreach (var entry in found)
						{
							if (attributeType == null || entry.GetCustomAttribute(attributeType) != null)
							{
								list.Add(entry as TResult);
							}
						}
					}
				}

				CollectIfTypeIsAssignable(current.GetProperties);
				CollectIfTypeIsAssignable(current.GetFields);
				CollectIfTypeIsAssignable(current.GetMethods);
				CollectIfTypeIsAssignable(current.GetConstructors);
				CollectIfTypeIsAssignable(current.GetEvents);

				if (!includeBases) break;
				if (list.Count > 100_000) throw new Exception("List is too big searching for  " + attributeType + " -> " + list.Count);
				current = current.BaseType;
			}
		}
	}
}