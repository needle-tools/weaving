#if EDITORPATCHING_INSTALLED

using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;
using Mono.Reflection;
using UnityEngine;
using Instruction = Mono.Cecil.Cil.Instruction;


namespace needle.Weaver
{
	public static class HarmonyExtensions
	{
		public static bool ApplyHarmonyPatches(this MethodDefinition method, ModuleDefinition module, Predicate<HarmonyLib.Patch> allowPatch = null)
		{
			var patched = Harmony.GetAllPatchedMethods();
			MethodBase patchedMethod = null;
			foreach (var m in patched)
			{
				if (m.FullName() == method.FullName)
				{
					patchedMethod = m;
					break;
				}
			}

			if (patchedMethod == null) return false;
			var processor = method.Body.GetILProcessor();
			
			// TODO: support for merging patches (prefix + postfix) see bottom of file
			var info = Harmony.GetPatchInfo(patchedMethod).Postfixes.FirstOrDefault(p => allowPatch == null || allowPatch(p)).PatchMethod;
			var _inst = info.GetInstructions();
			var cecilInst = _inst.ToCecilInstruction();
			processor.Clear();
			method.Body.Variables.Clear();
			for (var index = 0; index < cecilInst.Count; index++)
			{
				var i = cecilInst[index];
				if (i.Operand is MethodReference mr) 
					i.Operand = module.ImportReference(mr);
				else if (i.Operand is VariableDefinition vd)
				{
					method.Body.Variables.Add(vd);
					i.Operand = module.ImportReference(vd.VariableType);
				}
				else if (i.Operand is TypeReference tr)
					i.Operand = module.ImportReference(tr);
				else if (i.Operand is LocalVariableInfo lvi)
				{
					if(!method.HandleLocalVariableDefinition(processor, module, i, ref lvi))
						continue;
				}
				processor.Append(i);
			}
			return true;
		}

		public static bool HandleLocalVariableDefinition(this MethodDefinition method, ILProcessor processor, ModuleDefinition module, Instruction i, ref LocalVariableInfo operand)
		{
			var lt = operand.LocalType;
			Debug.Log("Type is " + lt);
			// TODO: move into InstructionConverter
			var tr = new TypeReference(lt.Namespace, lt.Name, module, module);
			var vd = new VariableDefinition(tr);
			typeof(VariableReference).GetField("index", (BindingFlags)~0).SetValue(vd, operand.LocalIndex);
			method.Body.Variables.Add(vd);
			Debug.Log(tr + "\n" + vd + "\n" + vd.Index + "\n" + vd.IsPinned + "\n" + method.Body.HasVariables +"\n" + i.OpCode);
			processor.Emit(i.OpCode, vd);
			for (var index = 0; index < method.Body.Variables.Count; index++)
			{
				var var = method.Body.Variables[index];
				Debug.Log("variable: " + index + ", " + var + " - " + var.Resolve() + " - " + var.VariableType);
			}
			return false;
		}
	}
}

// var proc = harmony.CreateProcessor(method);
// PatchProcessor processor = this.CreateProcessor(original);
// processor.AddPrefix(prefix);
// processor.AddPostfix(postfix);
// processor.AddTranspiler(transpiler);
// processor.AddFinalizer(finalizer);
// return processor.Patch();


#endif