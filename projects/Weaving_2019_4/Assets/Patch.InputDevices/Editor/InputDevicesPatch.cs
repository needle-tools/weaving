using System.Collections.Generic;
using needle.Weaver;
using UnityEngine.XR;
using InputDevices = needle.Patch.InputDevices.InputDevices;

namespace needle.Weavers.InputDevicesPatch
{
	public class InputDevicesPatch
	{
		[NeedlePatch("UnityEngine.XR.InputDevices")]
		private static InputDevice GetDeviceAtXRNode(XRNode node)
		{
			return new InputDevice();
			// return InputDevices.GetDeviceAtXRNode(node);
		}
		
		[NeedlePatch("UnityEngine.XR.InputDevices")]
		private static void GetDevices(List<InputDevice> inputDevices)
		{
			InputDevices.GetDeviceList(inputDevices);
		}
		
		[NeedlePatch("UnityEngine.XR.InputDevices")]
		private static bool IsDeviceValid(ulong deviceId)
		{
			return true;
		}
	}

	public class InputDevicePatch
	{
		[NeedlePatch("UnityEngine.XR.InputDevice")]
		private static ulong deviceId() => 42;
	}
}