// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// public class GraphBoardDepricated : MonoBehaviour {
// #region  Depricated
//     // Derpicated

//     /// <summary> this.transform</summary>
//     Transform boardPanel = null;
//     /// <summary> holds score and other UI</summary>
//     Transform detailsPanel = null;
//     /// <summary> Holds cells</summary>
//     Transform gamePanel = null;

//     Transform[] imagesTransforms = null;
//     BoardElement[] elements = null;
//     UnityEngine.UI.Image[] images = null;

//     List<BoardElement> matchedElements = new List<BoardElement>();
//     List<BoardElement> destroyedElements = new List<BoardElement>();
//     public static List<BoardElement> changedElemtns = new List<BoardElement>();

//     ///<summary > /// Holds values for swapping cells/// </summary>
//     BoardElement valueHolderElement;

//     /// <summary> The instance of the manager</summary>
//     public static GraphBoardDepricated inst;

//     /// <summary>Animations currently playing / qued to play. At server-client relationship this will be send to client to be played</summary>
//     private List<AnimationMessage> playingAnimations = new List<AnimationMessage>();

//     /// <summary> Holder for input element1</summary>
//     BoardElement firstElement = null;
//     /// <summary> Holder for input element2</summary>
//     BoardElement secondElement = null;

//     /// <summary> Prevents input overwriting</summary>
//     private bool areCellsSwapping = false;

//     void Awake() {
//         inst = this;
//         boardPanel = this.transform;
//         detailsPanel = boardPanel.GetChild(0);
//         gamePanel = boardPanel.GetChild(1);
//         imagesTransforms = new Transform[gamePanel.childCount];
//         valueHolderElement = this.gameObject.AddComponent<BoardElement>();
//         images = new UnityEngine.UI.Image[imagesTransforms.Length];
//         elements = new BoardElement[imagesTransforms.Length];
// #if UNITY_EDITOR
//         if (detailsPanel.gameObject.name != "DetailsPanel") {
//             Debug.LogError(boardPanel.gameObject.name + ".GetChild(0).name != DetailsPanel");
//         }
//         if (gamePanel.gameObject.name != "GamePanel") {
//             Debug.LogError(boardPanel.gameObject.name + ".GetChild(1).name != GamePanel");
//         }
//         // if (imagesTransforms.Length == 0) {
//         //     Debug.LogError("GamePanel: " + gamePanel.name + " has no child transforms");
//         // }
//         else {
//             for (int i = 0; i < gamePanel.childCount; i++) {
//                 if (gamePanel.GetChild(i).GetComponent<UnityEngine.UI.Image>() == null) {
//                     Debug.LogError(gamePanel.GetChild(i).name + " has no Image component");
//                     gamePanel.GetChild(i).gameObject.AddComponent<UnityEngine.UI.Image>();
//                 }

//             }
//         }

// #endif


//         int rowIndex = 0;
//         int collumIndex = 0;

//         for (int i = 0; i < gamePanel.childCount; i++) {
//             imagesTransforms[i] = gamePanel.GetChild(i);
//             images[i] = imagesTransforms[i].GetComponent<UnityEngine.UI.Image>();
//             elements[i] = imagesTransforms[i].gameObject.AddComponent<BoardElement>();

//             try {
//                 if (GetIfOnTopBoarder(i) == false) {
//                     elements[i].upperNeighbour = elements[i - collumsNumber];
//                     elements[i - collumsNumber].bottomNeighbour = elements[i];
//                 }
//                 if (GetIfOnLeftBoarder(i) == false) {
//                     elements[i].leftNeighbour = elements[i - 1];
//                     elements[i - 1].rightNeighbour = elements[i];
//                 }
//             }
//             catch {
//                 Debug.LogError("There was a problem with element: " + i);
//             }
//         }
//     }

