using System;
using System.Collections.Generic;
using System.Linq;
using needle.Weavers.InputDevicesPatch;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using Random = UnityEngine.Random;

namespace _Tests.Weaver_InputDevice
{
	[ExecuteInEditMode]
	public class UsingInputDevice : MonoBehaviour
	{
		public Text Text;

		private static MockInputDevice device;
		private static Quaternion _rotation = Quaternion.identity;

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
		private static void Init()
		{
			var deviceName = "<XRHMD>";
			device = new MockInputDevice(deviceName, XRNode.Head);
			device.SerialNumber = "0.0.1";
			device.Manufacturer = "Needle";
			device.DeviceCharacteristics = InputDeviceCharacteristics.HeadMounted | InputDeviceCharacteristics.TrackedDevice;

			XRInputSubsystem_Patch.SupportedTrackingOriginMode = TrackingOriginModeFlags.Device | TrackingOriginModeFlags.Floor;

			device.AddFeature(new InputFeatureUsage<bool>("isTracked"), () => true);
			device.AddFeature(new InputFeatureUsage<InputTrackingState>("trackingState"), () => InputTrackingState.Position | InputTrackingState.Rotation);
			device.AddFeature(new InputFeatureUsage<Vector3>("devicePosition"), () => Random.insideUnitSphere);
			device.AddFeature(new InputFeatureUsage<Quaternion>("deviceRotation"), () => Random.rotation);

			device.AddFeature(new InputFeatureUsage<Vector3>("leftEyePosition"), () => Random.insideUnitSphere * .3f, XRNode.LeftEye);
			device.AddFeature(new InputFeatureUsage<Quaternion>("leftEyeRotation"), () => Random.rotation, XRNode.LeftEye);
			device.AddFeature(new InputFeatureUsage<Vector3>("rightEyePosition"), () => Random.insideUnitSphere * .3f, XRNode.RightEye);
			device.AddFeature(new InputFeatureUsage<Quaternion>("rightEyeRotation"), () => Random.rotation, XRNode.RightEye);
			device.AddFeature(new InputFeatureUsage<Vector3>("centerEyePosition"), () => Vector3.LerpUnclamped(Vector3.zero, Vector3.up, Mathf.Sin(Time.time)), XRNode.CenterEye);
			device.AddFeature(new InputFeatureUsage<Quaternion>("centerEyeRotation"), () => _rotation, XRNode.CenterEye);
			
			XRInputSubsystem_Patch.RegisterInputDevice(device);
			XRInputSubsystem_Patch.Instance.Start();
		}

		private void Update()
		{
			_rotation = Quaternion.Lerp(Quaternion.Euler(0,-20, 0), Quaternion.Euler(0,20,0), Mathf.Sin(Time.time) * .5f + .5f);
			PrintDeviceList();
		}

		private int lastFrameReceivedEvent;


		[ContextMenu(nameof(PrintDeviceList))]
		private void PrintDeviceList()
		{
			var list = new List<InputDevice>();
			InputDevices.GetDevices(list);
			var headDevice = InputDevices.GetDeviceAtXRNode(XRNode.Head);
			
			XRInputSubsystem_Patch.CurrentTrackingMode = Random.value > .5 ? TrackingOriginModeFlags.Device : TrackingOriginModeFlags.Floor;
			
			if (Text)
			{
				Text.text = "Frame=" + Time.frameCount;
				Text.text += $"\nFound {list.Count} InputDevices";
				Text.text += "\n Has Head device... " + headDevice.name + " = " + headDevice.isValid + ", " + headDevice.manufacturer + ", " +
				             headDevice.serialNumber;

				var val = headDevice.TryGetFeatureValue(new InputFeatureUsage<Vector3>("centerEyePosition"), out var v3);
				var val2 = headDevice.TryGetFeatureValue(new InputFeatureUsage<Quaternion>("centerEyeRotation"), out var rot2);
				Text.text += "\n" + "vec3: " + v3 + " == " + val;
				Text.text += "\n" + "rot: " + val2 + " == " + rot2.eulerAngles;
				if (headDevice.subsystem != null)
				{
					headDevice.subsystem.boundaryChanged += b => lastFrameReceivedEvent = Time.frameCount;
					// var st = headDevice.subsystem.TrySetTrackingOriginMode(Random.value > .5 ? TrackingOriginModeFlags.Floor : TrackingOriginModeFlags.Device);
					// Text.text += "\n" + "set origin mode? " + st + ", subsystem is: " + headDevice.subsystem.GetType();
					Text.text += "\n" + "tracking mode: " + headDevice.subsystem.GetTrackingOriginMode();
					Text.text += "\n" + "last boundary event frame " + lastFrameReceivedEvent;
					Text.text += "\n" + "supported tracking mode: " + headDevice.subsystem.GetSupportedTrackingOriginModes();
				}


				var subsystems = new List<ISubsystem>();
				SubsystemManager.GetInstances(subsystems);
				Text.text += "\nSubsystems?: " + subsystems.Count + "\n" + string.Join("\n", subsystems);
			}

			Debug.Log("InputDevices: " + list.Count + "\n" + string.Join("\n", list.Select(e => { return e.name; })));
		}
	}
}