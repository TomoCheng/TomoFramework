using UnityEngine;

namespace Tomo.UI
{
	public abstract class Unit : MonoBehaviour
	{
        // Public
        public virtual void OnResolutionChanged() { }
        public void Refresh()
        {
            OnRefresh();
        }
        public void Tick()
        {
            OnTick();
        }

        // Protected
        protected virtual void OnRefresh() { }
        protected virtual void OnTick() { }
    }
}
