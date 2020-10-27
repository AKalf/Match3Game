using System.Numerics;
public class Messages {

    public enum MessageTypes { Wait, Animation, Audio, ServerStatus }

    public MessageTypes messageType;
    public int messageID = -1;
    public int dependencyID = -1;

    public Messages(int msgID, int msgDependencyID, MessageTypes type) {
        messageType = type;
    }
#region ANIMATION
    public class AnimationMessage : Messages {
        public enum AnimationMessageTypes { Scale, MoveTo, ChangeSprite, Scroll, MoveToHolder, PopUpBox }

        public AnimationMessageTypes type;
        public int[] indexInHierarchy;
        public float speed = 0;

        public float targetX;
        public float targetY;
        public float targetZ;
        public float targetW;

        public int posCollum = -1;
        public int posRow = -1;

        public int spriteIndexInArray = -1;

        public string text = "";
        // <summary> Used for scale and movement </summary>
        public AnimationMessage(int msgID, int dependencyID, AnimationMessageTypes type, int[] indexInHierarchy, float speed, float targetX, float targetY, float targetZ) : base(msgID, dependencyID, MessageTypes.Animation) {
            this.indexInHierarchy = indexInHierarchy;
            this.type = type;
            this.speed = speed;
            this.targetX = targetX;
            this.targetY = targetY;
            this.targetZ = targetZ;
        }
        // <summary> Used to move element on a specific board position  </summary>
        public AnimationMessage(int msgID, int dependencyID, AnimationMessageTypes type, int[] indexInHierarchy, float speed, int posCollum, int posRow) : base(msgID, dependencyID, MessageTypes.Animation) {
            this.indexInHierarchy = indexInHierarchy;
            this.type = type;
            this.speed = speed;
            this.posCollum = posCollum;
            this.posRow = posRow;
        }

        /// <summary> Used to change sprite </summary>
        public AnimationMessage(int msgID, int dependencyID, AnimationMessageTypes type, int[] indexInHierarchy, int targetSpriteIndex) : base(msgID, dependencyID, MessageTypes.Animation) {
            this.type = type;
            this.indexInHierarchy = indexInHierarchy;
            this.type = AnimationMessageTypes.ChangeSprite;
            this.spriteIndexInArray = targetSpriteIndex;
            this.speed = 1000000;
            this.targetX = 1;
            this.targetY = 1;
            this.targetZ = 1;
            this.targetW = 1;

        }

        /// <summary> Used to change sprite and color </summary>
        public AnimationMessage(int msgID, int dependencyID, AnimationMessageTypes type, int[] indexInHierarchy, int targetSpriteIndex, Vector4 color) : base(msgID, dependencyID, MessageTypes.Animation) {
            this.type = type;
            this.indexInHierarchy = indexInHierarchy;
            this.type = AnimationMessageTypes.ChangeSprite;
            this.spriteIndexInArray = targetSpriteIndex;

            this.speed = 1000000;
            targetX = color.X;
            targetY = color.Y;
            targetZ = color.Z;
            targetW = color.W;
        }

        /// <summary> Used to move to holder pos </summary>
        public AnimationMessage(int msgID, int dependencyID, int[] indexInHierarchy, int holderCollum) : base(msgID, dependencyID, MessageTypes.Animation) {
            this.type = AnimationMessageTypes.Scroll;
            this.indexInHierarchy = indexInHierarchy;
            this.speed = 1000000;
            this.posCollum = holderCollum;
        }
        /// <summary> Used to scroll win history </summary>
        public class ScrollWinHistoryMessage : AnimationMessage {
            public ScrollWinHistoryMessage(int msgID, int dependencyID, int[] indexInHierarchy, int targetSpriteIndex, string text) : base(msgID, dependencyID, indexInHierarchy, targetSpriteIndex) {
                this.type = AnimationMessageTypes.Scroll;
                this.indexInHierarchy = indexInHierarchy;
                this.spriteIndexInArray = targetSpriteIndex;
                this.speed = 1000000;

                this.text = text;
            }
        }
    }
    public class PopBoxMessage : AnimationMessage {

        public PopBoxMessage(int msgID, int msgDependencyID, string text, int spriteIndexInArray) : base(msgID, msgDependencyID, new int[0], spriteIndexInArray) {
            this.type = AnimationMessageTypes.PopUpBox;
            this.text = text;
            this.spriteIndexInArray = spriteIndexInArray;
        }
    }
#endregion

#region AUDIO
    public class AudioMessage : Messages {
        public UnityEngine.AudioClip clip = null; // this could also be retrieved with an index at an array
        public float delay = 0.0f;
        public float pitch = 1;

        public AudioMessage(UnityEngine.AudioClip clipToPlay, int messageID, int messageDependencyID = -1, float delay = 0.0f, float pitch = 1) : base(messageID, messageDependencyID, MessageTypes.Audio) {
            clip = clipToPlay;
            this.delay = delay;
            this.pitch = pitch;
        }

    }
#endregion

#region  Utility
    public class ServerStatusMessage : Messages {

        public bool isAvailable = false;
        public ServerStatusMessage(int msgID, int msgDependencyID, bool isAvailable) : base(msgID, msgDependencyID, MessageTypes.ServerStatus) {
            this.isAvailable = isAvailable;
        }
    }

#endregion
}