//     void Start() {
//         int index = 0;
//         foreach (Image img in images) {
//             img.color = availColors[index];
//             index++;
//             if (index > availColors.Length - 1) {
//                 index = 0;
//             }
//         }
//     }


//     void Update() {
//         if (playingAnimations.Count > 0) {
//             for (int i = 0; i < playingAnimations.Count; i++) {

//                 if (playingAnimations[i].hasFinished) {
//                     playingAnimations.Remove(playingAnimations[i]);
//                 }
//             }
//             return;
//         }
//         // else if (delay > 0 && timerDelay < delay) {
//         //     timerDelay += Time.deltaTime;
//         //     if (timerDelay >= delay) {
//         //         delay = 0;
//         //         timerDelay = 0;
//         //     }
//         // }
//         else if (playingAnimations.Count < 1 && shouldCheckForMatches) {
//             List<BoardElement> matchedElementsFounded = new List<BoardElement>();

//             // About first input
//             List<BoardElement> firstElementMatches = CheckForMatches(firstElement);


//             if (firstElementMatches.Count > 0) {
//                 foreach (BoardElement element in firstElementMatches) {
//                     if (!matchedElementsFounded.Contains(element)) {
//                         matchedElementsFounded.Add(element);
//                     }
//                 }
//             }
//             AlexDebugger.GetInstance().AddMessage("Total matches found for first element: " + firstElement.name + ", " + firstElementMatches.Count, "Matches");
//             //DebugElementsMatches(firstElementMatches);
//             // About second input
//             List<BoardElement> secondElementMatches = CheckForMatches(secondElement);


//             if (secondElementMatches.Count > 0) {
//                 foreach (BoardElement element in secondElementMatches) {
//                     if (!matchedElementsFounded.Contains(element)) {
//                         matchedElementsFounded.Add(element);
//                     }
//                 }
//             }
//             AlexDebugger.GetInstance().AddMessage("Total matches found for second element: " + secondElement.name + ", " + secondElementMatches.Count, "Matches");
//             //DebugElementsMatches(secondElementMatches);

//             if (matchedElementsFounded.Count > 0) {
//                 AlexDebugger.GetInstance().AddMessage("Total matches found: " + matchedElementsFounded.Count + " for input: " + firstElement.name + " and " + secondElement.name, "Matches");
//                 //DebugElementsMatches(matchedElementsFounded);
//                 matchedElements = matchedElementsFounded;
//             }
//             firstElement = null;
//             secondElement = null;
//             shouldCheckForMatches = false;
//             return;
//         }
//         else if (playingAnimations.Count < 1 && (matchedElements.Count > 0 || isGameMatching)) {
//             PlayMatchEffect(matchedElements);
//             matchedElements = MoveMatchedElementsUpwards(matchedElements);
//             if (matchedElements.Count < 1) {
//                 if (isGameMatching == false) {
//                     ReplaceElements(destroyedElements);
//                     destroyedElements.Clear();
//                     matchedElements.Clear();
//                     BoardManager.changedElemtns.Clear();
//                     isGameMatching = true;
//                     return;
//                 }
//                 else {

//                     matchedElements.Clear();
//                     CheckBoardForMatches();
//                     isGameMatching = false;
//                     return;
//                 }
//             }


//         }
//         else if (playingAnimations.Count < 1) {
//             OnDragEvent.hasDragBegin = false;
//             areCellsSwapping = false;
//         }
//     }

//     /// <summary/ > Depricated, based on graph structure</summary>
//     private void ChangeElementsNeighbours(BoardElement oldElement, BoardElement newElement) {

//         AlexDebugger.GetInstance().AddMessage("###### Swapping elements: " + oldElement.gameObject.name + " with " + newElement.gameObject.name + "##########", "Swap");
//         valueHolderElement.upperNeighbour = newElement.upperNeighbour;
//         valueHolderElement.leftNeighbour = newElement.leftNeighbour;
//         valueHolderElement.rightNeighbour = newElement.rightNeighbour;
//         valueHolderElement.bottomNeighbour = newElement.bottomNeighbour;

