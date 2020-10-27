using System.Numerics;
using UnityEngine;
public static class ConstantValues {

    public const float swappingSpeed = 17.0f;

    public const float scaleToZeroAnimationSpeed = 10.0f;

    //int maxScoreAllowed = 25;

    public const int totalCollums = 7;
    public const int totalRows = 8;

    public const float chanchesForCashElement = 5;
    public const int cashElementValue = 50;

    public const int numberOfCashTypes = 7;
    public static readonly float[] cashElementsValues = new float[numberOfCashTypes] { 0.01f, 0.02f, 0.10f, 0.20f, 1, 2, 6 };
    public static readonly int[] cashElementsChances = new int[numberOfCashTypes] { 35, 25, 16, 12, 10, 8, 5 };

    /// <summary> Possible values of cells</summary>
    private static readonly System.Numerics.Vector4[] availColors = {
        new System.Numerics.Vector4(new System.Numerics.Vector2(Color.red.r, Color.red.b), Color.red.g, Color.red.a),
        new System.Numerics.Vector4(new System.Numerics.Vector2(Color.blue.r, Color.blue.b), Color.blue.g, Color.blue.a),
        new System.Numerics.Vector4(new System.Numerics.Vector2(Color.green.r, Color.green.b), Color.green.g, Color.green.a),
        new System.Numerics.Vector4(new System.Numerics.Vector2(Color.cyan.r, Color.cyan.b), Color.cyan.g, Color.cyan.a),
        new System.Numerics.Vector4(new System.Numerics.Vector2(Color.magenta.r, Color.magenta.b), Color.magenta.g, Color.magenta.a)
    };

    public enum AvailableSprites {
        defaultElement,
        transparent,
        highlight,
        cashWhite,
        cashGrey,
        cashBlue,
        cashGreen,
        cashPurple,
        cashRed,
        cashGold,
        cross,
        bomb,
        bell
    }
    public static int[] GetHolderIndexInHierarchy(int collum) {
        return new int[] { 1, 1, collum };
    }
    public static System.Numerics.Vector4[] GetAvailableColors() {
        return availColors;
    }
}