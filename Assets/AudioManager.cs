using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour {

    const float minPitch = -0.5f;
    const float maxPitch = 0.5f;

    static AudioSource globalSource = null;
    static Queue<AudioClip> clipsToPlay = new Queue<AudioClip>();

    void Awake() {
        globalSource = this.gameObject.GetComponent<AudioSource>();
    }
    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }

    public static void PlayAudioClip(AudioClip clip) {
        clipsToPlay.Enqueue(clip);
    }
    public static void PlayClipFromSource(AudioClip clip, AudioSource source, bool randomPitch = false, float delay = 0.0f) {
        if (randomPitch) {
            source.pitch = Random.Range(minPitch, maxPitch);
        }
        source.clip = clip;
        source.PlayDelayed(delay);
        if (randomPitch) {
            source.pitch = 1;
        }
    }

}