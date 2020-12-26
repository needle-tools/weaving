using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace _Tests.Weaver_InputDevice
{
	public static class FakeInputDeviceAPI
	{
		private static void FakeDeviceList(List<InputDevice> list)
		{
			var dev = new InputDevice();
			list.Add(dev);
			for (var i = 0; i < 10; i++)
			{
				if(Random.value > .8f)
					list.Add(dev);
			}
		}
	}
}