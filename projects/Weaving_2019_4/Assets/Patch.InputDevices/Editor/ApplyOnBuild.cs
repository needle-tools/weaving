#if UNITY_WEBGL
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace Fody.Weavers.InputDeviceWeaver
{
	internal class ApplyOnBuild : IPreprocessBuildWithReport
	{
		public int callbackOrder => 1000;
		public void OnPreprocessBuild(BuildReport report)
		{
			Actions.WeaveWebGlInputDevices();
		}
	}
}
#endif