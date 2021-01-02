using System.Collections.Generic;
using needle.Weaver;
using UnityEngine;
using UnityEngine.XR;

namespace needle.Weavers.InputDevicesPatch
{
	[NeedlePatch(typeof(SubsystemManager))]
	public class SubsystemManager_Patch
	{	
		public static void GetInstances<T>(List<T> instances) where T : ISubsystem
		{
			InternalGetInstances(instances);
		}

		public static void InternalGetInstances<T>(List<T> instances)
		{
			if(instances == null) return;
			instances.Clear();
			if(XRInputSubsystem_Patch.Instance is T instance)
				instances.Add(instance);
			else 
				Debug.LogError("Failed adding mock subsystem");
		}
	}
}