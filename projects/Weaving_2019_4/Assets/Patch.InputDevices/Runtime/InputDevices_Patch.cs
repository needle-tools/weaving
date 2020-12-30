using System.Collections.Generic;
using System.Linq;
using needle.Weaver;
using UnityEngine;
using UnityEngine.XR;

namespace needle.Weavers.InputDevicesPatch
{
	[NeedlePatch(typeof(InputDevices))]
	public class InputDevices_Patch
	{
		private static readonly List<InputDevice> _buffer = new List<InputDevice>();
		private static IEnumerable<MockInputDevice> _inputDevices => XRInputSubsystem_Patch.InputDevices;

		
		private static InputDevice GetDeviceAtXRNode(XRNode node)
		{
			var mock = _inputDevices.FirstOrDefault(d => d.Node == node);
			if (mock == null || !XRInputSubsystem_Patch.Instance.TryGetInputDevices(_buffer)) 
				return new InputDevice();
			return _buffer.FirstOrDefault(d => d.name == mock?.Name);
		}

		private static void GetDevices_Internal(List<InputDevice> inputDevices)
		{
			XRInputSubsystem_Patch.Instance.TryGetInputDevices(inputDevices);
		}

		private static bool IsDeviceValid(ulong deviceId)
		{
			return _inputDevices.Any(d => deviceId == d.Id);
		}

		internal static string GetDeviceName(ulong deviceId) => XRInputSubsystem_Patch.TryGetDevice(deviceId)?.Name;
		
		internal static string GetDeviceManufacturer(ulong deviceId) => XRInputSubsystem_Patch.TryGetDevice(deviceId)?.Manufacturer;



		internal static bool TryGetFeatureUsages(ulong deviceId,  List<InputFeatureUsage> featureUsages)
		{
			var device = XRInputSubsystem_Patch.TryGetDevice(deviceId);
			if (device == null) return false;
			return device.TryGetUsages(featureUsages);
		}
		
		internal static bool TryGetFeatureValue_bool(
			ulong deviceId,
			string usage,
			out bool value)
		{
			var dev = _inputDevices.FirstOrDefault(d => d.Id == deviceId);
			value = false;
			return dev != null && dev.TryGetUsage(usage, out value);
		}

		internal static bool TryGetFeatureValue_UInt32(
			ulong deviceId,
			string usage,
			out uint value)
		{
			var dev = _inputDevices.FirstOrDefault(d => d.Id == deviceId);
			value = uint.MaxValue;
			return dev != null && dev.TryGetUsage(usage, out value);
		}
		
		internal static bool TryGetFeatureValue_float(
			ulong deviceId,
			string usage,
			out float value)
		{
			var dev = _inputDevices.FirstOrDefault(d => d.Id == deviceId);
			value = float.MaxValue;
			return dev != null && dev.TryGetUsage(usage, out value);
		}

		internal static bool TryGetFeatureValue_Vector3f(
			ulong deviceId,
			string usage,
			out Vector3 value)
		{
			var dev = _inputDevices.FirstOrDefault(d => d.Id == deviceId);
			value = Vector3.zero;
			return dev != null && dev.TryGetUsage(usage, out value);
		}

	}
}