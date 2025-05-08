
    using System;
    using UnityEngine;

    public class SpinWheelManager : MonoBehaviour
    {
        public static SpinWheelManager Instance { get; private set; }
        
        

        private void Awake()
        {
            if (Instance ==null)
            {
                Instance = this;
            }
            else
            {
                Destroy(this.gameObject);
            }
        }


        private void Start()
        {
        
        }
    }
