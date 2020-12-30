using Mono.Cecil;
using UnityEngine;

namespace needle.Weaver
{
	public static class MethodReferenceExtensions
	{
		public static bool DoSignaturesMatch(this MethodReference m0, MethodReference m1)
		{
			if (m0.ReturnType.FullName != m1.ReturnType.FullName)
			{
				// Debug.LogWarning("Return type mismatch");
				return false;
			}
			
			if (m0.Parameters.Count != m1.Parameters.Count)
			{
				// Debug.LogWarning("Parameters mismatch");
				return false;
			}

			if (m0.GenericParameters.Count != m1.GenericParameters.Count)
			{
				// Debug.LogWarning("Generic Parameters mismatch");
				return false;
			}
			

			for (var index = 0; index < m0.Parameters.Count; index++)
			{
				if (m0.Parameters[index].ParameterType.FullName != m1.Parameters[index].ParameterType.FullName)
				{
					// Debug.Log("Parameter mismatch - " + m0.Parameters[index] + " <> " + m1.Parameters[index]);
					return false;
				}
			}

			for (var index = 0; index < m0.GenericParameters.Count; index++)
			{
				if (m0.GenericParameters[index].FullName != m1.GenericParameters[index].FullName)
				{
					// Debug.Log("GenericParam Mismatch");
					return false;
				}
			}

			return true;
		}

	}
}