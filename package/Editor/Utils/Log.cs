using UnityEngine;

namespace needle.Weaver
{
	public static class Log
	{
		
		public static void Gray(object msg, object context = null)
		{
			var str = msg.ToString();
			Debug.Log(str.Gray(), context as Object);
		}
	}
}