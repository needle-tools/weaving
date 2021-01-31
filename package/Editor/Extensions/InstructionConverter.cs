using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Mono.Cecil;
using Mono.Cecil.Cil;
using UnityEngine;
using FlowControl = Mono.Cecil.Cil.FlowControl;
using OpCode = Mono.Cecil.Cil.OpCode;
using OpCodeType = Mono.Cecil.Cil.OpCodeType;
using OperandType = Mono.Cecil.Cil.OperandType;
using StackBehaviour = Mono.Cecil.Cil.StackBehaviour;

namespace needle.Weaver
{
	public static class InstructionConverter
	{
		public static void TryLog(object prev, object now)
		{
			try
			{
				Debug.Log(prev + " <-prev\n" + now + " <-now");
			}
			catch (Exception e)
			{
				Debug.LogWarning(e);
			}
		}
		
		public static IList<Instruction> ToCecilInstruction(this IList<Mono.Reflection.Instruction> instructions, bool debugLog = false)
		{
			var list = new List<Instruction>(instructions.Count);
			foreach (var inst in instructions)
			{
				var conv = inst.ToCecilInstruction();
				if (debugLog)
					TryLog(inst, conv);
				list.Add(conv);
			}

			return list.ResolveLabels(debugLog);
		}

		public static Instruction ToCecilInstruction(this Mono.Reflection.Instruction i)
		{
			return ToCecilInstruction(i.OpCode, i.Operand, i.Offset);
		}

		public static T ResolveLabels<T>(this T instructions, bool debugLog = false) where T : IList<Instruction>
		{
			for (var i = 0; i < instructions.Count; i++)
			{
				var inst = instructions[i];
				if (inst.Operand is Label label)
				{
					var index = (int) label.GetType().GetField("label", (BindingFlags) ~0).GetValue(label);
					inst.Operand = instructions[index];
					if(debugLog)
						Debug.Log(inst.OpCode.Name + " has Label with " + index + " pointing to " + inst.Operand);
				}
			}
			return instructions;
		}
		
		public static Instruction ToCecilInstruction(System.Reflection.Emit.OpCode _opcode, object _operand, int offset = 0)
		{
			// unpack and map
			var op = _opcode;
			var op1 = (byte) op1FieldInfo.Value.GetValue(_opcode);
			var op2 = (byte) op2FieldInfo.Value.GetValue(_opcode);
			var flow = (byte) op.FlowControl.ToCecilFlowControl();
			var opCodeType = (byte) op.OpCodeType.ToCecilOpCodeType();
			var operandType = op.OperandType.ToCecilOperandType();
			var pop = (byte) op.StackBehaviourPop.ToCecilStackBehaviour();
			var push = (byte) op.StackBehaviourPush.ToCecilStackBehaviour();

			var operand = _operand.ToCecilOperand();

			// pack
			var bytes = new[]
			{
				op1, // op1
				op2, // op2
				(byte) ToCecilCode(op1, op2), // code
				flow, // flow control
				opCodeType, // opcode type
				(byte) operandType, // operand type
				pop, // pop
				push, // push
			};
			var i1 = BitConverter.ToInt32(bytes, 0);
			var i2 = BitConverter.ToInt32(bytes, 4);

			// create
			var opcode = (OpCode) Activator.CreateInstance(typeof(OpCode), (BindingFlags) ~0, null, new object[] {i1, i2}, null, null);
			var instruction = (Instruction) Activator.CreateInstance(typeof(Instruction), (BindingFlags) ~0, null,
				new[] {opcode, operand}, null, null);
			instruction.Offset = offset;
			// Debug.Log(i.OpCode.Name + " = " + opcode.Name + "\n" + i.OpCode.OpCodeType + " = " + opcode.OpCodeType);
			return instruction;
		}
		/*
			these are the target data layouts
			although unfortunately bytes can not be mapped directly
			so we need to jump through some hoops. see below
			
			source
			internal byte op1;
		    internal byte op2; -> Value => this.size == (byte) 1 ? (short) this.op2 : (short) ((int) this.op1 << 8 | (int) this.op2);
		    private byte push;
		    private byte pop;
		    private byte size;
		    private byte type; -> OpCodeType
		    private byte args; -> OperandType
		    private byte flow; -> FlowControl
		 
			target:
			private readonly byte op1;
		    private readonly byte op2; -> Value => this.op1 != byte.MaxValue ? (short) ((int) this.op1 << 8 | (int) this.op2) : (short) this.op2;
		    private readonly byte code;
		    private readonly byte flow_control;
		    private readonly byte opcode_type;
		    private readonly byte operand_type;
		    private readonly byte stack_behavior_pop;
		    private readonly byte stack_behavior_push;
		 */


