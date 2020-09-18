using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssetLoader : MonoBehaviour {
    private static class PathManager {

        private static string boardElementDefaultIcon = "SquareIcon";

        public static string GetBoardElementDefaultIconPath() {
            return boardElementDefaultIcon;
        }

        private static string boardElementCashIcon = "CoinIcon";

        public static string GetBoardElementCashIconPath() {
            return boardElementCashIcon;
        }

    }

    [SerializeField]
    Sprite defaultElementSprite = null;
    [SerializeField]
    Sprite cashElementSprite = null;

    private static AssetLoader inst;

    private void Awake() {
        inst = this;
        //   defaultElementSprite = (Sprite) Resources.Load(PathManager.GetBoardElementDefaultIconPath(), typeof(Sprite));
        Debug.Log("Sprite: " + defaultElementSprite.name);
        // cashElementSprite = (Sprite) Resources.Load(PathManager.GetBoardElementCashIconPath(), typeof(Sprite));
        Debug.Log("Sprite: " + cashElementSprite.name);
    }
    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }
    public static Sprite GetDefaultElementSprite() {
        return inst.defaultElementSprite;
    }
    public static Sprite GetCashElementSprite() {
        return inst.cashElementSprite;
    }

}