using System.Collections.Generic;
using needle.Weaver;
using UnityEngine;
using UnityEngine.XR;

namespace needle.Weavers.InputDevicesPatch
{
	[NeedlePatch(typeof(InputDevices))]
	public class InputDevices_Patch
	{
		private static InputDevice GetDeviceAtXRNode(XRNode node)
		{
			return new InputDevice();
		}

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
		
		private static bool IsDeviceValid(ulong deviceId)
		{
			return true;
		}
		
		internal static bool TryGetFeatureValue_float(
			ulong deviceId,
			string usage,
			out float value)
		{
			value = deviceId;
			return true;
		}
	}
}