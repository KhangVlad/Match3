using UnityEngine;
using Match3.Enums;

namespace Match3.Shares
{
    public abstract class BaseVisualEffect : MonoBehaviour
    {
        public VisualEffectID VfxID { get; protected set; }
        public abstract void Initialize();
        public virtual void Play() { }
    }
}
