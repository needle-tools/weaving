using System.Collections.Generic;
using needle.Weaver;
using UnityEngine;
using UnityEngine.XR;

namespace needle.Weavers.InputDevicesPatch
{
	// TODO / Backlog: we could remove [NeedlePatch] attributes completely and just patch everything that matches props and methods in the target class
	
	[NeedlePatch(typeof(InputDevices))]
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

		[NeedlePatch]
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
		
		// TODO: test if we can patch constructor
		// [NeedlePatch]
		// internal InputDevicePatch(ulong deviceId)
		// {
		// 	this.m_DeviceId = deviceId;
		// 	this.m_Initialized = true;
		// }

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
		private bool IsValidId() => true;

		[NeedlePatch]
		private string name => "test-" + deviceId;
		
		[NeedlePatch]
		public bool TryGetFeatureValue(InputFeatureUsage<Vector3> usage, out Vector3 value)
		{
			value = new Vector3(0, 1, 0);
			return true;
		}

		private static XRInputSubsystem sub = new XRInputSubsystemPatch();
		
		[NeedlePatch]
		public XRInputSubsystem subsystem => (XRInputSubsystem) sub;
		// TODO: can we patch events eg from XRInputSubsystem so we can call them?
	}

	[NeedlePatch(typeof(XRInputSubsystem))]
	public class XRInputSubsystemPatch : XRInputSubsystem
	{
		public TrackingOriginModeFlags _origin;

		[NeedlePatch]
		public new bool TrySetTrackingOriginMode(TrackingOriginModeFlags origin)
		{
			this._origin = origin;
			return true;
		}

	}
}