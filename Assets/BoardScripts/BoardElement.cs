using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardElement
{
    // public BoardElement upperNeighbour = null;
    // public BoardElement bottomNeighbour = null;
    // public BoardElement rightNeighbour = null;
    // public BoardElement leftNeighbour = null;
    protected GameObject attachedGameobject = null;
    protected System.Type thisType = typeof(BoardElement);
    private int childIndex = -1;

    protected UnityEngine.UI.Image imageComponent = null;

    private Color color = Color.black;

    public BoardElement(GameObject gameobjectToAttach, int indexInParent, Color colorValue)
    {
        thisType = typeof(BoardElement);
        attachedGameobject = gameobjectToAttach;
        childIndex = indexInParent;
        this.imageComponent = attachedGameobject.GetComponent<UnityEngine.UI.Image>();
        this.imageComponent.color = colorValue;
        imageComponent.sprite = AssetLoader.GetDefaultElementSprite();
    }

    // public void SetIndexInParent(int index)
    // {
    //     childIndex = index;
    // }
    public int GetChildIndex()
    {
        return childIndex;
    }

    public virtual void OnElementCreation(Color newValue)
    {
        this.color = newValue;
        imageComponent.color = color;
    }
    public virtual void OnElementDestruction()
    {

    }
    public void SetValue(Color color)
    {
        this.color = color;
    }
    public Color GetElementValue()
    {
        return this.color;
    }
    public System.Type GetElementClassType()
    {
        return thisType;
    }
    public GameObject GetAttachedGameObject()
    {
        return attachedGameobject;
    }

}
public class CashBoardElement : BoardElement
{

    int cashValue = 0;

    public CashBoardElement(GameObject gameObjectToAttach, int indexInParent, Color colorValue, int cashValue) : base(gameObjectToAttach, indexInParent, colorValue)
    {
        base.imageComponent.color = Color.white;
        base.thisType = typeof(CashBoardElement);
        this.cashValue = cashValue;
        this.imageComponent.sprite = AssetLoader.GetCashElementSprite();
    }


    public override void OnElementCreation(Color newValue)
    {

    }
    public override void OnElementDestruction()
    {
        base.OnElementDestruction();
        MoneyManager.ChangeBalanceBy(cashValue);
    }
}