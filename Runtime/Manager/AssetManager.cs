using UnityEngine;
#if TOMO_ADDRESSABLES
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
#endif
using System.Threading;
#if TOMO_UNITASK
using Cysharp.Threading.Tasks;
#endif

namespace Tomo.Asset
{
	public class AssetManager : Core.Singleton<AssetManager>
	{
		// Public
#if TOMO_UNITASK
		public async UniTask<T_ASSET> LoadAsset<T_ASSET>(string address, CancellationToken cancellationToken = default) where T_ASSET : class
		{
			AsyncOperationHandle<T_ASSET> handle = Addressables.LoadAssetAsync<T_ASSET>(address);

			var loadAsset = await LoadProcess(handle, cancellationToken);
			if (cancellationToken.IsCancellationRequested) { Addressables.Release(handle); }

			return (cancellationToken.IsCancellationRequested ? null : loadAsset);
		}
		public async UniTask<GameObject> Instantiate(string address, Transform parent = null, bool inWorldSpace = false, CancellationToken cancellationToken = default)
		{
			AsyncOperationHandle<GameObject> handle = Addressables.InstantiateAsync(address, parent, inWorldSpace);

			var instance = await LoadProcess(handle, cancellationToken);
			if (cancellationToken.IsCancellationRequested) { Addressables.ReleaseInstance(handle); }

			return (cancellationToken.IsCancellationRequested ? null : instance);
		}
		public async UniTask<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance?> LoadScene(string address, UnityEngine.SceneManagement.LoadSceneMode loadMode, bool activateOnLoad = true, CancellationToken cancellationToken = default)
		{
			AsyncOperationHandle<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance> handle = Addressables.LoadSceneAsync(address, loadMode, activateOnLoad);

			var scene = await LoadProcess(handle, cancellationToken);
			if (cancellationToken.IsCancellationRequested) { Addressables.Release(handle); }

			return (cancellationToken.IsCancellationRequested ? null : scene);
		}
#endif
#if TOMO_ADDRESSABLES
		public void Release<T_ASSET>(T_ASSET obj) => Addressables.Release(obj);

		public void ReleaseInstance(GameObject instance) => Addressables.ReleaseInstance(instance);
#endif

		// Private
#if TOMO_UNITASK
		private async UniTask<T_ASSET> LoadProcess<T_ASSET>(AsyncOperationHandle<T_ASSET> handle, CancellationToken cancellationToken)
		{
			try
			{
				//var (isCanceled, result) = await handle.WithCancellation(cancellationToken).SuppressCancellationThrow();
				await UniTask.CompletedTask;
				bool isCanceled = false;
				T_ASSET result = default(T_ASSET);
				return isCanceled ? default : result;
			}
			catch (System.Exception e)
			{
				Debug.LogException(e);
				return default;
			}
		}
#endif
#if TOMO_UNITASK
		protected override async UniTask Initialize()
		{
			await Addressables.InitializeAsync();
		}
#endif
	}
}