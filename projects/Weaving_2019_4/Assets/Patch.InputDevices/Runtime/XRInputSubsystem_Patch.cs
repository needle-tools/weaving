using System;
using System.Collections.Generic;
using System.Linq;
using needle.Weaver;
using UnityEngine;
using UnityEngine.XR;

// disable hide member warning
#pragma warning disable 108,114

namespace needle.Weavers.InputDevicesPatch
{
	[NeedlePatch(typeof(XRInputSubsystem))]
	internal class XRInputSubsystem_Patch : XRInputSubsystem
	{
		private static readonly Lazy<XRInputSubsystem_Patch> _instance = new Lazy<XRInputSubsystem_Patch>(() => new XRInputSubsystem_Patch());
		public static XRInputSubsystem_Patch Instance => _instance.Value;

		public static TrackingOriginModeFlags SupportedTrackingOriginMode = TrackingOriginModeFlags.Floor;
		public static TrackingOriginModeFlags CurrentTrackingMode = TrackingOriginModeFlags.Device;

		public uint Index { get; set; }

		public static void RegisterInputDevice(MockInputDevice dev)
		{
			if (dev == null) return;
			if (!InputDevices.Contains(dev))
			{
				InputDevices.Add(dev);
				Debug.Log("Registered input device " + dev.Id + " - " + dev.Node);
			}
		}
		
		internal static readonly List<MockInputDevice> InputDevices = new List<MockInputDevice>();

		public static MockInputDevice TryGetDevice(ulong id) => InputDevices.FirstOrDefault(d => d.Id == id);


		
		public List<Vector3> Bounds = new List<Vector3>()
		{
			new Vector3(0, 0, 1),
			new Vector3(1, 0, 1), 
			new Vector3(1, 0, 0), 
			new Vector3(0, 0, 1)
		};

		
		private readonly Lazy<XRInputSubsystemDescriptor> descriptor = new Lazy<XRInputSubsystemDescriptor>(() =>
		{
			var desc = new XRInputSubsystemDescriptor();
			Debug.Log("Creating a subsystem descriptor");
			// desc.Create();
			return desc;
		});
		
		// implementation:


		public XRInputSubsystemDescriptor subsystemDescriptor => descriptor.Value;

		
		internal uint GetIndex() => Index;

		public void Start()
		{
			Debug.Log("Starting");
		}

		public void Stop()
		{
			Debug.Log("Stopping");
		}
		
		public void Destroy()
		{
			
		}
		
		internal void TryGetDeviceIds_AsList(List<ulong> deviceIds)
		{
			foreach (var dev in InputDevices) 
				deviceIds.Add(dev.Id);
		}

		public bool TryRecenter()
		{
			return true;
		}


		public bool TrySetTrackingOriginMode(TrackingOriginModeFlags origin)
		{
			CurrentTrackingMode = origin;
			return true;
		}

		public TrackingOriginModeFlags GetTrackingOriginMode() => CurrentTrackingMode;

		public TrackingOriginModeFlags GetSupportedTrackingOriginModes() => SupportedTrackingOriginMode;

		private bool TryGetBoundaryPoints_AsList(List<Vector3> boundaryPoints)
		{
			boundaryPoints.Clear();
			boundaryPoints.AddRange(boundaryPoints);
			return true;
		}


		public event Action<XRInputSubsystem> trackingOriginUpdated;
		public event Action<XRInputSubsystem> boundaryChanged;

		private static void InvokeTrackingOriginUpdatedEvent(IntPtr internalPtr)
		{
			// Instance.trackingOriginUpdated?.Invoke(internalPtr);
		}
		
		private static void InvokeBoundaryChangedEvent(IntPtr internalPtr)
		{
			// Instance.boundaryChanged?.Invoke(Instance);
		}


	}
}