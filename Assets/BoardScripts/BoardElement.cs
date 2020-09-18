using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardElement {
    // public BoardElement upperNeighbour = null;
    // public BoardElement bottomNeighbour = null;
    // public BoardElement rightNeighbour = null;
    // public BoardElement leftNeighbour = null;
    protected GameObject attachedGameobject = null;
    protected System.Type thisType = typeof(BoardElement);
    private int childIndex = -1;

    protected Color colorValue = Color.black;

    protected Sprite elementSprite = null;
    public BoardElement(GameObject gameobjectToAttach, int indexInParent, Color colorValue) {
        thisType = typeof(BoardElement);
        attachedGameobject = gameobjectToAttach;
        childIndex = indexInParent;
        this.colorValue = colorValue;
        elementSprite = AssetLoader.GetDefaultElementSprite();

    }
    public BoardElement(BoardElement elementToCopy) {
        thisType = typeof(BoardElement);
        attachedGameobject = elementToCopy.GetAttachedGameObject();
        childIndex = elementToCopy.GetChildIndex();
        this.colorValue = elementToCopy.GetElementValue();
    }

    public int GetChildIndex() {
        return childIndex;
    }

    /// <summary>
    /// Should called when the cell's element's value is changed
    /// </summary>
    /// <param name="newValue">The new color value of the element to set</param>
    public virtual void OnElementAppearance(Color newValue) {
        this.colorValue = newValue;
    }
    public virtual void OnElementDestruction() {

    }

    public Color GetElementValue() {
        return this.colorValue;
    }

    public System.Type GetElementClassType() {
        return thisType;
    }
    public GameObject GetAttachedGameObject() {
        return attachedGameobject;
    }
    public Sprite GetElementSprite() {
        return elementSprite;
    }
}

public class CashBoardElement : BoardElement {

    int cashValue = 0;
    bool hasBeenDestroyed = false;

    public CashBoardElement(GameObject gameObjectToAttach, int indexInParent, int cashValue):
        base(gameObjectToAttach, indexInParent, Color.white) {

            base.elementSprite = AssetLoader.GetCashElementSprite();
            base.thisType = typeof(CashBoardElement);
            this.cashValue = cashValue;
        }

    public override void OnElementAppearance(Color newValue) {

    }
    public override void OnElementDestruction() {
        base.OnElementDestruction();
        if (!hasBeenDestroyed) {
            hasBeenDestroyed = true;
            MoneyManager.ChangeBalanceBy(cashValue);
        }
    }

}