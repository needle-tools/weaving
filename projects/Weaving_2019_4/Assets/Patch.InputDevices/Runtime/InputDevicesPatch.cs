using System.Collections.Generic;
using needle.Weaver;
using UnityEngine;
using UnityEngine.XR;

namespace needle.Weavers.InputDevicesPatch
{
	[NeedlePatch("UnityEngine.XR.InputDevices")]
	public class InputDevicesPatch
	{
		[NeedlePatch]
		private static InputDevice GetDeviceAtXRNode(XRNode node)
		{
			return new InputDevice();
			// return InputDevices.GetDeviceAtXRNode(node);
		}

		[NeedlePatch]
		private static void GetDevices(List<InputDevice> inputDevices)
		{
			Debug.Log("Add devices " + Time.frameCount);
			var dev = new InputDevice();
			inputDevices.Add(dev);
			for (var i = 0; i < 10; i++)
			{
				if (Random.value > .8f)
					inputDevices.Add(dev);
			}
		}

		[NeedlePatch]
		private static bool IsDeviceValid(ulong deviceId)
		{
			return true;
		}
	}

	[NeedlePatch("UnityEngine.XR.InputDevice")]
	public class InputDevicePatch
	{
		private ulong m_DeviceId;

		[NeedlePatch]
		private ulong deviceId
		{
			get
			{
				this.m_DeviceId = (ulong) (Random.value * 1000);
				return this.m_DeviceId;
			}
		}

		[NeedlePatch]
		private bool isValid => true;

		[NeedlePatch]
		private string name => "test-" + deviceId;
	}
}