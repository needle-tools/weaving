using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;

namespace _Tests.Weaver_InputDevice
{
    [ExecuteInEditMode]
    public class UsingInputDevice : MonoBehaviour
    {
        private void OnEnable()
        {
            PrintDeviceList();
        }

        [ContextMenu(nameof(PrintDeviceList))]
        private void PrintDeviceList()
        {
            var list = new List<InputDevice>();
            InputDevices.GetDevices(list);
            Debug.Log("InputDevices: " + list.Count + "\n" + string.Join("\n", list.Select(e =>
            {
                return e.name;
            })));
        }
    }
}
