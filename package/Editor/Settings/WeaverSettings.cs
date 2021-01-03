using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

namespace needle.Weaver
{
	[FilePath("ProjectSettings/NeedleWeaverSettings.asset", FilePathAttribute.Location.ProjectFolder)]
	public class WeaverSettings : ScriptableSingleton<WeaverSettings>
	{
		public void Save() => Save(true);
		public bool PatchOnBuild = false;

		public bool IsDisabled(PatchMethodDatabase.Patch patch) => DisabledPatches.Contains(patch.TargetFullName);
		public bool IsDisabled(string patchFullName) => DisabledPatches.Contains(patchFullName);
 
		public List<string> DisabledPatches = new List<string>();
		
		public void SetPatchSettingState(PatchMethodDatabase.Patch patch, bool enabled)
		{
			SetPatchSettingState(patch.TargetFullName, enabled);
		}
		
		public void SetPatchSettingState(string fullname, bool enabled)
		{
			if (enabled)
			{
				var cnt = DisabledPatches.RemoveAll(p => p == fullname);
			}
			else if (!DisabledPatches.Contains(fullname))
			{
				DisabledPatches.Add(fullname);
			}
		}
	}

	[System.Serializable]
	public class SerializedPatchSetting
	{
		public string PatchFullName;
		public bool Enabled;
	}
}