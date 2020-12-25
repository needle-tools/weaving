﻿using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Fody;
using Mono.Cecil;
using Mono.Cecil.Cil;
using UnityEngine;

namespace ExampleProjectWeaver
{
    public class ModuleWeaver : BaseModuleWeaver
    {

        public override IEnumerable<string> GetAssembliesForScanning()
        {
            yield return "netstandard";
            yield return "mscorlib";
            yield return "UnityEngine";
        }
        
        
        // https://intellitect.com/creating-fody-addin/
        
        public override void Execute()
        {
            Debug.Log("Executing module weaver " + ModuleDefinition.Assembly.FullName);
            foreach(var type in ModuleDefinition.Types)
            {
                foreach ( MethodDefinition method in type.Methods )
                {
                    ProcessMethod( method );
                }
            }
        }

        private void ProcessMethod(MethodDefinition method)
        {
            Debug.Log("Process " + method.Name);
            ILProcessor processor = method.Body.GetILProcessor();
            Instruction current = method.Body.Instructions.First();
 
            //Create Nop instruction to use as a starting point
            //for the rest of our instructions
 
            var first = Instruction.Create( OpCodes.Ret );
            processor.InsertBefore( current, first );
            current = first;
 
            //Insert all instructions for debug output after Nop 
            // foreach ( Instruction instruction in GetInstructions( method ) )
            // {
            //     processor.InsertAfter( current, instruction );
            //     current = instruction;
            // }
        }
        
        private static readonly MethodInfo _stringJoinMethod;
        private static readonly MethodInfo _stringFormatMethod;
        private static readonly MethodInfo _debugWriteLineMethod;
        static ModuleWeaver()
        {
            //Find string.Join(string, object[]) method
            _stringJoinMethod = typeof( string )
                .GetMethods()
                .Where( x => x.Name == nameof( string.Join ) )
                .Single( x =>
                {
                    var parameters = x.GetParameters();
                    return parameters.Length == 2 &&
                           parameters[0].ParameterType == typeof( string ) &&
                           parameters[1].ParameterType == typeof( object[] );
                } );
 
            //Find string.Format(string, object) method
            _stringFormatMethod = typeof( string )
                .GetMethods()
                .Where( x => x.Name == nameof( string.Format ) )
                .Single( x =>
                {
                    var parameters = x.GetParameters();
                    return parameters.Length == 2 &&
                           parameters[0].ParameterType == typeof( string ) &&
                           parameters[1].ParameterType == typeof( object );
                } );
 
            //Find Debug.WriteLine(string) method
            _debugWriteLineMethod = typeof( System.Diagnostics.Debug )
                .GetMethods()
                .Where( x => x.Name == nameof( System.Diagnostics.Debug.WriteLine ) )
                .Single( x =>
                {
                    var parameters = x.GetParameters();
                    return parameters.Length == 1 &&
                           parameters[0].ParameterType == typeof( string );
                } );
        }
        
        private IEnumerable<Instruction> GetInstructions( MethodDefinition method )
        {
            yield return Instruction.Create( OpCodes.Ldstr, $"DEBUG: {method.Name}({{0}})" );
            yield return Instruction.Create( OpCodes.Ldstr, "," );
 
            yield return Instruction.Create( OpCodes.Ldc_I4, method.Parameters.Count );
            yield return Instruction.Create( OpCodes.Newarr, ModuleDefinition.ImportReference( typeof( object ) ) );
 
            for ( int i = 0; i < method.Parameters.Count; i++ )
            {
                yield return Instruction.Create( OpCodes.Dup );
                yield return Instruction.Create( OpCodes.Ldc_I4, i );
                yield return Instruction.Create( OpCodes.Ldarg, method.Parameters[i] );
                if ( method.Parameters[i].ParameterType.IsValueType )
                    yield return Instruction.Create( OpCodes.Box, method.Parameters[i].ParameterType );
                yield return Instruction.Create( OpCodes.Stelem_Ref );
            }
 
            yield return Instruction.Create( OpCodes.Call, ModuleDefinition.ImportReference( _stringJoinMethod ) );
            yield return Instruction.Create( OpCodes.Call, ModuleDefinition.ImportReference( _stringFormatMethod ) );
            yield return Instruction.Create( OpCodes.Call, ModuleDefinition.ImportReference( _debugWriteLineMethod ) );
        }


    }
}
