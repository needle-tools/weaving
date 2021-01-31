using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.Cecil;
using needle.Weaver;
using UnityEditor;
using UnityEngine;

namespace Fody.Weavers.InputDeviceWeaver
{
	public static class Actions
	{
		[MenuItem(Constants.MenuItemBase + nameof(Weave_WebGL_Patches))]
		public static void Weave_WebGL_Patches()
		{
			var res = new DefaultAssemblyResolver();
			res.AddSearchDirectory(Constants.WebGLAssembliesPath);
            
			if (!Directory.Exists(Constants.ManualPatchingAssembliesPath))
				Directory.CreateDirectory(Constants.ManualPatchingAssembliesPath);

			var dllNames = new string[]
			{
				"UnityEngine.SubsystemsModule.dll",
				"UnityEngine.XRModule.dll",
				"UnityEngine.VRModule.dll"
			};

			var dlls = Directory.GetFiles(Constants.WebGLAssembliesPath, "*.dll", SearchOption.AllDirectories).Where(f => dllNames.Any(f.EndsWith));
			var assemblies = new HashSet<string>();
			foreach (var dll in dlls) assemblies.Add(dll);
			AssemblyWeaver.ProcessAssemblies(assemblies, res);
		}
	}
}