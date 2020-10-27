using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssetLoader : MonoBehaviour {
    private static class PathManager {

        // Sprites
        public static string[] spritesPaths = new string[] {
            "SpriteResources/SquareIcon",
            "SpriteResources/TransparentIcon",
            "SpriteResources/HighlightIcon",

            "SpriteResources/WhiteCoinIcon",
            "SpriteResources/GreyCoinIcon",
            "SpriteResources/BlueCoinIcon",
            "SpriteResources/GreenCoinIcon",
            "SpriteResources/PurpleCoinIcon",
            "SpriteResources/RedCoinIcon",
            "SpriteResources/GoldCoinIcon",

            "SpriteResources/CrossIcon",
            "SpriteResources/BombIcon",
            "SpriteResources/BellIcon"
        };

        // Sound Effects
        public const string cellDrop_SFX = "AudioResources/Swap_Sound";

        public const string coinWin_SFX = "AudioResources/Coin_Sound";

    }

    private Sprite[] sprites = new Sprite[13];

    AudioClip cellDrop_SFX = null;

    AudioClip coinWin_SFX = null;
    //TO-DO: proper singleton
    private static AssetLoader inst;

    private void Awake() {
        inst = this;
        for (int i = 0; i < sprites.Length; i++) {
            // Debug.Log(PathManager.spritesPaths[i]);
            sprites[i] = (Sprite) Resources.Load(PathManager.spritesPaths[i], typeof(Sprite));
        }
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
    public static Sprite GetSprite(ConstantValues.AvailableSprites sprite) {
        return inst.sprites[(int) sprite];
    }
    public static Sprite GetSprite(int indexInArray) {
        return inst.sprites[indexInArray];
    }

    // Audio clips  
    public static AudioClip GetCellDropSFX() {
        return inst.cellDrop_SFX;
    }

    public static AudioClip GetCoinWinSFX() {
        return inst.coinWin_SFX;
    }
}