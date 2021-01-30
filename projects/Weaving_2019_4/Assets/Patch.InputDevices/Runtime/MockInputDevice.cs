using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR;

namespace needle.Weavers.InputDevicesPatch
{
	public class MockInputDevice
	{
		public ulong Id { get; private set; }
		public string Name { get; private set; }
		public string Manufacturer { get; set; }
		public string SerialNumber { get; set; }
		
		public XRNode Node { get; set; }
		public InputDeviceCharacteristics DeviceCharacteristics { get; set; }

		private static ulong _idCounter;

		public MockInputDevice(string name, XRNode node)
		{
			this.Id = _idCounter++;
			this.Name = name;
			this.Node = node;
		}

		public void AddUsage<T>(InputFeatureUsage<T> usage, Func<T> getValue)
		{
			var usg = (InputFeatureUsage) usage;
			if (!_registry.ContainsKey(usg))
				_registry.Add(usg, getValue);
			else _registry[usg] = getValue;
		}


		private Dictionary<InputFeatureUsage, Delegate> _registry = new Dictionary<InputFeatureUsage, Delegate>();
		
		public bool TryGetUsage<T>(string name, out T value)
		{
			// Debug.Log("try Get usage " + name);
			foreach (var kvp in _registry)
			{
				var usage = kvp.Key;
				if (usage.name == name && usage.type == typeof(T))
				{
					if (kvp.Value is Func<T> callback)
					{
						try
						{
							value = callback.Invoke();
							return true;
						}
						catch (Exception e)
						{
							Debug.LogException(e);
						}
					}
				}
			}

			value = default;
			return false;
		}

		public bool TryGetUsages(List<InputFeatureUsage> list)
		{
			list.AddRange(_registry.Keys);
			return true;
		}
	}
}