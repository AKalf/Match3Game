using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour {

    const float minPitch = -0.5f;
    const float maxPitch = 0.5f;

    static AudioSource[] sources = new AudioSource[5];

    static Queue<AudioMessage> messagesToBePlayed = new Queue<AudioMessage>();
    static int[] IDsPlayingAtSourceArrayIndex = new int[sources.Length];

    static List<int> IDsPlayed = new List<int>();

    void Awake() {
        for (int i = 0; i < sources.Length; i++) {
            sources[i] = this.gameObject.AddComponent<AudioSource>();
        }
    }
    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {
        if (messagesToBePlayed.Count > 0) {
            PlayMessages();
        }
    }
    private void PlayMessages() {
        int sourceAvailable = GetAvailableSource();

        if (sourceAvailable > -1) {
            AudioMessage msg = messagesToBePlayed.Dequeue();
            if (msg.messageID == -1000) {
                IDsPlayed.Clear();
                return;
            }
            PlayNextMessage(msg, sourceAvailable);
        }

    }
    private int GetAvailableSource() {
        int sourceAvailable = -1;
        for (int i = 0; i < sources.Length; i++) {
            if (!sources[i].isPlaying && IDsPlayingAtSourceArrayIndex[i] > -1) {
                IDsPlayed.Add(IDsPlayingAtSourceArrayIndex[i]);
                IDsPlayingAtSourceArrayIndex[i] = -1;
                sourceAvailable = i;
            }
            else if (IDsPlayingAtSourceArrayIndex[i] < 0) {
                sourceAvailable = i;
            }

        }
        return sourceAvailable;
    }

    private void PlayNextMessage(AudioMessage msg, int availableSoure) {
        if (msg.dependencyID > -1 && !IDsPlayed.Contains(msg.dependencyID)) {
            messagesToBePlayed.Enqueue(msg);
            PlayNextMessage(messagesToBePlayed.Dequeue(), availableSoure);
        }
        else {
            PlayClipFromSource(msg.clip, sources[availableSoure], msg.pitch, msg.delay);
            IDsPlayingAtSourceArrayIndex[availableSoure] = msg.messageID;
        }
    }

    public static void PlayClipFromSource(AudioClip clip, AudioSource source, float pitch = 1, float delay = 0.0f) {
        source.pitch = pitch;
        source.clip = clip;
        source.PlayDelayed(delay);
    }
    public static void ReceiveAudioMessages(List<AudioMessage> messages) {
        foreach (AudioMessage message in messages) {
            messagesToBePlayed.Enqueue(message);
        }
        // put a clear message to the end
        messagesToBePlayed.Enqueue(new AudioMessage(null, -1000));

    }

}