using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FixedElementData {
    public const float chanchesForCashElement = 5;
    public const int cashElementValue = 50;

    public const int numberOfCashTypes = 7;
    public static readonly float[] cashElementsValues = new float[numberOfCashTypes] { 0.01f, 0.02f, 0.10f, 0.20f, 1, 2, 6 };
    public static readonly int[] cashElementsChances = new int[numberOfCashTypes] { 35, 25, 16, 12, 10, 8, 5 };

}