using System.Collections.Generic;
using System.IO;
using Mono.Cecil;
using UnityEditor;

namespace needle.Weaver
{
	internal static class MenuItems
	{
		[MenuItem("Weaving/" + nameof(OpenFolders))]
		private static void OpenFolders()
		{
			EditorUtility.RevealInFinder(Constants.EngineAssembliesPath);
			EditorUtility.RevealInFinder(Constants.ManualPatchingAssembliesPath);
		}

		[MenuItem("Weaving/" + nameof(WeaveWebGLXRModule))]
		private static void WeaveWebGLXRModule()
		{
			var res = new DefaultAssemblyResolver();
			res.AddSearchDirectory(Constants.WebGLAssembliesPath);
            
			if (!Directory.Exists(Constants.ManualPatchingAssembliesPath))
				Directory.CreateDirectory(Constants.ManualPatchingAssembliesPath);
			var dlls = Directory.GetFiles(Constants.WebGLAssembliesPath, "UnityEngine.XRModule.dll", SearchOption.AllDirectories);
			var assemblies = new HashSet<string>();
			foreach (var dll in dlls) assemblies.Add(dll);
			FodyAssemblyProcessor.ProcessAssemblies(assemblies, res, false);
		}

	}
}