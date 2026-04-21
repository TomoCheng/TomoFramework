#if TOMO_UNITASK
using Cysharp.Threading.Tasks;
#endif
using UnityEngine;

namespace Tomo.Core
{
	public abstract class Singleton<T_TYPE> : MonoBehaviour where T_TYPE : MonoBehaviour
	{
		// Properties
		public static T_TYPE Instance { get{ return GetInstance(); } }

		// Protected
		protected virtual void Awake()
		{
			_instance = this as T_TYPE;
		}
#if TOMO_UNITASK
		protected virtual UniTask Initialize()
		{
			return UniTask.CompletedTask;
		}
#endif
        // Private
        private static T_TYPE GetInstance()
		{
			if (_instance == null)
			{
				_instance = FindAnyObjectByType<T_TYPE>();

				if (_instance == null)
				{
					Debug.LogError($"[Tomo.Framework] {typeof(T_TYPE).Name} No instance exist");
				}
			}
			return _instance;
		}

		// Variable
		private static T_TYPE _instance;
	}
}