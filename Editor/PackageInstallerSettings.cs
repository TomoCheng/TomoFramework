using UnityEngine;

namespace Tomo.Editor
{
    [CreateAssetMenu(fileName = "PackageInstallerSettings", menuName = "TomoFramework/PackageInstallerSettings")]
    public class PackageInstallerSettings : ScriptableObject
    {
        [System.Serializable]
        public struct PackageRequirement
        {
            public string PackageId;
            public string InstallSource;
            public VersionDefine VersionDefine;
            public AssetPackageImport AssetImport;
        }

        [System.Serializable]
        public struct VersionDefine
        {
            public string Expression;
            public string Symbol;
        }

        [System.Serializable]
        public struct AssetPackageImport
        {
            public string TargetFolder;
            public string SourcePackagePath;
        }

        [Header("Assembly Definition Settings")]
        public string TargetAsmdefName = "Tomo.Framework.Runtime";

        [Header("Auto-Install Packages")]
        public PackageRequirement[] PackagesToInstall = new PackageRequirement[]
        {
            new PackageRequirement
            {
                PackageId = "com.unity.addressables",
                InstallSource = "com.unity.addressables",
                VersionDefine = new VersionDefine { Expression = "1.0.0", Symbol = "TOMO_ADDRESSABLES" }
            },
            new PackageRequirement
            {
                PackageId = "com.cysharp.unitask",
                InstallSource = "https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask",
                VersionDefine = new VersionDefine { Expression = "2.0.0", Symbol = "TOMO_UNITASK" }
            },
            new PackageRequirement
            {
                PackageId = "com.unity.2d.sprite",
                InstallSource = "com.unity.2d.sprite",
                VersionDefine = new VersionDefine { Expression = "1.0.0", Symbol = "TOMO_2D_SPRITE" }
            },
            new PackageRequirement
            {
                PackageId = "com.unity.ugui",
                InstallSource = "com.unity.ugui",
                VersionDefine = new VersionDefine { Expression = "1.0.0", Symbol = "TOMO_UGUI" },
                AssetImport = new AssetPackageImport { TargetFolder = "Assets/TextMesh Pro", SourcePackagePath = "Packages/com.unity.ugui/Package Resources/TMP Essential Resources.unitypackage" }
            }
        };
    }
}