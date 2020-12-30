using System;
using System.Linq;
using UnityEngine;

namespace needle.Weaver
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Constructor, AllowMultiple = false)]
	public sealed class NeedlePatch : Attribute
	{
		public bool HasName => !string.IsNullOrEmpty(FullName);
		private string fullName;

		public string FullName
		{
			get => fullName;
			set => fullName = value;
		}

		public NeedlePatch(){}

		public NeedlePatch(string fullName)
		{
			this.FullName = fullName;
		}

		public void ResolveFullNameFromParentIfNull(Type parent, string selfName)
		{
			if (!string.IsNullOrEmpty(FullName)) return;
			var type = parent;
			var path = "";
			while (type != null)
			{
				NeedlePatch np = null;
				foreach (var c in type.GetCustomAttributes(typeof(NeedlePatch), true))
				{
					if (!(c is NeedlePatch p) || p.FullName == null) continue;
					np = p;
					break;
				}

				if (np != null)
				{
					FullName = np.FullName + path + "." + selfName;
					// Debug.Log(FullName);
					break;
				}
				type = type.DeclaringType;
				if (type == null) break;
				path += "." + type.Name;
			}
		}
	}
}