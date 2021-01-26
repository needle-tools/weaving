using System.Collections.Generic;
using needle.Weaver;

namespace UnityEngine
{
	
	[NeedlePatch(typeof(SubsystemManager))]
	public static class SubsystemManager
	{
		public static void GetInstances<T>(List<T> instances) where T : ISubsystem
		{
			// instances.Clear();
			Debug.Log("HELLO");
			// if(XRInputSubsystem_Patch.Instance is T instance)
			// 	instances.Add(instance);
			// else Debug.LogError("Failed adding mock subsystem");
		}

	}
}