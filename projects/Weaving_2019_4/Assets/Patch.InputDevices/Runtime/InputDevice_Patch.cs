using needle.Weaver;
using UnityEngine;
using UnityEngine.XR;

namespace needle.Weavers.InputDevicesPatch
{
	[NeedlePatch(typeof(InputDevice))]
	public class InputDevice_Patch
	{
		private ulong m_DeviceId = default;
		// private bool m_Initialized;
		
		public XRInputSubsystem subsystem => XRInputSubsystem_Patch.Instance;
		
		// [NeedlePatch]
		// internal InputDevice_Patch(ulong deviceId)
		// {
		// 	this.m_DeviceId = deviceId;
		// 	this.m_Initialized = true;
		// }
		
		// private bool isValid => true;
		private bool IsValidId() => XRInputSubsystem_Patch.TryGetDevice(m_DeviceId) != null;

		// private string name => XRInputSubsystem_Patch.TryGetDevice(m_DeviceId)?.Name;
	}
}