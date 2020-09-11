using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssetLoader : MonoBehaviour
{
    private static class PathManager
    {
        private static string boardElementDefaultIcon = "";

        public static string GetBoardElementDefaultIconPath()
        {
            return boardElementDefaultIcon;
        }

        private static string boardElementCashIcon = "";

        public static string GetBoardElementCashIconPath()
        {
            return boardElementCashIcon;
        }

    }

    static Sprite defaultElementSprite = null;
    static Sprite cashElementSprite = null;

    private void Awake()
    {
        defaultElementSprite = (Sprite)Resources.Load(PathManager.GetBoardElementDefaultIconPath());
        cashElementSprite = (Sprite)Resources.Load(PathManager.GetBoardElementCashIconPath());
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    public static Sprite GetDefaultElementSprite()
    {
        return defaultElementSprite;
    }
    public static Sprite GetCashElementSprite()
    {
        return cashElementSprite;
    }


}
