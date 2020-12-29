using System;

namespace needle.Weaver
{
	/// <summary>
	/// When added to a class or a method that also implements a harmony patch the patch will be used for weaving
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
	public class WeaveHarmony : Attribute
	{
		
	}
}