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
			var mock = new MockInputDevice("test", XRNode.Head);
			instances.Clear();
			if(XRInputSubsystem_Patch.Instance is T instance)
				instances.Add(instance);
			else 
				Debug.LogError("Failed adding mock subsystem");
		}

	}
}