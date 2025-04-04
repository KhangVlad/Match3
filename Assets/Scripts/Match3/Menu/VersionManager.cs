using UnityEngine;

namespace Match3
{
    public class VersionManager : MonoBehaviour
    {
        public static VersionManager Instance { get; private set; }
        public const int CURRENT_VERSION = 2;

        private void Awake()
        {
            if(Instance != null && Instance != this)
            {
                Destroy(this.gameObject);
                return;
            }
            Instance = this;
        }


    }
}
