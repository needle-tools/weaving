#if EDITORPATCHING_INSTALLED

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using Mono.Collections.Generic;
using Mono.Reflection;
using UnityEngine;
using UnityEngine.XR;
using Instruction = Mono.Cecil.Cil.Instruction;


namespace needle.Weaver
{
	public static class HarmonyExtensions
	{
		private static Harmony harmony = new Harmony("com.needle.weaver");
		
		// https://stackoverflow.com/questions/940675/getting-a-delegate-from-methodinfo
		public static Delegate CreateDelegate(this MethodInfo methodInfo, object target) {
			Func<Type[], Type> getType;
			var isAction = methodInfo.ReturnType.Equals((typeof(void)));
			var types = methodInfo.GetParameters().Select(p => p.ParameterType);

			if (isAction) {
				getType = Expression.GetActionType;
			}
			else {
				getType = Expression.GetFuncType;
				types = types.Concat(new[] { methodInfo.ReturnType });
			}

			if (methodInfo.IsStatic) {
				return Delegate.CreateDelegate(getType(types.ToArray()), methodInfo);
			}

			return Delegate.CreateDelegate(getType(types.ToArray()), target, methodInfo.Name);
		}
		
		public static bool ApplyHarmonyPatches(this MethodDefinition method, ModuleDefinition module, Predicate<HarmonyLib.Patch> allowPatch = null)
		{
			var patched = Harmony.GetAllPatchedMethods();
			MethodBase originalMethod = null;
			foreach (var m in patched)
			{
				if (m.FullName() == method.FullName)
				{
					originalMethod = m;
					break;
				}
			}

			if (originalMethod == null) return false;
			
			
			var patches = Harmony.GetPatchInfo(originalMethod);
			
			HarmonyMethod GetMethod(int index, IReadOnlyList<Patch> _patches)
			{
				return _patches != null && _patches.Count > index ? new HarmonyMethod(_patches[index].PatchMethod) : null;
			}

			IEnumerable<CodeInstruction> instructions = null;
			void OnReceiveInstructions(IEnumerable<CodeInstruction> inst) => instructions = inst;
			PatchFunctions.HasFinalInstructions += OnReceiveInstructions;
			
			var patchedMethod = harmony.Patch(originalMethod, 
				GetMethod(0, patches.Prefixes),
				GetMethod(0, patches.Postfixes),
				GetMethod(0, patches.Transpilers),
				GetMethod(0, patches.Finalizers)
				);
			
			PatchFunctions.HasFinalInstructions -= OnReceiveInstructions;
			
			harmony.Unpatch(originalMethod, HarmonyPatchType.All, harmony.Id);
				
			
			var processor = method.Body.GetILProcessor();
			
			
			
			// TODO: support for merging patches (prefix + postfix) see bottom of file
			// var patchedMethod = Harmony.GetPatchInfo(originalMethod).Postfixes.FirstOrDefault(p => allowPatch == null || allowPatch(p)).PatchMethod;
			// var _inst = patchedMethod.GetInstructions();
			var cecilInst = instructions.ToCecilInstruction(true);
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
					// i.Operand = module.ImportReference(vd.VariableType);
				}
				else if (i.Operand is TypeReference tr)
					i.Operand = module.ImportReference(tr);
				else if (i.Operand is LocalVariableInfo lvi)
				{
					if(method.HandleLocalVariableDefinition(processor, module, i, ref lvi))
						continue;
				}
				else if (i.Operand is MethodBase mb)
					i.Operand = module.ImportReference(mb);	
				processor.Append(i);
			}
			
			Debug.Log("---------");
			Debug.Log(method.Body.HasVariables);
			foreach(var var in method.Body.Variables)
				Debug.Log(var);

			// resolve variable definition references (used by harmony)
			foreach (var i in method.Body.Instructions)
			{
				if (i.Operand is int number)
				{
					Debug.Log(i);
					i.Operand = method.Body.Variables[number];
				}
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
			i.Operand = vd;
			processor.Append(i);
			return true;
		}

		public static List<Instruction> ToCecilInstruction(this IEnumerable<CodeInstruction> instructions, bool debugLog = false)
		{
			var list = new List<Instruction>();
			foreach (var inst in instructions)
			{
				var ci = inst.ToCecilInstruction();
				if (debugLog)
					InstructionConverter.TryLog(inst, ci);
				list.Add(ci);
			}

			return list.ResolveLabels(debugLog);
		}
		
		public static Instruction ToCecilInstruction(this CodeInstruction inst)
		{
			return InstructionConverter.ToCecilInstruction(inst.opcode, inst.operand);
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