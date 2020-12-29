#if EDITORPATCHING_INSTALLED

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Mono.Cecil;
using Instruction = Mono.Cecil.Cil.Instruction;

// ReSharper disable SimplifyLinqExpressionUseAll


namespace needle.Weaver
{
	public static class HarmonyExtensions
	{
		private static readonly Harmony harmony = new Harmony("com.needle.weaver");
		
		public static bool ApplyHarmonyPatches(this MethodDefinition method, ModuleDefinition module)
		{
			if (!module.CustomAttributes.Any(c => c.GetType() == typeof(WeaveHarmony)) && !method.CustomAttributes.Any(c => c.GetType() == typeof(WeaveHarmony)))
				return false;
			
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
				
			// TODO: support for merging patches (prefix + postfix) see bottom of file
			// var patchedMethod = Harmony.GetPatchInfo(originalMethod).Postfixes.FirstOrDefault(p => allowPatch == null || allowPatch(p)).PatchMethod;
			// var _inst = patchedMethod.GetInstructions();
			var cecilInst = instructions.ToCecilInstruction(true);
			return method.Write(cecilInst);
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