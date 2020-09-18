using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "Alex/Debugger")]
public class AlexDebugger : ScriptableObject {

    [SerializeField]
    AlexDebugger inst;

    static AlexDebugger instance;

    [SerializeField]
    public enum tags {
        Swap,
        Matches,
        UpwardMovement,
        Aftermatch,
        Effects,
        Input,
        Step1,
        Step2,
        Step3,
        Step4,
        Step5,
        Animations
    }

    [SerializeField]
    public List<tags> ignoredTag = new List<tags>();

    [SerializeField]
    public List<DebugMessage> messages = new List<DebugMessage>();

    static string pathToDB = "New Alex Debugger";

    [SerializeField]
    bool clearMessagesOnPlay = false;

    public static AlexDebugger GetInstance() {
        if (instance == null) {
            AlexDebugger obj = (AlexDebugger) Resources.Load(pathToDB, typeof(AlexDebugger));
            if (obj == null) {
                Debug.LogError("No asset AlexDebugger found");
            }
            else if (obj is AlexDebugger) {
                instance = (AlexDebugger) obj;
            }
        }
        return instance;
    }

    public void AddMessage(string msg, tags tag, Object trigger = null, [CallerMemberName] string callingMethod = "", [CallerFilePath] string callingFilePath = "", [CallerLineNumber] int callingFileLineNumber = 0) {
        if (ignoredTag.Contains(tag)) {
            return;
        }

        DebugMessage dMsg = new DebugMessage(msg + "    [Timestamp: " + Time.realtimeSinceStartup + "], [Line: " +
            callingFileLineNumber + "], \n [Function: " + callingMethod + "], \n [File path: " + callingFilePath + "]", tag, trigger);

        dMsg.name = dMsg.tag + "(" + dMsg.GetHashCode() + ")";

        //UnityEditor.AssetDatabase.CreateAsset(dMsg, "Assets/AlexScripts/AlexToolScripts/Resources/" + dMsg.name + ".asset");

        messages.Add(dMsg);
    }

    void OnEnable() {
        if (clearMessagesOnPlay) {
            ClearAllMessages();
        }
    }

    //[Sirenix.OdinInspector.Button]
    public void ClearMessagesForTag(tags tag) {

        bool hasFoundAsset = false;
        for (int i = 0; i < messages.Count; i++) {
            DebugMessage msg = messages[i];
            if (msg.tag == tag /*&& UnityEditor.AssetDatabase.LoadAssetAtPath("Assets/AlexScripts/AlexToolScripts/Resources/" + msg.name + ".asset", typeof(DebugMessage)) != null*/ ) {
                hasFoundAsset = true;
                //UnityEditor.AssetDatabase.DeleteAsset("Assets/AlexScripts/AlexToolScripts/Resources/" + msg.name + ".asset");
                messages.Remove(msg);
                //UnityEditor.AssetDatabase.SaveAssets();
            }
        }
        if (hasFoundAsset) {
            ClearMessagesForTag(tag);
        }
    }

    //[Sirenix.OdinInspector.Button]
    public void ClearAllMessages() {
        messages.Clear();
    }

}