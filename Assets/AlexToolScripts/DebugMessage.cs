﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class DebugMessage {
    [SerializeField]
    public string name = "";

    [SerializeField]
    public string message;
    [SerializeField]
    public string tag;

    [SerializeField]
    public Object obj = null;

    public DebugMessage(string msg, string tag, Object obj = null) {
        message = msg;
        this.tag = tag;
        this.obj = obj;
    }

}