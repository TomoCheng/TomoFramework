using UnityEngine;

namespace Tomo.Editor
{
	[CreateAssetMenu(fileName = "PackageInstallerSettings", menuName = "TomoFramework/Settings")]
	public class PackageInstallerSettings : ScriptableObject
	{
		[System.Serializable]
		public struct PackageRequirement
		{
			public string PackageId;
			public string InstallSource;
		}

		[System.Serializable]
		public struct VersionDefine
		{
			public string Name;
			public string Expression;
			public string Symbol;
		}

		[Header("Assembly Definition Settings")]
		public string TargetAsmdefName = "Tomo.Framework.Runtime";

		[Header("Auto-Install Packages")]
		public PackageRequirement[] PackagesToInstall = new PackageRequirement[]
		{
			new PackageRequirement { PackageId = "com.unity.addressables", InstallSource = "com.unity.addressables" },
			new PackageRequirement { PackageId = "com.cysharp.unitask", InstallSource = "https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask" },
			new PackageRequirement { PackageId = "com.unity.2d.sprite", InstallSource = "com.unity.2d.sprite" }
		};

		[Header("ASMDEF Version Defines")]
		public VersionDefine[] VersionDefines = new VersionDefine[]
		{
			new VersionDefine { Name = "com.cysharp.unitask", Expression = "2.0.0", Symbol = "TOMO_UNITASK" },
			new VersionDefine { Name = "com.unity.addressables", Expression = "1.0.0", Symbol = "TOMO_ADDRESSABLES" },
			new VersionDefine { Name = "com.unity.2d.sprite", Expression = "1.0.0", Symbol = "TOMO_2D_SPRITE" }
		};
	}
}