#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
[UnityEditor.CustomEditor(typeof(RaycastOnClick))]
public class RaycastOnClickInspector : Editor {
    /// <summary> /// GameObject that triggers the action /// </summary>
    UnityEngine.GameObject triggerGameObject = null;

    /// <summary> /// The index of the selected method in the available methods list /// </summary>
    int selectedMethodNameIndex = 0;

    /// <summary> /// The name of the selected method in the available methods list /// </summary>
    string selectedMethodName = "";

    /// <summary>/// Methods reflection representetion for script ///</summary>
    List<MethodInfo> scriptMethods = new List<MethodInfo>();

    /// <summary>/// Names of availble functions to trigger ///</summary>
    List<string> methodNames = new List<string>();

    /// <summary> /// The parameters of the action to be created /// </summary>
    List<System.Object> parameters = new List<System.Object>();

    /// <summary> /// The name of the parameters of the action to be created /// </summary>
    List<string> parametersNames = new List<string>();

    bool gotMethods = false;
    bool gotParameters = false;

    public override void OnInspectorGUI() {
        base.DrawDefaultInspector();
        RaycastOnClick targetInstance = (RaycastOnClick)target;
        SerializedObject objInstance = new SerializedObject(targetInstance);
        // always run serialization on script
        objInstance.Update();
        if (GUILayout.Button("Get methods")) {
            methodNames.Clear();
            // Get all methods of the script
            List<MethodInfo> methodInfo = typeof(RaycastOnClick).GetMethods().ToList();
            // Get each name
            foreach (MethodInfo method in methodInfo) {
                // if it is not a derived class (?! that is what i noticed)
                if (method.Equals(method.GetBaseDefinition())) {
                    scriptMethods.Add(method);
                    methodNames.Add(method.Name);
                }
            }
            // if names found, initalize  methods pop-up values
            if (scriptMethods.Count > 0) {
                selectedMethodNameIndex = 0;
                selectedMethodName = methodNames[0];
                gotMethods = true;
            }
        }
        // Display methods pop-up
        if (gotMethods) {
            selectedMethodNameIndex = EditorGUILayout.Popup("Available methods: " + methodNames.Count, selectedMethodNameIndex, methodNames.ToArray());
            selectedMethodName = methodNames[selectedMethodNameIndex];
            //Debug.Log ("Method index:  " + selectedMethodNameIndex + methodNames[selectedMethodNameIndex]);
        }
        if (GUILayout.Button("Get parameters for method: " + selectedMethodName)) {
            parameters.Clear();
            MethodInfo methodInfo = scriptMethods[methodNames.IndexOf(selectedMethodName)];
            ParameterInfo[] infos = methodInfo.GetParameters();
            foreach (ParameterInfo info in infos) {
                //Debug.Log("Parameter name " + info.Name + ", parameter type: " + info.ParameterType);
                // if parameter derives from System
                if (info.ParameterType.IsSubclassOf(typeof(System.Object)) || info.ParameterType.IsSubclassOf(typeof(UnityEngine.Object))) {
                    var param = DrawFieldBaseOnType(info.ParameterType, info.Name);
                    parameters.Add(param);
                    parametersNames.Add(info.Name);
                }
                else {
                    Debug.LogError("Cannot assign method! All method's parameters types must derive either from System.Object or UnityEngine.Object. Error occured with parameter " + info.Name + " of type: " + info.ParameterType);
                    EditorGUILayout.HelpBox("Cannot assign method! All method's parameters types must derive either from System.Object or UnityEngine.Object. Error occured with parameter " + info.Name + " of type: " + info.ParameterType, MessageType.Error);
                }
            }
            if (parameters.Count > 0) {
                gotParameters = true;
            }
        }
        if (gotParameters) {

            EditorGUILayout.LabelField("Trigger");
            triggerGameObject = EditorGUILayout.ObjectField(triggerGameObject, typeof(GameObject), true, GUILayout.MaxWidth(500), GUILayout.MinWidth(170))as GameObject;
            for (int i = 0; i < parameters.Count; i++) {
                //Debug.Log ("Parameter " + i + " is:  " + parameters[i]);
                System.Object param = DrawFieldBaseOnType(parameters[i], parameters[i].GetType(), parametersNames[i]);
                parameters[i] = param;
            }
        }
        if (GUILayout.Button("Add method for execution " + selectedMethodName)) {
            if (triggerGameObject != null) {
                SerializedAction action = new SerializedAction(triggerGameObject, selectedMethodName, parameters.ToArray(), parametersNames);
                targetInstance.ActionsToPerform.Add(action);
                Debug.Log("Added action " + action.methodName + " for exuction");
                parameters.Clear();
                triggerGameObject = null;

            }
            else {
                EditorGUILayout.HelpBox("Trigger gameobject has not been defined", MessageType.Error);
                Debug.LogError("Trigger gameobject has not been defined");
            }
        }
        if (GUILayout.Button("Clear delegates ")) {
            targetInstance.ActionsToPerform.Clear();
            targetInstance.ActionsToPerform = new List<SerializedAction>();
            Debug.Log("Actions to perform cleared. Number of actions: " + targetInstance.ActionsToPerform.Count);
        }
        if (targetInstance.ActionsToPerform.Count > 0) {
            foreach (SerializedAction action in targetInstance.ActionsToPerform) {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Action to execute: ", GUILayout.Width(125));
                EditorGUILayout.LabelField(action.methodName, EditorStyles.boldLabel);
                EditorGUILayout.EndHorizontal();
                action.triggerObject = EditorGUILayout.ObjectField("Trigger GameObject: ", action.triggerObject, typeof(GameObject), true)as GameObject;
                if (action.arguments == null) {
                    action.GetAction(targetInstance);
                }
                if (action.arguments != null) {
                    for (int i = 0; i < action.arguments.Length; i++) {
                        //Debug.Log ("Argument of action: " + action.methodName + " is " + action.arguments[i]);
                        if (action.arguments[i] != null) {
                            if (action.unityArguments[i] != null) {
                                UnityEngine.Object prevArg = action.unityArguments[i];
                                action.unityArguments[i] = DrawFieldBaseOnType(action.unityArguments[i], action.unityArguments[i].GetType(), action.argumentNames[i])as UnityEngine.Object;
                            }
                            else {
                                System.Object prevArg = action.arguments[i];
                                action.arguments[i] = DrawFieldBaseOnType(action.arguments[i], action.arguments[i].GetType(), action.argumentNames[i]);
                            }
                        }

                    }
                    if (GUILayout.Button("SAVE CHANGES FOR: " + action.methodName)) {
                        objInstance.ApplyModifiedProperties();
                        EditorUtility.SetDirty(targetInstance);
                        action.SerializeArguments();
                        AssetDatabase.SaveAssets();
                    }
                }
                else {
                    Debug.Log("Could not load given arguments!");
                    EditorGUILayout.HelpBox("Could not load given arguments!", MessageType.Error);
                }
            }
        }


    }

