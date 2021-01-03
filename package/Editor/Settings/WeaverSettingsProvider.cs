using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace needle.Weaver
{
	public class WeaverSettingsProvider : SettingsProvider
	{
		[SettingsProvider]
		public static SettingsProvider Create()
		{
			try
			{
				WeaverSettings.instance.Save();
				return new WeaverSettingsProvider("Project/Needle/Weaver", SettingsScope.Project);
			}
			catch (System.Exception e)
			{
				Debug.Log(e);
			}

			return null;
		}

		private WeaverSettingsProvider(string path, SettingsScope scopes, IEnumerable<string> keywords = null) : base(path, scopes, keywords)
		{
		}


		private static WeaverSettings Settings => WeaverSettings.instance;
		private Vector2 scroll;
		private bool patchesFoldout = false;

		public override void OnGUI(string searchContext)
		{
			EditorGUI.indentLevel++;
			InitStyles();

			EditorGUILayout.BeginVertical();

			scroll = EditorGUILayout.BeginScrollView(scroll);
			EditorGUILayout.BeginVertical();

			Settings.PatchOnBuild = EditorGUILayout.Toggle("Allow Patch on Build", Settings.PatchOnBuild);

			DrawPatchesGUI();

			EditorGUILayout.EndVertical();
			GUILayout.Space(20);
			EditorGUILayout.EndScrollView();

			GUILayout.FlexibleSpace();

			EditorGUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.EndVertical();
			EditorGUI.indentLevel--;
		}

		private GUIStyle patchTitleInactive, patchTitleActive, descriptionStyle;

		private void InitStyles()
		{
			if (descriptionStyle == null)
				descriptionStyle = new GUIStyle(EditorStyles.label)
				{
					wordWrap = true,
					normal = {textColor = new Color(.5f, .5f, .5f)}
				};

			if (patchTitleActive == null)
			{
				patchTitleActive = new GUIStyle(EditorStyles.label)
				{
					fontStyle = FontStyle.Bold,
				};
			}

			if (patchTitleInactive == null)
				patchTitleInactive = new GUIStyle(patchTitleActive)
				{
					normal = {textColor = new Color(.5f, .5f, .5f),},
				};
		}

		private class PatchGUIState
		{
			public bool Foldout;
			public List<PatchMethodDatabase.Patch> Patches = new List<PatchMethodDatabase.Patch>();
		}

		private static readonly Dictionary<string, PatchGUIState> byName = new Dictionary<string, PatchGUIState>();

		private void DrawPatchesGUI()
		{
			var foldoutText = "Patched Methods";
			if (byName.Count <= 0) foldoutText += " <Will load on open>";
			
			EditorGUILayout.BeginHorizontal();
			patchesFoldout = EditorGUILayout.Foldout(patchesFoldout, foldoutText, true);
			GUILayout.FlexibleSpace();
			EditorGUI.BeginDisabledGroup(Settings.DisabledPatches.Count <= 0);
			if (GUILayout.Button("Reset"))
			{
				Settings.DisabledPatches.Clear();
				Settings.Save();
			}
			EditorGUI.EndDisabledGroup();
			EditorGUILayout.EndHorizontal();
			
			if (patchesFoldout)
			{
				EditorGUI.indentLevel++;
				if (byName.Count <= 0)
				{
					var pm = PatchMethodDatabase.AllPatchedMethods();
					foreach (var patch in pm)
					{
						if (!byName.ContainsKey(patch.TargetBaseName)) byName.Add(patch.TargetBaseName, new PatchGUIState() {Foldout = true});
						byName[patch.TargetBaseName].Patches.Add(patch);
					}

					foreach (var kvp in byName)
					{
						kvp.Value.Patches = kvp.Value.Patches.OrderByDescending(p => p.FoundTarget).ThenBy(p => p.PatchName).ToList();
					}
				}

				foreach (var kvp in byName)
				{
					var name = kvp.Key;
					var state = kvp.Value;
					var patches = state.Patches;
					EditorGUILayout.BeginHorizontal();
					state.Foldout = EditorGUILayout.Foldout(state.Foldout, name + " [" + patches.Count + "]", true);
					GUILayout.FlexibleSpace();
					if (GUILayout.Button("Invert"))
					{
						foreach (var patch in patches)
						{
							Settings.SetPatchSettingState(patch.TargetFullName, Settings.IsDisabled(patch.TargetFullName));
						}

						Settings.Save();
					}

					EditorGUILayout.EndHorizontal();
					if (state.Foldout)
					{
						EditorGUI.indentLevel++;
						foreach (var patch in patches)
						{
							EditorGUI.BeginDisabledGroup(!patch.FoundTarget);
							EditorGUI.BeginChangeCheck();
							var ps = EditorGUILayout.ToggleLeft(
								new GUIContent(patch.PatchName, patch.TargetFullName + "\n" + patch.Target + "\nin " + patch.TargetFullName),
								!Settings.IsDisabled(patch.TargetFullName));
							if (EditorGUI.EndChangeCheck())
							{
								Settings.SetPatchSettingState(patch.TargetFullName, ps);
								Settings.Save();
							}

							EditorGUI.EndDisabledGroup();
						}

						EditorGUI.indentLevel--;
					}
				}

				EditorGUI.indentLevel--;
			}
		}
	}
}