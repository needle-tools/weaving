using System.Collections.Generic;
using UnityEngine.XR;
using InputDevices = needle.Patch.InputDevices.InputDevices;
using HarmonyLib;

namespace Fody.Weavers.InputDeviceWeaver
{
	[HarmonyPatch(typeof(UnityEngine.XR.InputDevices))]
	public class InputDevicesPatch
	{
		[HarmonyPostfix]
		[HarmonyPatch("GetDeviceAtXRNode")]
		private static InputDevice GetDeviceAtXRNode(InputDevice device, XRNode node)
		{
			// can we call the default constructor if there is none?
			// to research: if we try to declare "new InputDevice()" here building fails due to IL using "initobj" vs "newobj"
			return InputDevices.GetDeviceAtXRNode(node);
		}
		
		[HarmonyPostfix]
		[HarmonyPatch("GetDevices")]
		private static void GetDevices(List<InputDevice> inputDevices)
		{
			InputDevices.GetDeviceList(inputDevices);
		}
	}
}