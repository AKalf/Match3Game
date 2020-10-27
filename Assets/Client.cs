using System.Collections.Generic;
using UnityEngine;

public class Client : MonoBehaviour {
    [SerializeField]
    GameObject popUpPanel = null;

    static bool hasInit = false;
    static Queue<Messages> messages = new Queue<Messages>();
    static List<int> IDsPlayed = new List<int>();
    static bool isServerAvailable = false;
    static bool isGamePaused = false;
    public static void ReceiveMessagesFromServer(string json) {
        // translate json to message
    }
    public static void ReceiveMessagesFromServer(List<Messages> msgs) {
        if (msgs.Count > 0) {
            foreach (Messages msg in msgs) {
                //Debug.Log(msg.messageType);
                messages.Enqueue(msg);
            }
            hasInit = true;
        }
    }
    public static void ReceiveMessagesFromServer(Messages msg) {
        if (msg.messageType == Messages.MessageTypes.ServerStatus) {
            isServerAvailable = ((Messages.ServerStatusMessage) msg).isAvailable;
        }
        else {
            messages.Enqueue(msg);
        }
    }

    private void Update() {
        if (!Animations.AreAnimationsPlaying() && !AudioManager.IsAudioPlaying()) {
            ProcssNextBatch();
        }
    }
    public static void ProcssNextBatch() {
        if (messages.Count > 0 && hasInit) {
            List<Messages.AnimationMessage> newAnimationMessages = new List<Messages.AnimationMessage>();
            List<Messages.AudioMessage> newAudioMessages = new List<Messages.AudioMessage>();
            while (messages.Count > 0) {
                Messages msg = messages.Dequeue();
                switch (msg.messageType) {
                    case Messages.MessageTypes.Wait:
                        IDsPlayed.Clear();
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

    public static void SendInputToServer(int firstInputElementInputIndex, int secondInputElementIndex) {
        Server.GetServerInstance().ReceiveInputMessage(firstInputElementInputIndex, secondInputElementIndex);
    }

    public static bool GetIfServerAvailable() {
        return isServerAvailable;
    }

    public static bool GetIfGameIsPaused() {
        return isGamePaused;
    }
    public static void AddProccessedMessageID(int id) {
        IDsPlayed.Add(id);
    }
    public static List<int> GetPlayedMessagesIDs() {
        List<int> list = new List<int>();
        foreach (int id in IDsPlayed) {
            list.Add(id);
        }
        return list;
    }

    public void UnPauseGame() {
        isGamePaused = false;
        popUpPanel.SetActive(false);

    }
    public static void PauseGame() {
        isGamePaused = true;
    }
}