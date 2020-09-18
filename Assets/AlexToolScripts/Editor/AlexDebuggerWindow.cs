using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class AlexDebuggerWindow : EditorWindow {

    Dictionary<AlexDebugger.tags, List<string>> tagsAndMessages = new Dictionary<AlexDebugger.tags, List<string>>();
    Dictionary<AlexDebugger.tags, bool> tagsFoldouts = new Dictionary<AlexDebugger.tags, bool>();

    bool hasInit = false;

    private Dictionary<AlexDebugger.tags, Vector2> scrollPos;
    Vector2 messagScroll = Vector2.zero;

    Vector2 foldoutsScroll = Vector2.zero;

    bool showAllMessageFoldout = false;
    Vector2 allMessagesScroll = Vector2.zero;

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

            foldoutsScroll = EditorGUILayout.BeginScrollView(foldoutsScroll);
            foreach (AlexDebugger.tags tag in tagsAndMessages.Keys) {
                EditorGUILayout.BeginHorizontal();
                tagsFoldouts[tag] = EditorGUILayout.Foldout(tagsFoldouts[tag], "Show debug for tag: " + tag);
                GUILayout.Space(10);
                if (GUILayout.Button("Clear debug for tag:  " + tag, GUILayout.Width(300))) {
                    AlexDebugger.GetInstance().ClearMessagesForTag(tag);
                    hasInit = false;
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();

            messagScroll = EditorGUILayout.BeginScrollView(messagScroll, false, false);
            foreach (AlexDebugger.tags tag in tagsFoldouts.Keys) {
                if (tagsFoldouts[tag] == true) {
                    scrollPos[tag] = EditorGUILayout.BeginScrollView(scrollPos[tag], false, false);
                    foreach (string message in tagsAndMessages[tag]) {
                        EditorGUILayout.TextArea(message);
                        EditorGUILayout.Separator();

                    }
                    EditorGUILayout.EndScrollView();
                }
            }
            EditorGUILayout.EndScrollView();
        }
        showAllMessageFoldout = EditorGUILayout.Foldout(showAllMessageFoldout, "Show all messages based on time created");
        if (showAllMessageFoldout) {
            allMessagesScroll = EditorGUILayout.BeginScrollView(allMessagesScroll);
            List<DebugMessage> messages = AlexDebugger.GetInstance().messages;
            foreach (DebugMessage msg in messages) {
                EditorGUILayout.TextArea(msg.message + ". -[tag]- " + msg.tag + ", -[trigger]- " + ((msg.obj == null) ? "" : msg.obj.name));
                EditorGUILayout.Separator();
            }
            EditorGUILayout.EndScrollView();
        }
        if (GUILayout.Button("Destroy all messages")) {
            foreach (AlexDebugger.tags tag in tagsAndMessages.Keys) {
                for (int i = 0; i < tagsAndMessages[tag].Count; i++) {
                    tagsAndMessages[tag].Remove(tagsAndMessages[tag][i]);
                }
            }
            hasInit = false;
        }
    }
}