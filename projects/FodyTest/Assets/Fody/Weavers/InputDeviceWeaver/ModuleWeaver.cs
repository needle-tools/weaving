using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
			for (var index = 0; index < method.Body.Instructions.Count; index++)
			{
				var inst = method.Body.Instructions[index];
				if (inst.ToString()
					.Contains(
						"call System.Void UnityEngine.XR.InputDevices::GetDevices_Internal(System.Collections.Generic.List`1<UnityEngine.XR.InputDevice>)"))
				{
					var rep = Instruction.Create(OpCodes.Call, ModuleDefinition.ImportReference(replacementMethod));
					processor.Replace(index, rep);
				}
			}
			method.LogIL("AFTER PATCHING");
		}

		private void InjectedDeviceList(List<InputDevice> list)
		{
			var dev = new InputDevice();
			list.Add(dev);
		}
		
		
		private MethodInfo replacementMethod;
		
		public ModuleWeaver()
		{
			replacementMethod = GetType()
				.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
				.Where( x => x.Name == nameof(InjectedDeviceList))
				.Single( x =>
				{
					var parameters = x.GetParameters();
					return parameters.Length == 1 &&
					       parameters[0].ParameterType == typeof( List<InputDevice> );
				} );
		}
	}
}