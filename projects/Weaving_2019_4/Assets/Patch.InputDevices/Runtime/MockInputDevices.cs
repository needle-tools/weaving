using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.XR;

[assembly: Preserve]

namespace needle.Patch.InputDevices
{
	// https://docs.unity3d.com/Manual/ManagedCodeStripping.html
	public static class MockInputDevices
	{
		public static void GetDeviceList(List<InputDevice> list)
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