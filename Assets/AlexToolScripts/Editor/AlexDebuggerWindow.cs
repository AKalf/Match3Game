using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class AlexDebuggerWindow : EditorWindow {

    Dictionary<AlexDebugger.tags, List<string>> tagsAndMessages = new Dictionary<AlexDebugger.tags, List<string>>();
    Dictionary<AlexDebugger.tags, bool> tagsFoldouts = new Dictionary<AlexDebugger.tags, bool>();

    bool hasInit = false;

    private Dictionary<AlexDebugger.tags, Vector2> scrollPos;
    Vector2 messagScroll = new Vector2();

    [MenuItem("Tools/AlexDebugger")]
    static void ShowWindow() {
        // Get existing open window or if none, make a new one:
        var window = GetWindow<AlexDebuggerWindow>();
        window.titleContent = new GUIContent("Alex Debugger");
        window.Show();
    }
    void OnGUI() {
        if (!hasInit) {
            tagsAndMessages = new Dictionary<AlexDebugger.tags, List<string>>();
            tagsFoldouts = new Dictionary<AlexDebugger.tags, bool>();
            scrollPos = new Dictionary<AlexDebugger.tags, Vector2>();
            //scrollPos.Add("foldouts", Vector2.zero);
            foreach (DebugMessage msg in AlexDebugger.GetInstance().messages) {
                if (tagsAndMessages.ContainsKey(msg.tag) == false) {
                    if (msg.obj != null) {
                        msg.message += ", Trigger script: " + msg.name;
                    }
                    tagsAndMessages.Add(msg.tag, new List<string>() { msg.message });
                    tagsFoldouts.Add(msg.tag, false);
                    scrollPos.Add(msg.tag, Vector2.zero);
                }
                else {
                    tagsAndMessages[msg.tag].Add(msg.message);
                }
            }

            hasInit = true;
        }
        else {
            messagScroll = EditorGUILayout.BeginScrollView(messagScroll, false, false);
            foreach (AlexDebugger.tags tag in tagsAndMessages.Keys) {
                EditorGUILayout.BeginHorizontal();
                tagsFoldouts[tag] = EditorGUILayout.Foldout(tagsFoldouts[tag], "Show debug for tag: " + tag);
                if (tagsFoldouts[tag]) {
                    tagsFoldouts[tag] = true;
                }
                EditorGUILayout.EndHorizontal();
            }
            foreach (AlexDebugger.tags tag in tagsFoldouts.Keys) {
                if (tagsFoldouts[tag] == true) {
                    scrollPos[tag] = EditorGUILayout.BeginScrollView(scrollPos[tag], false, false);
                    foreach (string message in tagsAndMessages[tag]) {
                        EditorGUILayout.TextArea(message);
                        EditorGUILayout.Separator();

                    }
                    EditorGUILayout.EndScrollView();
                }
                GUILayout.Space(10);
                if (GUILayout.Button("Clear debug for tag:  " + tag)) {
                    AlexDebugger.GetInstance().ClearMessagesForTag(tag);
                    hasInit = false;
                }
            }
            EditorGUILayout.EndScrollView();
        }

    }
}