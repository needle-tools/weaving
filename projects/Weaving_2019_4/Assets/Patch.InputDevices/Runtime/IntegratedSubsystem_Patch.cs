using System;
using needle.Weaver;
using UnityEngine;
using UnityEngine.Subsystems;
using UnityEngine.XR;

#pragma warning disable 108,114

namespace needle.Weavers.InputDevicesPatch
{
	// TODO: figure out why patching base types does not work yet
	
	[NeedlePatch(typeof(IntegratedSubsystem))]
	internal class IntegratedSubsystem_Patch : IntegratedSubsystem
	{
		// private bool isRunning;
		// internal ISubsystemDescriptor m_SubsystemDescriptor;
		//
		// public void Start()
		// {
		// 	Debug.Log("Starting " + this);
		// 	isRunning = true;
		// 	m_SubsystemDescriptor = new XRInputSubsystemDescriptor();
		// }
		//
		// public void Stop()
		// {
		// 	isRunning = false;
		// }
		//
		// public void Destroy()
		// {
		// 	isRunning = false;
		// }
		//
		//
		// internal bool valid => true;
		//
		// internal bool IsRunning() => isRunning;
	}
}