//         //Change Top Neighbours
//         newElement.upperNeighbour = oldElement.upperNeighbour;
//         if (oldElement.upperNeighbour == newElement) {
//             newElement.upperNeighbour = oldElement;
//         }
//         else {
//             if (newElement.upperNeighbour != null) {
//                 newElement.upperNeighbour.bottomNeighbour = newElement;
//             }
//         }
//         oldElement.upperNeighbour = valueHolderElement.upperNeighbour;
//         if (valueHolderElement.upperNeighbour == oldElement) {
//             oldElement.upperNeighbour = newElement;
//         }
//         else {
//             if (oldElement.upperNeighbour != null) {
//                 oldElement.upperNeighbour.bottomNeighbour = oldElement;
//             }
//         }

//         // Right
//         newElement.rightNeighbour = oldElement.rightNeighbour;
//         if (oldElement.rightNeighbour == newElement) {
//             newElement.rightNeighbour = oldElement;
//         }
//         else {
//             if (newElement.rightNeighbour != null) {
//                 newElement.rightNeighbour.leftNeighbour = newElement;
//             }
//         }
//         oldElement.rightNeighbour = valueHolderElement.rightNeighbour;
//         if (valueHolderElement.rightNeighbour == oldElement) {
//             oldElement.rightNeighbour = newElement;
//         }
//         else {
//             if (oldElement.rightNeighbour != null) {
//                 oldElement.rightNeighbour.leftNeighbour = oldElement;
//             }
//         }

//         // Left
//         newElement.leftNeighbour = oldElement.leftNeighbour;
//         if (oldElement.leftNeighbour == newElement) {
//             newElement.leftNeighbour = oldElement;
//         }
//         else {
//             if (newElement.leftNeighbour != null) {
//                 newElement.leftNeighbour.rightNeighbour = newElement;
//             }
//         }
//         oldElement.leftNeighbour = valueHolderElement.leftNeighbour;
//         if (valueHolderElement.leftNeighbour == oldElement) {
//             oldElement.leftNeighbour = newElement;
//         }
//         else {
//             if (oldElement.leftNeighbour != null) {
//                 oldElement.leftNeighbour.rightNeighbour = oldElement;
//             }
//         }
//         // Bottom
//         newElement.bottomNeighbour = oldElement.bottomNeighbour;
//         if (oldElement.bottomNeighbour == newElement) {
//             newElement.bottomNeighbour = oldElement;
//         }
//         else {
//             if (newElement.bottomNeighbour != null) {
//                 newElement.bottomNeighbour.upperNeighbour = newElement;
//             }
//         }
//         oldElement.bottomNeighbour = valueHolderElement.bottomNeighbour;
//         if (valueHolderElement.bottomNeighbour == oldElement) {
//             oldElement.bottomNeighbour = newElement;

//         }
//         else {
//             if (oldElement.bottomNeighbour != null) {
//                 oldElement.bottomNeighbour.upperNeighbour = oldElement;
//             }
//         }


//     }


//     private List<BoardElement> CheckForMatches(BoardElement startingElement) {
//         List<BoardElement> elemetns = new List<BoardElement>();
//         KeyValuePair<int, int> startPosition = GetPositionOfElement(startingElement);
//         List<BoardElement> upperMatches = CheckUpperNeighboursForMatches(startingElement);
//         List<BoardElement> bottomMatches = CheckBottomNeighboursForMatches(startingElement);
//         List<BoardElement> leftMatches = CheckLeftNeighboursForMatches(startingElement);
//         List<BoardElement> rightMatches = CheckRightNeighboursForMatches(startingElement);
//         if (upperMatches.Count + bottomMatches.Count >= 2) {
//             string d = "";
//             foreach (BoardElement element in upperMatches) {
//                 if (element == null) {
//                     Debug.LogError("Null element after " + d);
//                 }
//                 else {
//                     d += element.name + ", ";
//                     if (element.upperNeighbour != null) {
//                         Debug.DrawLine(element.transform.position, element.upperNeighbour.transform.position, Color.red, 2.5f);
//                     }
//                     elemetns.Add(element);

