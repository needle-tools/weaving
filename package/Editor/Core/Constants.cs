using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace needle.Weaver
{
	public static class Constants
	{
		public static string EditorInstallationPath => Path.GetDirectoryName(EditorApplication.applicationPath);
		public static string WebGLAssembliesPath => EditorInstallationPath + "/Data/PlaybackEngines/WebGLSupport/Managed";
		public static string EngineAssembliesPath => EditorInstallationPath + "/Data/Managed";
		public static string ManualPatchingAssembliesPath => Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/needle/weaver/manual";

	}
}