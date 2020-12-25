using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.Cecil;
using UnityEngine;

namespace Fody
{
	public static class DllBackupHelper
	{
		public static bool IsModified(this ModuleDefinition module)
		{
			var processedMarkerType = module.Types.FirstOrDefault(t => t.FullName == "ProcessedByFody");
			return processedMarkerType != null;
		}
		
		public static void GetFromBackupIfAvailable(string assemblyPath, bool isModified)
		{
			// collect all files to restore
			if (!EnsureBackupAndGetPathInBackupStoreIfExists(assemblyPath, isModified, out var backupPath)) return;
			var copyList = new List<(string source, string target)> {(backupPath, assemblyPath)};
			foreach (var symbolPath in EnumerateSymbols(assemblyPath))
			{
				var res  = EnsureBackupAndGetPathInBackupStoreIfExists(symbolPath, isModified, out var backupSymbolPath);
				if (res) copyList.Add((backupSymbolPath, symbolPath));
			}
			// restore files
			try
			{
				foreach(var (src, target) in copyList)
				{
					if (File.Exists(src))
					{
						Debug.Log("Restore from backup " + target);
						File.Copy(src, target, true);
					}
				}
			}
			catch (Exception e)
			{
				Debug.LogException(e);
			}
		}
		
		/// <summary>
		/// copies the assembly to a backup directory if not modified
		/// returns false if assembly does not exist or exception occured
		/// </summary>
		private static bool EnsureBackupAndGetPathInBackupStoreIfExists(string assemblyPath, bool isModified, out string backupStoreFullPath)
		{
			backupStoreFullPath = null;
			try
			{
				var backupPath = GetBackupRelativeAssemblyPath(assemblyPath);
				if (string.IsNullOrEmpty(backupPath)) return false;
				var local = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
				var sub = "/needle/weaver/backup/ " + Application.unityVersion;
				var full = local + sub + "/" + backupPath;
				var dir = Path.GetDirectoryName(full);
				if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);
				if (!isModified && !File.Exists(full)) 
				{
					Debug.Log("Add " + assemblyPath + " to backup\n" + full);
					File.Copy(assemblyPath, full);
				}
				backupStoreFullPath = full;
				return true;
			}
			catch (Exception e)
			{
				Debug.LogException(e);
			}

			return false;
		}

		private static string GetBackupRelativeAssemblyPath(string assemblyPath)
		{
			assemblyPath = assemblyPath.Replace("\\", "/"); 
			const string k_ScriptAssembliesFolderName = "/ScriptAssemblies/";
			var pathIndex = assemblyPath.LastIndexOf(k_ScriptAssembliesFolderName, StringComparison.InvariantCulture);
			if (pathIndex > 0)
				return assemblyPath.Substring(pathIndex);

			const string k_DataPath = "Editor/Data/Managed/";
			pathIndex = assemblyPath.LastIndexOf(k_DataPath, StringComparison.InvariantCulture);
			if (pathIndex > 0)
				return assemblyPath.Substring(pathIndex);

			Debug.LogWarning("Backup path not found for " + assemblyPath);
			return null;
		}

		private static IEnumerable<string> EnumerateSymbols(string assemblyPath)
		{
			var symbol = Path.ChangeExtension(assemblyPath, ".pdb");
			if(File.Exists(symbol))
				yield return symbol;
			symbol = assemblyPath + ".mdb";
			if(File.Exists(symbol))
				yield return symbol;
		}
	}
}