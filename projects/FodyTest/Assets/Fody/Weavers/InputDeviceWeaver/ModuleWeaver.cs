using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using UnityEngine;
using UnityEngine.XR;

namespace Fody.Weavers.InputDeviceWeaver
{
	public class ModuleWeaver : BaseModuleWeaver
	{
		public override void Execute()
		{
			Debug.Log("Executing InputDevice weaver " + ModuleDefinition.Assembly.FullName);
			foreach(var type in ModuleDefinition.Types)
			{
			    foreach (var method in type.Methods)
			    {
			        ProcessMethod( method );
			    }
			}
		}

		public override IEnumerable<string> GetAssembliesForScanning()
		{
			yield break;
		}

		private void ProcessMethod(MethodDefinition method)
		{
			if (method.Name != "GetDevices") return;
			Debug.Log("Process " + method.Name);
			ILProcessor processor = method.Body.GetILProcessor();
			Instruction current = method.Body.Instructions.First();
			Debug.Log("Instructions? " + method.Body.Instructions.Count);
			foreach (var instru in method.Body.Instructions)
			{
				Debug.Log(instru);
			}
		}

		private void InjectedDeviceList(List<InputDevice> list)
		{
			var dev = new InputDevice();
			list.Add(dev);
		}
	}
}