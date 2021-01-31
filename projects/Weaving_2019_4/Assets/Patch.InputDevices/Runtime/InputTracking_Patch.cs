using System.Collections.Generic;
using needle.Weaver;
using UnityEngine;
using UnityEngine.XR;

namespace needle.Weavers.InputDevicesPatch
{
	[NeedlePatch(typeof(InputTracking))]
	public class InputTracking_Patch
	{
		private static void GetNodeStates_Internal(List<XRNodeState> nodeStates)
		{
			var ns = new XRNodeState();
			ns.nodeType = XRNode.CenterEye;
			ns.position = Random.insideUnitSphere;
			ns.rotation = Random.rotation;
			nodeStates.Add(ns);
			
			ns = new XRNodeState();
			ns.nodeType = XRNode.LeftEye;
			ns.position = Random.insideUnitSphere;
			ns.rotation = Random.rotation;
			nodeStates.Add(ns);
			
			ns = new XRNodeState();
			ns.nodeType = XRNode.RightEye;
			ns.position = Random.insideUnitSphere;
			ns.rotation = Random.rotation;
			nodeStates.Add(ns);
			
			ns = new XRNodeState();
			ns.nodeType = XRNode.LeftHand;
			ns.position = Random.insideUnitSphere;
			ns.rotation = Random.rotation;
			nodeStates.Add(ns);
			
			ns = new XRNodeState();
			ns.nodeType = XRNode.RightHand;
			ns.position = Random.insideUnitSphere;
			ns.rotation = Random.rotation;
			nodeStates.Add(ns);
		}
	}
}