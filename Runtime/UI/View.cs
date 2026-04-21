using System.Threading;
using UnityEngine;

namespace Tomo.UI
{
	public abstract class View : UICanvas
	{
		// Properties
		public ViewLayer ViewLayer { get; set; }
		public bool IsOpen => _isOpen;

		// Public
		public abstract void Initialize();
		public bool Open(ViewLayer viewLayer)
		{
			ViewLayer = viewLayer;
			return OnOpen();
		}
		public bool Close()
		{
			return OnClose();
		}

		// Private

		// Protected
		protected virtual bool OnOpen()
		{
			if (IsOpen) { return false; }

			_viewCloseCancelToken = new CancellationTokenSource();
			_isOpen = true;
			return true;
		}
		protected virtual bool OnClose()
		{
			if (!IsOpen) { return false; }

			_viewCloseCancelToken?.Cancel();
			_isOpen = false;
			return true;
		}

		// Variables
		private CancellationTokenSource _viewCloseCancelToken;
		private bool _isOpen;
	}
}
