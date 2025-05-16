using UnityEngine;
using Match3.Enums;
using UnityEngine.Pool;

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

        public virtual void SetParticleQuantity(int quantity = 5)
        {
            var main = _ps.main;
            main.maxParticles = quantity * 2;
            ParticleSystem.EmissionModule emission = _ps.emission;
            emission.burstCount = quantity; // emit 50 particles per second
        }

        public virtual void SetLifeTime(float lifeTime = 0.65f)
        {
            var main = _ps.main;
            main.startLifetime = lifeTime;
        }

         public virtual void SetSpeed(float startSpeed = 3.5f, float endSpeed = 5.5f)
        {
            var main = _ps.main;
            main.startSpeed = new ParticleSystem.MinMaxCurve(startSpeed,endSpeed);
        }
    }
}
