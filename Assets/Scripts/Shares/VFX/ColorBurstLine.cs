using UnityEngine;
using System.Collections;
using DG.Tweening;
using Match3.Enums;

namespace Match3.Shares
{
    public class ColorBurstLine : BaseVisualEffect
    {
        private LineRenderer _lr;

        public override void Initialize()
        {
            VfxID = VisualEffectID.ColorBurstLine;
        }

        private void Awake()
        {
            _lr = GetComponent<LineRenderer>();
        }

        public void SetLine(Vector2 p1, Vector2 p2, float duration)
        {
            StartCoroutine(SetLineCoroutine(p1, p2, duration));
        }
        private IEnumerator SetLineCoroutine(Vector2 p1, Vector2 p2, float duration)
        {
            _lr.positionCount = 2;
            _lr.SetPosition(0, p1);
            _lr.SetPosition(1, p2);

            // wait a little bit when line reach max length
            yield return new WaitForSeconds(duration);
            _lr.positionCount = 0;
            Destroy(this.gameObject);
        }
    }

}
