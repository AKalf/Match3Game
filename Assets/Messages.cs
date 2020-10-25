using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Messages {

    public enum MessageTypes { Wait, Animation, Audio }

    public MessageTypes messageType;
    public int messageID = -1;
    public int dependencyID = -1;

    public Messages(int msgID, int msgDependencyID, MessageTypes type) {
        messageType = type;
    }
#region ANIMATION
    public class AnimationMessage : Messages {
        public enum AnimationMessageTypes { Scale, MoveTo, ChangeSprite, Scroll }

        public AnimationMessageTypes type;
        public int[] indexInHierarchy;
        public float speed = 0;
        public float targetX;
        public float targetY;
        public float targetZ;

        public int spriteIndexInArray = -1;

        public Color targetColor;
        public string text = "";
        /// <summary> Used for scale and movement </summary>
        public AnimationMessage(int msgID, int dependencyID, AnimationMessageTypes type, int[] indexInHierarchy, float speed, float targetX, float targetY, float targetZ) : base(msgID, dependencyID, MessageTypes.Animation) {
            this.indexInHierarchy = indexInHierarchy;
            this.type = type;
            this.speed = speed;
            this.targetX = targetX;
            this.targetY = targetY;
            this.targetZ = targetZ;
        }

        /// <summary> Used to change sprite </summary>
        public AnimationMessage(int msgID, int dependencyID, AnimationMessageTypes type, int[] indexInHierarchy, int targetSpriteIndex) : base(msgID, dependencyID, MessageTypes.Animation) {
            this.type = type;
            this.indexInHierarchy = indexInHierarchy;
            this.type = AnimationMessageTypes.ChangeSprite;
            this.spriteIndexInArray = targetSpriteIndex;
            this.speed = 1000000;
            this.targetColor = Color.white;

        }

        /// <summary> Used to change sprite and color </summary>
        public AnimationMessage(int msgID, int dependencyID, AnimationMessageTypes type, int[] indexInHierarchy, int targetSpriteIndex, Color color) : base(msgID, dependencyID, MessageTypes.Animation) {
            this.type = type;
            this.indexInHierarchy = indexInHierarchy;
            this.type = AnimationMessageTypes.ChangeSprite;
            this.spriteIndexInArray = targetSpriteIndex;

            this.speed = 1000000;
            targetColor = color;
        }
        /// <summary> Used to scroll win history </summary>
        public AnimationMessage(int msgID, int dependencyID, int[] indexInHierarchy, int targetSpriteIndex, string text) : base(msgID, dependencyID, MessageTypes.Animation) {
            this.type = AnimationMessageTypes.Scroll;
            this.indexInHierarchy = indexInHierarchy;
            this.spriteIndexInArray = targetSpriteIndex;
            this.speed = 1000000;
            targetColor = Color.white;
            this.text = text;
        }

    }
#endregion

#region AUDIO
    public class AudioMessage : Messages {
        public AudioClip clip = null; // this could also be retrieved with an index at an array
        public float delay = 0.0f;
        public float pitch = 1;

        public AudioMessage(AudioClip clipToPlay, int messageID, int messageDependencyID = -1, float delay = 0.0f, float pitch = 1) : base(messageID, messageDependencyID, MessageTypes.Audio) {
            clip = clipToPlay;
            this.delay = delay;
            this.pitch = pitch;
        }

    }
#endregion

}