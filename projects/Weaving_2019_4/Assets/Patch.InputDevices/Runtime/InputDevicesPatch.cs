using System;
using System.Collections.Generic;
using _Tests.Weaver_InputDevice;
using UnityEngine;
using UnityEngine.XR;

#if UNITY_EDITOR
using HarmonyLib;
#endif

namespace Fody.Weavers.InputDeviceWeaver
{
#if UNITY_EDITOR
	[HarmonyPatch(typeof(InputDevices))]
#endif
	public class InputDevicesPatch// : IPreprocessBuildWithReport
	{
		#if UNITY_EDITOR
		[HarmonyPostfix]
		[HarmonyPatch("GetDevices")]
		#endif
		private static void GetDevices_Postfix(List<InputDevice> inputDevices)
		{
			// inputDevices.Clear();
			// Debug.Log("Add devices");
			// inputDevices.Add(new InputDevice());
			// for (var i = 0; i < 10; i++)
			// {
			// 	if (Random.value > .8f)
			// 		inputDevices.Add(dev);
			// }
			FakeInputDeviceAPI.FakeDeviceList(inputDevices);
			// return false;
		}
		
		// [HarmonyPostfix]
		// [HarmonyPatch("GetDevices")]
		// private static void GetDevices_Postfix(List<InputDevice> inputDevices)
		// {
		// 	Debug.Log("POSTFIX");
		// }

		// public int callbackOrder => 1000;
		// public void OnPreprocessBuild(BuildReport report)
		// {
		// 	TestHarmonyPatchILConversion.PrintIL();
		// 	Debug.LogError("error to stop build");
		// }
	}
}