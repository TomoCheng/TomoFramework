#if TOMO_UNITASK
using Cysharp.Threading.Tasks;
#endif
using System.Collections.Generic;
using System.Threading;
using Tomo.Asset;
using Tomo.Core;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UCamera = UnityEngine.Camera;

namespace Tomo.UI
{
	public class UIManager : Singleton<UIManager>
	{
		// Public
#if TOMO_UNITASK
		public override async UniTask Initialize()
		{
			await base.Initialize();

			_viewMap = new Dictionary<string, View>();
			_loadedResourceMap = new Dictionary<string, GameObject>();
			_loadingTaskMap = new Dictionary<string, UniTask<bool>>();
			_residentUIs = new HashSet<string>();
			_labelOwnedUIs = new HashSet<string>();
			GetCamera(true);

			await PreloadUIs();
		}
#endif
		public UCamera GetCamera()
		{
			return GetCamera(false);
		}
		public void CheckResolutionChanged()
		{
			if (Screen.width != _screenWidth || Screen.height != _screenHeight)
			{
				foreach (Unit unit in GetComponentsInChildren<Unit>())
				{
					unit.OnResolutionChanged();
				}

				_screenWidth = Screen.width;
				_screenHeight = Screen.height;
			}
		}
		public T_VIEW BootView<T_VIEW>(string name, ViewLayer layer = ViewLayer.DEFAULT, System.Action<T_VIEW> initAction = null) where T_VIEW : View
		{
			return BootViewInternal(name, layer, initAction);
		}
#if TOMO_UNITASK
		public async UniTask<T_VIEW> BootViewAsync<T_VIEW>(string name, ViewLayer layer = ViewLayer.DEFAULT, System.Action<T_VIEW> initAction = null, CancellationToken cancellationToken = default) where T_VIEW : View
		{
			bool loaded = await LoadUI(name, cancellationToken);
			if (!loaded) { return null; }

			return BootViewInternal(name, layer, initAction);
		}

		// Load / Unload
		public async UniTask<bool> LoadUI(string name, CancellationToken cancellationToken = default)
		{
			if (_loadedResourceMap.ContainsKey(name)) { return true; }

			// 同名載入共用一個 task;載入本身不吃呼叫端的 token,
			// 避免第一個呼叫者取消時連帶讓其他等待者失敗
			if (!_loadingTaskMap.TryGetValue(name, out UniTask<bool> loadingTask))
			{
				loadingTask = LoadInternal(name).Preserve();
				_loadingTaskMap.Add(name, loadingTask);
			}
			var (isCanceled, loaded) = await loadingTask
				.AttachExternalCancellation(cancellationToken)
				.SuppressCancellationThrow();
			return !isCanceled && loaded;
		}
		public async UniTask<bool> LoadUI(AssetLabelReference label, CancellationToken cancellationToken = default)
		{
			var assets = await AssetManager.Instance.LoadAssetsByLabel<GameObject>(label.labelString, cancellationToken);
			if (assets == null) { return false; }

			foreach (GameObject asset in assets)
			{
				if (!_loadedResourceMap.ContainsKey(asset.name))
				{
					_loadedResourceMap[asset.name] = asset;
					_labelOwnedUIs.Add(asset.name);
				}
			}
			return true;
		}
		public void UnloadUI(string name)
		{
			if (_residentUIs.Contains(name))
			{
				Debug.LogWarning($"[Tomo.Framework] UnloadUI [{name}] skipped, resident UI");
				return;
			}
			// 以 label 批次載入的資源由 list handle 持有,無法逐一 Release
			if (_labelOwnedUIs.Contains(name))
			{
				Debug.LogWarning($"[Tomo.Framework] UnloadUI [{name}] skipped, loaded by label");
				return;
			}
			if (_viewMap.TryGetValue(name, out View view) && view != null)
			{
				Debug.LogWarning($"[Tomo.Framework] UnloadUI [{name}] skipped, view is still alive");
				return;
			}
			_viewMap.Remove(name);

			if (_loadedResourceMap.TryGetValue(name, out GameObject asset))
			{
				_loadedResourceMap.Remove(name);
				AssetManager.Instance.Release(asset);
			}
		}
		public bool IsLoaded(string name)
		{
			return _loadedResourceMap.ContainsKey(name);
		}
#endif

