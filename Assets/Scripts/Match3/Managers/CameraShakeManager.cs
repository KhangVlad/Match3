using UnityEngine;
using System.Collections;

namespace Match3
{
    public class CameraShakeManager : MonoBehaviour
    {
        public static CameraShakeManager Instance { get; private set; }

        private Vector3 originalPosition;
        private Coroutine shakeCoroutine;

        private void Awake()
        {
            // Singleton
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);

        
            CameraController.OnCameraPositionUpdated += UpdateCameraPosition;
        }

        private void OnDestroy()
        {
            CameraController.OnCameraPositionUpdated -= UpdateCameraPosition;
        }

        public void Shake(float intensity, float duration)
        {
            if (shakeCoroutine != null)
                StopCoroutine(shakeCoroutine);

            shakeCoroutine = StartCoroutine(ShakeCoroutine(intensity, duration));
        }

        private IEnumerator ShakeCoroutine(float intensity, float duration)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                Vector2 offset = Random.insideUnitCircle * intensity;
                transform.localPosition = originalPosition + new Vector3(offset.x, offset.y, 0f);
                elapsed += Time.deltaTime;
                yield return null;
            }

            transform.localPosition = originalPosition;
        }

        private void UpdateCameraPosition(Vector3 position)
        {
            originalPosition = position;
        }

    }
}
