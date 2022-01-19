using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UnityDecoupledBehavior
{

    [RequireComponent(typeof(Button))]
    public class Btn_Sound : MonoBehaviour
    {
        // Start is called before the first frame update
        [SerializeField]
        AudioClip btnSound;
        Button btn;

        public static AudioSource Audsrc;
        public static AudioClip AuddefaultClip;
        private void Awake()
        {
            btn = GetComponent<Button>();
            btn.onClick.AddListener(() =>
            {
                if (Audsrc != null)
                {
                    if (btnSound != null)
                    {
                        Audsrc.PlayOneShot(btnSound);
                    }
                    else
                    {
                        Audsrc.PlayOneShot(AuddefaultClip);
                    }
                }
            });
        }

        public static void AddAudioToListener(AudioSource src, AudioClip defaultClip)
        {
            Audsrc = src;
            AuddefaultClip = defaultClip;

        }

    }
}

