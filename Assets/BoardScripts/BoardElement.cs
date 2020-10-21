using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardElement {

    public enum BoardElementTypes { Default, Cash, Cross, Bomb, Bell }

    protected GameObject attachedGameobject = null;
    protected System.Type thisType = typeof(BoardElement);
    private int childIndex = -1;

    protected Color colorValue = Color.black;

    protected Sprite elementSprite = null;

    protected bool hasBeenDestroyed = false;
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
        hasBeenDestroyed = false;
        this.colorValue = newValue;
    }
    public virtual bool OnElementDestruction(BoardElement[, ] positions) {
        hasBeenDestroyed = true;
        return false;
    }
    public virtual bool OnElementDestruction(BoardElement[, ] positions, ref bool[, ] matchedElementsPositions) {
        hasBeenDestroyed = true;
        return false;
    }

    public virtual bool OnElementDestruction(ref BoardElement[, ] positions, ref bool[, ] matchedElementsPositions, ref List<AnimationMessage> playingAnimations, BoardElement otherElement) {
        hasBeenDestroyed = true;
        return false;
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

    private float cashValue = 0;

    public CashBoardElement(GameObject gameObjectToAttach, int indexInParent, int cashElementTypeIndex):
        base(gameObjectToAttach, indexInParent, Color.white) {
            base.elementSprite = AssetLoader.GetCashElementSprite(cashElementTypeIndex);
            base.thisType = typeof(CashBoardElement);
            this.cashValue = FixedElementData.cashElementsValues[cashElementTypeIndex] * MoneyManager.GetSwapCost() * 100;
        }

    public override void OnElementAppearance(Color newValue) {
        hasBeenDestroyed = false;
    }
    public override bool OnElementDestruction(BoardElement[, ] positions) {
        if (hasBeenDestroyed) {
            return false;
        }
        DetailsManager.WriteDestroyedCashElement(this);
        MoneyManager.ChangeBalanceBy(GetCashValue());
        hasBeenDestroyed = true;
        return false;
    }

    public float GetCashValue() {
        return cashValue;
    }

}

public class CrossBoardElement : BoardElement {

    public CrossBoardElement(GameObject gameObjectToAttach, int indexInParent, Color colorValue):
        base(gameObjectToAttach, indexInParent, colorValue) {

            base.elementSprite = AssetLoader.GetCrossElementSprite();
            base.thisType = typeof(CrossBoardElement);
        }

    public override void OnElementAppearance(Color newValue) {
        this.colorValue = newValue;
        hasBeenDestroyed = false;
    }

    public override bool OnElementDestruction(BoardElement[, ] positions, ref bool[, ] matchedElementsPositions) {
        if (hasBeenDestroyed) {
            return false;
        }
        KeyValuePair<int, int> pos = BoardFunctions.GetBoardPositionOfElement(this, positions);
        BoardFunctions.DestroyAllElementsCrossStyle(pos.Key, pos.Value, ref matchedElementsPositions);
        matchedElementsPositions[pos.Key, pos.Value] = true;
        hasBeenDestroyed = true;
        return true;

    }
}
public class BombBoardElement : BoardElement {

    public enum BombExplosionStyle { NormalStyle, CrossStyle, DoubleBombStyle }

    private BombExplosionStyle thisBombStyle = BombExplosionStyle.NormalStyle;

    public BombBoardElement(GameObject gameObjectToAttach, int indexInParent):
        base(gameObjectToAttach, indexInParent, Color.white) {
            base.elementSprite = AssetLoader.GetBombElementSprite();
            base.thisType = typeof(BombBoardElement);
            base.colorValue = Color.white;
        }

    public override void OnElementAppearance(Color newValue) {
        base.colorValue = Color.white;
        hasBeenDestroyed = false;
    }

    public override bool OnElementDestruction(BoardElement[, ] positions, ref bool[, ] matchedElementsPositions) {
        if (hasBeenDestroyed) {
            return false;
        }
        KeyValuePair<int, int> pos = BoardFunctions.GetBoardPositionOfElement(this, positions);
        switch (thisBombStyle) {
            case BombExplosionStyle.CrossStyle:
                BoardFunctions.DestroyAllElementsCrossStyle(pos.Key, pos.Value, ref matchedElementsPositions);
                break;
            case BombExplosionStyle.DoubleBombStyle:
                BoardFunctions.DestroyElementsDoubleBombStyle(pos.Key, pos.Value, ref matchedElementsPositions);
                break;
            case BombExplosionStyle.NormalStyle:
                BoardFunctions.DestroyElementsBombStyle(pos.Key, pos.Value, ref matchedElementsPositions);
                break;
            default:
                BoardFunctions.DestroyElementsBombStyle(pos.Key, pos.Value, ref matchedElementsPositions);
                break;
        }
        matchedElementsPositions[pos.Key, pos.Value] = true;
        hasBeenDestroyed = true;
        return true;

    }
    public void SetExplosionStyleTo(BombExplosionStyle style) {
        thisBombStyle = style;
    }
}
public class BellBoardElement : BoardElement {

    public BellBoardElement(GameObject gameObjectToAttach, int indexInParent):
        base(gameObjectToAttach, indexInParent, Color.white) {

            base.elementSprite = AssetLoader.GetBellElementSprite();
            base.thisType = typeof(BellBoardElement);
        }

    public override void OnElementAppearance(Color newValue) {
        base.colorValue = Color.white;
        hasBeenDestroyed = false;
    }

    public override bool OnElementDestruction(ref BoardElement[, ] positions, ref bool[, ] matchedElementsPositions, ref List<AnimationMessage> playingAnimations, BoardElement otherElement) {
        if (hasBeenDestroyed) {
            return false;
        }
        if (otherElement == null) {
            Debug.LogError(GetAttachedGameObject().name + " Bell OnElementDestruction() triggered with null as gameobject");
        }
        KeyValuePair<int, int> pos = BoardFunctions.GetBoardPositionOfElement(this, positions);
        BoardFunctions.ActivateBellFunction(ref positions, ref matchedElementsPositions, otherElement.GetElementValue(), ref playingAnimations, otherElement);
        matchedElementsPositions[pos.Key, pos.Value] = true;
        hasBeenDestroyed = true;
        return true;

    }
}