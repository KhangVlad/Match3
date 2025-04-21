using UnityEngine;

namespace LapTest
{
    [RequireComponent(typeof(LineRenderer))]
    public class LineController : MonoBehaviour
    {
        public int segmentCount = 25;
        public float springSpeed = 30f;
        public float damping = 0.9f;
        public float curveTightness = 0.5f;

   
        private LineRenderer line;
        private Vector2[] positions;
        private Vector2[] velocities;
        private Vector2 _target;
        private Vector2 _startPoint = Vector2.zero;

        void Start()
        {
            line = GetComponent<LineRenderer>();
            line.positionCount = segmentCount;

            positions = new Vector2[segmentCount];
            velocities = new Vector2[segmentCount];

            for (int i = 0; i < segmentCount; i++)
                positions[i] = _startPoint;

            _target = _startPoint;
        }

        void Update()
        {
            // Click to set punch direction
            if (Input.GetMouseButtonDown(0))
            {
                _target = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            }

            if (Input.GetMouseButtonDown(1))
            {
                Debug.Log($"A:  {_startPoint}");
                _target = _startPoint + (_startPoint - _target).normalized * 0.1f;
            }


            // First segment is always the shoulder (anchor)
            positions[0] = _startPoint;

            // Last segment chases the target (fist)
            velocities[segmentCount - 1] += (_target - positions[segmentCount - 1]) * springSpeed * Time.deltaTime;
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
                Vector2 curvedPos = BezierLerp(_startPoint, positions[segmentCount - 1], t, curveTightness);
                line.SetPosition(i, curvedPos);
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
    }
}
