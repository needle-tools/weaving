using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using _Tests.Weaver_InputDevice;
using HarmonyLib;
using Mono.Cecil;
using Mono.Reflection;
using needle.EditorPatching;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.XR;
using Instruction = Mono.Reflection.Instruction;
using OpCode = Mono.Cecil.Cil.OpCode;

namespace Editor
{
	[HarmonyPatch(typeof(InputDevices))]
	public class InputDevicesPatch : IPreprocessBuildWithReport
	{
		[HarmonyPrefix]
		[HarmonyPatch("GetDevices")]
		private static bool GetDevices(List<InputDevice> inputDevices)
		{
			FakeInputDeviceAPI.FakeDeviceList(inputDevices);
			return false;
		}

		public int callbackOrder => 1000;
		public void OnPreprocessBuild(BuildReport report)
		{
			PatchToIL.PrintIL();
			Debug.LogError("error to stop build");
		}
	}

	public static class PatchToIL
	{
		[MenuItem("Weaving/" + nameof(PrintIL))]
		public static void PrintIL()
		{
			// var opCodeConstructor = typeof(OpCode).GetConstructor((BindingFlags)~0, null, CallingConventions.Any, );
			// if (opCodeConstructor == null) throw new Exception("Failed finding opcode constructor");

			var md = typeof(InputDevices).GetMethod("GetDevices", (BindingFlags) ~0);
			var inst2 = md.GetInstructions();
			foreach(var instruction in inst2) Debug.Log(instruction);


			var patchedMethods = Harmony.GetAllPatchedMethods().ToArray();
			Debug.Log("Patched methods: " + patchedMethods.Length);
			foreach (MethodBase method in patchedMethods)
			{
				if (method == null) continue;

				var info = Harmony.GetPatchInfo(method);
				
				foreach (var pf in info.EnumeratePatches())
				{
					var pm = pf.PatchMethod;
					Debug.Log("PATCHED");
					var inst = pm.GetInstructions();
					foreach(Instruction instruction in inst) Debug.Log(instruction);
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


		// https://github.com/jbevain/cecil/blob/6edabe0525a17bda3613c2f4e5b28db568cb2c21/Mono.Cecil.Cil/CodeReader.cs#L141
		private static OpCode ReadOpCode (Func<byte> readByte)
		{
			var il_opcode = readByte();
			Debug.Log(il_opcode + " is one byte? " + (il_opcode == 0xfe));
			return il_opcode != 0xfe ? OneByteOpCode [il_opcode] : TwoBytesOpCode [readByte()];
		}
		internal static readonly OpCode[] OneByteOpCode = new OpCode[225];
		internal static readonly OpCode[] TwoBytesOpCode = new OpCode[31];
		
		
		// from https://github.com/jbevain/cecil/blob/master/Test/Mono.Cecil.Tests/Extensions.cs
		public static TypeDefinition ToDefinition (this Type self)
		{
			var module = ModuleDefinition.ReadModule (new MemoryStream (File.ReadAllBytes (self.Module.FullyQualifiedName)));
			return (TypeDefinition) module.LookupToken (self.MetadataToken);
		}
		
		public static MethodDefinition ToDefinition (this System.Reflection.MethodBase method)
		{
			var declaring_type = method.DeclaringType.ToDefinition ();
			return (MethodDefinition) declaring_type.Module.LookupToken (method.MetadataToken);
		}
	}
}