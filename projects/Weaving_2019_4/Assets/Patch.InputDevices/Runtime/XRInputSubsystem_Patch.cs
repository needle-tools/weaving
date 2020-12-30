using System;
using System.Collections.Generic;
using needle.Weaver;
using UnityEngine;
using UnityEngine.XR;
using Random = UnityEngine.Random;

// disable hide member warning
#pragma warning disable 108,114

namespace needle.Weavers.InputDevicesPatch
{
	[NeedlePatch(typeof(XRInputSubsystem))]
	internal class XRInputSubsystem_Patch : XRInputSubsystem
	{
		internal uint GetIndex() => 0;

		public bool TryRecenter()
		{
			return true;
		}

		public TrackingOriginModeFlags _origin;

		public bool TrySetTrackingOriginMode(TrackingOriginModeFlags origin)
		{
			this._origin = origin;
			return true;
		}

		public TrackingOriginModeFlags GetTrackingOriginMode() => _origin;

		public TrackingOriginModeFlags GetSupportedTrackingOriginModes() => (TrackingOriginModeFlags) ~0;

		private bool TryGetBoundaryPoints_AsList(List<Vector3> boundaryPoints)
		{
			boundaryPoints.Clear();
			boundaryPoints.Add(new Vector3(0, 0, 1));
			boundaryPoints.Add(new Vector3(1, 0, 1));
			boundaryPoints.Add(new Vector3(1, 0, 0));
			boundaryPoints.Add(new Vector3(1, 0, 0));
			return true;
		}


		public event Action<XRInputSubsystem> trackingOriginUpdated;
		public event Action<XRInputSubsystem> boundaryChanged;

		private static void InvokeTrackingOriginUpdatedEvent(IntPtr internalPtr)
		{
		}

		private List<ulong> m_DeviceIdsCache;

		internal void TryGetDeviceIds_AsList(List<ulong> deviceIds)
		{
			deviceIds.Add((ulong) (Random.value * 100));
		}
	}

	[NeedlePatch(typeof(IntegratedSubsystem))]
	internal class IntegratedSubsystem_Patch : ISubsystem
	{
		internal IntPtr m_Ptr;
		internal ISubsystemDescriptor m_subsystemDescriptor;
		private bool isRunning;

		public void Start()
		{
			isRunning = true;
		}

		public void Stop()
		{
			isRunning = false;
		}

		public void Destroy()
		{
		}

		/// <summary>
		///   <para>Whether or not the subsystem is running.</para>
		/// </summary>
		public bool running => this.valid && this.Internal_IsRunning();

		internal bool valid => this.m_Ptr != IntPtr.Zero;

		internal bool Internal_IsRunning() => isRunning;
	}
}