using needle.Weaver;
using UnityEngine;
using UnityEngine.XR;

namespace needle.Weavers.InputDevicesPatch
{
	[NeedlePatch(typeof(InputDevice))]
	public class InputDevice_Patch
	{
		private ulong m_DeviceId = default;
		
		public XRInputSubsystem subsystem => XRInputSubsystem_Patch.Instance;
		
		private bool IsValidId() => XRInputSubsystem_Patch.TryGetDevice(m_DeviceId) != null;
	}
}