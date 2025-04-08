using UnityEngine;
using FMODUnity;
using FMOD.Studio;

namespace Match3
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Volume")] [Range(0f, 1f)] private float _masterVolume = 0.5f;
        [Range(0f, 1f)] private float _musicVolume = 0.5f;

        [Range(0f, 1f)] private float _soundVolume = 0.5f;
        private Bus _masterBus;
        private Bus _musicBus;
        private Bus _soundBus;


        //--------------------------------------------------------------------
        // 1: Using the EventReference type will present the designer with
        //    the UI for selecting events.
        //--------------------------------------------------------------------

        //[Header("Music")][SerializeField] private EventReference _music;

        [Header("SFX")] [SerializeField] private EventReference _button;
        [SerializeField] private EventReference _match3;
        [SerializeField] private EventReference _match4;
        [SerializeField] private EventReference _match5;
        [SerializeField] private EventReference _match6;
        [SerializeField] private EventReference _win;
        [SerializeField] private EventReference _gameover;
        [SerializeField] private EventReference _closeBtn;

        [SerializeField] private EventReference _backgroundMusic;

        //--------------------------------------------------------------------
        // 2: Using the EventInstance class will allow us to manage an event
        //    over its lifetime, including starting, stopping and changing 
        //    parameters.
        //--------------------------------------------------------------------
        private EventInstance _musicEventInstance;


        #region Properties

        public float MasterVolume
        {
            get => _masterVolume;
        }

        public float MusicVolume
        {
            get => _musicVolume;
        }

        public float SoundVolume
        {
            get => _soundVolume;
        }

        public float DefaultMasterVolume { get; } = 1.0f;
        public float DefaultMusicVolume { get; } = 0.5f;
        public float DefaultSoundVolume { get; } = 0.5f;

        #endregion

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(this.gameObject);
            }

            RuntimeManager.LoadBank("Master", true);
            // DontDestroyOnLoad(this.gameObject);
            _masterBus = RuntimeManager.GetBus("bus:/");
            _musicBus = RuntimeManager.GetBus("bus:/MUSIC");
            _soundBus = RuntimeManager.GetBus("bus:/SFX");
        }

        private void Start()
        {
            //PlayMusic(_music);
            PlayMusic(_backgroundMusic);
        }

        private void OnDestroy()
        {
            //StopMusic();
        }


        #region Volume

        public void SetMasterVolume(float volume)
        {
            _masterVolume = volume;
            _masterBus.setVolume(_masterVolume);
        }

        public void SetMusicVolume(float volume)
        {
            _musicVolume = volume;
            _musicBus.setVolume(_musicVolume);
        }

        public void SetSoundVolume(float volume)
        {
            _soundVolume = volume;
            _soundBus.setVolume(_soundVolume);
        }

        #endregion


        private void PlayMusic(EventReference musicEventReference)
        {
            _musicEventInstance = CreateInstance(musicEventReference);
            _musicEventInstance.start();
        }

        private void StopMusic()
        {
            _musicEventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        }

        public EventInstance CreateInstance(EventReference eventReference)
        {
            EventInstance instance = RuntimeManager.CreateInstance(eventReference);
            return instance;
        }

        public void PlayButtonSfx()
        {
            RuntimeManager.PlayOneShot(_button);
        }

        public void PlayMatch3Sfx()
        {
            RuntimeManager.PlayOneShot(_match3);
        }

        public void PlayMatch4Sfx()
        {
            RuntimeManager.PlayOneShot(_match4);
        }

        public void PlayMatch5Sfx()
        {
            RuntimeManager.PlayOneShot(_match5);
        }

        public void PlayColorBurstSFX()
        {
            RuntimeManager.PlayOneShot(_match6);
        }

        public void PlayWinSfx()
        {
            RuntimeManager.PlayOneShot(_win);
        }

        public void PlayGameoverSfx()
        {
            RuntimeManager.PlayOneShot(_gameover);
        }


        public void PlayCloseBtnSfx()
        {
            RuntimeManager.PlayOneShot(_closeBtn);
        }
    }
}