//                 }


//                 d = "";
//                 foreach (BoardElement element in bottomMatches) {
//                     if (element == null) {
//                         Debug.LogError("Null element after " + d);
//                     }
//                     else {
//                         d += element.name + ", ";
//                         if (element.bottomNeighbour != null) {
//                             Debug.DrawLine(element.transform.position, element.bottomNeighbour.transform.position, Color.red, 2.5f);
//                         }
//                         elemetns.Add(element);
//                     }
//                 }

//                 if (upperMatches > 0) {
//                     AlexDebugger.GetInstance().AddMessage("Upper matches for element: " + startingElement.name + " are: " + upperMatches, "Matches");
//                 }
//                 if (bottomMatches > 0) {
//                     AlexDebugger.GetInstance().AddMessage("Bottom matches for element: " + startingElement.name + " are: " + bottomMatches, "Matches");
//                 }
//                 if (!elemetns.Contains(startingElement)) {
//                     //startingElement.GetComponent<UnityEngine.UI.Image>().color = startingElement.GetComponent<UnityEngine.UI.Image>().color + new Color(0, 0, 0, -0.5f);
//                     elemetns.Insert(0, startingElement);
//                 }
//             }

//             if (leftMatches.Count + rightMatches.Count >= 2) {


//                 d = "";
//                 foreach (BoardElement element in leftMatches) {
//                     if (element == null) {
//                         Debug.LogError("Null element after " + d);
//                     }
//                     else {
//                         d += element.name + ", ";
//                         if (element.leftNeighbour != null)
//                             Debug.DrawLine(element.transform.position, element.leftNeighbour.transform.position, Color.black, 2.5f);
//                         element.GetComponent<UnityEngine.UI.Image>().color = element.GetComponent<UnityEngine.UI.Image>().color + new Color(0, 0, 0, -0.5f);
//                         elemetns.Add(element);
//                     }
//                 }
//                 if (leftMatches.Count > 0) {
//                     AlexDebugger.GetInstance().AddMessage("Left matches for element: " + startingElement.name + " are: " + leftMatches.Count, "Matches");
//                 }
//                 d = "";
//                 foreach (BoardElement element in rightMatches) {
//                     if (element == null) {
//                         Debug.LogError("Null element after " + d);
//                     }
//                     else {
//                         d += element.name + ", ";
//                         if (element.rightNeighbour != null)
//                             Debug.DrawLine(element.transform.position, element.rightNeighbour.transform.position, Color.black, 2.5f);
//                         element.GetComponent<UnityEngine.UI.Image>().color = element.GetComponent<UnityEngine.UI.Image>().color + new Color(0, 0, 0, -0.5f);
//                         elemetns.Add(element);
//                     }

//                 }
//                 if (rightMatches.Count > 0) {
//                     AlexDebugger.GetInstance().AddMessage("Right matches for element: " + startingElement.name + " are: " + rightMatches.Count, "Matches");
//                 }

