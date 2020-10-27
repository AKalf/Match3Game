using System.Collections.Generic;
using System.Numerics;

public class BoardElement {

    public enum BoardElementTypes { Default, Cash, Cross, Bomb, Bell }

    protected System.Type thisType = typeof(BoardElement);
    private int[] transformIndexInHierarchy;
    private int[] imageTransformIndex;
    private int[] highlightImageTransformIndex;
    protected Vector4 colorValue = new Vector4(0, 0, 0, 0);

    protected int elementSpriteIndexInArray = -1;

    protected bool hasBeenDestroyed = false;
    public BoardElement(int[] indexInParent, Vector4 colorValue) {
        thisType = typeof(BoardElement);
        transformIndexInHierarchy = indexInParent;
        imageTransformIndex = new int[transformIndexInHierarchy.Length + 1];
        transformIndexInHierarchy.CopyTo(imageTransformIndex, 0);
        imageTransformIndex[imageTransformIndex.Length - 1] = 0;
        highlightImageTransformIndex = new int[transformIndexInHierarchy.Length + 1];
        transformIndexInHierarchy.CopyTo(highlightImageTransformIndex, 0);
        highlightImageTransformIndex[highlightImageTransformIndex.Length - 1] = 1;
        this.colorValue = colorValue;
        elementSpriteIndexInArray = (int) ConstantValues.AvailableSprites.defaultElement;

    }

    public int[] GetTransformIndex() {
        return transformIndexInHierarchy;
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
    public virtual void OnElementAppearance(Vector4 newValue) {
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
    public Vector4 GetElementValue() {
        return this.colorValue;
    }

    public System.Type GetElementClassType() {
        return thisType;
    }
    public int GetElementSpriteIndex() {
        return elementSpriteIndexInArray;
    }
}

public class CashBoardElement : BoardElement {

    private float cashValue = 0;

    public CashBoardElement(int[] indexInParent, int cashElementTypeIndex):
        base(indexInParent, Vector4.One) {
            base.elementSpriteIndexInArray = (int) ConstantValues.AvailableSprites.cashWhite + cashElementTypeIndex;
            base.thisType = typeof(CashBoardElement);
            this.cashValue = ConstantValues.cashElementsValues[cashElementTypeIndex] * MoneyManager.GetSwapCost() * 100;
        }

    public override void OnElementAppearance(Vector4 newValue) {
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

    public CrossBoardElement(int[] indexInParent, Vector4 colorValue):
        base(indexInParent, colorValue) {

            base.elementSpriteIndexInArray = (int) ConstantValues.AvailableSprites.cross;
            base.thisType = typeof(CrossBoardElement);
        }

    public override void OnElementAppearance(Vector4 newValue) {
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

    public BombBoardElement(int[] indexInParent):
        base(indexInParent, Vector4.One) {
            base.elementSpriteIndexInArray = (int) ConstantValues.AvailableSprites.bomb;
            base.thisType = typeof(BombBoardElement);
            base.colorValue = Vector4.One;
        }

    public override void OnElementAppearance(Vector4 newValue) {
        base.colorValue = Vector4.One;
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

    public BellBoardElement(int[] indexInParent):
        base(indexInParent, Vector4.One) {

            base.elementSpriteIndexInArray = (int) ConstantValues.AvailableSprites.bell;
            base.thisType = typeof(BellBoardElement);
        }

    public override void OnElementAppearance(Vector4 newValue) {
        base.colorValue = Vector4.One;
        hasBeenDestroyed = false;
    }

    public override bool OnElementDestruction(BoardManager board, BoardElement otherElement) {
        if (hasBeenDestroyed) {
            return false;
        }
        // if (otherElement == null) {
        //     Debug.LogError(BoardFunctions.GetTransformByIndex(otherElement.GetTransformIndex()) + " Bell OnElementDestruction() triggered with null as gameobject");
        // }
        KeyValuePair<int, int> pos = BoardFunctions.GetBoardPositionOfElement(this, board.elementsPositions);
        BoardFunctions.ActivateBellFunction(board, otherElement);
        board.matchedElemPositions[pos.Key, pos.Value] = true;
        hasBeenDestroyed = true;
        return true;

    }
}