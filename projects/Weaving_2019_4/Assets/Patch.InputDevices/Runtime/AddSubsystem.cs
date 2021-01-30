using System.Collections.Generic;
using System.Reflection;
using needle.Weavers.InputDevicesPatch;
using UnityEngine;

namespace Patch.InputDevices.Runtime
{
	public static class AddSubsystem
	{
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		private static void Init()
		{
#if UNITY_EDITOR
			if (Application.isPlaying)
			{
				Debug.Log("Would inject subsystem in webgl build but not in editor");
				return;
			}
#endif
			var type = typeof(SubsystemManager);
			var field = type.GetField("s_IntegratedSubsystems", (BindingFlags) ~0);
			var list = field?.GetValue(null) as List<IntegratedSubsystem>;
			var my = XRInputSubsystem_Patch.Instance;
			list?.Add(my);

			var ml = new List<ISubsystem>();
			SubsystemManager.GetInstances(ml);
			if (ml.Count <= 0) Debug.LogError("Failed adding Subsystem for webgl support");
			else Debug.Log("Added subsystem successfully");
		}
	}
}