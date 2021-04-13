using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pandora.Audio {
    public class MatchAudioRepo : MonoBehaviour {
        public AudioClip MatchClip;

        static MatchAudioRepo _instance = null;

        static public MatchAudioRepo Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = GameObject.Find("Arena").GetComponent<MatchAudioRepo>();
                }

                return _instance;
            }
        }

        AudioSource audioSource;

        void Start() {
            audioSource = Camera.main.GetComponent<AudioSource>();

            PlayMatchAudio();
        }

        public void PlayMatchAudio() {
            audioSource.clip = MatchClip;

#if !UNITY_EDITOR
            audioSource.Play();
#endif
        }
    }
}