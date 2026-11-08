#if TOMO_UNITASK
using Cysharp.Threading.Tasks;
#endif
using UnityEngine;

namespace Tomo.Core
{
	public abstract class SingletonBase : MonoBehaviour
	{
#if TOMO_UNITASK
		public virtual UniTask Initialize()
		{
			return UniTask.CompletedTask;
		}
#endif
	}

	public abstract class Singleton<T_TYPE> : SingletonBase where T_TYPE : SingletonBase
	{
		// Properties
		public static T_TYPE Instance { get { return GetInstance(); } }

		// Public
#if TOMO_UNITASK
		public override UniTask Initialize()
		{
			_instance = this as T_TYPE;
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