using System;
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

		private static readonly IList<IPatchGUIProvider> _guiProviders = new List<IPatchGUIProvider>();

		public static void Register(IPatchGUIProvider prov)
		{
			if (prov is null) return;
			if (!_guiProviders.Contains(prov)) _guiProviders.Add(prov);
		}

		private static WeaverSettings Settings => WeaverSettings.instance;
		private Vector2 scroll;

		private bool patchesFoldout
		{
			get => SessionState.GetBool("Needle.PatchesFoldout", false);
			set => SessionState.SetBool("Needle.PatchesFoldout", value);
		}

		public override void OnGUI(string searchContext)
		{
			InitStyles();

			EditorGUILayout.BeginVertical();

			scroll = EditorGUILayout.BeginScrollView(scroll);
			EditorGUILayout.BeginVertical();
			Settings.PatchOnBuild = EditorGUILayout.ToggleLeft("Patch On Build", Settings.PatchOnBuild);

			DrawPatchesGUI();
			GUILayout.Space(10);

			foreach (var gui in _guiProviders)
			{
				try
				{
					EditorGUILayout.LabelField(ObjectNames.NicifyVariableName(gui.GetType().Name), EditorStyles.boldLabel);
					gui.OnDrawGUI();
				}
				catch (Exception e)
				{
					Debug.LogException(e);
				}
			}

			EditorGUILayout.EndVertical();
			GUILayout.Space(20);
			EditorGUILayout.EndScrollView();

			GUILayout.FlexibleSpace();

			EditorGUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.EndVertical();
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
		private static bool patchesAreKnown => byName.Count > 0;

		private void DrawPatchesGUI()
		{
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Needle Patches", EditorStyles.boldLabel);
			// GUILayout.FlexibleSpace();
			EditorGUILayout.EndHorizontal();

			var foldoutText = "Patch Methods";
			if (byName.Count <= 0)
			{
				foldoutText += " <Will load on open>";
			}

			foldoutText += " (Disabled: " + Settings.DisabledPatches.Count + ")";
			EditorGUILayout.BeginHorizontal();
			patchesFoldout = EditorGUILayout.Foldout(patchesFoldout, foldoutText, true);
			GUILayout.FlexibleSpace();
			if (patchesAreKnown)
			{
				EditorGUI.BeginDisabledGroup(Settings.DisabledPatches.Count <= 0);
				if (GUILayout.Button(patchesAreKnown ? "All" : "Enable all"))
				{
					Undo.RecordObject(Settings, "Enable patches");
					Settings.DisabledPatches.Clear();
					Settings.Save();
				}

				EditorGUI.EndDisabledGroup();
				if (GUILayout.Button("None"))
				{
					Undo.RecordObject(Settings, "Disable patches");
					foreach (var etr in byName.Values.SelectMany(p => p.Patches)) Settings.SetPatchSettingState(etr, false);
					Settings.Save();
				}

				if (GUILayout.Button("Invert"))
				{
					Undo.RecordObject(Settings, "Invert patches");
					foreach (var etr in byName.Values.SelectMany(p => p.Patches)) Settings.SetPatchSettingState(etr, Settings.IsDisabled(etr));
					Settings.Save();
				}
			}

			EditorGUILayout.EndHorizontal();

			if (patchesFoldout)
			{
				EditorGUI.indentLevel++;
				if (byName.Count <= 0)
				{
					var pm = PatchMethodDatabase.AllPatchedMethods();
					foreach (var patch in pm)
					{
						if (!byName.ContainsKey(patch.TargetBaseName)) byName.Add(patch.TargetBaseName, new PatchGUIState() {Foldout = false});
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
					var active = state.Patches.Count(p => !Settings.IsDisabled(p.TargetFullName));
					state.Foldout = EditorGUILayout.Foldout(state.Foldout, name + " (Enabled: " + active + "/" + patches.Count + ")", true);
					GUILayout.FlexibleSpace();
					if (GUILayout.Button("All"))
					{
						Undo.RecordObject(Settings, "Enable all " + name);
						foreach (var patch in patches)
						{
							Settings.SetPatchSettingState(patch.TargetFullName, true);
						}

						Settings.Save();
					}

					if (GUILayout.Button("None"))
					{
						Undo.RecordObject(Settings, "Disable all " + name);
						foreach (var patch in patches)
						{
							Settings.SetPatchSettingState(patch.TargetFullName, false);
						}

						Settings.Save();
					}

					if (GUILayout.Button("Invert"))
					{
						Undo.RecordObject(Settings, "Invert Patches state " + name);
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
							var displayName = patch.PatchName;
							if (patch.FoundTarget) displayName = patch.Target.ToString();
							var isActive = !Settings.IsDisabled(patch.TargetFullName);
							var style = isActive ? patchTitleActive : patchTitleInactive;
							var ps = EditorGUILayout.ToggleLeft(
								new GUIContent(displayName, patch.TargetFullName + "\n" + patch.Target + "\nin " + patch.PatchFullName),
								!Settings.IsDisabled(patch.TargetFullName), style);
							if (EditorGUI.EndChangeCheck())
							{
								Undo.RecordObject(Settings, (ps ? "Enable" : "Disable") + " patch: " + patch.PatchFullName);
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