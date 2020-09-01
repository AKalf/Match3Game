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
        }
        else if (secondCell == null && firstCell != cell) {
            secondCell = cell;
            BoardElement firstElement = firstCell.GetComponent<BoardElement>();
            BoardElement secondElement = secondCell.GetComponent<BoardElement>();
            if (BoardManager.GetInstance().GetIfNeighbours(firstElement, secondElement)) {
                Debug.Log("Correct input: " + firstCell.name + ", with " + secondCell.name);


                BoardManager.GetInstance().SwapElements(firstElement, secondElement, rewire : false);

                firstCell = null;
                secondCell = null;
            }
            else {
                BoardManager.GetInstance().SwapElements(firstCell.GetComponent<BoardElement>(), secondCell.GetComponent<BoardElement>(), rewire : true);
                firstCell = null;
                secondCell = null;
            }
            //Debug.Log("Swapping: " + firstCell.transform.name + " with " + secondCell.transform.name);

        }
    }


}