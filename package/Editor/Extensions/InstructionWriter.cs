using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using Mono.Reflection;
using UnityEngine;
using Instruction = Mono.Cecil.Cil.Instruction;
using ParameterAttributes = Mono.Cecil.ParameterAttributes;

namespace needle.Weaver
{
	public static class InstructionWriter
	{
		public static bool Write(this MethodDefinition method, MemberInfo patch, bool debugLog = false)
		{
			var il = string.Empty;

			if (debugLog)
			{
				try
				{
					il += method.CaptureILString() + "\n";
				}
				catch (Exception e)
				{
					Debug.LogWarning(e);
				}
			}

			var patchType = patch.ReflectedType;
			var patchFullName = patchType.FullName + "." + patch.Name;
			using (var assembly = AssemblyDefinition.ReadAssembly(patchType?.Assembly.Location))
			{
				var methods = assembly.MainModule.GetTypes().SelectMany(t => t.Methods);
				foreach (var pm in methods)
				{
					var methodFullName = pm.DeclaringType.FullName + "." + pm.Name;
					if (methodFullName == patchFullName)
					{
						// TODO: ensure parameter and generics match 
						
						var module = method.Module;

						// foreach (var param in method.Parameters) Debug.Log(param);
						// if (!method.IsStatic)
						// {	
						// 	method.Parameters.Add(new ParameterDefinition("this", ParameterAttributes.None, new TypeReference(method.DeclaringType.Namespace, method.DeclaringType.Name, module, module)));
						// }
						
						method.Body.Variables.Clear();
						foreach(var v in pm.Body.Variables){
							var nv = new VariableDefinition(module.ImportReference(v.VariableType));
							method.Body.Variables.Add(nv);
						}
						
						var ip = method.Body.GetILProcessor();
						ip.Clear();
						foreach (var inst in pm.Body.Instructions)
						{
							switch (inst.Operand)
							{
								case FieldInfo fi:
									inst.Operand = module.ImportReference(fi);
									ResolveReferencesToSelf(method, patch, inst);
									break;
								case FieldReference fr:
									inst.Operand = module.ImportReference(fr);
									ResolveReferencesToSelf(method, patch, inst);
									break;
								case MethodBase mb:
									inst.Operand = module.ImportReference(mb);
									ResolveReferencesToSelf(method, patch, inst);
									break;
								case MethodReference mr:
									inst.Operand = module.ImportReference(mr);
									ResolveReferencesToSelf(method, patch, inst);
									break;
								case Type t:
									inst.Operand = module.ImportReference(t);
									ResolveReferencesToSelf(method, patch, inst);
									break;
								case TypeReference tr:
									inst.Operand = module.ImportReference(tr);
									ResolveReferencesToSelf(method, patch, inst);
									break;
							}

							ip.Append(inst);
						}

						method.Body.Optimize();

						if (debugLog)
						{
							try
							{
								Debug.Log($"<b>Applied patch</b> to: {method.FullName} \n" +
								          $"BEFORE: \n" +
								          $"{il} \n" +
								          $"AFTER: \n" +
								          $"{method.CaptureILString()}");
							}
							catch (Exception e)
							{
								Debug.LogWarning(e);
							}
						}

						return true;
					}
				}
			}

			return false;
		}

		private static void ResolveReferencesToSelf(MethodDefinition method, MemberInfo patch, Instruction instruction)
		{
			if (instruction.Operand == null) return;
			switch (instruction.Operand)
			{
				case FieldInfo fi:
					Debug.Log("FI: " + fi + "\n" + fi.DeclaringType);
					break;
				case FieldReference fr:
					var dc = fr.DeclaringType;
					Debug.Log("FIELD: " + fr + "\n" + dc + "\n" + dc.FullName);
					if (dc?.FullName == patch.DeclaringType?.FullName)
					{
						var possibleMatch = method.DeclaringType.Fields.FirstOrDefault(f => f.Name == fr.Name);
						if (possibleMatch != null)
						{
							Debug.Log(possibleMatch);
							instruction.Operand = possibleMatch;
						}
					}
					break;
				case MethodBase mb:
					break;
				case MethodReference mr:
					// Debug.Log("METHOD REFERENCE " + mr + "\nhas this? " + mr.HasThis);
					if (mr.HasThis)
					{
						var possibleMatches = method.DeclaringType.Methods.Where(m => m.Name == mr.Name).Where(m => m.Parameters.Count == mr.Parameters.Count);
						// TODO: make sure generic parameters are handled
						foreach(var possible in possibleMatches)
						{
							var mismatch = false;
							for (var index = 0; index < possible.Parameters.Count; index++)
							{
								var par = possible.Parameters[index];
								if (mr.Parameters[index].ParameterType != par.ParameterType)
								{
									mismatch = true;
									break;
								}
							}
							if (mismatch) continue;
							Debug.Log("RESOLVED THIS METHOD REFERENCE: " + possible);
							instruction.Operand = possible;
							break;
						}
					}
					break;
				case Type t:
					break;
				case TypeReference tr:
					break;
			}
		}
		
