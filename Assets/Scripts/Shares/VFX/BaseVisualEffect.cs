using UnityEngine;
using Match3.Enums;
using UnityEngine.Pool;
using log4net.Appender;

namespace Match3.Shares
{
    public abstract class BaseVisualEffect : MonoBehaviour
    {
        protected ObjectPool<BaseVisualEffect> pool;
        public VisualEffectID VfxID { get; protected set; }
        private ParticleSystem _ps;

        private void Awake()
        {
            _ps = GetComponent<ParticleSystem>();
        }

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

        public virtual void SetParticleQuantity(int quantity)
        {
            var main = _ps.main;
            main.maxParticles = quantity * 2;
            ParticleSystem.EmissionModule emission = _ps.emission;
            emission.burstCount = quantity; // emit 50 particles per second
        }
    }
}
