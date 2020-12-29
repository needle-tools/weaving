using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Reflection;
using UnityEngine;
using Instruction = Mono.Cecil.Cil.Instruction;

namespace needle.Weaver
{
	public static class InstructionWriter
	{
		public static bool Write(this MethodDefinition method, MethodInfo patch, bool debugLog = false)
		{
			var il = debugLog ? method.CaptureILString() : null;
			var instructions = patch.GetInstructions().ToCecilInstruction();
			if (method.Write(instructions))
			{
				if (debugLog)
				{
					Debug.Log($"<b>Applied patch</b> to: {method.FullName} \n" +
					          $"BEFORE: \n" +
					          $"{il} \n" +
					          $"AFTER: \n" +
					          $"{method.CaptureILString()}");
				}
				return true;
			}
			return false;
		}

		public static bool Write(this MethodDefinition method, IEnumerable<Instruction> instructions)
		{
			if (method == null) return false;
			if (!method.HasBody) return false;
			if (instructions == null) return false;

			var module = method.Module;
			var processor = method.Body.GetILProcessor();
			var _instructions = instructions.ToArray();

			processor.Clear();
			method.Body.Variables.Clear();

			for (var index = 0; index < _instructions.Length; index++)
			{
				var i = _instructions[index];
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
					if (method.HandleLocalVariableDefinition(processor, module, i, ref lvi))
						continue;
				}
				else if (i.Operand is MethodBase mb)
					i.Operand = module.ImportReference(mb);

				processor.Append(i);
			}


			// resolve parameter variable references (used by harmony)
			foreach (var i in method.Body.Instructions)
			{
				bool isLoadOp() => i.OpCode == OpCodes.Ldarg || i.OpCode == OpCodes.Ldarg_0 || i.OpCode == OpCodes.Ldarg_1 || i.OpCode == OpCodes.Ldarg_2 ||
				                   i.OpCode == OpCodes.Ldarg_3 || i.OpCode == OpCodes.Ldarga || i.OpCode == OpCodes.Ldarg_S || i.OpCode == OpCodes.Ldarga_S;

				if (i.Operand is int number && isLoadOp())
				{
					var pr = new VariableDefinition(method.Parameters[number].ParameterType);
					method.Body.Variables.Add(pr);
					i.Operand = pr;
				}
			}

			return true;
		}

		public static bool HandleLocalVariableDefinition(this MethodDefinition method, ILProcessor processor, ModuleDefinition module, Instruction i,
			ref LocalVariableInfo operand)
		{
			var lt = operand.LocalType;
			// Debug.Log("Type is " + lt);
			// TODO: move into InstructionConverter
			var tr = new TypeReference(lt.Namespace, lt.Name, module, module);
			var vd = new VariableDefinition(tr);
			typeof(VariableReference).GetField("index", (BindingFlags) ~0).SetValue(vd, operand.LocalIndex);
			method.Body.Variables.Add(vd);
			// Debug.Log(tr + "\n" + vd + "\n" + vd.Index + "\n" + vd.IsPinned + "\n" + method.Body.HasVariables +"\n" + i.OpCode);
			i.Operand = vd;
			processor.Append(i);
			return true;
		}
	}
}