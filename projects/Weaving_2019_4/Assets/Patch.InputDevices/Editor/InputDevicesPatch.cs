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
			return new InputDevice();
		}
		
		[HarmonyPostfix]
		[HarmonyPatch("GetDevices")]
		private static void GetDevices(List<InputDevice> inputDevices)
		{
			InputDevices.GetDeviceList(inputDevices);
		}

		// [HarmonyPostfix]
		// [HarmonyPatch("GetDevices")]
		// private static void GetDevices_Postfix(List<InputDevice> inputDevices)
		// {
		// 	Debug.Log("POSTFIX");
		// }

	}
}