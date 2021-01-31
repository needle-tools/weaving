using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.XR;
using Debug = UnityEngine.Debug;

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

		public void AddFeature<T>(InputFeatureUsage<T> usage, Func<T> getValue, XRNode node = (XRNode) (-1))
		{
			var usg = (InputFeatureUsage) usage;
			if (!_registry.ContainsKey(usg))
				_registry.Add(usg, getValue);
			else
			{
				_registry[usg] = getValue;
			}

			if (node != (XRNode) (-1))
			{
				if (!_nodes.ContainsKey(node)) _nodes.Add(node, new List<Delegate>());
				_nodes[node].Add(getValue);
			}
		}

		private readonly Dictionary<XRNode, List<Delegate>> _nodes = new Dictionary<XRNode, List<Delegate>>();
		private readonly Dictionary<InputFeatureUsage, Delegate> _registry = new Dictionary<InputFeatureUsage, Delegate>();


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

		public void TryGetNodes(List<XRNodeState> nodes)
		{
			var state = new XRNodeState()
			{
				nodeType = Node,
				uniqueID = this.Id,
			};
			nodes.Add(state);
			// TODO: this architecture does not support velocity etc......
			foreach (var node in _nodes)
			{
				state.nodeType = node.Key;
				foreach (var del in node.Value)
				{
					try
					{
						var val = del.DynamicInvoke();
						switch (val)
						{
							case Vector3 vec:
								state.position = vec;
								break;
							case Quaternion rot:
								state.rotation = rot;
								break;
						}
					}
					catch (Exception e)
					{
						Debug.LogError(e);
					}
				}
				nodes.Add(state);
			}
		}
	}
}