using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoneyManager : MonoBehaviour {
    private static float balance = 100;

    private static float swapCost = 0.01f;
    private static float swapCostMin = 0.01f;
    private static float swapCostMax = 1.0f;

    private static MoneyManager instance = null;

    private void Awake() {
        if (instance == null) {
            instance = this;
        }
        else if (instance != this) {
            Destroy(this);
        }

    }

    public static void SetBalanceTo(float amount) {
        balance = amount;
    }

    public static void ChangeBalanceBy(float amount) {
        balance += amount;
        Server.GetServerInstance().SendMessageToClientChangeBalanceTo(balance, new GameClient(BoardManager.GetInstance()));
    }

    // TO-DO: delete this function
    /// <summary> Only use for demo </summary>
    public void InstanceChangeBalanceBy(float amount) {
        ChangeBalanceBy(amount);
    }

    public static float GetBalance() {
        return balance;
    }

    public static void SetSwapCostTo(float amount) {
#if UNITY_EDITOR
        if (amount < swapCostMin || amount > swapCostMax) {
            Debug.LogError("Amount: " + amount + " is smaller or bigger than min/max (" + swapCostMin + "/" + swapCostMax + ") allows for cost of swap");
            return;
        }
#endif
        swapCost = amount;
        Server.GetServerInstance().SendMessageToClientChangeSwapCost(swapCost, new GameClient(BoardManager.GetInstance()));
    }
    public static void ChangeSwapCostBy(float amount) {
        if (swapCost + amount <= swapCostMax && swapCost + amount >= swapCostMin) {
            swapCost += amount;
            Server.GetServerInstance().SendMessageToClientChangeSwapCost(swapCost, new GameClient(BoardManager.GetInstance()));
        }
    }
    // TO-DO: delete this function
    /// <summary> Only use for demo </summary>
    public void InstanceChangeSwapCostBy(float amount) {
        if (swapCost + amount <= swapCostMax && swapCost + amount >= swapCostMin) {
            swapCost += amount;
            Server.GetServerInstance().SendMessageToClientChangeSwapCost(swapCost, new GameClient(BoardManager.GetInstance()));
        }
    }

    public static float GetSwapCost() {
        return swapCost;
    }

    public static float GetSwapCostMax() {
        return swapCostMax;
    }

    public static float GetSwapCostMin() {
        return swapCostMin;
    }
}