    private static System.Object DrawFieldBaseOnType(System.Type type, string paramName) {
        const int minWidth = 170;
        const int maxWidth = 500;
        //EditorGUILayout.LabelField ("Param name: " + paramName + " Type of param: " + type.Name);
        if (type == typeof(int)) {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("int: ", GUILayout.Width(25));
            int value = EditorGUILayout.IntField(0, GUILayout.Width(35));
            EditorGUILayout.EndHorizontal();
            return value;
        }
        else if (type == typeof(float)) {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("float: ", GUILayout.Width(35));
            float value = EditorGUILayout.FloatField(0, GUILayout.Width(35));
            EditorGUILayout.EndHorizontal();
            return value;
        }
        else if (type == typeof(string)) {
            return EditorGUILayout.TextField("", GUILayout.MaxWidth(maxWidth), GUILayout.MinWidth(minWidth));
        }
        else if (type == typeof(bool)) {
            return EditorGUILayout.Toggle(false, GUILayout.MaxWidth(maxWidth), GUILayout.MinWidth(minWidth));
        }
        else {
            return EditorGUILayout.ObjectField(new Object(), type, true, GUILayout.MaxWidth(maxWidth), GUILayout.MinWidth(minWidth));
        }
    }

    private static System.Object DrawFieldBaseOnType(System.Object obj, System.Type type, string paramName) {

        const int minWidth = 170;
        const int maxWidth = 500;
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Param name: ", GUILayout.Width(100));
        EditorGUILayout.LabelField(paramName, EditorStyles.boldLabel, GUILayout.MaxWidth(200), GUILayout.MinWidth(50));
        EditorGUILayout.LabelField("Type of param: ", GUILayout.Width(125));
        EditorGUILayout.LabelField(type.Name, EditorStyles.boldLabel, GUILayout.MaxWidth(200), GUILayout.MinWidth(50));
        EditorGUILayout.EndHorizontal();

        if (type == typeof(int)) {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("int: ", GUILayout.Width(25));
            int value = EditorGUILayout.IntField((int)obj, GUILayout.Width(35));
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(10);
            return value;
        }
        else if (type == typeof(float)) {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("float: ", GUILayout.Width(35));
            float value = EditorGUILayout.FloatField((float)obj, GUILayout.Width(35));
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(10);
            return value;
        }
        else if (type == typeof(string)) {
            string text = EditorGUILayout.TextField((string)obj, GUILayout.MaxWidth(maxWidth), GUILayout.MinWidth(minWidth));
            GUILayout.Space(10);
            return text;
        }
        else if (type == typeof(bool)) {
            bool b = EditorGUILayout.Toggle(false, GUILayout.MaxWidth(maxWidth), GUILayout.MinWidth(minWidth));
            GUILayout.Space(10);
            return b;
        }
        else {
            UnityEngine.Object newObj = EditorGUILayout.ObjectField((Object)obj, type, true, GUILayout.MaxWidth(maxWidth), GUILayout.MinWidth(minWidth));
            GUILayout.Space(10);
            return newObj;
        }
    }
}
#endif