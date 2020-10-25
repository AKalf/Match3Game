using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Client : MonoBehaviour {

    static bool hasInit = false;
    static Queue<Messages> messages = new Queue<Messages>();
    public static void ReceiveMessagesFromServer(string json) {
        // translate json to message
    }
    public static void ReceiveMessagesFromServer(List<Messages> msgs) {
        if (msgs.Count > 0) {
            foreach (Messages msg in msgs) {
                messages.Enqueue(msg);
            }

            hasInit = true;
        }
    }
    private void Update() {
        if (!Animations.AreAnimationsPlaying() && !AudioManager.IsAudioPlaying()) {
            SendNextBatch();
        }
    }
    public static void SendNextBatch() {
        if (messages.Count > 0 && hasInit) {
            List<Messages.AnimationMessage> newAnimationMessages = new List<Messages.AnimationMessage>();
            List<Messages.AudioMessage> newAudioMessages = new List<Messages.AudioMessage>();
            while (messages.Count > 0) {
                Messages msg = messages.Dequeue();
                switch (msg.messageType) {
                    case Messages.MessageTypes.Wait:
                        Animations.ReceiveAnimationMessages(newAnimationMessages);
                        AudioManager.ReceiveAudioMessages(newAudioMessages);
                        return;
                        //break;
                    case Messages.MessageTypes.Animation:
                        newAnimationMessages.Add((Messages.AnimationMessage) msg);
                        break;
                    case Messages.MessageTypes.Audio:
                        newAudioMessages.Add((Messages.AudioMessage) msg);
                        break;
                }
            }
        }
    }
}