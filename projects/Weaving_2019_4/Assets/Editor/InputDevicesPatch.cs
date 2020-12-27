using System.Collections.Generic;
using _Tests.Weaver_InputDevice;
using HarmonyLib;
using UnityEditor;
using UnityEngine.XR;

namespace Editor
{
	[HarmonyPatch(typeof(InputDevices))]
	public class InputDevicesPatch
	{
		[HarmonyPrefix]
		[HarmonyPatch("GetDevices")]
		private static bool GetDevices(List<InputDevice> inputDevices)
		{
			FakeInputDeviceAPI.FakeDeviceList(inputDevices);
			return false;
		}
	}
}