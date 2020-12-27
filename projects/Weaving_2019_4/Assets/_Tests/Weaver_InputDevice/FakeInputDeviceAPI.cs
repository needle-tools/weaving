using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.XR;

[assembly: Preserve]

namespace _Tests.Weaver_InputDevice
{
	// https://docs.unity3d.com/Manual/ManagedCodeStripping.html
	public static class FakeInputDeviceAPI
	{
		private static void FakeDeviceList(List<InputDevice> list)
		{
			Debug.Log("Add devices");
			var dev = new InputDevice();
			list.Add(dev);
			for (var i = 0; i < 10; i++)
			{
				if (Random.value > .8f)
					list.Add(dev);
			}
		}
	}
}