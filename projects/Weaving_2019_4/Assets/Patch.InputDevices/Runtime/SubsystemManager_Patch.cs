using System.Collections.Generic;
using needle.Weaver;
using UnityEngine;
using UnityEngine.XR;

namespace needle.Weavers.InputDevicesPatch
{
	[NeedlePatch(typeof(SubsystemManager))]
	public static class SubsystemManager_Patch
	{	
		// maybe this is missing: https://stackoverrun.com/de/q/1200700
		public static void GetInstances<T>(List<T> instances) where T : ISubsystem
		{
			if(instances == null) return;
			instances.Clear();
			if(XRInputSubsystem_Patch.Instance is T instance)
				instances.Add(instance);
			else 
				Debug.LogError("Failed adding mock subsystem");
		}
		//
		// internal static void ReportSingleSubsystemAnalytics(string id)
		// {
		// 	
		// }

	}
}