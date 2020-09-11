using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SerializedAction {

    /// <summary>/// The object that triggeres the action ///</summary>
    [SerializeField]
    public GameObject triggerObject;

    /// <summary>/// The name of the method of the action ///</summary>
    public string methodName = "";

    /// <summary>/// The string that holds the serialized arguments ///</summary>
    [SerializeField]
    public string serializedArray = "";

    /// <summary>/// The arguments for the method ///</summary>
    public System.Object[] arguments;

    /// <summary>/// The names of the arguments for debugging ///</summary>
    [SerializeField]
    public List<string> argumentNames = new List<string>();

    /// <summary>/// To use only internally. The arguments of type UnityEngine.Object. They are stored seperatly and seriliazation is handled by Unity. ///</summary>
    [SerializeField]
    public Object[] unityArguments;

    /// <summary>/// To use only internally. The arguments of type UnityEngine.Vector. They are stored seperatly and seriliazation is handled by Unity. ///</summary>
    // private Vector3[] vectorArguments;

    /// <summary>/// The action produced after deserializing and processing ///</summary>
    private System.Action action = null;

    private void NameChanged() {
        Debug.LogError("Name changed to " + methodName);
    }

    public SerializedAction(GameObject triggerGameObject, string method, System.Object[] args, List<string> paramNames) {
        Debug.Log("############### CREATING NEW SERIAZIZED ACTION #########################");
        triggerObject = triggerGameObject;
        methodName = method;
        arguments = new System.Object[args.Length];
        unityArguments = new Object[args.Length];
        Debug.Log("Trigger gameobject " + triggerObject.name + ", method: " + methodName);
        for (int i = 0; i < args.Length; i++) {
            if (args[i].GetType().IsSubclassOf(typeof(UnityEngine.Object))) {
                Debug.Log("Arguments for serialization: " + paramNames[i] + " = " + args[i].ToString() + ", is UnityEngine.Object: " + args[i].GetType().ToString());
                unityArguments[i] = args[i] as UnityEngine.Object;
                arguments[i] = null;

            }
            else {
                Debug.Log("Arguments for serialization: " + args[i].ToString() + ", is System.Object: " + args[i].GetType().ToString());
                unityArguments[i] = null;
                arguments[i] = args[i] as System.Object;
            }
        }
        argumentNames = paramNames;
        SerializeArguments();

        Debug.Log("serializedArray: " + serializedArray);
    }

    public System.Action GetAction(MonoBehaviour instanceToInvokesMethod) {

        arguments = XmlDeserializeFromString(serializedArray);
        if (arguments == null) {
            arguments = new System.Object[0];
        }
#if UNITY_EDITOR
        Debug.Log("Getting action: " + methodName);
        Debug.Log("Instance that invokes method: " + instanceToInvokesMethod.name);
        Debug.Log("Method name to invoke: " + methodName);
        Debug.Log("System.Object method arguments as XML string: " + serializedArray);
        if (arguments != null) {
            Debug.Log("Number of arguments: " + arguments.Length);
        }
#endif

        if (action == null) {
            action = (() => instanceToInvokesMethod.GetType().GetMethod(methodName).Invoke(instanceToInvokesMethod, arguments));
        }
        Debug.Log("Action: " + methodName + " returned");
        return action;
    }

    public void SerializeArguments() {
        serializedArray = XmlSerializeToString(arguments);
    }

    public string XmlSerializeToString(System.Object[] array) {
        Debug.Log("########### SERILIZATION - START #############");
        var serializer = new System.Xml.Serialization.XmlSerializer(array.GetType());
        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        using(System.IO.TextWriter writer = new System.IO.StringWriter(sb)) {
            serializer.Serialize(writer, array);
        }
#if UNITY_EDITOR
        Debug.Log("Trying to deserialize for testing...");

        System.Object[] args = XmlDeserializeFromString(sb.ToString());

        for (int i = 0; i < args.Length; i++) {
            Debug.Log("Serialized argument: " + i + ") " + argumentNames[i] + " = " + args[i].ToString() + " succesfully");
        }
        Debug.Log("XML: " + sb.ToString());
        Debug.Log("############ SERILIZATION - END ############");
#endif
        return sb.ToString();
    }

    public System.Object[] XmlDeserializeFromString(string objectData) {
        Debug.Log("########### DESERILIZATION - START #############");
        Debug.Log("XML for deserilization: " + objectData);
        System.Object[] result = null;
        if (objectData != null && objectData.Trim() != "") {
            var serializer = new System.Xml.Serialization.XmlSerializer(typeof(System.Object[]));

            using(System.IO.TextReader reader = new System.IO.StringReader(objectData)) {
                result = serializer.Deserialize(reader)as System.Object[];
            }
            if (result != null) {
                for (int i = 0; i < result.Length; i++) {

                    if (unityArguments[i] != null) {
                        result[i] = unityArguments[i];
                    }

                }
            }
            else {
                result = new System.Object[unityArguments.Length];
                for (int i = 0; i < result.Length; i++) {
                    result[i] = unityArguments[i];
                }
            }
        }
        else {
            result = new System.Object[unityArguments.Length];
            for (int i = 0; i < result.Length; i++) {
                result[i] = unityArguments[i];
            }
        }
#if UNITY_EDITOR
        Debug.Log("Results length: " + result.Length);
        for (int i = 0; i < result.Length; i++) {
            object obj = result[i];
            if (obj != null && argumentNames != null && i < argumentNames.Count) {
                Debug.Log("Deserialized argument " + i + "): " + argumentNames[i] + " = " + obj.ToString());
            }
        }
        Debug.Log("########### DESERILIZATION - END #############");
#endif
        return result;
    }

}