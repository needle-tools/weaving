using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace needle.Weaver
{
	public static class TypeHelper
	{
		private static readonly Dictionary<Type, IReadOnlyList<PropertyInfo>> propertyCache = new Dictionary<Type, IReadOnlyList<PropertyInfo>>();
		
		public static IReadOnlyList<PropertyInfo> GetPropertiesWithAttribute<T>() where T : Attribute
		{
			if (propertyCache.ContainsKey(typeof(T)))
			{
				return propertyCache[typeof(T)];
			}

			var list = new List<PropertyInfo>();
			propertyCache.Add(typeof(T), list);
			
			var assemblies = AppDomain.CurrentDomain.GetAssemblies();
			foreach (var assembly in assemblies)
			{
				foreach (var type in assembly.GetTypes())
				{
					CollectInTypesRecursive<T>(type, list);
				}
			}

			return list;
		}
		
		public static IReadOnlyList<PropertyInfo> GetPropertiesWithAttribute<T>(Type type) where T : Attribute
		{
			if (propertyCache.ContainsKey(typeof(T)))
				return propertyCache[typeof(T)];
			var list = new List<PropertyInfo>();
			CollectInTypesRecursive<T>(type, list);
			return list;
		}

		private static void CollectInTypesRecursive<T>(Type current, ICollection<PropertyInfo> list) where T : Attribute
		{
			while (current != null)
			{
				var props = current.GetProperties((BindingFlags) ~0);
				foreach (var prop in props)
				{
					if (prop.GetCustomAttribute<T>() != null)
					{
						list.Add(prop);
						if (list.Count > 100_000) throw new Exception("List is too big searching for  " + typeof(T) + " -> " + list.Count);
					}
				}

				current = current.BaseType;
			}
		}
	}
}