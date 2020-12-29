using System;

namespace needle.Weaver
{
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Constructor)]
	public sealed class NeedlePatch : Attribute
	{
		public readonly string FullName;

		public NeedlePatch(string fullName)
		{
			this.FullName = fullName;
		}
	}
}