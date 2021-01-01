// using System;
// using System.Collections.Generic;
// using Mono.Cecil;
// using needle.Weaver;
// using UnityEngine;
//
// namespace Fody.Weavers.InputDeviceWeaver
// {
// 	public class WeaveHarmonyPatches : BaseModuleWeaver
// 	{
// 		public override void Execute()
// 		{
// 			foreach(var type in ModuleDefinition.Types)
// 			{
// 			    foreach (var method in type.Methods)
// 			    {
// 				    try
// 				    {
// 					    ProcessMethod(method);
// 				    }
// 				    catch (Exception e)
// 				    {
// 					    Debug.LogException(e);
// 				    }
// 			    }
// 			}
// 		}
//
// 		public override IEnumerable<string> GetAssembliesForScanning()
// 		{
// 			yield return "netstandard";
// 			yield return "mscorlib";
// 			yield return "System";
// 		}
//
// 		private void ProcessMethod(MethodDefinition method)
// 		{
// 			var il = method.CaptureILString();
// 			if (method.ApplyHarmonyPatches(ModuleDefinition))
// 			{
// 				Debug.Log($"<b>Applied harmony patch</b> to: {method.FullName} \n" +
// 				          $"BEFORE: \n" +
// 				          $"{il} \n" +
// 				          $"AFTER: \n" +
// 				          $"{method.CaptureILString()}"
// 				          );
// 			}
// 		}
//
//
//
// 		// patch.PatchMethod.GetInstructions().ToCecilInstruction(true)
// 				
// 		// var found = false;
// 		// for (var index = method.Body.Instructions.Count - 1; index >= 0; index--)
// 		// {
// 		// 	var inst = method.Body.Instructions[index];
// 		// 	if (inst.ToString().Contains("callvirt System.Void System.Collections.Generic.List`1<UnityEngine.XR.InputDevice>::Clear()"))
// 		// 	{
// 		// 		found = true;
// 		// 		var instructions = GetInstructions(method);
// 		// 		var current = Instruction.Create(OpCodes.Nop);
// 		// 		processor.InsertAfter(index, current);
// 		// 		foreach (var instruction in instructions )
// 		// 		{
// 		// 			processor.InsertAfter( current, instruction); 
// 		// 			current = instruction;
// 		// 		}
// 		// 	}
// 		//
// 		// 	if (!found) method.Body.Instructions.RemoveAt(index);
// 		// }
// 		// private void Compare(Mono.Reflection.Instruction mono, Instruction cecil, Instruction expected)
// 		// {
// 		// 	var str = "";
// 		// 	str += mono.Operand
// 		// }
//
// 		// private List<Instruction> GetInstructions(MethodDefinition method)
// 		// {
// 		// 	var list = new List<Instruction>();
// 		// 	list.Add(Instruction.Create( OpCodes.Ldarg_0));
// 		// 	list.Add(Instruction.Create(OpCodes.Call, ModuleDefinition.ImportReference(replacementMethod)));
// 		// 	list.Add(Instruction.Create(OpCodes.Nop));
// 		// 	list.Add(Instruction.Create(OpCodes.Ret));
// 		// 	return list;
// 		// }
// 		//
// 		// private MethodInfo replacementMethod;
// 		//
// 		// public WeaveHarmonyPatches()
// 		// {
// 		// 	replacementMethod = typeof(InputDevices)
// 		// 		.GetMethods((BindingFlags)~0)
// 		// 		.Where( x => x.Name == "FakeDeviceList")
// 		// 		.Single( x =>
// 		// 		{
// 		// 			var parameters = x.GetParameters();
// 		// 			return parameters.Length == 1 &&
// 		// 			       parameters[0].ParameterType == typeof( List<InputDevice> );
// 		// 		} );
// 		// }
//
// 	}
// }