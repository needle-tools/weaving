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

        private MockInputDevice device;

        private void Start()
        {
            device = new MockInputDevice("MyInputDevice", XRNode.Head);
            device.Manufacturer = "Needle";
            device.AddUsage(new InputFeatureUsage<float>("test"), () => Random.value);
            device.AddUsage(new InputFeatureUsage<Vector3>("test"), () => Random.insideUnitSphere);
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
                Text.text += "\n Has Head device... " + headDevice.name + " = " + headDevice.isValid + ", " + headDevice.manufacturer;
                var val = headDevice.TryGetFeatureValue(new InputFeatureUsage<float>("test"), out var res);
                Text.text += "\n" + "float: " + res + " == " + val;
                val = headDevice.TryGetFeatureValue(new InputFeatureUsage<Vector3>("test"), out var v3);
                Text.text += "\n" + "vec3: " + v3 + " == " + val;
                if (headDevice.subsystem != null)
                {
                    headDevice.subsystem.boundaryChanged += b => lastFrameReceivedEvent = Time.frameCount;
                    var st = headDevice.subsystem.TrySetTrackingOriginMode(Random.value > .5 ? TrackingOriginModeFlags.Floor : TrackingOriginModeFlags.Device);
                    Text.text += "\n" + "set origin mode? " + st + ", subsystem is: " + headDevice.subsystem.GetType();
                    Text.text += "\n" + "internal tracking mode: " + (headDevice.subsystem as XRInputSubsystem_Patch)?._origin.ToString();
                    Text.text += "\n" + "last boundary event frame " + lastFrameReceivedEvent;
                }
            }
            Debug.Log("InputDevices: " + list.Count + "\n" + string.Join("\n", list.Select(e =>
            {
                return e.name;
            })));
        }
		
    }
}