//                 if (leftMatches > 0) {
//                     AlexDebugger.GetInstance().AddMessage("Left matches for element: " + startingElement.name + " are: " + leftMatches, "Matches");
//                 }
//                 if (rightMatches > 0) {
//                     AlexDebugger.GetInstance().AddMessage("Bottom matches for element: " + startingElement.name + " are: " + rightMatches, "Matches");
//                 }
//                 if (!elemetns.Contains(startingElement)) {
//                     elemetns.Insert(0, startingElement);
//                     //startingElement.GetComponent<UnityEngine.UI.Image>().color = startingElement.GetComponent<UnityEngine.UI.Image>().color + new Color(0, 0, 0, -0.5f);
//                 }
//             }
//             return elemetns;
//         }
//     }
//     private List<BoardElement> CheckUpperNeighboursForMatches(BoardElement start) {
//         if (start.upperNeighbour != null && start.upperNeighbour.color != Color.white) {
//             if (start.color == start.upperNeighbour.color) {
//                 elemetns.Add(start.upperNeighbour);
//                 foreach (BoardElement elem in CheckUpperNeighboursForMatches(start.upperNeighbour)) {
//                     elemetns.Add(elem);
//                 }
//             }
//         }
//         Debug.Log("Total upperMatches for " + start.name + ", " + elemetns.Count);
//         return elemetns;
//     }


//     private List<BoardElement> CheckBottomNeighboursForMatches(BoardElement start) {
//         if (start.bottomNeighbour != null && start.bottomNeighbour.color != Color.white) {
//             if (start.color == start.bottomNeighbour.color) {
//                 elemetns.Add(start.bottomNeighbour);
//                 foreach (BoardElement elem in CheckBottomNeighboursForMatches(start.bottomNeighbour)) {
//                     elemetns.Add(elem);
//                 }
//             }
//         }
//         Debug.Log("Total bottomMatches for " + start.name + ", " + elemetns.Count);
//         return elemetns;
//     }


//     private List<BoardElement> CheckLeftNeighboursForMatches(BoardElement start) {
//         if (start.leftNeighbour != null && start.leftNeighbour.color != Color.white) {
//             if (start.color == start.leftNeighbour.color) {
//                 elemetns.Add(start.leftNeighbour);
//                 foreach (BoardElement elem in CheckLeftNeighboursForMatches(start.leftNeighbour)) {
//                     elemetns.Add(elem);
//                 }
//             }
//         }
//         Debug.Log("Total leftMatches for " + start.name + ", " + elemetns.Count);
//         return elemetns;
//     }


//     private List<BoardElement> CheckRightNeighboursForMatches(BoardElement start) {
//         if (start.rightNeighbour != null && start.rightNeighbour.color != Color.white) {
//             if (start.color == start.rightNeighbour.color) {
//                 elemetns.Add(start.rightNeighbour);
//                 foreach (BoardElement elem in CheckRightNeighboursForMatches(start.rightNeighbour)) {
//                     elemetns.Add(elem);
//                 }
//             }
//         }
//         Debug.Log("Total rightMatches for " + start.name + ", " + elemetns.Count);
//         return elemetns;
//     }


//     private void PlayMatchEffect(List<BoardElement> elements) {
//         Debug.Log("####### Scale to zero Effects played for " + elements.Count + ", elements...");
//         AlexDebugger.GetInstance().AddMessage("####### Effects played for " + elements.Count + ", elements...", "Effects");
//         for (int elemIndex = 0; elemIndex < elements.Count; elemIndex++) {
//             BoardElement element = elements[elemIndex];
//             Animations.Animation scaleToZero = Animations.AddAnimationScaleToZero(element.transform, swappingSpeed);
//             playingAnimations.Add(scaleToZero);
//             AlexDebugger.GetInstance().AddMessage("Scale to zero played for " + element.gameObject.name, "Effects");
//         }
//     }


//     private List<BoardElement> MoveMatchedElementsUpwards(List<BoardElement> elements) {
//         List<BoardElement> matchedElements = new List<BoardElement>();
//         if (elements.Count > 0) {

//             foreach (BoardElement e in elements) {
//                 matchedElements.Add(e);
//             }
//             // for (int i = 0; i < elements.Count; i++) {
//             BoardElement currentElement = elements[0];
//             BoardElement elementAbove = GetElementAbove(currentElement, elements);

//             AlexDebugger.GetInstance().AddMessage("####### Moving upwards " + currentElement.name + ", elements", "UpawardMovement");
//             currentElement.color = Color.white;
//             //element.GetComponent<UnityEngine.UI.Image>().color = element.color;

