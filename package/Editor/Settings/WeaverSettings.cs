namespace needle.Weaver
{
	[FilePath("ProjectSettings/NeedleWeaverSettings.asset", FilePathAttribute.Location.ProjectFolder)]
	public class WeaverSettings : ScriptableSingleton<WeaverSettings>
	{
		public void Save() => Save(true);
		public bool PatchOnBuild = false;
	}

	[System.Serializable]
	public class PatchConfig
	{
		
	}
}