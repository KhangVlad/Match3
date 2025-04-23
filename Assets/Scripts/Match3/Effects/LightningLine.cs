using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Match3.Enums;


namespace Match3.Shares
{
    public class LightningLine : BaseVisualEffect
    {
        private LineRenderer _lr;

        private void Awake()
        {
            _lr = GetComponent<LineRenderer>();
        }

        public override void Initialize()
        {
            VfxID = VisualEffectID.LightingLine;
        }


        private void SetLinePositions(Vector2 p1, Vector2 p2)
        {
            _lr.positionCount = 2;
            _lr.SetPosition(0, p1);
            _lr.SetPosition(1, p2);
        }


        public void ActiveLightningLine(Vector2 p1, Vector2 p2, float moveTime, float duration)
        {
            StartCoroutine(ActiveLightningLineCoroutine(p1, p2, moveTime, duration));
        }
        private IEnumerator ActiveLightningLineCoroutine(Vector2 p1, Vector2 p2, float moveTime, float duration)
        {
            if (moveTime < 0)
            {
                SetLinePositions(p1, p2);
                yield break;
            }

            float elapsedTime = 0f;

            while (elapsedTime < moveTime)
            {
                // calculate interpolation factor (0 - 1)
                float t = elapsedTime / moveTime;

                // update lne position
                Vector2 targetPosition = Vector2.Lerp(p1, p2, t);
                SetLinePositions(p1, targetPosition);


                // Increment elapsed time
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            // Ensure the final position is exact
            SetLinePositions(p1, p2);

            // wait a little bit when line reach max length
            if (duration > moveTime)
            {
                yield return new WaitForSeconds(duration - moveTime);
            }

            _lr.positionCount = 0;

            //Destroy(this.gameObject);
            ReturnToPool();
        }
    }
}