		public static Code ToCecilCode(byte op1, byte op2)
		{
			var names = reflectionOpCodeNamesArray.Value;
			var name = op1 == byte.MaxValue ? names[(int) op2] : names[256 + (int) op2];
			var cecilNames = cecilOpCodeNamesArray.Value;
			for (var i = 0; i < cecilNames.Length; i++)
			{
				var cname = cecilNames[i];
				if (cname == name)
				{
					return (Code) i;
				}
			}

			throw new Exception("Could not find cecil code for " + name);
		}

		private static readonly Lazy<string[]> reflectionOpCodeNamesArray = new Lazy<string[]>(() =>
		{
			var opCodeNamesType = typeof(System.Reflection.Emit.OpCode).Assembly.GetTypes()
				.FirstOrDefault(t => t.FullName == "System.Reflection.Emit.OpCodeNames");
			var namesArrayField = opCodeNamesType?.GetField("names", (BindingFlags) ~0);
			var namesArray = namesArrayField?.GetValue(null) as string[];
			return namesArray;
		});

		private static readonly Lazy<string[]> cecilOpCodeNamesArray = new Lazy<string[]>(() =>
		{
			var opCodeNamesType = typeof(OpCode).Assembly.GetTypes().FirstOrDefault(t => t.FullName == "Mono.Cecil.Cil.OpCodeNames");
			var namesArrayField = opCodeNamesType?.GetField("names", (BindingFlags) ~0);
			var namesArray = namesArrayField?.GetValue(null) as string[];
			return namesArray;
		});

		private static readonly Lazy<FieldInfo> op1FieldInfo =
			new Lazy<FieldInfo>(() => typeof(System.Reflection.Emit.OpCode).GetField("op1", (BindingFlags) ~0));

		private static readonly Lazy<FieldInfo> op2FieldInfo =
			new Lazy<FieldInfo>(() => typeof(System.Reflection.Emit.OpCode).GetField("op2", (BindingFlags) ~0));

		private static object ToCecilOperand(this object _operand)
		{
			// try
			// {
			// 	Debug.Log("Operand: " + _operand);
			// 	if (_operand != null && _operand is int n)
			// 	{
			// 		Debug.Log("NUMBER " + n);
			// 	}
			// }
			// catch { /*ignore */ }
			
			if (_operand is Mono.Reflection.Instruction inst)
			{
				return inst.ToCecilInstruction();
			}

