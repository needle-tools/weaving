using System.Collections.Generic;
using UnityEngine.XR;
using InputDevices = needle.Patch.InputDevices.InputDevices;
using UnityEngine;

#if UNITY_EDITOR
using HarmonyLib;
#endif

namespace Fody.Weavers.InputDeviceWeaver
{
#if UNITY_EDITOR
	[HarmonyPatch(typeof(UnityEngine.XR.InputDevices))]
#endif
	public class InputDevicesPatch
	{
		
#if UNITY_EDITOR
		[HarmonyPostfix]
		[HarmonyPatch("GetDeviceAtXRNode")]
#endif
		private static void GetDeviceAtXRNode(ref InputDevice __result, XRNode node)
		{
			// can we call the default constructor if there is none?
			// to research: if we try to declare "new InputDevice()" here building fails due to IL using "initobj" vs "newobj"
			// __result = InputDevices.GetDeviceAtXRNode(node);
			__result = new InputDevice();
		}
		
#if UNITY_EDITOR
		[HarmonyPostfix]
		[HarmonyPatch("GetDevices")]
#endif
		private static void GetDevices(List<InputDevice> inputDevices)
		{
			InputDevices.GetDeviceList(inputDevices);
		}
	}
}