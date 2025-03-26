using UnityEngine;
using System.Collections.Generic;

namespace Match3
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }


        private void Awake()
        {
            Instance = this;

            Application.targetFrameRate = 60;
        }
    }

}
