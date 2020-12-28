using System.Collections.Generic;
using System.IO;
using Mono.Cecil;
using needle.Weaver;
using UnityEditor;

namespace Fody.Weavers.InputDeviceWeaver
{
	public static class Actions
	{
		[MenuItem(Constants.MenuItemBase + nameof(WeaveWebGlInputDevices))]
		public static void WeaveWebGlInputDevices()
		{
			var res = new DefaultAssemblyResolver();
			res.AddSearchDirectory(Constants.WebGLAssembliesPath);
            
			if (!Directory.Exists(Constants.ManualPatchingAssembliesPath))
				Directory.CreateDirectory(Constants.ManualPatchingAssembliesPath);
			var dlls = Directory.GetFiles(Constants.WebGLAssembliesPath, "UnityEngine.XRModule.dll", SearchOption.AllDirectories);
			var assemblies = new HashSet<string>();
			foreach (var dll in dlls) assemblies.Add(dll);
			FodyAssemblyProcessor.ProcessAssemblies(assemblies, res);
		}
	}
}