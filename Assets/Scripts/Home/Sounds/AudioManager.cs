// using UnityEngine;
// using FMODUnity;
//
// public class AudioManager : MonoBehaviour
// {
//     public static AudioManager Instance { get; private set; }
//     [SerializeField] private EventReference _buttonClickSound;
//
//     private void Awake()
//     {
//         if (Instance == null)
//         {
//             Instance = this;
//             DontDestroyOnLoad(this.gameObject);
//         }
//         else
//         {
//             Destroy(gameObject);
//         }
//     }
//
//
//     public void PlayButtonClickSound()
//     {
//         RuntimeManager.PlayOneShot(_buttonClickSound);
//     }
// }