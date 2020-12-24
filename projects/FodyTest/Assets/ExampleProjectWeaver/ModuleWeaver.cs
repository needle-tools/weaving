using System.Collections.Generic;
using Fody;
using UnityEngine;

namespace ExampleProjectWeaver
{
    public class ModuleWeaver : BaseModuleWeaver
    {

        public override void Execute()
        {
            Debug.Log("Executing module weaver");//" " + ModuleDefinition.Assembly.FullName);
        }

        public override IEnumerable<string> GetAssembliesForScanning()
        {
            yield return "netstandard";
            yield return "mscorlib";
        }
    }
}
