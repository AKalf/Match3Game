using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardElement : MonoBehaviour {
    // public BoardElement upperNeighbour = null;
    // public BoardElement bottomNeighbour = null;
    // public BoardElement rightNeighbour = null;
    // public BoardElement leftNeighbour = null;
    public int childIndex = -1;
    public Color color = Color.black;


    // Start is called before the first frame update
    void Awake() {
        color = GetComponent<UnityEngine.UI.Image>().color;
    }

    // Update is called once per frame
    void Update() {

    }
    /// Depricated
    // public bool GetIfNeighbourWith(BoardElement element) {
    //     if (element == null) {
    //         return false;
    //     }
    //     else if (
    //         element != upperNeighbour && element != bottomNeighbour &&
    //         element != leftNeighbour && element != rightNeighbour) {
    //         return false;
    //     }
    //     else {
    //         return true;
    //     }
    // }


}