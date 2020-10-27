using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Animations : MonoBehaviour {

    public static Dictionary<RectTransform, List<Animation>> animatedTransforms = new Dictionary<RectTransform, List<Animation>>();
    //static List<int> animationIDsPlayed = new List<int>();

    private static Animation currentAnimation = null;
    List<RectTransform> toRemove = new List<RectTransform>();

    private double timer = 0.0f;
    private double timerThreshold = 0.01f;

    /// <summary> Holds cells</summary>
    [SerializeField]
    RectTransform canvasTransform = null;
    RectTransform boardTransform = null;
    RectTransform gamePanel = null;
    [SerializeField]
    RectTransform popUpPanel = null;
    private static UnityEngine.UI.Text popUpText = null;
    private static UnityEngine.UI.Image popUpImage = null;

    private AudioSource animationsAudioSource = null;
    private static bool areAnimationsPlaying = false;

    private static Vector3[, ] positionsOnWorld;

    public static Animations inst = null;

    /// <summary>
    /// Scale local scale of a transfrom to one
    /// </summary>
    public static Animation AddAnimationScaleTo(Messages.AnimationMessage msg) {
        RectTransform transfromToScale = GetMessageTransform(msg);
        if (animatedTransforms.ContainsKey(transfromToScale) == false) {
            animatedTransforms.Add(transfromToScale, new List<Animation>());
        }
        Animation anim = new Animation(msg.messageID, msg.dependencyID, transfromToScale, msg.speed, Messages.AnimationMessage.AnimationMessageTypes.Scale, new Vector3(msg.targetX, msg.targetY, msg.targetZ));
        animatedTransforms[transfromToScale].Add(anim);
        // transfromToScale.GetComponent<BoardElement>().AddAnimationToPlay(anim);
        return anim;
    }

    public static Animation AddAnimationChangeSprite(Messages.AnimationMessage msg) {
        RectTransform elementTransform = GetMessageTransform(msg);
        if (animatedTransforms.ContainsKey(elementTransform) == false) {
            animatedTransforms.Add(elementTransform, new List<Animation>());
        }
        Animation anim = new Animation(msg.messageID, msg.dependencyID, msg.type, elementTransform, AssetLoader.GetSprite(msg.spriteIndexInArray), new Color(msg.targetX, msg.targetY, msg.targetZ, msg.targetW));
        animatedTransforms[elementTransform].Add(anim);
        // transfromToScale.GetComponent<BoardElement>().AddAnimationToPlay(anim);
        return anim;
    }

    public static Animation AddAnimationScrollWinHistory(Messages.AnimationMessage msg) {
        RectTransform elementTransform = GetMessageTransform(msg);
        if (animatedTransforms.ContainsKey(elementTransform) == false) {
            animatedTransforms.Add(elementTransform, new List<Animation>());
        }
        Animation anim = new Animation(msg.messageID, msg.dependencyID, Messages.AnimationMessage.AnimationMessageTypes.Scroll, elementTransform, msg.text, AssetLoader.GetSprite(msg.spriteIndexInArray));
        animatedTransforms[elementTransform].Add(anim);
        // transfromToScale.GetComponent<BoardElement>().AddAnimationToPlay(anim);
        return anim;
    }

    public static Animation AddAnimationPopUpBox(Messages.AnimationMessage msg) {
        RectTransform elementTransform = GetMessageTransform(msg);
        if (animatedTransforms.ContainsKey(elementTransform) == false) {
            animatedTransforms.Add(elementTransform, new List<Animation>());
        }
        Sprite spr = null;
        if (msg.spriteIndexInArray > -1) {
            spr = AssetLoader.GetSprite(msg.spriteIndexInArray);
        }
        else {
            spr = AssetLoader.GetSprite(ConstantValues.AvailableSprites.transparent);
        }
        Animation anim = new Animation(msg.messageID, msg.dependencyID, Messages.AnimationMessage.AnimationMessageTypes.PopUpBox, elementTransform, msg.text, spr);
        animatedTransforms[elementTransform].Add(anim);
        // transfromToScale.GetComponent<BoardElement>().AddAnimationToPlay(anim);
        return anim;
    }

    /// <summary>
    /// Move a transfrom to a position
    /// </summary>
    public static Animation AddAnimationMoveToPosition(Messages.AnimationMessage msg) {
        RectTransform tranformToMove = GetMessageTransform(msg);
        if (animatedTransforms.ContainsKey(tranformToMove) == false) {
            animatedTransforms.Add(tranformToMove, new List<Animation>());
        }
        Animation anim;
        if (msg.posCollum == -1 || msg.posRow == -1) {
            anim = new Animation(msg.messageID, msg.dependencyID, tranformToMove, msg.speed, Messages.AnimationMessage.AnimationMessageTypes.MoveTo, new Vector3(msg.targetX, msg.targetY, msg.targetZ));
        }
        else {
            anim = new Animation(msg.messageID, msg.dependencyID, tranformToMove, msg.speed, Messages.AnimationMessage.AnimationMessageTypes.MoveTo, positionsOnWorld[msg.posCollum, msg.posRow]);
        }
        // tranformToMove.GetComponent<BoardElement>().AddAnimationToPlay(anim);
        animatedTransforms[tranformToMove].Add(anim);
        return anim;
    }
    public static Animation AddAnimationMoveToPosition(int messageID, int dependencyID, RectTransform tranformToMove, float speed, Vector3 target) {
        if (animatedTransforms.ContainsKey(tranformToMove) == false) {
            animatedTransforms.Add(tranformToMove, new List<Animation>());
        }
        Animation anim = new Animation(messageID, dependencyID, tranformToMove, speed, Messages.AnimationMessage.AnimationMessageTypes.MoveTo, target);
        // tranformToMove.GetComponent<BoardElement>().AddAnimationToPlay(anim);
        animatedTransforms[tranformToMove].Add(anim);
        return anim;
    }

    private static void MoveTo(Transform trans, float speed, Vector3 target) {
        //Debug.Log("Speed: " + speed + " final speed: " + speed * Time.deltaTime);
        trans.GetComponent<RectTransform>().anchoredPosition = Vector3.Lerp(trans.GetComponent<RectTransform>().anchoredPosition, target, speed * Time.deltaTime);

    }

    private static void ScaleTo(Transform transfromToScale, float speed, Vector3 target) {
        transfromToScale.localScale = Vector3.Lerp(transfromToScale.localScale, target, speed * Time.deltaTime);

    }

    private static void ChangeSprite(Transform boardElementGameObject, Sprite sprite, Color targetColor) {
        UnityEngine.UI.Image targetImageComponent = boardElementGameObject.GetComponent<UnityEngine.UI.Image>();
        targetImageComponent.sprite = sprite;
        targetImageComponent.color = targetColor;
    }

    public static void ReceiveAnimationMessages(List<Messages.AnimationMessage> messages) {
        for (int i = 0; i < messages.Count; i++) {
            switch (messages[i].type) {
                case Messages.AnimationMessage.AnimationMessageTypes.MoveTo:
                    AddAnimationMoveToPosition(messages[i]);
                    break;
                case Messages.AnimationMessage.AnimationMessageTypes.Scale:
                    AddAnimationScaleTo(messages[i]);
                    break;
                case Messages.AnimationMessage.AnimationMessageTypes.ChangeSprite:
                    AddAnimationChangeSprite(messages[i]);
                    break;
                case Messages.AnimationMessage.AnimationMessageTypes.Scroll:
                    AddAnimationScrollWinHistory(messages[i]);
                    break;
                case Messages.AnimationMessage.AnimationMessageTypes.MoveToHolder:
                    AddAnimationMoveToPosition(messages[i].messageID, messages[i].dependencyID, GetMessageTransform(messages[i]), messages[i].speed, GetMessageTransform(ConstantValues.GetHolderIndexInHierarchy(messages[i].posCollum)).anchoredPosition);
                    break;
                case Messages.AnimationMessage.AnimationMessageTypes.PopUpBox:
                    AddAnimationPopUpBox(messages[i]);
                    break;

            }
        }

    }

    private static RectTransform GetMessageTransform(Messages.AnimationMessage message) {
        RectTransform trans = inst.canvasTransform;

        for (int y = 0; y < message.indexInHierarchy.Length; y++) {
            // Debug.Log(message.type);
            // Debug.Log("index: " + y);
            // Debug.Log(trans.name);
            trans = trans.GetChild(message.indexInHierarchy[y]).GetComponent<RectTransform>();
        }
        //Debug.Log("End");
        return trans;
    }

    private static RectTransform GetMessageTransform(int[] indexInHierarchy) {
        RectTransform trans = inst.canvasTransform;

        for (int y = 0; y < indexInHierarchy.Length; y++) {
            // Debug.Log(message.type);
            // Debug.Log("index: " + y);
            // Debug.Log(trans.name);
            trans = trans.GetChild(indexInHierarchy[y]).GetComponent<RectTransform>();
        }
        //Debug.Log("End");
        return trans;
    }

    private void Awake() {
        inst = this;
        animationsAudioSource = this.gameObject.AddComponent<AudioSource>();
        boardTransform = canvasTransform.GetChild(0).GetComponent<RectTransform>();
        gamePanel = boardTransform.GetChild(1).GetComponent<RectTransform>();
        positionsOnWorld = new Vector3[ConstantValues.totalCollums, ConstantValues.totalRows];
        popUpText = popUpPanel.GetChild(0).GetComponent<UnityEngine.UI.Text>();
        popUpImage = popUpPanel.GetChild(1).GetComponent<UnityEngine.UI.Image>();
    }

    private void Start() {
        StartCoroutine("CoWaitForPosition");
    }

    private void Update() {

        if (animatedTransforms.Keys.Count > 0 && !Client.GetIfGameIsPaused()) {
            areAnimationsPlaying = true;
            foreach (RectTransform trans in animatedTransforms.Keys) {

                int indexOfNextAnimation = 0;
                while (animatedTransforms[trans][indexOfNextAnimation].dependencyID != -1 && Client.GetPlayedMessagesIDs().Contains(animatedTransforms[trans][indexOfNextAnimation].dependencyID)) {
                    indexOfNextAnimation++;
                }
                currentAnimation = animatedTransforms[trans][indexOfNextAnimation];

                if (currentAnimation.hasFinished) {
                    //Debug.Log("----- Animation finished: " + currentAnimation.animationType + " for transform: " + currentAnimation.transform.name);
                    //AlexDebugger.GetInstance().AddMessage("Animation finished: " + currentAnimation.animationType + " for transfrom " + currentAnimation.transform.name, AlexDebugger.tags.Animations);
                    //if (currentAnimation.speed > 5000) {
                    //AlexDebugger.GetInstance().AddMessage("Transfrom " + currentAnimation.transform.name + " moved to holders position", AlexDebugger.tags.Animations);
                    //}
                    Client.AddProccessedMessageID(currentAnimation.animationID);
                    animatedTransforms[currentAnimation.transform].Remove(currentAnimation);
                    if (animatedTransforms[currentAnimation.transform].Count < 1) {
                        toRemove.Add(trans);
                    }

                }
                else {
                    PlayAnimation(currentAnimation);
                    // break;
                }
            }
            if (toRemove.Count > 0) {
                foreach (RectTransform t in toRemove) {
                    animatedTransforms.Remove(t);
                }
                if (animatedTransforms.Keys.Count < 1) {
                    BoardManager.areAnimationsPlaying = false;
                }
                toRemove.Clear();
            }

        }
        else if (animatedTransforms.Count == 0) {
            areAnimationsPlaying = false;
        }
    }

    IEnumerator CoWaitForPosition() {
        yield return new WaitForEndOfFrame();
        int i = 0;
        for (int row = 0; row < ConstantValues.totalRows; row++) {
            for (int collum = 0; collum < ConstantValues.totalCollums; collum++) {
                positionsOnWorld[collum, row] = gamePanel.GetChild(i).GetComponent<RectTransform>().anchoredPosition;
                i++;
            }
        }

    }

    public void PlayAnimation(Animations.Animation animToPlay) {
        switch (animToPlay.animationType) {
            case Messages.AnimationMessage.AnimationMessageTypes.Scale:
                ScaleTo(animToPlay.transform, animToPlay.speed, animToPlay.target);
                animToPlay.isPlaying = true;
                if (Vector3.Distance(animToPlay.transform.localScale, animToPlay.target) < 0.001f) {
                    animToPlay.transform.localScale = animToPlay.target;
                    animToPlay.isPlaying = false;
                    animToPlay.hasFinished = true;
                }

                break;
            case Messages.AnimationMessage.AnimationMessageTypes.MoveTo:
                //Debug.Log("Anim to play speed: " + animToPlay.speed + " time " + Time.deltaTime);

                MoveTo(animToPlay.transform, animToPlay.speed, animToPlay.target);
                animToPlay.isPlaying = true;
                animToPlay.lastTimePlay = Time.time;
                if (Vector3.Distance(animToPlay.transform.GetComponent<RectTransform>().anchoredPosition, animToPlay.target) < 2.0f) {
                    animToPlay.isPlaying = false;
                    animToPlay.hasFinished = true;
                    animToPlay.transform.GetComponent<RectTransform>().anchoredPosition = animToPlay.target;
                }
                break;
            case Messages.AnimationMessage.AnimationMessageTypes.ChangeSprite:
                animToPlay.isPlaying = true;
                ChangeSprite(animToPlay.transform, animToPlay.targetSprite, animToPlay.targetColor);
                animToPlay.hasFinished = true;
                animToPlay.isPlaying = false;
                break;
            case Messages.AnimationMessage.AnimationMessageTypes.Scroll:
                animToPlay.isPlaying = true;
                DetailsManager.WriteDestroyedCashElement(animToPlay.text, animToPlay.targetSprite);
                animToPlay.hasFinished = true;
                animToPlay.isPlaying = false;
                break;
            case Messages.AnimationMessage.AnimationMessageTypes.PopUpBox:
                animToPlay.isPlaying = true;
                popUpText.text = animToPlay.text;
                popUpImage.sprite = animToPlay.targetSprite;
                popUpPanel.gameObject.SetActive(true);
                Client.PauseGame();
                animToPlay.hasFinished = true;
                animToPlay.isPlaying = false;
                break;
        }
    }

    public static bool AreAnimationsPlaying() {
        return areAnimationsPlaying;
    }

    public class Animation {
        public RectTransform transform = null;
        public int animationID = -1;
        public int dependencyID = -1;
        public float speed = 0.0f;
        public Messages.AnimationMessage.AnimationMessageTypes animationType = Messages.AnimationMessage.AnimationMessageTypes.Scale;
        public Vector3 target = Vector3.zero;
        public bool isPlaying = false;
        public bool hasFinished = false;
        public float lastTimePlay = 0.0f;

        public Sprite targetSprite = null;
        public Color targetColor = Color.black;

        public string text;

        /// <summary>Used for scale and movement animations</summary>
        public Animation(int animationID, int dependencyID, RectTransform transform, float speed, Messages.AnimationMessage.AnimationMessageTypes type, Vector3 target) {
            this.animationID = animationID;
            this.dependencyID = dependencyID;
            this.transform = transform;
            this.speed = speed;
            this.animationType = type;
            this.target = target;
        }

        /// <summary>Used for change sprite and color animations</summary>
        public Animation(int animationID, int dependencyID, Messages.AnimationMessage.AnimationMessageTypes type, RectTransform imageComponentTransform, Sprite target, Color color) {
            this.animationID = animationID;
            this.dependencyID = dependencyID;
            this.transform = imageComponentTransform;
            this.speed = 10000000;
            this.animationType = type;
            this.targetSprite = target;
            targetColor = color;
        }
        /// <summary>Used for change sprite animations</summary>
        public Animation(int animationID, int dependencyID, Messages.AnimationMessage.AnimationMessageTypes type, RectTransform imageComponentTransform, Sprite target) {
            this.animationID = animationID;
            this.dependencyID = dependencyID;
            this.transform = imageComponentTransform;
            this.speed = 10000000;
            this.animationType = type;
            this.targetSprite = target;
            this.targetColor = Color.white;
        }

        /// <summary>Used for scroll and pop-up messages animations</summary>
        public Animation(int animationID, int dependencyID, Messages.AnimationMessage.AnimationMessageTypes type, RectTransform imageComponentTransform, string text, Sprite target) {
            this.animationID = animationID;
            this.dependencyID = dependencyID;
            this.speed = 10000000;
            this.transform = imageComponentTransform;
            this.animationType = type;
            this.targetSprite = target;
            this.text = text;
        }

    }
}