		// reference https://en.wikipedia.org/wiki/List_of_CIL_instructions

		[Obsolete("call with method to patch for now")]
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
					method.HandleLocalVariableDefinition(processor, module, i, ref lvi);
				}
				else if (i.Operand is MethodBase mb)
					i.Operand = module.ImportReference(mb);

				processor.Append(i);

				try
				{
					EnsureVariables(i, method);
				}
				catch (Exception e)
				{
					Debug.LogError(e + "\n\n" + method.CaptureILString());
				}
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

		private static void EnsureVariables(Instruction current, MethodDefinition method)
		{
			if (GetStackIndex(current, method, out int stack))
			{
				var type = FindPreviousStackType(current, method);
				if (type == null)
					throw new Exception("Could not find stack type for: " + current + "\n\nVariables?:\n" + string.Join("\n", method.Body.Variables));
				var vd = new VariableDefinition(type);
				vd.SetIndex(stack);
				method.Body.Variables.Add(vd);
			}
		}

		private static bool GetStackIndex(Instruction current, MethodDefinition method, out int stack)
		{
			switch (current.OpCode.Code)
			{
				case Code.Stloc: // Pop a value from stack into local variable indx.
					throw new NotImplementedException(current.ToString());
					break;
				case Code.Stloc_0:
					if (method.Body.Variables.Count <= 0)
					{
						stack = 0;
						return true;
					}

					break;
				case Code.Stloc_1:
					if (method.Body.Variables.Count <= 1)
					{
						stack = 1;
						return true;
					}

					break;
				case Code.Stloc_2:
					if (method.Body.Variables.Count <= 2)
					{
						stack = 2;
						return true;
					}

					break;
				case Code.Stloc_3:
					if (method.Body.Variables.Count <= 3)
					{
						stack = 3;
						return true;
					}

					break;
				case Code.Stloc_S: // Pop a value from stack into local variable indx, short form.	
					throw new NotImplementedException(current.ToString());
					break;
			}

			stack = -1;
			return false;
		}

		private static TypeReference FindPreviousStackType(Instruction current, MethodDefinition method)
		{
			if (current == null) return null;
			current = current.Previous;
			while (current != null)
			{
				// if (current.Operand is TypeReference tr) return tr;

				switch (current.OpCode.Code)
				{
					case Code.Ldloc:
						throw new NotImplementedException(current.ToString());
						break;
					case Code.Ldloc_0:
						return method.Body.Variables[0].VariableType;
						break;
					case Code.Ldloc_1:
						return method.Body.Variables[1].VariableType;
						break;
					case Code.Ldloc_2:
						return method.Body.Variables[2].VariableType;
						break;
					case Code.Ldloc_3:
						return method.Body.Variables[3].VariableType;
						break;
					case Code.Ldloca:
						throw new NotImplementedException(current.ToString());
						break;
					case Code.Ldloc_S:
						throw new NotImplementedException(current.ToString());
						break;
					case Code.Ldloca_S:
						throw new NotImplementedException(current.ToString());
						break;
				}

				current = current.Previous;
			}

			return null;
		}

		public static void HandleLocalVariableDefinition(this MethodDefinition method, ILProcessor processor, ModuleDefinition module, Instruction i,
			ref LocalVariableInfo operand)
		{
			var lt = operand.LocalType;
			// Debug.Log("Type is " + lt);
			// TODO: move into InstructionConverter
			var tr = new TypeReference(lt.Namespace, lt.Name, module, module);
			var vd = new VariableDefinition(tr);
			vd.SetIndex(operand.LocalIndex);
			method.Body.Variables.Add(vd);
			// Debug.Log(tr + "\n" + vd + "\n" + vd.Index + "\n" + vd.IsPinned + "\n" + method.Body.HasVariables +"\n" + i.OpCode);
			i.Operand = vd;
		}


		private static Lazy<FieldInfo> VariableIndexFieldInfo = new Lazy<FieldInfo>(() => typeof(VariableReference).GetField("index", (BindingFlags) ~0));

		private static void SetIndex(this VariableReference var, int index)
		{
			if (var == null) return;
			VariableIndexFieldInfo.Value.SetValue(var, index);
		}
	}
}