//             if (destroyedElements.Contains(currentElement) == false) {
//                 //BoardManager.changedElemtns.Add(currentElement);
//                 destroyedElements.Add(currentElement);
//             }
//             if (elementAbove != null) {
//                 // if (BoardManager.changedElemtns.Contains(currentElement) == false) {
//                 //     BoardManager.changedElemtns.Add(currentElement.upperNeighbour);
//                 // }
//                 SwapCells(currentElement, elementAbove, false, false);
//             }
//             else {
//                 matchedElements.Remove(currentElement);
//             }

//             //}
//         }
//         return matchedElements;
//     }

//     private void ReplaceElements(List<BoardElement> elements) {
//         Debug.Log("####### Replacing " + elements.Count + ", new elements...");
//         foreach (BoardElement element in elements) {
//             element.color = BoardManager.GetAvailableColors()[Random.Range(0, GetAvailableColors().Length)];
//             AlexDebugger.GetInstance().AddMessage("Replacing " + element.name, "Effects");
//             element.GetComponent<UnityEngine.UI.Image>().color = element.color;
//             playingAnimations.Add(Animations.AddAnimationScaleToOne(element.transform, swappingSpeed));

//         }

//     }


//     private void CheckBoardForMatches() {
//         AlexDebugger.GetInstance().AddMessage("####### Checking after-match...", "AfterMatch");
//         Debug.Log("####### Checking after-match...");
//         string d = "New combos: ";
//         foreach (BoardElement elem in GetCopyOfAllElements()) {
//             foreach (BoardElement elem1 in CheckForMatches(elem)) {
//                 if (matchedElements.Contains(elem1) == false) {
//                     d += elem1.name + ",";
//                     matchedElements.Add(elem1);
//                 }
//             }
//         }
//         if (matchedElements.Count > 0) {
//             AlexDebugger.GetInstance().AddMessage(d, "AfterMatch");
//             Debug.Log(d);
//         }
//         else {
//             AlexDebugger.GetInstance().AddMessage("No new matches found", "AfterMatch");
//             Debug.Log("No new matches found");
//         }


//     }

//     private BoardElement[] GetCopyOfAllElements() {
//         BoardElement[] array = new BoardElement[elements.Length];
//         elements.CopyTo(array, 0);
//         return array;
//     }


//     private bool GetIfOnLeftBoarder(int i) {
//         for (int row = 0; row < rowsNumbers; row++) {
//             if (i == row * collumsNumber) {
//                 return true;
//             }
//         }
//         return false;

//     }
//     private bool GetIfOnRightBoarder(int i) {
//         for (int row = 0; row < rowsNumbers - 1; row++) {
//             if (i == row * collumsNumber + 3) {
//                 return true;
//             }
//         }
//         return false;

//     }
//     private bool GetIfOnTopBoarder(int i) {
//         if (i < collumsNumber) {
//             return true;
//         }
//         return false;
//     }
//     private bool GetIfOnBottomBoarder(int i) {
//         if (i > positions.GetLength(1) - 1 - collumsNumber) {
//             return true;
//         }
//         return false;
//     }

//     private BoardElement GetElementAbove(BoardElement start, List<BoardElement> matches) {
//         KeyValuePair<int, int> startPosition = GetPositionOfElement(start);
//         if (startPosition.Value - 1 > -1) {
//             if (matches.Contains(positions[startPosition.Key, startPosition.Value - 1]) || positions[startPosition.Key, startPosition.Value - 1].color == Color.white) {
//                 return GetElementAbove(positions[startPosition.Key, startPosition.Value - 1], matches);
//             }
//             else {
//                 return positions[startPosition.Key, startPosition.Value - 1];
//             }
//         }
//         else {
//             return null;
//         }
//     }

// #endregion
// }