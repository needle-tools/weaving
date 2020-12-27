using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;
using UnityEngine;

namespace Fody.Weavers.DebugLogWeaver
{
    // https://www.codersblock.org/blog//2014/06/integrating-monocecil-with-unity.html
    
    // https://intellitect.com/creating-fody-addin/
    
    // C# to IL
    // https://stackoverflow.com/questions/6574858/convert-c-sharp-code-to-il-code
    // idea: similar to harmony create patch methods, marked with attribute. capture their IL in a module weaver and store it somewhere
    // then in the injection replace the IL with the stored IL
    
    public class ModuleWeaver : BaseModuleWeaver
    {

        public override IEnumerable<string> GetAssembliesForScanning()
        {
            yield return "netstandard";
            yield return "mscorlib";
            yield return "UnityEngine";
        }
        public override void Execute()
        {
            Debug.Log("Executing module weaver " + ModuleDefinition.Assembly.FullName);
            foreach(var type in ModuleDefinition.Types)
            {
                foreach ( MethodDefinition method in type.Methods )
                {
                    if(method.FullName.Contains("CodeToWeave"))
                        ProcessMethod( method );
                }
            }
        }

        private void ProcessMethod(MethodDefinition method)
        {
            Debug.Log("Process " + method.Name);
            ILProcessor processor = method.Body.GetILProcessor();
            Instruction current = method.Body.Instructions.First();
            method.LogIL("BEFORE PROCESSING");
            
            //Create Nop instruction to use as a starting point
            //for the rest of our instructions
 
            var first = Instruction.Create( OpCodes.Nop );
            processor.InsertBefore( current, first );
            current = first;
 
            // Insert all instructions for debug output after Nop 
             foreach ( Instruction instruction in GetInstructions( method ) )
             {
                 processor.InsertAfter( current, instruction );
                 current = instruction;
             }
             
             method.LogIL("AFTER PROCESSING");
        }
        
        private IEnumerable<Instruction> GetInstructions( MethodDefinition method )
        {
            yield return Instruction.Create( OpCodes.Ldstr, $"{method.Name}({{0}}) has been patched at " + DateTime.Now.ToString(CultureInfo.InvariantCulture));
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
            yield return Instruction.Create( OpCodes.Call, ModuleDefinition.ImportReference( _unityDebugLogMethod ) );
        }

        private static readonly MethodInfo _stringJoinMethod;
        private static readonly MethodInfo _stringFormatMethod;
        private static readonly MethodInfo _debugWriteLineMethod;
        private static readonly MethodInfo _unityDebugLogMethod;

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
            
            _unityDebugLogMethod = typeof( UnityEngine.Debug )
                .GetMethods()
                .Where( x => x.Name == nameof( UnityEngine.Debug.Log ) )
                .Single( x =>
                {
                    var parameters = x.GetParameters();
                    return parameters.Length == 1 &&
                           parameters[0].ParameterType == typeof( object );
                } );
        }
        

    }
}
