using System.Collections.Generic;
using System.Reflection;
using needle.Weavers.InputDevicesPatch;
using UnityEngine;

namespace _Tests.Weaver_InputDevice
{
	public static class ReflectIntegratedSubsystems
	{
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		private static void Init()
		{
			var type = typeof(SubsystemManager);
			var field = type.GetField("s_IntegratedSubsystems", (BindingFlags) ~0);
			var list = field.GetValue(null) as List<IntegratedSubsystem>;
			var my = XRInputSubsystem_Patch.Instance;
			list.Add(my);

			var ml = new List<ISubsystem>();
			SubsystemManager.GetInstances(ml);
			Debug.Log(ml.Count);
			foreach (var s in ml) Debug.Log(s);
		}
	}
}