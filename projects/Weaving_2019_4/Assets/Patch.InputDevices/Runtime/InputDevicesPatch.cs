using System;
using System.Collections.Generic;
using needle.Weaver;
using UnityEngine;
using UnityEngine.XR;
using Random = UnityEngine.Random;

namespace needle.Weavers.InputDevicesPatch
{
	[NeedlePatch(typeof(InputDevices))]
	public class InputDevicesPatch
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

	[NeedlePatch(typeof(InputDevice))]
	public class InputDevicePatch
	{
		private ulong m_DeviceId;
		private bool m_Initialized;
		
		[NeedlePatch]
		internal InputDevicePatch(ulong deviceId)
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
		
		private static XRInputSubsystem sub = new XRInputSubsystemPatch();
		
		public XRInputSubsystem subsystem => (XRInputSubsystem) sub;
	}

	[NeedlePatch(typeof(XRInputSubsystem))]
	internal class XRInputSubsystemPatch : XRInputSubsystem
	{
		public TrackingOriginModeFlags _origin;
		
		public event Action<XRInputSubsystem> boundaryChanged;

		public new bool TrySetTrackingOriginMode(TrackingOriginModeFlags origin)
		{
			this._origin = origin;
			boundaryChanged?.Invoke(this);
			return true;
		}

	}
}