			if (_operand is Mono.Reflection.Instruction[] instructions)
			{
				var arr = new Instruction[instructions.Length];
				for (var i = 0; i < instructions.Length; i++)
					arr[i] = instructions[i].ToCecilInstruction();
				return arr;
			}

			
			if (_operand != null)
			{
				// Debug.LogWarning(instruction.Operand);
				
				// for cecil to write the module we need to convert the operand data
				// it expects internally types like IMetadataTokenProvider etc
				// we can just read the assembly where the type is defined to get a MethodReference
				
				// TODO: cleanup this reflection
				var type = _operand.GetType();
				
				object GetField(string name)
				{
					return type.GetField(name, (BindingFlags) ~0).GetValue(_operand);
				}
				object GetProperty(string name)
				{
					return type.GetProperty(name, (BindingFlags) ~0).GetValue(_operand);
				}
				object GetMethod(string name) 
				{
					return type.GetMethod(name, (BindingFlags) ~0).Invoke(_operand, null);
				}

				
				if(type.FullName == "System.Reflection.MonoMethod")
				{
					var t = (Type) GetProperty("DeclaringType");
					var method = (MethodInfo) GetMethod("GetBaseMethod");
					// cache modules?
					var mod = ModuleDefinition.ReadModule(t.Assembly.Location);
					var reference = mod.ImportReference(method);
					mod.Dispose();
					return reference;
				}
				
				
				if(_operand.GetType().FullName == "System.RuntimeType")
				{
					var typeName = (string) _operand.GetType().GetProperty("AssemblyQualifiedName", (BindingFlags) ~0).GetValue(_operand);
					var actualType = Type.GetType(typeName);
					if (actualType == null) throw new Exception("Could not get type for " + _operand);
					var mod = ModuleDefinition.ReadModule(actualType.Assembly.Location);
					var tr = new TypeReference(actualType.Namespace, actualType.Name, mod, mod);
					mod.Dispose();
					return tr;
				}

				
				
				if(_operand is LocalVariableInfo lvi)
				{
					// var lt = lvi.LocalType;
					// if (lt == null) throw new Exception("Could not get local type for " + _operand);
					// var mod = ModuleDefinition.ReadModule(lt.Assembly.Location);
					// var tr = new TypeReference(lt.Namespace, lt.Name, mod, mod);
					// var vd = new VariableDefinition(tr);
					//
					// Debug.LogWarning(vd + " - " + tr);
					// mod.Dispose();
					return lvi;
				}
				
			}
			
			return _operand;
		}
		
		// if (_operand is MemberInfo member)
		// {
		// 	Debug.Log(member.GetType());
		// 	Debug.Log(_operand.GetType().GetProperty("ReflectedType", (BindingFlags)~0).GetValue(_operand));
		// 	Debug.Log(_operand.GetType().BaseType.GetProperty("ReflectedTypeInternal", (BindingFlags)~0).GetValue(_operand));
		// 	Debug.Log(typeof(MethodBase).GetProperty("FullName", (BindingFlags)~0).GetValue(_operand));
		// 	var prop = member.GetType().GetProperty("AssemblyQualifiedName", (BindingFlags) ~0);
		// 	var aqn = (string)prop.GetValue(member);
		// 	var t = Type.GetType(aqn);
		// 	var mod = ModuleDefinition.ReadModule(t.Assembly.Location);
		// 	var reference = mod.ImportReference(t);
		// 	mod.Dispose();
		// 	return reference;
		// }

		// string TryFindFullName(Type _type)
		// {
		// 	while (_type != null)
		// 	{
		// 		try
		// 		{
		// 			var prop = _type.GetProperty("FullName", (BindingFlags) ~0);
		// 			if(prop != null)
		// 				return (string) prop.GetValue(_operand);
		// 		}
		// 		catch
		// 		{
		// 			// ignored
		// 		}
		//
		// 		_type = _type.BaseType;
		// 	}
		//
		// 	return null;
		// }
				
		// if (type.FullName == "System.Reflection.MonoCMethod")
		// {
		// 	var fullName = TryFindFullName(type);
		// 	Debug.Log(fullName);
		// 	var t = Type.GetType(fullName);
		// 	Debug.Log(t + ", " + t.Assembly);
		// 	var mod = ModuleDefinition.ReadModule(t.Assembly.Location);
		// 	var reference = mod.ImportReference(_operand as MethodBase);
		// 	mod.Dispose();
		// 	return _operand;
		// }
		// private static IReflectionImporter reflection_importer;
		// internal static IReflectionImporter ReflectionImporter
		// {
		// 	get
		// 	{
		// 		if (reflection_importer == null)
		// 			Interlocked.CompareExchange<IReflectionImporter>(ref reflection_importer, (IReflectionImporter) new DefaultReflectionImporter(this, (IReflectionImporter) null);
		// 		return reflection_importer;
		// 	}
		// }
		
