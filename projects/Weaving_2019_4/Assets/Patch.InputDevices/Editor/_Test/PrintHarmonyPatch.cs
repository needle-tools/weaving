using System.Linq;
using System.Reflection;
using HarmonyLib;
using Mono.Reflection;
using needle.Weaver;
using UnityEditor;
using UnityEngine;

namespace Fody.Weavers.InputDeviceWeaver
{
	public static class PrintHarmonyPatch
	{

		[MenuItem(Constants.MenuItemBase + nameof(PrintNeedlePatchIL))]
		public static void PrintNeedlePatchIL()
		{
			var methods = UnityEditor.TypeCache.GetMethodsWithAttribute<NeedlePatch>();
			foreach (var method in methods)
			{
				method.GetInstructions().ToCecilInstruction(true);
			}
		}
		
		[MenuItem(Constants.MenuItemBase + nameof(PrintIL))]
		public static void PrintIL()
		{
			// var opCodeConstructor = typeof(OpCode).GetConstructor((BindingFlags)~0, null, CallingConventions.Any, );
			// if (opCodeConstructor == null) throw new Exception("Failed finding opcode constructor");

			// var md = typeof(InputDevices).GetMethod("GetDevices", (BindingFlags) ~0);
			// var inst2 = md.GetInstructions();
			// foreach(var instruction in inst2) Debug.Log(instruction);

			// TODO: figure out how to merge patched methods (or how to get the final method)

			var patchedMethods = Harmony.GetAllPatchedMethods().ToArray();
			Debug.Log("Patched methods: " + patchedMethods.Length);
			foreach (MethodBase method in patchedMethods)
			{
				if (method == null) continue;

				var info = Harmony.GetPatchInfo(method);
				
				// var proc = harmony.CreateProcessor(method);
				// PatchProcessor processor = this.CreateProcessor(original);
				// processor.AddPrefix(prefix);
				// processor.AddPostfix(postfix);
				// processor.AddTranspiler(transpiler);
				// processor.AddFinalizer(finalizer);
				// return processor.Patch();
				
				// var patch = info.EnumeratePatches().OrderByDescending(p => p.priority).FirstOrDefault(p => p.PatchMethod != null);
				foreach (var patch in info.Postfixes)
				{
					Debug.Log("<b>PATCHED</b> " + method.FullDescription() + "\nin " + patch.owner);
					var inst = patch.PatchMethod.GetInstructions();
					var cecilInst = inst.ToCecilInstruction(true);
					break;
				}
				
				// https://github.com/jbevain/mono.reflection
				// var inst = method.GetInstructions();
				// foreach(var instruction in inst) Debug.Log(instruction);

				// var def = method.ToDefinition();
				// def.LogIL(method.Name);

				// https://stackoverflow.com/a/12052346
				// MethodRental.SwapMethodBody();

				// string methodName = method.Name;
				// string className = method.ReflectedType?.FullName;
				// string fullMethodName = className != null ? className + "." + methodName : methodName;
				// fullMethodName += "(" + string.Join(",", method.GetParameters().Select(o => $"{o.ParameterType} {o.Name}").ToArray()) + ")";
				// var asm = method.ReflectedType.Assembly;

				// var asmdef = AssemblyDefinition.ReadAssembly(method.ReflectedType.Module.FullyQualifiedName);
				// var toInspect = asmdef.MainModule
				// 	.GetTypes()
				// 	.SelectMany(typeDefinition => typeDefinition.Methods
				// 			.Where(m => m.HasBody && m.Name == method.Name)
				// 			.Select(m => new {td = typeDefinition, methodDefinition = m})
				// 	);
				//
				//
				// var pa = string.Join(",", method.GetParameters().Select(pi => pi.ParameterType + "_" + pi.Name));
				//
				// foreach (var entry in )
				// {
				// 	entry.methodDefinition.LogIL(entry.td + ", " + entry.methodDefinition);
				// }


				// var body = method.GetMethodBody();
				// if (body == null) continue;
				// var raw = body.GetILAsByteArray();
				// Debug.Log(method.Name + " has " + raw.Length + " bytes\n" + string.Join("\n", raw));
				// // var sub = new byte[4];
				//
				// var start = 0;
				// var position = 0;
				// var end = raw.Length;
				//
				// byte ReadByte()
				// {
				// 	return raw[position++];
				// }
				//
				// while (position < end)
				// {
				// 	var offset = position - start;
				// 	var opcode = ReadOpCode(ReadByte);
				// 	Debug.Log(opcode + ", " + opcode.OperandType);
				// 	var instruction = Activator.CreateInstance(typeof(Instruction), (BindingFlags) ~0, null, new object[] {offset, opcode}, null, null);
				// 	// var instruction = Instruction.Create(offset, opcode);
				// 	Debug.Log(instruction);
				// }

				// for (var i = 0; i < raw.Length - 8; i += 8)
				// {
				// 	var packed1 = BitConverter.ToInt32(raw, i);
				// 	var packed2 = BitConverter.ToInt32(raw, i+4);
				// 	Debug.Log(packed1 + ", " + packed2);
				// 	var obj = Activator.CreateInstance(typeof(OpCode), (BindingFlags) ~0, null, new object[] {packed1, packed2}, null, null);
				// 	// var obj = (OpCode) opCodeConstructor.Invoke(new object[]{packed1, packed2});
				// 	Debug.Log(obj);
				//
				// }
				// MethodDefinition md = new MethodDefinition("", MethodAttributes.Abstract, null);
				// var proc = md.Body.GetILProcessor();
				// proc.Emit();
			}
		}


		// // https://github.com/jbevain/cecil/blob/6edabe0525a17bda3613c2f4e5b28db568cb2c21/Mono.Cecil.Cil/CodeReader.cs#L141
		// private static OpCode ReadOpCode (Func<byte> readByte)
		// {
		// 	var il_opcode = readByte();
		// 	Debug.Log(il_opcode + " is one byte? " + (il_opcode == 0xfe));
		// 	return il_opcode != 0xfe ? OneByteOpCode [il_opcode] : TwoBytesOpCode [readByte()];
		// }
		// internal static readonly OpCode[] OneByteOpCode = new OpCode[225];
		// internal static readonly OpCode[] TwoBytesOpCode = new OpCode[31];
		//
		//
		
	}
}