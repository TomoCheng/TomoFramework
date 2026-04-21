using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace Tomo.Editor
{
	[InitializeOnLoad]
	public class AutoPackageInstaller
	{
		static AutoPackageInstaller()
		{
			InstallMissingPackages();
			EditorApplication.delayCall += EnsureVersionDefines;
		}

		private static void InstallMissingPackages()
		{
			PackageInstallerSettings settings = GetSettings();
			if (settings == null || settings.PackagesToInstall == null) return;

			ListRequest request = Client.List(true);
			EditorApplication.update += Progress;

			void Progress()
			{
				if (request.IsCompleted)
				{
					EditorApplication.update -= Progress;
					if (request.Status == StatusCode.Success)
					{
						var installedPackageIds = request.Result.Select(p => p.name).ToList();
						foreach (var requirement in settings.PackagesToInstall)
						{
							bool isInstalled = installedPackageIds.Contains(requirement.PackageId);
							if (!isInstalled)
							{
								Debug.Log($"[TomoFramework] Missing package [{requirement.PackageId}] detected, installing: {requirement.InstallSource}");
								Client.Add(requirement.InstallSource);
							}
						}
					}
				}
			}
		}

		private static void EnsureVersionDefines()
		{
			PackageInstallerSettings settings = GetSettings();
			if (settings == null || settings.VersionDefines == null) return;

			string[] guids = AssetDatabase.FindAssets($"{settings.TargetAsmdefName} t:AssemblyDefinitionAsset");
			if (guids.Length == 0) return;

			string path = AssetDatabase.GUIDToAssetPath(guids[0]);
			string fullPath = Path.GetFullPath(path);
			string content = File.ReadAllText(fullPath);

			bool isChanged = false;

			foreach (var define in settings.VersionDefines)
			{
				if (!content.Contains(define.Name))
				{
					content = InjectDefine(content, define.Name, define.Expression, define.Symbol);
					isChanged = true;
				}
			}

			if (isChanged)
			{
				File.WriteAllText(fullPath, content);
				AssetDatabase.ImportAsset(path);
				Debug.Log($"<color=#00FF00>[TomoFramework]</color> Automatically updated {settings.TargetAsmdefName}.asmdef with all required symbols.");
			}
		}

		private static string InjectDefine(string json, string name, string expression, string define)
		{
			string aEntry = $"{{\"name\": \"{name}\", \"expression\": \"{expression}\", \"define\": \"{define}\"}}";
			if (json.Contains("\"versionDefines\": []"))
				return json.Replace("\"versionDefines\": []", $"\"versionDefines\": [\n        {aEntry}\n    ]");
			else if (json.Contains("\"versionDefines\": ["))
				return json.Replace("\"versionDefines\": [", $"\"versionDefines\": [\n        {aEntry},");
			return json;
		}

		private static PackageInstallerSettings GetSettings()
		{
			string[] guids = AssetDatabase.FindAssets("t:PackageInstallerSettings");
			if (guids.Length == 0) return null;
			string path = AssetDatabase.GUIDToAssetPath(guids[0]);
			return AssetDatabase.LoadAssetAtPath<PackageInstallerSettings>(path);
		}
	}
}