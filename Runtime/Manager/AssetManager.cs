using UnityEngine;
#if TOMO_ADDRESSABLES
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
#endif
using System.Threading;
#if TOMO_UNITASK
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
#endif

namespace Tomo.Asset
{
	public class AssetManager : Core.Singleton<AssetManager>
	{
		// Public
#if TOMO_UNITASK
		public override async UniTask Initialize()
		{
			await base.Initialize();
			await Addressables.InitializeAsync();
		}
		public async UniTask<T_ASSET> LoadAsset<T_ASSET>(string address, CancellationToken cancellationToken = default) where T_ASSET : class
		{
			var (success, asset) = await LoadProcess(Addressables.LoadAssetAsync<T_ASSET>(address), cancellationToken);
			return success ? asset : null;
		}
		public async UniTask<IList<T_ASSET>> LoadAssetsByLabel<T_ASSET>(string label, CancellationToken cancellationToken = default) where T_ASSET : class
		{
			var (success, assets) = await LoadProcess(Addressables.LoadAssetsAsync<T_ASSET>(label, null), cancellationToken);
			return success ? assets : null;
		}
		public async UniTask<GameObject> Instantiate(string address, Transform parent = null, bool inWorldSpace = false, CancellationToken cancellationToken = default)
		{
			var (success, instance) = await LoadProcess(Addressables.InstantiateAsync(address, parent, inWorldSpace), cancellationToken);
			return success ? instance : null;
		}
		public async UniTask<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance?> LoadScene(string address, UnityEngine.SceneManagement.LoadSceneMode loadMode, bool activateOnLoad = true, CancellationToken cancellationToken = default)
		{
			var (success, scene) = await LoadProcess(Addressables.LoadSceneAsync(address, loadMode, activateOnLoad), cancellationToken);
			return success ? scene : (UnityEngine.ResourceManagement.ResourceProviders.SceneInstance?)null;
		}
#endif
#if TOMO_ADDRESSABLES
		public void Release<T_ASSET>(T_ASSET obj) => Addressables.Release(obj);

		public void ReleaseInstance(GameObject instance) => Addressables.ReleaseInstance(instance);
#endif

		// Private
#if TOMO_UNITASK
		// 取消、失敗或例外時一律釋放 handle,避免 Addressables 參考計數洩漏
		private async UniTask<(bool success, T_ASSET result)> LoadProcess<T_ASSET>(AsyncOperationHandle<T_ASSET> handle, CancellationToken cancellationToken)
		{
			try
			{
				var (isCanceled, result) = await handle
					.ToUniTask(cancellationToken: cancellationToken)
					.SuppressCancellationThrow();

				if (!isCanceled && handle.Status == AsyncOperationStatus.Succeeded)
				{
					return (true, result);
				}
			}
			catch (System.Exception e)
			{
				Debug.LogException(e);
			}

			if (handle.IsValid()) { Addressables.Release(handle); }
			return (false, default);
		}
#endif
	}
}