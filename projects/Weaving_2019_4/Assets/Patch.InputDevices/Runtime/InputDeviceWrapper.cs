using UnityEngine;
using UnityEngine.XR;

namespace needle.Patch.InputDevices
{
	public class InputDeviceWrapper
	{
		private readonly InputDevice device;
		
		public InputDeviceWrapper(InputDevice device, ulong deviceId)
		{
			this.device = device;	
		}
	}
}