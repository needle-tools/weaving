using UnityEditor;

namespace needle.Weaver
{
	public static class Actions
	{
		[MenuItem(Constants.MenuItemBase + nameof(OpenFolders), priority = -1000)]
		public static void OpenFolders()
		{
			EditorUtility.RevealInFinder(Constants.EngineAssembliesPath);
			EditorUtility.RevealInFinder(Constants.ManualPatchingAssembliesPath);
		}
	}
}