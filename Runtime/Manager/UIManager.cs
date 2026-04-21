using System.Collections.Generic;
using Tomo.Core;
using UnityEngine;
using UCamera = UnityEngine.Camera;

namespace Tomo.UI
{
	public class UIManager : Singleton<UIManager>
	{
		// Public
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

		// Private
		private UCamera GetCamera(bool isCreate)
		{
			if (_camera == null && isCreate)
			{
				GameObject cameraObject = new GameObject("UICamera");
				cameraObject.transform.SetParent(_UIRoot.parent, false);

				var UICamera = cameraObject.AddComponent<UICamera>();
				UICamera.Setup();

				_camera = cameraObject.GetComponent<UCamera>();
			}
			return _camera;
		}
		private bool BootView<T_VIEW>(string name, ViewLayer layer, System.Action<T_VIEW> initAction = null) where T_VIEW : View
		{
			T_VIEW view = GetView<T_VIEW>(name, true);
			if (view == null) { return false; }

			if (initAction != null) { initAction(view); }

			view.SetCamera(GetCamera());
			view.gameObject.transform.SetAsLastSibling();
			view.Open(layer);


			UpdateMenuOrderAndLayer();
			return true;
		}
		private T_VIEW GetView<T_VIEW>(string name, bool isCreate) where T_VIEW : View
		{
			if (!_viewMap.ContainsKey(name)) { _viewMap.Add(name, null); }

			View view = _viewMap[name];
			bool viewNotExist = view == null;
			if (viewNotExist)
			{
				view = null;
				_viewMap[name] = null;
				if (isCreate)
				{
					view = Create<T_VIEW>(name, false);
					view.Initialize();
					_viewMap[name] = view;
				}
			}
			if (view is T_VIEW) { return (T_VIEW)view; }
			else { return default; }
		}
		public T_TYPE Create<T_TYPE>(string name, bool isLocalize = true)
		{
			GameObject loadedObject = null;

			if (!_loadedResourceMap.TryGetValue(name, out loadedObject))
			{
				return default;
			}

			RectTransform resource = loadedObject.transform as RectTransform;
			if (resource == null)
			{
				return default;
			}

			RectTransform instance = Instantiate(resource);
			instance.name = name;
			instance.localPosition = resource.localPosition;
			instance.localRotation = resource.localRotation;
			instance.localScale = resource.localScale;

			T_TYPE component = instance.GetComponent<T_TYPE>();
			UnityEngine.Assertions.Assert.IsTrue(component != null, $"Created instance [{name}] dose not have component [{typeof(T_TYPE)}]");

			return component;
		}
		private void UpdateMenuOrderAndLayer()
		{
			var viewList = new List<View>();
			int aIndex = 0;
			foreach (RectTransform rect in _UIRoot)
			{
				if (rect.TryGetComponent(out View view))
				{
					int order = GetBaseOrder(view.ViewLayer) + aIndex++;
					aIndex += view.SetOrder(order);
					viewList.Add(view);
				}
			}
			viewList.Sort((viewA, viewB) => viewA.GetOrder() - viewB.GetOrder());
		}
		private int GetBaseOrder(ViewLayer layer)
		{
			return (int)layer * MAX_ORDER_IN_LAYER;
		}

		// Serialized properties
		[SerializeField] private Transform _UIRoot;

		// Variable
		private int _screenWidth;
		private int _screenHeight;
		private UCamera _camera;
		private Dictionary<string, View> _viewMap;
		private Dictionary<string, GameObject> _loadedResourceMap;

		// Constants
		private const int MAX_ORDER_IN_LAYER = 1000;
	}

}