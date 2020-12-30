using needle.Weaver;
using UnityEngine;
using UnityEngine.XR;

namespace needle.Weavers.InputDevicesPatch
{
	[NeedlePatch(typeof(InputDevice))]
	public class InputDevice_Patch
	{
		private ulong m_DeviceId;
		private bool m_Initialized;
		
		[NeedlePatch]
		internal InputDevice_Patch(ulong deviceId)
		{
			this.m_DeviceId = deviceId;
			this.m_Initialized = true;
		}

		private ulong deviceId
		{
			get
			{
				this.m_DeviceId = (ulong) (Random.value * 1000);
				return this.m_DeviceId;
			}
		}
		
		private bool isValid => true;
		private bool IsValidId() => true;
		
		private string name => "test-" + deviceId;
		
		public bool TryGetFeatureValue(InputFeatureUsage<Vector3> usage, out Vector3 value)
		{
			value = new Vector3(0, 1, 0);
			return true;
		}
		
		private static XRInputSubsystem sub = new XRInputSubsystem_Patch();
		
		public XRInputSubsystem subsystem => (XRInputSubsystem) sub;
	}
}