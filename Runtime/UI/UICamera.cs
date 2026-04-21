using UnityEngine;

namespace Tomo.UI
{
	[RequireComponent(typeof(Camera))]
	[DisallowMultipleComponent]
	public class UICamera : MonoBehaviour
	{
		public void Setup()
		{
			gameObject.layer = LayerMask.NameToLayer("UI");
		}
	}
}