using System;

namespace needle.Weaver
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Constructor)]
	public sealed class NeedlePatch : Attribute
	{
		public bool HasName => !string.IsNullOrEmpty(FullName);
		public readonly string FullName;
		
		public NeedlePatch(){}

		public NeedlePatch(string fullName)
		{
			this.FullName = fullName;
		}
	}
}