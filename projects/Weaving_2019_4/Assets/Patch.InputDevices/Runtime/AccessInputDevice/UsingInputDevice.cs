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
        
        private void Update()
        {
            PrintDeviceList();
        }

        [ContextMenu(nameof(PrintDeviceList))]
        private void PrintDeviceList()
        {
            var list = new List<InputDevice>();
            InputDevices.GetDevices(list);
            var headDevice = InputDevices.GetDeviceAtXRNode(XRNode.Head);
            if (Text)
            {
                Text.text = $"Found {list.Count} InputDevices";
                Text.text += "\n Has Head device... " + headDevice.name + " = " + headDevice.isValid;
                var val = headDevice.TryGetFeatureValue(new InputFeatureUsage<float>("test"), out var res);
                Text.text += "\n" + "float: " + res + " == " + val;
                val = headDevice.TryGetFeatureValue(new InputFeatureUsage<Vector3>("test"), out var v3);
                Text.text += "\n" + "vec3: " + v3 + " == " + val;
                if (headDevice.subsystem != null)
                {
                    var st = headDevice.subsystem.TrySetTrackingOriginMode(TrackingOriginModeFlags.Floor);
                    Text.text += "\n" + "set origin mode? " + st + ", subsystem is: " + headDevice.subsystem.GetType();
                    Text.text += "\n" + "internal tracking mode: " + (headDevice.subsystem as XRInputSubsystemPatch)?._origin.ToString();
                }
            }
            Debug.Log("InputDevices: " + list.Count + "\n" + string.Join("\n", list.Select(e =>
            {
                return e.name;
            })));
        }
		
    }
}
