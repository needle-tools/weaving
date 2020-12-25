using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using UnityEngine;

namespace Fody.InputDeviceWeaver
{
	public class ModuleWeaver : BaseModuleWeaver
	{
		public override void Execute()
		{
			Debug.Log("Executing InputDevice weaver " + ModuleDefinition.Assembly.FullName);
			foreach(var type in ModuleDefinition.Types)
			{
			    foreach ( MethodDefinition method in type.Methods )
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
			Debug.Log("Process " + method.Name);
		}
	}
}