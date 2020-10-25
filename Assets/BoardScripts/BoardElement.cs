using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardElement {

    public enum BoardElementTypes { Default, Cash, Cross, Bomb, Bell }

    protected GameObject attachedGameobject = null;
    protected System.Type thisType = typeof(BoardElement);
    private int[] transformIndex;
    private int[] imageTransformIndex;
    private int[] highlightImageTransformIndex;
    protected Color colorValue = Color.black;

    protected int elementSpriteIndexInArray = -1;

    protected bool hasBeenDestroyed = false;
    public BoardElement(GameObject gameobjectToAttach, int[] indexInParent, Color colorValue) {
        thisType = typeof(BoardElement);
        attachedGameobject = gameobjectToAttach;
        transformIndex = indexInParent;
        imageTransformIndex = new int[transformIndex.Length + 1];
        transformIndex.CopyTo(imageTransformIndex, 0);
        imageTransformIndex[imageTransformIndex.Length - 1] = 0;
        highlightImageTransformIndex = new int[transformIndex.Length + 1];
        transformIndex.CopyTo(highlightImageTransformIndex, 0);
        highlightImageTransformIndex[highlightImageTransformIndex.Length - 1] = 1;
        this.colorValue = colorValue;
        elementSpriteIndexInArray = (int) FixedElementData.AvailableSprites.defaultElement;

    }

    public int[] GetTransformIndex() {
        return transformIndex;
    }
    public int[] GetImageTransformIndex() {
        return imageTransformIndex;
    }
    public int[] GetHighlightImageTransformIndex() {
        return highlightImageTransformIndex;
    }
    /// <summary>
    /// Should called when the cell's element's value is changed
    /// </summary>
    /// <param name="newValue">The new color value of the element to set</param>
    public virtual void OnElementAppearance(Color newValue) {
        hasBeenDestroyed = false;
        this.colorValue = newValue;
    }
    public virtual bool OnElementDestruction(BoardManager board) {
        hasBeenDestroyed = true;
        return false;
    }

    public virtual bool OnElementDestruction(BoardManager board, BoardElement otherElement) {
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
    public int GetElementSpriteIndex() {
        return elementSpriteIndexInArray;
    }
}

public class CashBoardElement : BoardElement {

    private float cashValue = 0;

    public CashBoardElement(GameObject gameObjectToAttach, int[] indexInParent, int cashElementTypeIndex):
        base(gameObjectToAttach, indexInParent, Color.white) {
            base.elementSpriteIndexInArray = (int) FixedElementData.AvailableSprites.cashWhite + cashElementTypeIndex;
            base.thisType = typeof(CashBoardElement);
            this.cashValue = FixedElementData.cashElementsValues[cashElementTypeIndex] * MoneyManager.GetSwapCost() * 100;
        }

    public override void OnElementAppearance(Color newValue) {
        hasBeenDestroyed = false;
    }
    public override bool OnElementDestruction(BoardManager board) {
        if (hasBeenDestroyed) {
            return false;
        }
        MoneyManager.ChangeBalanceBy(GetCashValue());
        hasBeenDestroyed = true;
        return false;
    }

    public float GetCashValue() {
        return cashValue;
    }

}

public class CrossBoardElement : BoardElement {

    public CrossBoardElement(GameObject gameObjectToAttach, int[] indexInParent, Color colorValue):
        base(gameObjectToAttach, indexInParent, colorValue) {

            base.elementSpriteIndexInArray = (int) FixedElementData.AvailableSprites.cross;
            base.thisType = typeof(CrossBoardElement);
        }

    public override void OnElementAppearance(Color newValue) {
        this.colorValue = newValue;
        hasBeenDestroyed = false;
    }

    public override bool OnElementDestruction(BoardManager board) {
        if (hasBeenDestroyed) {
            return false;
        }
        KeyValuePair<int, int> pos = BoardFunctions.GetBoardPositionOfElement(this, board.elementsPositions);
        BoardFunctions.DestroyAllElementsCrossStyle(pos.Key, pos.Value, ref board.matchedElemPositions);
        board.matchedElemPositions[pos.Key, pos.Value] = true;
        hasBeenDestroyed = true;
        return true;

    }
}
public class BombBoardElement : BoardElement {

    public enum BombExplosionStyle { NormalStyle, CrossStyle, DoubleBombStyle }

    private BombExplosionStyle thisBombStyle = BombExplosionStyle.NormalStyle;

    public BombBoardElement(GameObject gameObjectToAttach, int[] indexInParent):
        base(gameObjectToAttach, indexInParent, Color.white) {
            base.elementSpriteIndexInArray = (int) FixedElementData.AvailableSprites.bomb;
            base.thisType = typeof(BombBoardElement);
            base.colorValue = Color.white;
        }

    public override void OnElementAppearance(Color newValue) {
        base.colorValue = Color.white;
        hasBeenDestroyed = false;
    }

    public override bool OnElementDestruction(BoardManager board) {
        if (hasBeenDestroyed) {
            return false;
        }
        KeyValuePair<int, int> pos = BoardFunctions.GetBoardPositionOfElement(this, board.elementsPositions);
        switch (thisBombStyle) {
            case BombExplosionStyle.CrossStyle:
                BoardFunctions.DestroyAllElementsCrossStyle(pos.Key, pos.Value, ref board.matchedElemPositions);
                break;
            case BombExplosionStyle.DoubleBombStyle:
                BoardFunctions.DestroyElementsDoubleBombStyle(pos.Key, pos.Value, ref board.matchedElemPositions);
                break;
            case BombExplosionStyle.NormalStyle:
                BoardFunctions.DestroyElementsBombStyle(pos.Key, pos.Value, ref board.matchedElemPositions);
                break;
            default:
                BoardFunctions.DestroyElementsBombStyle(pos.Key, pos.Value, ref board.matchedElemPositions);
                break;
        }
        board.matchedElemPositions[pos.Key, pos.Value] = true;
        hasBeenDestroyed = true;
        return true;

    }
    public void SetExplosionStyleTo(BombExplosionStyle style) {
        thisBombStyle = style;
    }
}
public class BellBoardElement : BoardElement {

    public BellBoardElement(GameObject gameObjectToAttach, int[] indexInParent):
        base(gameObjectToAttach, indexInParent, Color.white) {

            base.elementSpriteIndexInArray = (int) FixedElementData.AvailableSprites.bell;
            base.thisType = typeof(BellBoardElement);
        }

    public override void OnElementAppearance(Color newValue) {
        base.colorValue = Color.white;
        hasBeenDestroyed = false;
    }

    public override bool OnElementDestruction(BoardManager board, BoardElement otherElement) {
        if (hasBeenDestroyed) {
            return false;
        }
        if (otherElement == null) {
            Debug.LogError(GetAttachedGameObject().name + " Bell OnElementDestruction() triggered with null as gameobject");
        }
        KeyValuePair<int, int> pos = BoardFunctions.GetBoardPositionOfElement(this, board.elementsPositions);
        BoardFunctions.ActivateBellFunction(board, otherElement);
        board.matchedElemPositions[pos.Key, pos.Value] = true;
        hasBeenDestroyed = true;
        return true;

    }
}