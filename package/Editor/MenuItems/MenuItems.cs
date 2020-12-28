using System.Collections.Generic;
using System.IO;
using Mono.Cecil;
using UnityEditor;

namespace needle.Weaver
{
	internal static class MenuItems
	{
		[MenuItem(Constants.MenuItemBase + nameof(OpenFolders))]
		private static void OpenFolders()
		{
			EditorUtility.RevealInFinder(Constants.EngineAssembliesPath);
			EditorUtility.RevealInFinder(Constants.ManualPatchingAssembliesPath);
		}
	}
}