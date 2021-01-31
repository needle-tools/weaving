using Fody.Weavers.InputDeviceWeaver;
using Mono.Cecil;
using needle.Weaver;
using UnityEditor;
using UnityEngine;
using Actions = Fody.Weavers.InputDeviceWeaver.Actions;

namespace Patch.InputDevices.Editor
{
	internal class WebGlPatchGui : IPatchGUIProvider
	{
		[InitializeOnLoadMethod]
		private static void Init() => WeaverSettingsProvider.Register(new WebGlPatchGui());
			
		public void OnDrawGUI()
		{
			EditorGUILayout.BeginHorizontal();
			if(GUILayout.Button("Patch WebGL XR Assemblies"))
				Actions.Weave_WebGL_Patches();
			GUILayout.FlexibleSpace();
			EditorGUILayout.EndHorizontal();
		}
	}
}