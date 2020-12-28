using System.Collections.Generic;
using _Tests.Weaver_InputDevice;
using HarmonyLib;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.XR;
using Instruction = Mono.Reflection.Instruction;

namespace Fody.Weavers.InputDeviceWeaver
{
	[HarmonyPatch(typeof(InputDevices))]
	public class InputDevicesPatch// : IPreprocessBuildWithReport
	{	
		[HarmonyPrefix]
		[HarmonyPatch("GetDevices")]
		private static bool GetDevices_Prefix(List<InputDevice> inputDevices)
		{
			FakeInputDeviceAPI.FakeDeviceList(inputDevices);
			return false;
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