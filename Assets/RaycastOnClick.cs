using System.Collections.Generic;
//using DG.Tweening;
using UnityEngine;

public class RaycastOnClick : MonoBehaviour {

    [SerializeField]
    private Camera mainCamera;

    /// <summary>/// Functions to trigger on raycast ///</summary>
    [SerializeField]
    public List<SerializedAction> ActionsToPerform = new List<SerializedAction>();

    private void Update() {
        if (Input.GetMouseButtonDown(0)) {
            //Debug.Log("Click");
            OnClick();
        }
    }
    private void OnClick() {
        RaycastHit2D[] hits = new RaycastHit2D[10];
        ContactFilter2D contactFilter = new ContactFilter2D();
        int numberOfHits = Physics2D.Raycast(mainCamera.ScreenToWorldPoint(Input.mousePosition), Vector3.forward, contactFilter, hits, Mathf.Infinity);
        Debug.DrawRay(mainCamera.ScreenToWorldPoint(Input.mousePosition), Vector3.forward * 10000, Color.yellow, 100);
        Debug.Log("Hits: " + hits.Length);
        for (int i = 0; i < numberOfHits; i++) {
            Debug.Log(hits[i].transform.gameObject.name);
            foreach (SerializedAction action in ActionsToPerform) {
                if (action.triggerObject == hits[i].transform.gameObject) {
                    action.GetAction(this).Invoke();
                }
            }
        }
    }
#region  Functions to trigger
    // public void Bounce(Transform transformToBounce) {
    //     Sequence bounceSequence = DOTween.Sequence();
    //     Vector3 orScale = transformToBounce.localScale * 1;
    //     bounceSequence.Append(transformToBounce.DOScale(new Vector3(orScale.x * 0.8f, orScale.y * 1.2f, orScale.z * 0.8f), 0.1f));
    //     bounceSequence.Append(transformToBounce.DOScale(new Vector3(orScale.x * 1.2f, orScale.y * 0.8f, orScale.z * 0.8f), 0.1f));
    //     bounceSequence.Append(transformToBounce.DOScale(orScale, 0.1f));
    //     bounceSequence.Play();
    // }

    // public void StretchInstanceTransforms(Transform transform, int Xamount, int Yamount, float duration) {
    //     Vector2 amount = new Vector2(Xamount, Yamount);
    //     Neil.SpriteAnimations.StretchAnimation.StretchTransform(transform, amount, duration);
    // }

    // public void TriggerTrackingEvent(int contentID = -1, int tries = 0, bool contentCompleted = false, int score = -1) {
    //     string childID = Neil.Session.GetChildID();
    //     System.DateTime date = System.DateTime.Now;
    //     TrackingEvent trackingEvent = new TrackingEvent(childID, contentID, date, tries, contentCompleted, score);
    //     SerializedTrackingData.GetInstance().AddTrackingEvent(trackingEvent);
    // }

#endregion
}