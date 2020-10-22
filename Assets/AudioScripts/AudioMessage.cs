using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioMessage {
    public AudioClip clip = null; // this could also be retrieved with an index at an array
    public float delay = 0.0f;
    public int messageID = -1;
    public int dependencyID = -1;
    public float pitch = 1;

    public AudioMessage(AudioClip clipToPlay, int messageID, int messageDependencyID = -1, float delay = 0.0f, float pitch = 1) {
        clip = clipToPlay;
        this.delay = delay;
        this.messageID = messageID;
        dependencyID = messageDependencyID;
        this.pitch = pitch;
    }

}