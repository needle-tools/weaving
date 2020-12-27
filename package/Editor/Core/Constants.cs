using System;
using System.IO;
using UnityEditor;

namespace needle.Weaver
{
	public static class Constants
	{
		// assembly paths
		public static string EditorInstallationPath => Path.GetDirectoryName(EditorApplication.applicationPath);
		public static string EngineAssembliesPath => EditorInstallationPath + "/Data/Managed";
		public static string WebGLAssembliesPath => EditorInstallationPath + "/Data/PlaybackEngines/WebGLSupport/Managed";
		
		// backup paths
		public static string ManualPatchingAssembliesPath => Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/needle/weaver/manual";
		public static string AssembliesBackupPath => Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/needle/weaver/backup";

	}
}