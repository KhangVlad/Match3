using UnityEngine;
using UnityEngine.Rendering.Universal;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;

public static class Light2DExtensions
{
    public static TweenerCore<float, float, FloatOptions> DOIntensity(this Light2D target, float endValue, float duration)
    {
        return DOTween.To(() => target.intensity, x => target.intensity = x, endValue, duration)
            .SetTarget(target);
    }
}