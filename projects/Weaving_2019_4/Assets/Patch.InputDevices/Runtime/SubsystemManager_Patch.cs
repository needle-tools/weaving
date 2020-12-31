using System.Collections.Generic;
using needle.Weaver;
using UnityEngine;

namespace needle.Weavers.InputDevicesPatch
{
	[NeedlePatch(typeof(SubsystemManager))]
	public class SubsystemManager_Patch
	{
		private static void InternalGetDevices(List<ISubsystem> instances)
		{
			instances.Clear();
			if(XRInputSubsystem_Patch.Instance is ISubsystem instance)
				instances.Add(instance);
			else Debug.LogError("Failed adding mock subsystem");
		}
		
		public static void GetInstances<T>(List<T> instances) where T : ISubsystem
		{
			InternalGetDevices(instances as List<ISubsystem>);
		}

	}
}