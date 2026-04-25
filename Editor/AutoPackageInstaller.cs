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
            EditorApplication.delayCall += ProcessAutoImports;
        }

        private static void InstallMissingPackages()
        {
            PackageInstallerSettings settings = GetSettings();
            if (settings == null || settings.PackagesToInstall == null) return;

            ListRequest request = Client.List();
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
                                Debug.Log($"[TomoFramework] Missing package [{requirement.PackageId}] detected, starting to install...");
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
            if (settings == null || settings.PackagesToInstall == null) return;

            string[] guids = AssetDatabase.FindAssets($"{settings.TargetAsmdefName} t:AssemblyDefinitionAsset");
            if (guids.Length == 0) return;

            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            string fullPath = Path.GetFullPath(path);
            string content = File.ReadAllText(fullPath);

            bool isChanged = false;

            foreach (var req in settings.PackagesToInstall)
            {
                if (!content.Contains(req.VersionDefine.Symbol))
                {
                    content = InjectDefine(content, req.PackageId, req.VersionDefine.Expression, req.VersionDefine.Symbol);
                    isChanged = true;
                }
            }

            if (isChanged)
            {
                File.WriteAllText(fullPath, content);
                AssetDatabase.ImportAsset(path);
                Debug.Log($"<color=#FFBB44>[TomoFramework]</color> {settings.TargetAsmdefName}.asmdef setup complete.");
            }
        }

        private static void ProcessAutoImports()
        {
            PackageInstallerSettings settings = GetSettings();
            if (settings == null || settings.PackagesToInstall == null) return;

            foreach (var requirement in settings.PackagesToInstall)
            {
                if (string.IsNullOrEmpty(requirement.AssetImport.TargetFolder) ||
                    string.IsNullOrEmpty(requirement.AssetImport.SourcePackagePath)) continue;

                if (AssetDatabase.IsValidFolder(requirement.AssetImport.TargetFolder)) continue;

                var packageObj = AssetDatabase.LoadAssetAtPath<Object>(requirement.AssetImport.SourcePackagePath);

                if (packageObj != null)
                {
                    Debug.Log($"<color=#FFBB44>[TomoFramework]</color> Missing asset {requirement.PackageId} detected, starting to import...");
                    AssetDatabase.ImportPackage(requirement.AssetImport.SourcePackagePath, false);
                }
            }
        }

        private static string InjectDefine(string json, string name, string expression, string define)
        {
            string entry = $"{{\"name\": \"{name}\", \"expression\": \"{expression}\", \"define\": \"{define}\"}}";
            if (json.Contains("\"versionDefines\": []"))
                return json.Replace("\"versionDefines\": []", $"\"versionDefines\": [\n        {entry}\n    ]");
            else if (json.Contains("\"versionDefines\": ["))
                return json.Replace("\"versionDefines\": [", $"\"versionDefines\": [\n        {entry},");
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