﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OnDragEvent : MonoBehaviour {

    public static bool hasDragBegin = false;

    // Start is called before the first frame update
    void Start() {
        //Fetch the Event Trigger component from your GameObject
        EventTrigger trigger = GetComponent<EventTrigger>();
        //Create a new entry for the Event Trigger
        EventTrigger.Entry entryBegin = new EventTrigger.Entry();
        //Add a Drag type event to the Event Trigger
        entryBegin.eventID = EventTriggerType.BeginDrag;
        //call the OnDragDelegate function when the Event System detects dragging
        entryBegin.callback.AddListener((data) => { OnDragDelegate((PointerEventData)data); });
        //Add the trigger entry
        trigger.triggers.Add(entryBegin);

        EventTrigger.Entry entryEnd = new EventTrigger.Entry();
        //Add a Drag type event to the Event Trigger
        entryEnd.eventID = EventTriggerType.PointerUp;
        //call the OnDragDelegate function when the Event System detects dragging
        entryEnd.callback.AddListener((data) => { Ray((PointerEventData)data); });
        //Add the trigger entry
        trigger.triggers.Add(entryEnd);
    }

    // Update is called once per frame
    void Update() {

    }

    public void OnDragDelegate(PointerEventData data) {
        if (!hasDragBegin) {
            InputManager.GetInstance().HandleInputForCell(this.gameObject);
            hasDragBegin = true;
        }
    }
    public void Ray(PointerEventData data) {
        PointerEventData pointerData = new PointerEventData(EventSystem.current);

        pointerData.position = Input.mousePosition;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        if (results.Count > 0) {
            //WorldUI is my layer name
            if (results[0].gameObject.GetComponent<OnDragEvent>() != null) {
                InputManager.GetInstance().HandleInputForCell(results[0].gameObject);
                results.Clear();
            }

        }

    }
}