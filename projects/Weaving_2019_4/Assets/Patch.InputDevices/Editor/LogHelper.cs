using UnityEngine;

namespace Fody.Weavers.InputDeviceWeaver
{
	public static class LogHelper
	{
		public static string LogMe(object log)
		{
			Debug.Log(log);
			return "";
		}
	}
}