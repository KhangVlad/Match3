using UnityEngine;
using System.Collections;
using DG.Tweening;
using Match3.Enums;

namespace Match3.Shares
{
    public class ColorBurstLine : BaseVisualEffect
    {
        public int segmentCount = 25;
        public float springSpeed = 30f;
        public float damping = 0.9f;
        public float curveTightness = 0.5f;

        public Vector2 startPoint = Vector2.zero;

        private LineRenderer line;
        private Vector2[] positions;
        private Vector2[] velocities;
        private Vector2 target;

        private Transform _targetTransform;
        private bool _moveTargetTransform = false;
        private Vector2 _offset;

        public override void Initialize()
        {
            VfxID = VisualEffectID.ColorBurstLine;
        }


        void Awake()
        {
            line = GetComponent<LineRenderer>();
            line.positionCount = segmentCount;

            positions = new Vector2[segmentCount];
            velocities = new Vector2[segmentCount];

            for (int i = 0; i < segmentCount; i++)
                positions[i] = startPoint;

            target = startPoint;
        }


        public void SetLine(Vector2 pointA, Vector2 pointB)
        {
            startPoint = pointA;
            target = pointB;
            line.enabled = true;
            line.positionCount = segmentCount;
        }

        public void CallbackLine()
        {
            target = startPoint;
        }

        public void SetTargetTransform(Transform transform, Vector2 offset)
        {
            _targetTransform = transform;
            this._offset = offset;
        }


        void Update()
        {
            // First segment is always the shoulder (anchor)
            positions[0] = startPoint;

            // Last segment chases the target (fist)
            velocities[segmentCount - 1] += (target - positions[segmentCount - 1]) * springSpeed * Time.deltaTime;
            velocities[segmentCount - 1] *= Mathf.Pow(damping, Time.deltaTime * 60);
            positions[segmentCount - 1] += velocities[segmentCount - 1] * Time.deltaTime;

            // Middle segments follow the next (like a rope)
            for (int i = segmentCount - 2; i > 0; i--)
            {
                Vector2 direction = positions[i + 1] - positions[i];
                velocities[i] += direction * springSpeed * Time.deltaTime;
                velocities[i] *= Mathf.Pow(damping, Time.deltaTime * 60);
                positions[i] += velocities[i] * Time.deltaTime;
            }

            // Curve it using interpolation (make it look rubbery)
            for (int i = 0; i < segmentCount; i++)
            {
                float t = i / (float)(segmentCount - 1);
                Vector2 curvedPos = BezierLerp(startPoint, positions[segmentCount - 1], t, curveTightness);
                line.SetPosition(i, curvedPos);
            }

            if (_targetTransform != null && line.positionCount > 0)
            {
                if (_moveTargetTransform == false)
                {
                    if (Vector2.Distance(_targetTransform.position, positions[segmentCount - 1] + _offset) < 0.1f)
                    {
                        _moveTargetTransform = true;
                    }
                }
                else
                {
                    _targetTransform.position = positions[segmentCount - 1] + _offset;
                }
            }
        }

        // Custom interpolation that bends toward the middle like an elastic rope
        Vector2 BezierLerp(Vector2 a, Vector2 b, float t, float bendFactor)
        {
            Vector2 mid = (a + b) * 0.5f + Vector2.Perpendicular(b - a).normalized * bendFactor;
            Vector2 p1 = Vector2.Lerp(a, mid, t);
            Vector2 p2 = Vector2.Lerp(mid, b, t);
            return Vector2.Lerp(p1, p2, t);
        }

        private void ResetLine()
        {
            for (int i = 0; i < segmentCount; i++)
            {
                positions[i] = startPoint;
                velocities[i] = Vector2.zero;
            }

            target = startPoint;
            line.positionCount = 0;
        }
        public override void ReturnToPool()
        {
            base.ReturnToPool();
            ResetLine();
            _targetTransform = null;
            _offset = default;
            line.enabled = false;
            _moveTargetTransform = false;
        }
    }

}
