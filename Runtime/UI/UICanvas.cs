using UnityEngine;

namespace Tomo.UI
{
	// Interface
	public interface ICanvas
	{
		int SetOrder(int order);
		float GetScaleFactor();
		Vector3 ScreenToCanvasPosition(Vector3 position);
		Vector3 ScreenToCanvasPosition(Vector2 position);
		Vector3 CanvasToScreenPosition(Vector2 position);
		Vector3 CanvasToScreenPosition(Vector3 position);
	}

	[RequireComponent(typeof(Canvas))]
	public class UICanvas : MonoBehaviour, ICanvas
	{
		// Properties
		public int SetOrder(int order) { _canvas.sortingOrder = order; return SyncOrders(); }
		public int GetOrder() { return _canvas.sortingOrder; }
		protected Canvas GetCanvas() { return _canvas; }

		// Public
		public void SetCamera(Camera camera)
		{
			if (_canvas.renderMode == RenderMode.ScreenSpaceOverlay) { return; }
			_canvas.worldCamera = camera;
		}

		// Private
		private static int SyncChildOrders(Transform transform, int baseOrder)
		{
			int order = baseOrder;
			int orderOffset = 0;

			foreach (Transform child in transform)
			{
				if (child.TryGetComponent<ICanvas>(out var gameCanvas))
				{
					int canvasOffset = gameCanvas.SetOrder(order);
					order += canvasOffset;
					orderOffset += canvasOffset;
					continue;
				}

				if (child.TryGetComponent<Canvas>(out var canvas))
				{
					order++;
					orderOffset++;
					canvas.overrideSorting = true;
					canvas.sortingOrder = order;
				}

				var particleSystems = child.GetComponents<ParticleSystem>();
				if (particleSystems.Length > 0)
				{
					++order;
					++orderOffset;
					foreach (ParticleSystem particleSystem in particleSystems)
					{
						particleSystem.GetComponent<Renderer>().sortingOrder = order;
					}
				}

				int childOffset = SyncChildOrders(child, order);
				order += childOffset;
				orderOffset += childOffset;
			}

			return orderOffset;
		}

		// Protected
		protected virtual void Awake()
		{
			_canvas = GetComponent<Canvas>();
			if (_canvas == null) { return; }

			Camera camera = Camera.main;
			if (!_useMainCamera)
			{
				camera = UIManager.Instance.GetCamera();
			}
			_canvas.worldCamera = camera;
			if (_canvas.renderMode == RenderMode.ScreenSpaceOverlay) { _canvas.worldCamera = null; }
		}
		protected virtual int SyncOrders()
		{
			int order = _canvas.sortingOrder;
			int orderOffset = 0;

			var particleSystems = GetComponents<ParticleSystem>();
			if (particleSystems.Length > 0)
			{
				++order;
				++orderOffset;
				foreach (ParticleSystem particleSystem in particleSystems)
				{
					particleSystem.GetComponent<Renderer>().sortingOrder = order;
				}
			}

			return (orderOffset + SyncChildOrders(transform, order));
		}

		// ICanvas implementation
		int ICanvas.SetOrder(int order) => SetOrder(order);
		float ICanvas.GetScaleFactor() { return _canvas.scaleFactor; }

		Vector3 ICanvas.ScreenToCanvasPosition(Vector2 position) { return (this as ICanvas).ScreenToCanvasPosition((Vector3)position); }
		Vector3 ICanvas.ScreenToCanvasPosition(Vector3 position)
		{
			if (_canvas.renderMode != RenderMode.ScreenSpaceOverlay)
			{
				if (_canvas.worldCamera != null)
					position = _canvas.worldCamera.ScreenToWorldPoint(position);
				else
					Debug.LogWarning($"[UICanvas] {gameObject.name} misses world camera!");
			}
			return position;
		}

		Vector3 ICanvas.CanvasToScreenPosition(Vector2 position) { return (this as ICanvas).CanvasToScreenPosition((Vector3)position); }
		Vector3 ICanvas.CanvasToScreenPosition(Vector3 position)
		{
			if (_canvas.renderMode != RenderMode.ScreenSpaceOverlay)
			{
				position = _canvas.worldCamera.WorldToScreenPoint(position);
			}
			return position;
		}

		// Serialized properties
		[SerializeField] private bool _useMainCamera;

		// Variable
		private Canvas _canvas;
	}
}
