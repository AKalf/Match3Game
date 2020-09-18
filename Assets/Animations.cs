using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Animations : MonoBehaviour {

    private static float movingSpeed = 0.2f;

    public enum AnimationTypes { ScaleToZero, MoveTo, ScaleToOne, ChangeSprite }

    public static Dictionary<Transform, List<Animation>> animatedTransforms = new Dictionary<Transform, List<Animation>>();

    private static Animation currentAnimation = null;
    List<Transform> toRemove = new List<Transform>();

    private double timer = 0.0f;
    private double timerThreshold = 0.01f;

    /// <summary> Holds cells</summary>
    [SerializeField]
    Transform gamePanel = null;

    public static Animations inst = null;

    private static bool shouldPlayAnimations = false;
    /// <summary>
    /// Scale local scale of a transfrom to zero
    /// </summary>
    public static Animation AddAnimationScaleToZero(Transform transfromToScale, float speed) {
        if (animatedTransforms.ContainsKey(transfromToScale) == false) {
            animatedTransforms.Add(transfromToScale, new List<Animation>());
        }
        Animation anim = new Animation(transfromToScale, speed, AnimationTypes.ScaleToZero, Vector3.zero);

        //transfromToScale.GetComponent<BoardElement>().AddAnimationToPlay(anim);
        animatedTransforms[transfromToScale].Add(anim);
        return anim;
    }
    /// <summary>
    /// Scale local scale of a transfrom to one
    /// </summary>
    public static Animation AddAnimationScaleToOne(Transform transfromToScale, float speed) {
        if (animatedTransforms.ContainsKey(transfromToScale) == false) {
            animatedTransforms.Add(transfromToScale, new List<Animation>());
        }
        Animation anim = new Animation(transfromToScale, speed, AnimationTypes.ScaleToOne, Vector3.one);
        animatedTransforms[transfromToScale].Add(anim);
        // transfromToScale.GetComponent<BoardElement>().AddAnimationToPlay(anim);
        return anim;
    }

    public static Animation AddAnimationChangeSprite(Transform imageComponentTranform, Sprite targetSprite, Color targetColor) {
        if (animatedTransforms.ContainsKey(imageComponentTranform) == false) {
            animatedTransforms.Add(imageComponentTranform, new List<Animation>());
        }
        Animation anim = new Animation(imageComponentTranform, targetSprite, targetColor);
        animatedTransforms[imageComponentTranform].Add(anim);
        // transfromToScale.GetComponent<BoardElement>().AddAnimationToPlay(anim);
        return anim;
    }

    private static void ScaleTo(Transform transfromToScale, float speed, Vector3 target) {
        transfromToScale.localScale = Vector3.Lerp(transfromToScale.localScale, target, speed);

    }
    /// <summary>
    /// Move a transfrom to a position
    /// </summary>
    public static Animation MoveToPosition(Transform tranformToMove, float speed, Vector3 targetPosition) {
        if (animatedTransforms.ContainsKey(tranformToMove) == false) {
            animatedTransforms.Add(tranformToMove, new List<Animation>());
        }
        Animation anim = new Animation(tranformToMove, speed, AnimationTypes.MoveTo, targetPosition);
        // tranformToMove.GetComponent<BoardElement>().AddAnimationToPlay(anim);
        animatedTransforms[tranformToMove].Add(anim);
        return anim;
    }

    private static void MoveTo(Transform trans, float speed, Vector3 target) {
        trans.position = Vector3.Lerp(trans.position, target, speed);
    }

    private static void ChangeSprite(Transform boardElementGameObject, Sprite targetSprite, Color targetColor) {
        UnityEngine.UI.Image targetImageComponent = boardElementGameObject.GetChild(0).GetComponent<UnityEngine.UI.Image>();
        targetImageComponent.sprite = targetSprite;
        targetImageComponent.color = targetColor;
    }

    public static void SetAnimationMessages(AnimationMessage[] messages) {
        for (int i = 0; i < messages.Length; i++) {
            switch (messages[i].type) {
                case AnimationTypes.MoveTo:
                    MoveToPosition(inst.gamePanel.GetChild(messages[i].childIndex), movingSpeed, new Vector3(messages[i].targetX, messages[i].targetY, messages[i].targetZ));
                    break;
                case AnimationTypes.ScaleToOne:
                    AddAnimationScaleToOne(inst.gamePanel.GetChild(messages[i].childIndex), movingSpeed);
                    break;
                case AnimationTypes.ScaleToZero:
                    AddAnimationScaleToZero(inst.gamePanel.GetChild(messages[i].childIndex), movingSpeed);
                    break;
                case AnimationTypes.ChangeSprite:
                    AddAnimationChangeSprite(inst.gamePanel.GetChild(messages[i].childIndex), messages[i].targetSprite, messages[i].targetColor);
                    break;
            }
        }
        shouldPlayAnimations = true;
    }

    private void Awake() {
        inst = this;
    }

    private void Update() {

        if (animatedTransforms.Keys.Count > 0 && shouldPlayAnimations) {

            foreach (Transform trans in animatedTransforms.Keys) {
                currentAnimation = animatedTransforms[trans][0];
                if (currentAnimation.hasFinished) {
                    //Debug.Log("----- Animation finished: " + currentAnimation.animationType + " for transform: " + currentAnimation.transform.name);
                    AlexDebugger.GetInstance().AddMessage("Animation finished: " + currentAnimation.animationType + " for transfrom " + currentAnimation.transform.name, AlexDebugger.tags.Animations);
                    if (currentAnimation.speed > 5000) {
                        AlexDebugger.GetInstance().AddMessage("Transfrom " + currentAnimation.transform.name + " moved to holders position", AlexDebugger.tags.Animations);
                    }
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
                foreach (Transform t in toRemove) {
                    animatedTransforms.Remove(t);
                }
                if (animatedTransforms.Keys.Count < 1) {
                    shouldPlayAnimations = false;
                    BoardManager.areAnimationsPlaying = false;
                }
                toRemove.Clear();
            }

        }
    }
    // private Animation CheckIfTransformIsAnimated(Animation animToPlay) {
    //     foreach (Animation anim in animatedTransforms[animToPlay.transform]) {
    //         if (anim.isPlaying && anim != animToPlay) {
    //             return anim;
    //         }
    //     }
    //     return animToPlay;

    // }
    public void PlayAnimation(Animations.Animation animToPlay) {
        switch (animToPlay.animationType) {
            case AnimationTypes.ScaleToZero:
                ScaleTo(animToPlay.transform, animToPlay.speed, animToPlay.target);
                animToPlay.isPlaying = true;
                if (Vector3.Distance(animToPlay.transform.localScale, animToPlay.target) < 0.001f) {
                    animToPlay.transform.localScale = animToPlay.target;
                    animToPlay.isPlaying = false;
                    animToPlay.hasFinished = true;
                }

                break;
            case AnimationTypes.ScaleToOne:
                ScaleTo(animToPlay.transform, animToPlay.speed, animToPlay.target);
                animToPlay.isPlaying = true;
                if (Vector3.Distance(animToPlay.transform.localScale, animToPlay.target) < 0.001f) {
                    animToPlay.transform.localScale = animToPlay.target;
                    animToPlay.isPlaying = false;
                    animToPlay.hasFinished = true;
                }

                break;
            case AnimationTypes.MoveTo:
                MoveTo(animToPlay.transform, animToPlay.speed, animToPlay.target);
                animToPlay.isPlaying = true;
                if (Vector3.Distance(animToPlay.transform.position, animToPlay.target) < 0.05f) {
                    animToPlay.isPlaying = false;
                    animToPlay.hasFinished = true;
                    animToPlay.transform.position = animToPlay.target;
                }
                break;
            case AnimationTypes.ChangeSprite:
                animToPlay.isPlaying = true;
                ChangeSprite(animToPlay.transform, animToPlay.targetSprite, animToPlay.targetColor);
                animToPlay.hasFinished = true;
                animToPlay.isPlaying = false;
                break;
        }
    }
    // Moved to each gameobjerct's Update
    // private void PlayAnimation(Animation animToPlay) {
    //     switch (animToPlay.animationType) {
    //         case AnimationTypes.ScaleToZero:
    //             ScaleTo(animToPlay.transform, animToPlay.speed, animToPlay.target);
    //             animToPlay.isPlaying = true;
    //             if (Vector3.Distance(animToPlay.transform.localScale, animToPlay.target) < 0.05f) {
    //                 animToPlay.transform.localScale = animToPlay.target;
    //                 animToPlay.isPlaying = false;
    //                 animToPlay.hasFinished = true;
    //             }

    //             break;
    //         case AnimationTypes.ScaleToOne:
    //             ScaleTo(animToPlay.transform, animToPlay.speed, animToPlay.target);
    //             animToPlay.isPlaying = true;
    //             if (Vector3.Distance(animToPlay.transform.localScale, animToPlay.target) < 0.05f) {
    //                 animToPlay.transform.localScale = animToPlay.target;
    //                 animToPlay.isPlaying = false;
    //                 animToPlay.hasFinished = true;
    //             }

    //             break;
    //         case AnimationTypes.MoveTo:
    //             MoveTo(animToPlay.transform, animToPlay.speed, animToPlay.target);
    //             animToPlay.isPlaying = true;
    //             if (Vector3.Distance(animToPlay.transform.position, animToPlay.target) < 0.05f) {
    //                 animToPlay.isPlaying = false;
    //                 animToPlay.hasFinished = true;
    //                 animToPlay.transform.position = animToPlay.target;
    //             }
    //             break;
    //     }
    // }

    public interface IAnimationFunctions {
        void PlayAnimation();
    }
    public class Animation {
        public Transform transform = null;
        public float speed = 0.0f;
        public AnimationTypes animationType = AnimationTypes.ScaleToZero;
        public Vector3 target = Vector3.zero;
        public bool isPlaying = false;
        public bool hasFinished = false;
        public double lastTimePlay = 0.0f;

        public Sprite targetSprite = null;
        public Color targetColor = Color.black;
        public Animation(Transform transform, float speed, AnimationTypes type, Vector3 target) {
            this.transform = transform;
            this.speed = speed;
            this.animationType = type;
            this.target = target;
        }

        public Animation(Transform imageComponentTransform, Sprite target, Color color) {
            this.transform = imageComponentTransform;
            this.speed = 10000000;
            this.animationType = AnimationTypes.ChangeSprite;
            this.targetSprite = target;
            targetColor = color;
        }

    }
}

public class AnimationMessage {
    public Animations.AnimationTypes type;
    public int childIndex = -1;
    public float speed = 0;
    public float targetX;
    public float targetY;
    public float targetZ;

    public Sprite targetSprite = null;
    public Color targetColor;

    public AnimationMessage(Animations.AnimationTypes type, int childIndex, float speed, float targetX, float targetY, float targetZ) {
        this.childIndex = childIndex;
        this.type = type;
        this.speed = speed;
        this.targetX = targetX;
        this.targetY = targetY;
        this.targetZ = targetZ;
    }
    /// <summary>
    /// Used with animations like ScaleToZero or ScaleToOne
    /// </summary>
    /// <param name="type"></param>
    /// <param name="childIndex"></param>
    /// <param name="speed"></param>
    public AnimationMessage(Animations.AnimationTypes type, int childIndex, float speed) {
        this.speed = speed;
        this.childIndex = childIndex;
        this.type = type;
    }

    /// <summary>
    /// Used for ChangeSprite AnimationType
    /// </summary>
    /// <param name="childIndex">the transform index in parent</param>
    /// <param name="targetSprite">the sprite to change to</param>
    public AnimationMessage(int childIndex, Sprite targetSprite, Color color) {

        this.childIndex = childIndex;
        this.type = Animations.AnimationTypes.ChangeSprite;
        this.targetSprite = targetSprite;
        this.speed = 1000000;
        targetColor = color;
    }
}