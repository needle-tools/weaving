using System;
using needle.Weaver;
using UnityEngine;
#pragma warning disable 108,114

namespace needle.Weavers.InputDevicesPatch
{
	// TODO: figure out why patching base types does not work yet
	
	[NeedlePatch(typeof(IntegratedSubsystem))]
	internal class IntegratedSubsystem_Patch : IntegratedSubsystem
	{
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


		internal bool valid => true;

		internal bool Internal_IsRunning() => isRunning;
	}
}