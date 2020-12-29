using System.Collections.Generic;
using needle.Weaver;
using UnityEngine.XR;
using InputDevices = needle.Patch.InputDevices.InputDevices;

namespace needle.Weavers.InputDevicesPatch
{
	public class InputDevicesPatch
	{
		[NeedlePatch("UnityEngine.XR.InputDevices.GetDeviceAtXRNode")]
		private static InputDevice GetDeviceAtXRNode(XRNode node)
		{
			return new InputDevice();
		}
		
		[NeedlePatch("UnityEngine.XR.InputDevices.GetDevices")]
		private static void GetDevices(List<InputDevice> inputDevices)
		{
			InputDevices.GetDeviceList(inputDevices);
		}
		
	}

	public class InputDevicePatch
	{
		[NeedlePatch("UnityEngine.XR.InputDevice.deviceId")]
		private static ulong DeviceId() => 42;
	}
}