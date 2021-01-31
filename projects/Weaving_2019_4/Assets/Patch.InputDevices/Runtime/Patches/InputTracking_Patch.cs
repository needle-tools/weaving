using System;
using System.Collections.Generic;
using needle.Weaver;
using UnityEngine;
using UnityEngine.XR;
using Random = UnityEngine.Random;

namespace needle.Weavers.InputDevicesPatch
{
	[NeedlePatch(typeof(InputTracking))]
	public class InputTracking_Patch
	{
		private static void GetNodeStates_Internal(List<XRNodeState> nodeStates)
		{
			var devices = XRInputSubsystem_Patch.InputDevices;
			foreach (var device in devices)
			{
				device.TryGetNodes(nodeStates);
			}
		}

		private static ulong GetDeviceIdAtXRNode(XRNode node)
		{
			return 0;
		}

		internal static void GetDeviceIdsAtXRNode_Internal(XRNode node, List<ulong> deviceIds)
		{
			
		}

		public static void Recenter()
		{
			
		}


		public static string GetNodeName(ulong uniqueId)
		{
			return string.Empty;
		}

		private static void GetLocalPosition_Injected(XRNode node, out Vector3 ret)
		{
			ret = Vector3.zero;
		}

		private static void GetLocalRotation_Injected(XRNode node, out Quaternion ret)
		{
			ret = Quaternion.identity;
		}

	}
}