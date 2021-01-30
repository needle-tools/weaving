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
        public bool createDevice = true;
        public Text Text;

        private MockInputDevice device;

        private void Start()
        {
          	if (!createDevice) return;
        
            var deviceName = "NeedleDevice";
            device = new MockInputDevice(deviceName, XRNode.Head);
            device.SerialNumber = "0.0.42";
            device.Manufacturer = "Needle";
            var start = Camera.main.transform.position;
            device.AddUsage(new InputFeatureUsage<bool>("IsTracked"), () => true);
            device.AddUsage(new InputFeatureUsage<InputTrackingState>("TrackingState"), () => InputTrackingState.Position | InputTrackingState.Rotation);
            device.AddUsage(new InputFeatureUsage<Vector3>("DevicePosition"), () => start + Random.insideUnitSphere);
            device.AddUsage(new InputFeatureUsage<Vector3>("EyePosition"), () => start + Random.insideUnitSphere);
            device.AddUsage(new InputFeatureUsage<Quaternion>("DeviceRotation"), () => Quaternion.identity);
            device.AddUsage(new InputFeatureUsage<Quaternion>("EyeRotation"), () => Quaternion.identity);
            
            
            device.AddUsage(new InputFeatureUsage<bool>("deviceIsTracked"), () => true);
            device.AddUsage(new InputFeatureUsage<InputTrackingState>("deviceTrackingState"), () => InputTrackingState.All);
            device.AddUsage(new InputFeatureUsage<Vector3>("devicePosition"), () => start + Random.insideUnitSphere * .3f);
            device.AddUsage(new InputFeatureUsage<Quaternion>("deviceRotation"), () => Quaternion.identity);
            device.AddUsage(new InputFeatureUsage<Vector3>("centerEyePosition"), () => start + Random.insideUnitSphere * .3f);
            device.AddUsage(new InputFeatureUsage<Quaternion>("centerEyeRotation"), () => Quaternion.identity);
            device.AddUsage(new InputFeatureUsage<Vector3>("leftEyePosition"), () => start + Random.insideUnitSphere * .3f);
            device.AddUsage(new InputFeatureUsage<Quaternion>("leftEyeRotation"), () => Quaternion.identity);
            device.AddUsage(new InputFeatureUsage<Vector3>("rightEyePosition"), () => start + Random.insideUnitSphere * .3f);
            device.AddUsage(new InputFeatureUsage<Quaternion>("rightEyeRotation"), () => Quaternion.identity);
            device.DeviceCharacteristics = InputDeviceCharacteristics.TrackedDevice | InputDeviceCharacteristics.HeadMounted | InputDeviceCharacteristics.Camera;
            XRInputSubsystem_Patch.RegisterInputDevice(device);
        }

        private void Update()
        {
            PrintDeviceList();
        }

        private int lastFrameReceivedEvent;


        [ContextMenu(nameof(PrintDeviceList))]
        private void PrintDeviceList()
        {
            var list = new List<InputDevice>();
            InputDevices.GetDevices(list);
            var headDevice = InputDevices.GetDeviceAtXRNode(XRNode.Head);
            if (Text)
            {
                Text.text = "Frame=" + Time.frameCount;
                Text.text += $"\nFound {list.Count} InputDevices";
                Text.text += "\n Has Head device... " + headDevice.name + " = " + headDevice.isValid + ", " + headDevice.manufacturer + ", " + headDevice.serialNumber;
      
                var val = headDevice.TryGetFeatureValue(new InputFeatureUsage<Vector3>("centerEyePosition"), out var v3);
                Text.text += "\n" + "vec3: " + v3 + " == " + val;
                if (headDevice.subsystem != null)
                {
                    headDevice.subsystem.boundaryChanged += b => lastFrameReceivedEvent = Time.frameCount;
                    // var st = headDevice.subsystem.TrySetTrackingOriginMode(Random.value > .5 ? TrackingOriginModeFlags.Floor : TrackingOriginModeFlags.Device);
                    // Text.text += "\n" + "set origin mode? " + st + ", subsystem is: " + headDevice.subsystem.GetType();
                    Text.text += "\n" + "internal tracking mode: " + (headDevice.subsystem as XRInputSubsystem_Patch)?._origin.ToString();
                    Text.text += "\n" + "last boundary event frame " + lastFrameReceivedEvent;
                    Text.text += "\n" + "supported tracking mode: " + headDevice.subsystem.GetSupportedTrackingOriginModes();
                    
                }


                var subsystems = new List<ISubsystem>();
                SubsystemManager.GetInstances(subsystems);
                Text.text += "\nSubsystems?: " + subsystems.Count + "\n" + string.Join("\n", subsystems);

            }
            Debug.Log("InputDevices: " + list.Count + "\n" + string.Join("\n", list.Select(e =>
            {
                return e.name;
            })));
        }
		
    }
}
