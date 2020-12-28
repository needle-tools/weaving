using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using _Tests.Weaver_InputDevice;
using HarmonyLib;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using Mono.Reflection;
using needle.Weaver;
using UnityEngine;
using UnityEngine.XR;
using Instruction = Mono.Cecil.Cil.Instruction;
using MethodBody = Mono.Cecil.Cil.MethodBody;

namespace Fody.Weavers.InputDeviceWeaver
{
	public class ModuleWeaver : BaseModuleWeaver
	{
		public override void Execute()
		{
			Debug.Log("Executing InputDevice weaver " + ModuleDefinition.Assembly.FullName);
			foreach(var type in ModuleDefinition.Types)
			{
				// Debug.Log(type.FullName);
				if (type.Name != "InputDevices") continue;
			    foreach (var method in type.Methods)
			    {
				    try
				    {
					    ProcessMethod(method);
				    }
				    catch (Exception e)
				    {
					    Debug.LogException(e);
				    }
			    }
			}
		}

		public override IEnumerable<string> GetAssembliesForScanning()
		{
			yield return "netstandard";
			yield return "mscorlib";
			yield return "System";
		}

		private void ProcessMethod(MethodDefinition method)
		{
			if (method.Name == "GetDevices")
			{
				FixGetDevices(method);
			}
			
			return;
			
			if 
				(!method.HasBody)
			{
				method.LogIL("BEFORE PATCHING " + method.Name);
				method.IsManaged = true;
				method.IsIL = true;
				method.IsNative = false;
				method.PInvokeInfo = null;
				method.IsInternalCall = false;
				method.IsPInvokeImpl = false;
				method.NoInlining = true;
				method.Body = new MethodBody(method); 
				var processor = method.Body?.GetILProcessor();
				var mrt = method.MethodReturnType;
				if (mrt != null)
				{
					var rt = mrt.ReturnType;
					// TODO: figure out how to add mscore lib to resolve rt.Resolve() to get TypeDefinition?!
					// Debug.Log(rt.Name);
					// TODO: figure out better way to get return types
					var tempVar = new VariableDefinition(rt);
					method.Body.Variables.Add(tempVar);
					// Debug.Log("tempvar? " + tempVar.VariableType);
					switch (rt.Name)
					{
						case "String":
							processor.Emit(OpCodes.Ldstr, "");
							break;
						default:
							processor.Emit(OpCodes.Ldloc, tempVar);
							break;
						case "Void":
							break;
					}
				}
				processor.Append(Instruction.Create(OpCodes.Ret));
				method.LogIL("AFTER PATCHING "  + method.Name);
			}
		}

		private void FixGetDevices(MethodDefinition method)
		{
			method.LogIL("BEFORE PATCHING " + method.Name);
			Debug.Log(method.FullName);
			var patched = Harmony.GetAllPatchedMethods();
			MethodBase patchedMethod = null;
			foreach (var m in patched)
			{
				Debug.Log(m.FullName());
				if (m.FullName() == method.FullName)
				{
					patchedMethod = m;
					break;
				}
			}
			
			if (patchedMethod == null) throw new Exception("Missing patched method to apply");
			var processor = method.Body.GetILProcessor();
			
			var info = Harmony.GetPatchInfo(patchedMethod).Prefixes.FirstOrDefault().PatchMethod;
			var _inst = info.GetInstructions();
			var cecilInst = _inst.ToCecilInstruction();
			processor.Clear();
			for (var index = 0; index < cecilInst.Count; index++)
			{
				var i = cecilInst[index];
				if (i.Operand is MethodReference mr)
				{
					Debug.Log("import " + mr);
					// ModuleDefinition.ImportReference(replacementMethod);
					ModuleDefinition.ImportReference(mr);
				}
				processor.Append(i);
				if (i.OpCode.FlowControl == FlowControl.Call)
				{
					// ModuleDefinition.ImportReference(i.Operand);
					var expected = Instruction.Create(OpCodes.Call, ModuleDefinition.ImportReference(replacementMethod));
					Debug.Log("COMPARE: \n" + _inst[index] + " <- mono reflection\n" + i + " <- cur \n"  + expected + " <- expected \n");
					Debug.Log("Mono.Reflection: " + _inst[index]);
				}
			}

			// patch.PatchMethod.GetInstructions().ToCecilInstruction(true)
				
			// var found = false;
			// for (var index = method.Body.Instructions.Count - 1; index >= 0; index--)
			// {
			// 	var inst = method.Body.Instructions[index];
			// 	if (inst.ToString().Contains("callvirt System.Void System.Collections.Generic.List`1<UnityEngine.XR.InputDevice>::Clear()"))
			// 	{
			// 		found = true;
			// 		var instructions = GetInstructions(method);
			// 		var current = Instruction.Create(OpCodes.Nop);
			// 		processor.InsertAfter(index, current);
			// 		foreach (var instruction in instructions )
			// 		{
			// 			processor.InsertAfter( current, instruction); 
			// 			current = instruction;
			// 		}
			// 	}
			//
			// 	if (!found) method.Body.Instructions.RemoveAt(index);
			// }
			method.LogIL("AFTER PATCHING "  + method.Name);
		}

		// private void Compare(Mono.Reflection.Instruction mono, Instruction cecil, Instruction expected)
		// {
		// 	var str = "";
		// 	str += mono.Operand
		// }

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