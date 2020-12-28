using System.Collections.Generic;
using Editor.Attributes;
using UnityEngine.XR;
using InputDevices = needle.Patch.InputDevices.InputDevices;
using HarmonyLib;
using UnityEngine;

namespace Fody.Weavers.InputDeviceWeaver
{
	[HarmonyPatch(typeof(UnityEngine.XR.InputDevices))]
	public class InputDevicesPatch
	{
		[NeedlePatch]
		private static InputDevice GetDeviceAtXRNode(XRNode node)
		{
			return new InputDevice();
		}
		
		[HarmonyPostfix]
		[HarmonyPatch("GetDeviceAtXRNode")]
		private static void GetDeviceAtXRNode(ref InputDevice __result, XRNode node)
		{
			// can we call the default constructor if there is none?
			// to research: if we try to declare "new InputDevice()" here building fails due to IL using "initobj" vs "newobj"
			// __result = InputDevices.GetDeviceAtXRNode(node);
			// __result = new InputDevice();
		}
		
		[HarmonyPostfix]
		[HarmonyPatch("GetDevices")]
		private static void GetDevices(List<InputDevice> inputDevices)
		{
			InputDevices.GetDeviceList(inputDevices);
		}
	}
}