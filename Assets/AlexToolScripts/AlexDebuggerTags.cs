using System.Collections.Generic;
using UnityEngine;

public class AlexDebuggerTags {

    public string tagName = "";

    public Color tagLetterColor = Color.black;

    public List<string> categories = new List<string>();

    public AlexDebuggerTags(string name, Color color) {
        tagName = name;
        tagLetterColor = color;
    }

}