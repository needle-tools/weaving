using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;

namespace _Tests.Weaver_InputDevice
{
    [ExecuteInEditMode]
    public class UsingInputDevice : MonoBehaviour
    {
        public Text Text;
        
        private void OnEnable()
        {
            PrintDeviceList();
        }

        private void Update()
        {
            PrintDeviceList();
        }

        [ContextMenu(nameof(PrintDeviceList))]
        private void PrintDeviceList()
        {
            var list = new List<InputDevice>();
            InputDevices.GetDevices(list);
            if(Text)
                Text.text = $"Found {list.Count} InputDevices";
            Debug.Log("InputDevices: " + list.Count + "\n" + string.Join("\n", list.Select(e =>
            {
                return e.name;
            })));
        }
    }
}
