using System.Collections.Generic;
using needle.Weaver;
using UnityEngine.XR;

namespace needle.Weavers.InputDevicesPatch
{
	[NeedlePatch(typeof(InputTracking))]
	public class InputTracking_Patch
	{
		public static void Recenter()
		{
			
		}

		public static void GetNodeStates_Internal(List<XRNodeState> nodeStates)
		{
			
		}
	}
}