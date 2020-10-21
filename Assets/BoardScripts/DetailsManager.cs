using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class DetailsManager : MonoBehaviour {

    [SerializeField]
    private RectTransform detailsPanel = null;
    [SerializeField]
    RectTransform infoPanel = null;
    bool isInfoShown = true;
    [SerializeField]
    public Text scoreText = null;
    [SerializeField]
    public Transform scoreImageParent = null;

    private static Text balanceText = null;
    private static Text swapCostText = null;

    private static DetailsManager instance = null;

    private static string[] scoreTextHistory = new string[5];
    private static Sprite[] scoreImageHistory = new Sprite[5];
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

        balanceText = detailsPanel.GetChild(1).GetComponent<Text>();
        swapCostText = detailsPanel.GetChild(2).GetComponent<Text>();
        StartCoroutine(CloseInfoPanel());
    }

    public static DetailsManager GetInstance() {
        return instance;
    }

    private static void ScrollArrays(string textNewEntry, Sprite imageNewEntry) {
        string finalText = "";
        for (int i = 0; i < scoreTextHistory.Length - 1; i++) {
            if (scoreImageHistory[i + 1] != null) {
                scoreTextHistory[i] = scoreTextHistory[i + 1];
                finalText += scoreTextHistory[i] + "\n";
                scoreImageHistory[i] = scoreImageHistory[i + 1];
                instance.scoreImageParent.GetChild(i).GetComponent<Image>().sprite = scoreImageHistory[i];
            }
        }
        scoreTextHistory[scoreTextHistory.Length - 1] = textNewEntry;
        finalText += textNewEntry;
        instance.scoreText.text = finalText;
        scoreImageHistory[scoreImageHistory.Length - 1] = imageNewEntry;
        instance.scoreImageParent.GetChild(scoreImageHistory.Length - 1).GetComponent<Image>().sprite = imageNewEntry;
    }
    public static void WriteDestroyedCashElement(CashBoardElement element) {
        ScrollArrays("+ " + element.GetCashValue(), element.GetElementSprite());
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
        Transform childrenLayoutHolder = instance.infoPanel.GetChild(0);
        for (int i = 0; i < childrenLayoutHolder.childCount; i++) {
            childrenLayoutHolder.GetChild(i).GetChild(0).GetComponent<Text>().text = (FixedElementData.cashElementsValues[i] * amount * 100).ToString();
        }
    }

    public void ToggleInfoPanel() {
        if (isInfoShown) {
            Animations.AddAnimationMoveToPosition(infoPanel.transform, BoardManager.inst.GetSwappingSpeed(), -Vector3.right * infoPanel.rect.width);
            isInfoShown = false;
        }
        else {
            Animations.AddAnimationMoveToPosition(infoPanel.transform, BoardManager.inst.GetSwappingSpeed(), Vector3.zero);
            isInfoShown = true;
        }
    }
    IEnumerator CloseInfoPanel() {
        yield return new WaitForEndOfFrame();
        ToggleInfoPanel();
    }
}