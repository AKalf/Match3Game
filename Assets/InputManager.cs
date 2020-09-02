using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour {


    private GameObject firstCell;
    private GameObject secondCell;

    private bool isInteractionCorrect = false;


    private Animations.Animation firstElementAnimation;
    private Animations.Animation secondElementAnimation;


    private static InputManager instance = null;
    // Start is called before the first frame update
    void Awake() {
        if (instance == null) {
            instance = this;
        }
        else {
            Destroy(instance);
            instance = this;
        }

        OnDragEvent.hasDragBegin = false;
        firstCell = null;
        secondCell = null;

    }
    public static InputManager GetInstance() {
        return instance;
    }

    // Update is called once per frame
    void Update() {


    }


    public void HandleInputForCell(GameObject cell) {
        //Debug.Log("Input cell: " + cell.gameObject.name);
        if (!BoardManager.GetInstance().IsAvailable() || cell == null) {
            return;
        }
        else if (cell.GetComponent<BoardElement>() == null) {
            return;
        }
        if (firstCell == null) {
            firstCell = cell;
            return;
        }
        else if (secondCell == null && firstCell != cell) {
            secondCell = cell;
            BoardManager.inst.HandleInput(GetIndexInParent(firstCell.transform), GetIndexInParent(secondCell.transform));
            //Debug.Log("Swapping: " + firstCell.transform.name + " with " + secondCell.transform.name);
        }
        firstCell = null;
        secondCell = null;
    }
    public int GetIndexInParent(Transform trans) {
        for (int i = 0; i < trans.parent.childCount; i++) {
            if (trans.parent.GetChild(i) == trans) {
                return i;
            }
        }
        return -1;
    }


}