		public static OpCodeType ToCecilOpCodeType(this System.Reflection.Emit.OpCodeType b)
		{
			switch (b)
			{
#pragma warning disable 618
				case System.Reflection.Emit.OpCodeType.Annotation:
#pragma warning restore 618
					return OpCodeType.Annotation;
				case System.Reflection.Emit.OpCodeType.Macro:
					return OpCodeType.Macro;
				case System.Reflection.Emit.OpCodeType.Nternal:
					return OpCodeType.Nternal;
				case System.Reflection.Emit.OpCodeType.Objmodel:
					return OpCodeType.Objmodel;
				case System.Reflection.Emit.OpCodeType.Prefix:
					return OpCodeType.Prefix;
				case System.Reflection.Emit.OpCodeType.Primitive:
					return OpCodeType.Primitive;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		public static OperandType ToCecilOperandType(this System.Reflection.Emit.OperandType b)
		{
			switch (b)
			{
				case System.Reflection.Emit.OperandType.InlineBrTarget:
					return OperandType.InlineBrTarget;
				case System.Reflection.Emit.OperandType.InlineField:
					return OperandType.InlineField;
				case System.Reflection.Emit.OperandType.InlineI:
					return OperandType.InlineI;
				case System.Reflection.Emit.OperandType.InlineI8:
					return OperandType.InlineI8;
				case System.Reflection.Emit.OperandType.InlineMethod:
					return OperandType.InlineMethod;
				case System.Reflection.Emit.OperandType.InlineNone:
					return OperandType.InlineNone;
#pragma warning disable 618
				case System.Reflection.Emit.OperandType.InlinePhi:
#pragma warning restore 618
					return OperandType.InlinePhi;
				case System.Reflection.Emit.OperandType.InlineR:
					return OperandType.InlineR;
				case System.Reflection.Emit.OperandType.InlineSig:
					return OperandType.InlineSig;
				case System.Reflection.Emit.OperandType.InlineString:
					return OperandType.InlineString;
				case System.Reflection.Emit.OperandType.InlineSwitch:
					return OperandType.InlineSwitch;
				case System.Reflection.Emit.OperandType.InlineTok:
					return OperandType.InlineTok;
				case System.Reflection.Emit.OperandType.InlineType:
					return OperandType.InlineType;
				case System.Reflection.Emit.OperandType.InlineVar:
					return OperandType.InlineVar;
				case System.Reflection.Emit.OperandType.ShortInlineBrTarget:
					return OperandType.ShortInlineBrTarget;
				case System.Reflection.Emit.OperandType.ShortInlineI:
					return OperandType.ShortInlineI;
				case System.Reflection.Emit.OperandType.ShortInlineR:
					return OperandType.ShortInlineR;
				case System.Reflection.Emit.OperandType.ShortInlineVar:
					return OperandType.ShortInlineVar;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		public static FlowControl ToCecilFlowControl(this System.Reflection.Emit.FlowControl fc)
		{
			switch (fc)
			{
				case System.Reflection.Emit.FlowControl.Branch:
					return FlowControl.Branch;
				case System.Reflection.Emit.FlowControl.Break:
					return FlowControl.Break;
				case System.Reflection.Emit.FlowControl.Call:
					return FlowControl.Call;
				case System.Reflection.Emit.FlowControl.Cond_Branch:
					return FlowControl.Cond_Branch;
				case System.Reflection.Emit.FlowControl.Meta:
					return FlowControl.Meta;
				case System.Reflection.Emit.FlowControl.Next:
					return FlowControl.Next;
#pragma warning disable 618
				case System.Reflection.Emit.FlowControl.Phi:
#pragma warning restore 618
					return FlowControl.Phi;
				case System.Reflection.Emit.FlowControl.Return:
					return FlowControl.Return;
				case System.Reflection.Emit.FlowControl.Throw:
					return FlowControl.Throw;
				default:
					throw new ArgumentOutOfRangeException(nameof(fc), fc, null);
			}
		}

		public static StackBehaviour ToCecilStackBehaviour(this System.Reflection.Emit.StackBehaviour sb)
		{
			switch (sb)
			{
				case System.Reflection.Emit.StackBehaviour.Pop0:
					return StackBehaviour.Pop0;
				case System.Reflection.Emit.StackBehaviour.Pop1:
					return StackBehaviour.Pop1;
				case System.Reflection.Emit.StackBehaviour.Pop1_pop1:
					return StackBehaviour.Pop1_pop1;
				case System.Reflection.Emit.StackBehaviour.Popi:
					return StackBehaviour.Popi;
				case System.Reflection.Emit.StackBehaviour.Popi_pop1:
					return StackBehaviour.Popref_pop1;
				case System.Reflection.Emit.StackBehaviour.Popi_popi:
					return StackBehaviour.Popref_popi;
				case System.Reflection.Emit.StackBehaviour.Popi_popi8:
					return StackBehaviour.Popi_popi8;
				case System.Reflection.Emit.StackBehaviour.Popi_popi_popi:
					return StackBehaviour.Popi_popi_popi;
				case System.Reflection.Emit.StackBehaviour.Popi_popr4:
					return StackBehaviour.Popi_popr4;
				case System.Reflection.Emit.StackBehaviour.Popi_popr8:
					return StackBehaviour.Popi_popr8;
				case System.Reflection.Emit.StackBehaviour.Popref:
					return StackBehaviour.Popref;
				case System.Reflection.Emit.StackBehaviour.Popref_pop1:
					return StackBehaviour.Popref_pop1;
				case System.Reflection.Emit.StackBehaviour.Popref_popi:
					return StackBehaviour.Popref_popi;
				case System.Reflection.Emit.StackBehaviour.Popref_popi_pop1:
					throw new Exception("Enum is not defined in mono cecil(?) for: " + sb);
				case System.Reflection.Emit.StackBehaviour.Popref_popi_popi:
					return StackBehaviour.Popref_popi_popi;
				case System.Reflection.Emit.StackBehaviour.Popref_popi_popi8:
					return StackBehaviour.Popref_popi_popi8;
				case System.Reflection.Emit.StackBehaviour.Popref_popi_popr4:
					return StackBehaviour.Popref_popi_popr4;
				case System.Reflection.Emit.StackBehaviour.Popref_popi_popr8:
					return StackBehaviour.Popref_popi_popr8;
				case System.Reflection.Emit.StackBehaviour.Popref_popi_popref:
					return StackBehaviour.Popref_popi_popref;
				case System.Reflection.Emit.StackBehaviour.Push0:
					return StackBehaviour.Push0;
				case System.Reflection.Emit.StackBehaviour.Push1:
					return StackBehaviour.Push1;
				case System.Reflection.Emit.StackBehaviour.Push1_push1:
					return StackBehaviour.Push1_push1;
				case System.Reflection.Emit.StackBehaviour.Pushi:
					return StackBehaviour.Pushi;
				case System.Reflection.Emit.StackBehaviour.Pushi8:
					return StackBehaviour.Pushi8;
				case System.Reflection.Emit.StackBehaviour.Pushr4:
					return StackBehaviour.Pushr4;
				case System.Reflection.Emit.StackBehaviour.Pushr8:
					return StackBehaviour.Pushr8;
				case System.Reflection.Emit.StackBehaviour.Pushref:
					return StackBehaviour.Pushref;
				case System.Reflection.Emit.StackBehaviour.Varpop:
					return StackBehaviour.Varpop;
				case System.Reflection.Emit.StackBehaviour.Varpush:
					return StackBehaviour.Varpush;
				default:
					throw new ArgumentOutOfRangeException(nameof(sb), sb, null);
			}


			// unfortunately values are stored differently in enums in cecil and reflection.emit

			// private static string[] OpCodeFieldNamesInOrder = new[]
			// {
			// 	"op1",
			// 	"op2",
			// 	"push",
			// 	"pop",
			// 	"size",
			// 	"type",
			// 	"args",
			// 	"flow"
			// };

			// private static byte[] GetBytes(System.Reflection.Emit.OpCode opcode, string[] names, int start, int length)
			// {
			// 	var type = opcode.GetType();
			// 	byte[] arr = new byte[length];
			// 	var i = 0;
			// 	for (var index = start; index < names.Length && i < length; index++)
			// 	{
			// 		var name = names[index];
			// 		var field = type.GetField(name, (BindingFlags) ~0);
			// 		if (field == null) throw new MissingFieldException("Tried accessing field " + name + " on " + type);
			// 		var data = (byte) field?.GetValue(opcode);
			// 		arr[i] = data;
			// 		i++;
			// 	}
			//
			// 	return arr;
			// }
		}
	}
}