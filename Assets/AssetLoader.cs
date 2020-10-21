using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssetLoader : MonoBehaviour {
    private static class PathManager {

        // Sprites
        public const string boardElementDefaultIconPath = "SpriteResources/SquareIcon";

        public static readonly string[] boardElementCashIconsPaths = new string[FixedElementData.numberOfCashTypes] { "SpriteResources/WhiteCoinIcon", "SpriteResources/GreyCoinIcon", "SpriteResources/BlueCoinIcon", "SpriteResources/GreenCoinIcon", "SpriteResources/PurpleCoinIcon", "SpriteResources/RedCoinIcon", "SpriteResources/GoldCoinIcon" };

        public const string boardElementCrossIconPath = "SpriteResources/CrossIcon";

        public const string boardElementBombIconPath = "SpriteResources/BombIcon";

        public const string boardElementBellIconPath = "SpriteResources/BellIcon";

        // Sound Effects
        public const string cellDrop_SFX = "";

        public const string coinWin_SFX = "";

    }

    Sprite defaultElementSprite = null;

    Sprite[] cashElementSprites;

    Sprite crossElementSprite = null;

    Sprite bombElementSprite = null;

    Sprite bellElementSprite = null;

    AudioClip cellDrop_SFX = null;

    AudioClip coinWin_SFX = null;
    //TO-DO: proper singleton
    private static AssetLoader inst;

    private void Awake() {
        inst = this;
        defaultElementSprite = (Sprite) Resources.Load(PathManager.boardElementDefaultIconPath, typeof(Sprite));
        //Debug.Log("Sprite: " + defaultElementSprite.name);
        cashElementSprites = new Sprite[PathManager.boardElementCashIconsPaths.Length];
        for (int i = 0; i < cashElementSprites.Length; i++) {
            cashElementSprites[i] = (Sprite) Resources.Load(PathManager.boardElementCashIconsPaths[i], typeof(Sprite));
        }

        //Debug.Log("Sprite: " + cashElementSprite.name);
        crossElementSprite = (Sprite) Resources.Load(PathManager.boardElementCrossIconPath, typeof(Sprite));

        bombElementSprite = (Sprite) Resources.Load(PathManager.boardElementBombIconPath, typeof(Sprite));

        bellElementSprite = (Sprite) Resources.Load(PathManager.boardElementBellIconPath, typeof(Sprite));

        // Audio clips
        cellDrop_SFX = (AudioClip) Resources.Load(PathManager.cellDrop_SFX, typeof(AudioClip));

        coinWin_SFX = (AudioClip) Resources.Load(PathManager.coinWin_SFX, typeof(AudioClip));
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
    public static Sprite GetCashElementSprite(int index) {
        return inst.cashElementSprites[index];
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

    // Audio clips
    public static AudioClip GetCellDropSFX() {
        return inst.cellDrop_SFX;
    }

    public static AudioClip GetCoinWinSFX() {
        return inst.coinWin_SFX;
    }
}