using UnityEngine;
using Match3.Enums;
using UnityEngine.Pool;

namespace Match3.Shares
{
    public abstract class BaseVisualEffect : MonoBehaviour
    {
        protected ObjectPool<BaseVisualEffect> pool;
        public VisualEffectID VfxID { get; protected set; }
        public abstract void Initialize();
        public virtual void Play(float duration = 1f)
        {
            ReturnToPool(duration);
        }
        public void SetPool(ObjectPool<BaseVisualEffect> pool)
        {
            this.pool = pool;
        }
        public virtual void ReturnToPool()
        {
            pool?.Release(this);
        }

        public virtual void ReturnToPool(float duration)
        {
            Invoke(nameof(ReturnToPool), duration);
        }
    }
}
