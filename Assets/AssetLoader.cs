using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssetLoader : MonoBehaviour {
    private static class PathManager {

        public const string boardElementDefaultIconPath = "SquareIcon";

        public const string boardElementCashIconPath = "CoinIcon";

        public const string boardElementCrossIconPath = "CrossIcon";

        public const string boardElementBombIconPath = "BombIcon";

        public const string boardElementBellIconPath = "BellIcon";

    }

    Sprite defaultElementSprite = null;

    Sprite cashElementSprite = null;

    Sprite crossElementSprite = null;

    Sprite bombElementSprite = null;

    Sprite bellElementSprite = null;

    //TO-DO: proper singleton
    private static AssetLoader inst;

    private void Awake() {
        inst = this;
        defaultElementSprite = (Sprite) Resources.Load(PathManager.boardElementDefaultIconPath, typeof(Sprite));
        //Debug.Log("Sprite: " + defaultElementSprite.name);
        cashElementSprite = (Sprite) Resources.Load(PathManager.boardElementCashIconPath, typeof(Sprite));
        //Debug.Log("Sprite: " + cashElementSprite.name);
        crossElementSprite = (Sprite) Resources.Load(PathManager.boardElementCrossIconPath, typeof(Sprite));

        bombElementSprite = (Sprite) Resources.Load(PathManager.boardElementBombIconPath, typeof(Sprite));

        bellElementSprite = (Sprite) Resources.Load(PathManager.boardElementBellIconPath, typeof(Sprite));
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
    public static Sprite GetCrossElementSprite() {
        return inst.crossElementSprite;
    }
    public static Sprite GetBombElementSprite() {
        return inst.bombElementSprite;
    }
    public static Sprite GetBellElementSprite() {
        return inst.bellElementSprite;
    }

}