		// Private
#if TOMO_UNITASK
		private async UniTask PreloadUIs(CancellationToken cancellationToken = default)
		{
			await LoadUI(_preloadLabel, cancellationToken);

			foreach (string name in _loadedResourceMap.Keys)
			{
				_residentUIs.Add(name);
			}
		}
		private async UniTask<bool> LoadInternal(string name)
		{
			GameObject asset = await AssetManager.Instance.LoadAsset<GameObject>(name, this.GetCancellationTokenOnDestroy());
			_loadingTaskMap.Remove(name);

			if (asset == null)
			{
				Debug.LogError($"[Tomo.Framework] Failed to load UI [{name}]");
				return false;
			}
			_loadedResourceMap.Add(name, asset);
			return true;
		}
#endif
		private UCamera GetCamera(bool isCreate)
		{
			if (_camera == null && isCreate)
			{
				GameObject cameraObject = new GameObject("UICamera");
				cameraObject.transform.SetParent(_UIRoot.parent, false);

				var uiCamera = cameraObject.AddComponent<UICamera>();
				uiCamera.Setup();

				_camera = cameraObject.GetComponent<UCamera>();
			}
			return _camera;
		}
		private T_VIEW BootViewInternal<T_VIEW>(string name, ViewLayer layer, System.Action<T_VIEW> initAction) where T_VIEW : View
		{
			T_VIEW view = GetViewInternal<T_VIEW>(name, true);
			if (view == null) { return null; }

			if (initAction != null) { initAction(view); }

			view.SetCamera(GetCamera());
			view.transform.SetAsLastSibling();
			view.Open(layer);

			UpdateMenuOrderAndLayer();
			return view;
		}
		private T_VIEW GetViewInternal<T_VIEW>(string name, bool isCreate) where T_VIEW : View
		{
			_viewMap.TryGetValue(name, out View view);

			// view == null 也涵蓋「已被 Destroy 的假 null」,這裡順手清掉舊參考
			if (view == null)
			{
				_viewMap.Remove(name);
				if (!isCreate) { return null; }

				view = Create<T_VIEW>(name);
				if (view == null) { return null; }

				view.Initialize();
				_viewMap[name] = view;
			}

			T_VIEW typedView = view as T_VIEW;
			if (typedView == null)
			{
				Debug.LogWarning($"[Tomo.Framework] View [{name}] exists but is not [{typeof(T_VIEW).Name}]");
			}
			return typedView;
		}
		private T_TYPE Create<T_TYPE>(string name) where T_TYPE : Component
		{
			if (!_loadedResourceMap.TryGetValue(name, out GameObject loadedObject))
			{
				Debug.LogError($"[Tomo.Framework] UI [{name}] is not loaded, call LoadUI first");
				return null;
			}

			RectTransform resource = loadedObject.transform as RectTransform;
			if (resource == null)
			{
				Debug.LogError($"[Tomo.Framework] UI [{name}] is not a RectTransform");
				return null;
			}

			RectTransform instance = Instantiate(resource, _UIRoot);
			instance.name = name;

			T_TYPE component = instance.GetComponent<T_TYPE>();
			if (component == null)
			{
				Debug.LogError($"[Tomo.Framework] Created instance [{name}] does not have component [{typeof(T_TYPE)}]");
				Destroy(instance.gameObject);
				return null;
			}
			return component;
		}
		private void UpdateMenuOrderAndLayer()
		{
			var layerIndexMap = new Dictionary<ViewLayer, int>();
			foreach (RectTransform rect in _UIRoot)
			{
				if (rect.TryGetComponent(out View view))
				{
					layerIndexMap.TryGetValue(view.ViewLayer, out int index);
					int order = GetBaseOrder(view.ViewLayer) + index;
					index += 1 + view.SetOrder(order);
					layerIndexMap[view.ViewLayer] = index;
				}
			}
		}
		private int GetBaseOrder(ViewLayer layer)
		{
			return (int)layer * MAX_ORDER_IN_LAYER;
		}

		// Serialized properties
		[SerializeField] private Transform _UIRoot;
		[SerializeField] private AssetLabelReference _preloadLabel;

		// Variable
		private int _screenWidth;
		private int _screenHeight;
		private UCamera _camera;
		private Dictionary<string, View> _viewMap;
		private Dictionary<string, GameObject> _loadedResourceMap;
#if TOMO_UNITASK
		private Dictionary<string, UniTask<bool>> _loadingTaskMap;
#endif
		private HashSet<string> _residentUIs;
		private HashSet<string> _labelOwnedUIs;

		// Constants
		private const int MAX_ORDER_IN_LAYER = 1000;
	}
}