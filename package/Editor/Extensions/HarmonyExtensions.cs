#if EDITORPATCHING_INSTALLED

using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Mono.Cecil;
using Mono.Reflection;
using UnityEngine;


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
			for (var index = 0; index < cecilInst.Count; index++)
			{
				var i = cecilInst[index];
				if (i.Operand is MethodReference mr) 
					i.Operand = module.ImportReference(mr);
				else if (i.Operand is TypeReference tr)
					i.Operand = module.ImportReference(tr);
				processor.Append(i);
			}
			return true;
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