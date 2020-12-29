using System.Collections.Generic;
using System.IO;
using Mono.Cecil;
using UnityEditor;

namespace needle.Weaver
{
	internal static class MenuItems
	{
		[MenuItem(Constants.MenuItemBase + nameof(OpenFolders), priority = -1000)]
		private static void OpenFolders()
		{
			EditorUtility.RevealInFinder(Constants.EngineAssembliesPath);
			EditorUtility.RevealInFinder(Constants.ManualPatchingAssembliesPath);
		}
	}
}