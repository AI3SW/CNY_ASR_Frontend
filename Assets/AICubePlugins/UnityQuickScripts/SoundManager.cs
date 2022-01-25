using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System;
using UnityEngine.Audio;
using UnityEngine.Events;

namespace UnityDecoupledBehavior
{
    public class SoundManager : MonoBehaviour
    {
        [SerializeField]
        [Range(SoundManager.FADE_MIN, SoundManager.FADE_MAX)]
        float ScreensaverVol = 0.3f;

        [SerializeField]
        [Range(SoundManager.FADE_MIN, SoundManager.FADE_MAX)]
        float gameVol = 0.1f;

        [SerializeField]
        [Range(SoundManager.FADE_MIN, SoundManager.FADE_MAX)]
        float mainVol = 0.4f;
        [SerializeField]
        AudioClip defaultBtnSound;

        [SerializeField]
        List<AudioClip> BGMs;
        [SerializeField]
        List<AudioClip> SFXs;
        [SerializeField]
        AudioSource BTN_SoundSrc;

        [SerializeField]
        AudioSource BGM_SoundSrc;

        [SerializeField]
        AudioSource SFX_SoundSrc;

        #region AudioSettings
        public const float FADE_MIN = 0.0001f;
        public const float FADE_MAX = 2f;
        public float MasterVolume
        {

            get => currentMaster;
            set
            {
                currentMaster = Mathf.Clamp(value, FADE_MIN, FADE_MAX);
                setVolume("Master", currentMaster);

            }
        }
        [SerializeField]
        float currentMaster;

        public float BGMVolume
        {

            get => currentBGM;
            set
            {
                currentBGM = Mathf.Clamp(value, FADE_MIN, FADE_MAX);
                setVolume("BGM", currentBGM);

            }
        }
        [SerializeField]
        float currentBGM;

        public float SFXVolume
        {

            get => currentSFX;
            set
            {
                currentSFX = Mathf.Clamp(value, FADE_MIN, FADE_MAX);
                setVolume("SFX", currentSFX);

            }
        }
        [SerializeField]
        float currentSFX;

        public float UIVolume
        {

            get => currentUI;
            set
            {
                currentUI = Mathf.Clamp(value, FADE_MIN, FADE_MAX);
                setVolume("UI", currentUI);
            }
        }
        [SerializeField]
        float currentUI;

        public AudioMixer MainMixer;
        public AudioMixerGroup BGM_Channel;
        public AudioMixerGroup SFX_Channel;
        public AudioMixerGroup UI_Channel;

        public void setVolume(string param, float val)
        {
            //log a value from Fademin to fade max == -80 to 6, which is the range for the DB threshold
            MainMixer.SetFloat(param + "Volume", Mathf.Log(val) * 20f);
        }
        #endregion
        // Start is called before the first frame update

        public class AudioFadeSettings
        {
            public AudioSource src;
            public AudioClip clip;
            public float duration;
            public float diffInValue;
            public float currentTime;

            public UnityAction onCancel;
            public UnityAction<AudioSource, AudioClip, float> onFinish;

            public bool isFading;

            public void StartFading(AudioClip nextclip, float endVal, float fadeTime, UnityAction<AudioSource, AudioClip, float> finishCallback, UnityAction cancelCallback = null)
            {
                if (isFading) CancelFading();

                clip = nextclip;
                diffInValue = (endVal - src.volume) * Time.deltaTime;
                duration = fadeTime;
                currentTime = 0;

                isFading = true;
                onFinish = finishCallback;
                onCancel = cancelCallback;

            }

            public void CancelFading()
            {
                if (isFading) onCancel?.Invoke();
                isFading = false;
            }

            public void onUpdate()
            {
                if (isFading)
                {
                    currentTime += Time.deltaTime;
                    src.volume += diffInValue;
                    if (currentTime > duration)
                    {
                        isFading = false;
                        onFinish?.Invoke(src, clip, duration);
                    }
                }
            }

            private AudioFadeSettings() { }
            public AudioFadeSettings(AudioSource audsrc)
            {
                src = audsrc;
                duration = 0;
                isFading = false;

            }
        }

        Dictionary<AudioSource, AudioFadeSettings> fadeList;

        void Start()
        {
            Btn_Sound.AddAudioToListener(BTN_SoundSrc, defaultBtnSound);
            fadeList = new Dictionary<AudioSource, AudioFadeSettings>();
            fadeList.Add(BGM_SoundSrc, new AudioFadeSettings(BGM_SoundSrc));
            fadeList.Add(SFX_SoundSrc, new AudioFadeSettings(SFX_SoundSrc));
            PlayBGM(0);
            BGMVolume = ScreensaverVol;
            MasterVolume = mainVol;

        }

        #region GenericAudioSourceControl
        void PlayAudioSource(AudioSource src, List<AudioClip> listOfAudio, int index, float transition = 0f)
        {
            if (index >= listOfAudio.Count || index < 0)
            {
                Debug.LogError("Invalid Index");
                return;
            }

            if (src.isPlaying)
            {
                if (transition == 0f)
                {
                    src.Stop();
                    PlayClip(src, listOfAudio[index]);
                }
                else
                {

                    fadeList[src].StartFading(listOfAudio[index], 0, transition / 2f, playAudioSource2);
                    //await Task.Delay(Mathf.RoundToInt(1000 * transition / 2f));
                    //PlayClip(src, listOfAudio[index]);
                    //fadeList[src].StartFading(listOfAudio[index], 1, transition / 2f, null);
                }

            }
            else
            {
                PlayClip(src, listOfAudio[index]);
            }
        }

        void playAudioSource2(AudioSource newaud, AudioClip newclip, float duration)
        {
            PlayClip(newaud, newclip);
            fadeList[newaud].StartFading(newclip, 1, duration, null);
        }

        void PlayClip(AudioSource source, AudioClip clip)
        {
            if (source.isPlaying) source.Stop();
            source.clip = clip;
            //await Task.Delay(100);
            source.Play();
        }

        #endregion
        #region Public Function
        public void PlayBGM(int index, float transition = 0f)
        {
            PlayAudioSource(BGM_SoundSrc, BGMs, index, transition);
        }
        /// <summary>
        /// OneShots cant be stopped
        /// </summary>
        /// <param name="index"></param>
        public void PlayOneShot(int index)
        {
            SFX_SoundSrc.PlayOneShot(SFXs[index]);
        }
        public void PlayOneShot(AudioClip clip)
        {
            SFX_SoundSrc.PlayOneShot(clip);
        }
        public void StopAudioSource(AudioSource src)
        {
            src.Stop();
        }

        #endregion

        private void Update()
        {
            foreach (KeyValuePair<AudioSource, AudioFadeSettings> fader in fadeList)
            {
                fader.Value.onUpdate();
            }


        }
    }

}
