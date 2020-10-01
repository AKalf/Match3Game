using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class DetailsManager : MonoBehaviour {

    [SerializeField]
    private Transform detailsPanel = null;

    private static Text scoreText = null;
    private static Text balanceText = null;
    private static Text swapCostText = null;

    private static DetailsManager instance = null;

    private void Awake() {
        if (instance == null) {
            instance = this;
        }
        else if (instance != this) {
            Destroy(this);
        }
    }

    // Start is called before the first frame update
    void Start() {
        scoreText = detailsPanel.GetChild(0).GetComponent<Text>();
        balanceText = detailsPanel.GetChild(1).GetComponent<Text>();
        swapCostText = detailsPanel.GetChild(2).GetComponent<Text>();
    }

    // Update is called once per frame
    void Update() {

    }

    public static DetailsManager GetInstance() {
        return instance;
    }

    public static void ChangeScoreTextTo(int amount) {
#if UNITY_EDITOR
        if (amount < 0) {
            Debug.LogError("Negative value was passed for score text");

        }
#endif
        scoreText.text = "Score: " + amount.ToString();
    }

    public static void ChangeBalanceTextTo(float amount) {
#if UNITY_EDITOR
        if (amount < 0) {
            Debug.LogError("Negative value was passed for balance text");
        }
#endif
        balanceText.text = "Balance: " + amount.ToString();
    }

    public static void ChangeSwapCostTo(float amount) {
#if UNITY_EDITOR
        if (amount < 0) {
            Debug.LogError("Negative value was passed for swap cost text");
        }
#endif
        swapCostText.text = "Swap cost: " + amount.ToString();
    }

}