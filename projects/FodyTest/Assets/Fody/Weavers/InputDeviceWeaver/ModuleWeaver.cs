using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using _Tests.Weaver_InputDevice;
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
			method.LogIL("BEFORE PATCHING");
			var found = false;
			for (var index = method.Body.Instructions.Count - 1; index >= 0; index--)
			{
				var inst = method.Body.Instructions[index];
				if (inst.ToString().Contains("callvirt System.Void System.Collections.Generic.List`1<UnityEngine.XR.InputDevice>::Clear()"))
				{
					found = true;
					var instructions = GetInstructions(method);
					var current = Instruction.Create(OpCodes.Nop);
					processor.InsertAfter(index, current);
					foreach (var instruction in instructions )
					{
						processor.InsertAfter( current, instruction); 
						current = instruction;
					}
				}

				if (!found) method.Body.Instructions.RemoveAt(index);
			}
			method.LogIL("AFTER PATCHING");
		}

		private List<Instruction> GetInstructions(MethodDefinition method)
		{
			var list = new List<Instruction>();
			list.Add(Instruction.Create( OpCodes.Ldarg_0));
			list.Add(Instruction.Create(OpCodes.Call, ModuleDefinition.ImportReference(replacementMethod)));
			list.Add(Instruction.Create(OpCodes.Nop));
			list.Add(Instruction.Create(OpCodes.Ret));
			return list;
		}
		
		private MethodInfo replacementMethod;
		
		public ModuleWeaver()
		{
			replacementMethod = typeof(FakeInputDeviceAPI)
				.GetMethods((BindingFlags)~0)
				.Where( x => x.Name == "FakeDeviceList")
				.Single( x =>
				{
					var parameters = x.GetParameters();
					return parameters.Length == 1 &&
					       parameters[0].ParameterType == typeof( List<InputDevice> );
				} );
		}
	}
}