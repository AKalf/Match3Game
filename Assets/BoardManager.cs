using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoardManager : MonoBehaviour {
    [SerializeField]
    private float swappingSpeed = 3.0f;
    [SerializeField]
    int collumsNumber = 4;
    [SerializeField]
    int rowsNumbers = 4;

    /// <summary> Possible values of cells</summary>
    static Color[] availColors = { Color.red, Color.blue, Color.green };

    /// <summary> this.transform</summary>
    Transform boardPanel = null;
    /// <summary> holds score and other UI</summary>
    Transform detailsPanel = null;
    /// <summary> Holds cells</summary>
    Transform gamePanel = null;

    /// <summary> The elements of the board</summary>
    BoardElement[, ] positions = null;
    /// <summary> Flagged elements that create matches</summary>
    bool[, ] matchedElemPositions = null;
    /// <summary> Flagged destroyed elements, that need replacing</summary>
    bool[, ] positionsDestroyed = null;
    /// <summary> Helping transforms to create the drop effect</summary>
    Transform[] holders = null;

    // TO-DO: Create a proper singleton

    /// <summary> The instance of the manager</summary>
    public static BoardManager inst;

    /// <summary>Animations currently playing / qued to play. At server-client relationship this will be send to client to be played</summary>
    private List<AnimationMessage> playingAnimations = new List<AnimationMessage>();

    /// <summary> should be set false by the client</summary>
    public static bool areAnimationsPlaying = false;

    /// <summary> Holder for input element1</summary>
    BoardElement firstElement = null;
    /// <summary> Holder for input element2</summary>
    BoardElement secondElement = null;

    /// <summary> Prevents input overwriting</summary>
    private bool areCellsSwapping = false;

    /// <summary> Step1 in Update, handle input</summary>
    bool shouldCheckIfInputCreatesMatches = false;
    /// <summary> Step2 if there are matches play effects</summary>
    bool shouldPlayEffects = false;
    /// <summary> Step3 re-orient board by moving non-destroyed elements donwards</summary>
    bool areElemetsReorienting = false;
    /// <summary> Step4 move any destroyed elemets to holders position</summary>
    bool shouldMoveElemToHolders = false;
    /// <summary> Step5 re-create elemets and move them to their position</summary>
    bool shouldGenerateElem = false;
    /// <summary> Step6 check for new matches at the board</summary>
    bool shouldCheckBoard = false;


    // float timerDelay = 0.0f;
    // float delay = 0.0f;


    void Awake() {
        inst = this;
        boardPanel = this.transform;
        detailsPanel = boardPanel.GetChild(0);
        gamePanel = boardPanel.GetChild(1);

        positions = new BoardElement[collumsNumber, rowsNumbers];
        matchedElemPositions = new bool[collumsNumber, rowsNumbers];
        positionsDestroyed = new bool[collumsNumber, rowsNumbers];
        holders = new Transform[transform.parent.GetChild(1).childCount];

        for (int i = 0; i < holders.Length; i++) {
            holders[i] = transform.parent.GetChild(1).GetChild(i);
        }
        // Debug
        CheckPanels();

        int rowIndex = 0;
        int collumIndex = 0;

        // Assign elements to the board
        for (int i = 0; i < gamePanel.childCount; i++) {
            if (collumIndex < positions.GetLength(0) && rowIndex < positions.GetLength(1)) {
                positions[collumIndex, rowIndex] = gamePanel.GetChild(i).gameObject.AddComponent<BoardElement>();
                positions[collumIndex, rowIndex].childIndex = i;
                Debug.Log(positions[collumIndex, rowIndex].name + " is at position: c" + collumIndex + ", r" + rowIndex);
                collumIndex++;
                // fix collum index
                if (collumIndex == collumsNumber) {
                    rowIndex++;
                    collumIndex = 0;
                }
            }

        }
    }

    // Start is called before the first frame update
    void Start() {
        // Assign color values to elemets
        int colorIndex = 0;
        for (int row = 0; row < positions.GetLength(1); row++) {
            for (int collum = 0; collum < positions.GetLength(0); collum++) {
                positions[collum, row].color = availColors[colorIndex];
                positions[collum, row].GetComponent<Image>().color = positions[collum, row].color;
                colorIndex++;
                if (colorIndex > availColors.Length - 1) {
                    colorIndex = 0;
                }
            }
        }
    }

    // Update is called once per frame
    void Update() {
        // Do nothing.
        if (areAnimationsPlaying) {
            return;
        }
        // else if (delay > 0 && timerDelay < delay) {
        //     timerDelay += Time.deltaTime;
        //     if (timerDelay >= delay) {
        //         delay = 0;
        //         timerDelay = 0;
        //     }
        // }

        // Step1: check if input create matches and if true move to step2
        else if (shouldCheckIfInputCreatesMatches) {
            Debug.Log("Checking for matches based on input");
            // About first input
            KeyValuePair<int, int> firstPosition = GetPositionOfElement(firstElement);
            int numberOfMatchesForFirst = CheckForMatches(firstPosition.Key, firstPosition.Value);
            // About second input
            KeyValuePair<int, int> secondPosition = GetPositionOfElement(secondElement);
            int numberOfMatchesForSecond = CheckForMatches(secondPosition.Key, secondPosition.Value);


            if (numberOfMatchesForFirst > 0 || numberOfMatchesForSecond > 0) {
                shouldPlayEffects = true;
                AlexDebugger.GetInstance().AddMessage("Total matches found: for input: " + firstElement.name + " and " + secondElement.name + " are: " + numberOfMatchesForFirst + numberOfMatchesForSecond, "Matches");
            }
            //Animations.SetAnimationMessages(playingAnimations.ToArray());
            //playingAnimations.Clear();
            firstElement = null;
            secondElement = null;
            shouldCheckIfInputCreatesMatches = false;
            return;
        }
        // Step2: play effects for matched elements and move to step3
        else if (shouldPlayEffects) {
            Debug.Log("Playing effects for matches");
            for (int row = 0; row < positions.GetLength(0); row++) {
                for (int collum = 0; collum < positions.GetLength(1); collum++) {
                    if (matchedElemPositions[collum, row] == true) {
                        PlayMatchEffect(collum, row);
                    }
                }
            }
            if (playingAnimations.Count > 0) {
                Animations.SetAnimationMessages(playingAnimations.ToArray());
                playingAnimations.Clear();
                areAnimationsPlaying = true;
            }
            shouldPlayEffects = false;
            areElemetsReorienting = true;
            return;
        }
        // Step3: repeat until non-destroyed elements have droped to lower position and then move to step4
        else if (areElemetsReorienting) {
            Debug.Log("Re-orienting board");
            bool hasChanged = false;
            for (int row = rowsNumbers - 1; row > -1; row--) {
                for (int collum = 0; collum < collumsNumber; collum++) {
                    if (matchedElemPositions[collum, row] == true) {
                        MoveMatchedElementUpwards(collum, row);
                        hasChanged = true;
                    }
                }
                // placed here so the entire row's animation play simutaniously
                if (hasChanged) {
                    //Animations.SetAnimationMessages(playingAnimations.ToArray());
                    //playingAnimations.Clear();
                    // Wait for new animtions to play
                    return;
                }
            }
            if (!hasChanged) {
                Debug.Log("Re-orienting finished");
                areElemetsReorienting = false;
                shouldGenerateElem = true;
            }
            //Animations.SetAnimationMessages(playingAnimations.ToArray());
            //playingAnimations.Clear();
            return;
        }
        // Step4: replace destroyed elements by assigning new color,  move to step5
        else if (shouldGenerateElem) {
            Debug.Log("Generating new elements");
            for (int row = 0; row < positions.GetLength(0); row++) {
                for (int collum = 0; collum < positions.GetLength(1); collum++) {
                    if (positionsDestroyed[collum, row] == true) {
                        ReplaceElement(collum, row);
                        positionsDestroyed[collum, row] = false;
                    }
                }
            }
            if (playingAnimations.Count > 0) {
                Animations.SetAnimationMessages(playingAnimations.ToArray());
                playingAnimations.Clear();
                areAnimationsPlaying = true;
            }
            shouldGenerateElem = false;
            shouldCheckBoard = true;
            return;
        }
        // Step5: check for new matches
        else if (shouldCheckBoard) {
            Debug.Log("checking board for combos");
            int newTotalMatches = CheckBoardForMatches();
            // Go back to step2
            if (newTotalMatches > 0) {
                AlexDebugger.GetInstance().AddMessage("Total new matches found: " + newTotalMatches, "AfterMatch");
                shouldPlayEffects = true;
            }
            // Check for possible inputs
            else {
                for (int row = 0; row < positions.GetLength(0); row++) {
                    for (int collum = 0; collum < positions.GetLength(1); collum++) {
                        if (isPotentialInput(collum, row) > 0) {
                            positions[collum, row].transform.GetChild(0).gameObject.SetActive(true);
                        }
                        else {
                            positions[collum, row].transform.GetChild(0).gameObject.SetActive(false);
                        }
                    }
                }

                AlexDebugger.GetInstance().AddMessage("No new matches found", "AfterMatch");
                Debug.Log("No new matches found");

            }
            if (playingAnimations.Count > 0) {
                Animations.SetAnimationMessages(playingAnimations.ToArray());
                playingAnimations.Clear();
                areAnimationsPlaying = true;
            }
            shouldCheckBoard = false;
            return;
        }
        // New input can be processed
        else {
            OnDragEvent.hasDragBegin = false;
            areCellsSwapping = false;
        }
    }


    public static BoardManager GetInstance() {
        return inst;
    }

    /// <summary>Returns the position of the element on the board</summary>
    private KeyValuePair<int, int> GetPositionOfElement(BoardElement element) {
        for (int row = 0; row < positions.GetLength(1); row++) {
            for (int collumn = 0; collumn < positions.GetLength(0); collumn++) {
                if (positions[collumn, row] == element) {
                    return new KeyValuePair<int, int>(collumn, row);
                }
            }
        }
        Debug.LogError("Could not find element: " + element.gameObject.name + " on board!");
        return new KeyValuePair<int, int>();
    }

    /// <summary>Sets swapping animations and changes positions on the board if it is not gonna rewire </summary>
    public void SwapElements(BoardElement firstElement, BoardElement secondElement, bool rewire = false, bool shouldCheckMatches = true) {

        //Animations.Animation firstElementMoveAnim = Animations.MoveToPosition(firstElement.transform, swappingSpeed, secondElement.transform.position);
        Vector3 targetPos = secondElement.transform.position;
        playingAnimations.Add(new AnimationMessage(Animations.AnimationTypes.MoveTo, firstElement.childIndex, swappingSpeed, targetPos.x, targetPos.y, targetPos.z));

        targetPos = firstElement.transform.position;
        //Animations.Animation secondElementMoveAnim = Animations.MoveToPosition(secondElement.transform, swappingSpeed, firstElement.transform.position);
        playingAnimations.Add(new AnimationMessage(Animations.AnimationTypes.MoveTo, secondElement.childIndex, swappingSpeed, targetPos.x, targetPos.y, targetPos.z));

        if (!rewire) {

            this.firstElement = firstElement;
            this.secondElement = secondElement;
            SwapBoardElementNeighbours(this.firstElement, this.secondElement);
            this.shouldCheckIfInputCreatesMatches = shouldCheckMatches;
        }
        else {
            targetPos = firstElement.transform.position;
            playingAnimations.Add(new AnimationMessage(Animations.AnimationTypes.MoveTo, firstElement.childIndex, swappingSpeed, targetPos.x, targetPos.y, targetPos.z));
            targetPos = secondElement.transform.position;
            playingAnimations.Add(new AnimationMessage(Animations.AnimationTypes.MoveTo, secondElement.childIndex, swappingSpeed, targetPos.x, targetPos.y, targetPos.z));
            this.firstElement = null;
            this.secondElement = null;

        }
        if (playingAnimations.Count > 0) {
            Animations.SetAnimationMessages(playingAnimations.ToArray());
            playingAnimations.Clear();
            areAnimationsPlaying = true;
        }
        areCellsSwapping = true;
    }

    /// <summary>Swaps to elements positions on the board </summary>
    private void SwapBoardElementNeighbours(BoardElement oldElement, BoardElement newElement) {
        GameObject oldGameObject = oldElement.gameObject;
        GameObject newGameObject = newElement.gameObject;
        KeyValuePair<int, int> oldElementPosition = GetPositionOfElement(oldElement);
        KeyValuePair<int, int> newElementPosition = GetPositionOfElement(newElement);
        positions[oldElementPosition.Key, oldElementPosition.Value] = newGameObject.GetComponent<BoardElement>();
        positions[newElementPosition.Key, newElementPosition.Value] = oldGameObject.GetComponent<BoardElement>();
#if UNITY_EDITOR
        Debug.Log(newElementPosition.Key + ", " + newElementPosition.Value + " set to position " + oldElementPosition.Key + ", " + oldElementPosition.Value);
        Debug.Log(oldElementPosition.Key + ", " + oldElementPosition.Value + " set to position " + newElementPosition.Key + ", " + newElementPosition.Value);
        if (positions[oldElementPosition.Key, oldElementPosition.Value] == positions[newElementPosition.Key, newElementPosition.Value]) {
            Debug.LogError("Swap was not correct");
        }
#endif
    }

    /// <summary> Flags positions that belong to a match </summary>
    /// <param name="shouldFlag"> if false, elements wont be marked as matched (usefull when want to check possible matches)</param>
    /// <returns>returns the number of matches found</returns>
    private int CheckForMatches(int startCollum, int startRow, bool shouldFlag = true) {

        int upperMatches = CheckUpperNeighboursForMatches(startCollum, startRow);
        int bottomMatches = CheckBottomNeighboursForMatches(startCollum, startRow);
        int leftMatches = CheckLeftNeighboursForMatches(startCollum, startRow);
        int rightMatches = CheckRightNeighboursForMatches(startCollum, startRow);

        if (shouldFlag) {
            // if more than 2 element matches, add vertical mathced elements
            if (upperMatches + bottomMatches >= 2) {
                matchedElemPositions[startCollum, startRow] = true;
                for (int row = startRow - upperMatches; row <= startRow + bottomMatches; row++) {
                    matchedElemPositions[startCollum, row] = true;
                    AlexDebugger.GetInstance().AddMessage(positions[startCollum, row].name + "is an " + ((row < startRow) ? " on top" : "on bottom") + " match of " + positions[startCollum, row].name, "Matches");
                }
                if (upperMatches > 0) {
                    AlexDebugger.GetInstance().AddMessage("Upper matches for element: " + positions[startCollum, startRow].name + " are: " + upperMatches, "Matches");
                }
                if (bottomMatches > 0) {
                    AlexDebugger.GetInstance().AddMessage("Bottom matches for element: " + positions[startCollum, startRow].name + " are: " + bottomMatches, "Matches");
                }
            }
            // if more than 2 element matches, add horizontal mathced elements
            if (leftMatches + rightMatches >= 2) {
                matchedElemPositions[startCollum, startRow] = true;
                for (int collum = startCollum - leftMatches; collum <= startCollum + rightMatches; collum++) {
                    matchedElemPositions[collum, startRow] = true;
                }
                if (leftMatches > 0) {
                    AlexDebugger.GetInstance().AddMessage("Left matches for element: " + positions[startCollum, startRow].name + " are: " + leftMatches, "Matches");
                }
                if (rightMatches > 0) {
                    AlexDebugger.GetInstance().AddMessage("Bottom matches for element: " + positions[startCollum, startRow].name + " are: " + rightMatches, "Matches");
                }
            }
        }
        if (upperMatches + bottomMatches >= 2 || leftMatches + rightMatches >= 2) {
            AlexDebugger.GetInstance().AddMessage("Total matches found for: " + positions[startCollum, startRow].name + " are: " + upperMatches + bottomMatches + leftMatches + rightMatches, "Matches");
            return upperMatches + bottomMatches + leftMatches + rightMatches;
        }
        else {
            return 0;
        }
    }

    /// <summary> Returns the number of elements matched towards top </summary>
    private int CheckUpperNeighboursForMatches(int collum, int row) {
        int numberOfElem = 0;
        if (row - 1 > -1) {
            if (positions[collum, row].color == positions[collum, row - 1].color) {
                numberOfElem++;
                numberOfElem += CheckUpperNeighboursForMatches(collum, row - 1);
            }
        }
        return numberOfElem;
    }
    /// <summary> Returns the number of elements matched towards bottom </summary>
    private int CheckBottomNeighboursForMatches(int collum, int row) {
        int numberOfElem = 0;
        if (row + 1 < rowsNumbers) {
            if (positions[collum, row].color == positions[collum, row + 1].color) {
                numberOfElem++;
                numberOfElem += CheckBottomNeighboursForMatches(collum, row + 1);

            }
        }
        return numberOfElem;

    }
    /// <summary> Returns the number of elements matched towards left </summary>
    private int CheckLeftNeighboursForMatches(int collum, int row) {
        int numberOfElem = 0;
        if (collum - 1 > -1) {
            if (positions[collum, row].color == positions[collum - 1, row].color) {
                numberOfElem++;
                numberOfElem += CheckLeftNeighboursForMatches(collum - 1, row);

            }

        }
        return numberOfElem;
    }
    /// <summary> Returns the number of elements matched towards right </summary>
    private int CheckRightNeighboursForMatches(int collum, int row) {
        int numberOfElem = 0;
        if (collum + 1 < collumsNumber) {
            if (positions[collum, row].color == positions[collum + 1, row].color) {
                numberOfElem++;
                numberOfElem += CheckRightNeighboursForMatches(collum + 1, row);

            }
        }
        return numberOfElem;
    }

    /// <summary> Used recursivly to move non-destroyed elements dowards</summary>
    private void MoveMatchedElementUpwards(int collumn, int row) {
        AlexDebugger.GetInstance().AddMessage("####### Moving upwards " + positions[collumn, row].name + ", elements", "UpawardMovement");
        // swap with next non matched element on the collum if not on top of collum
        if (row != 0) {
            for (int i = row - 1; i > -1; i--) {
                // if non-matched element found, swap
                if (matchedElemPositions[collumn, i] == false) {
                    SwapElements(positions[collumn, row], positions[collumn, i], false, false);
                    // make changes to the flag array
                    matchedElemPositions[collumn, row] = false;
                    matchedElemPositions[collumn, i] = true;
                    return;
                }
            }
        }
        // if element reached top marked it as destroyed and non-matched
        positionsDestroyed[collumn, row] = true;
        matchedElemPositions[collumn, row] = false;
    }

    /// <summary>Checks each element on the board for matches and flags them</summary>
    private int CheckBoardForMatches() {
        AlexDebugger.GetInstance().AddMessage("####### Checking after-match...", "AfterMatch");
        Debug.Log("####### Checking after-match...");
        int totalMatches = 0;
        // Search for matches and flag them
        for (int row = 0; row < positions.GetLength(0); row++) {
            for (int collum = 0; collum < positions.GetLength(1); collum++) {
                totalMatches += CheckForMatches(collum, row);
            }
        }
        return totalMatches;

    }

    /// <summary>Re-creates destroyed elements, by moving them to holders position and then making a drop effect</summary>
    private void ReplaceElement(int collum, int row) {
        positions[collum, row].color = GetAvailableColors()[Random.Range(0, GetAvailableColors().Length)];
        AlexDebugger.GetInstance().AddMessage("Replacing " + positions[collum, row], "Effects");
        positions[collum, row].GetComponent<UnityEngine.UI.Image>().color = positions[collum, row].color;

        Vector3 originalPos = positions[collum, row].transform.position;
        positions[collum, row].transform.position = holders[collum].position;
        positions[collum, row].transform.localScale = Vector3.one;
        playingAnimations.Add(new AnimationMessage(Animations.AnimationTypes.MoveTo, positions[collum, row].childIndex, 200, holders[collum].position.x, holders[collum].position.y, holders[collum].position.z));
        playingAnimations.Add(new AnimationMessage(Animations.AnimationTypes.ScaleToOne, positions[collum, row].childIndex, 200));
        playingAnimations.Add(new AnimationMessage(Animations.AnimationTypes.MoveTo, positions[collum, row].childIndex, swappingSpeed, originalPos.x, originalPos.y, originalPos.z));
    }

    /// <summary>Play scale-to-zero effect on element's transform</summary>
    private void PlayMatchEffect(int collum, int row) {
        AlexDebugger.GetInstance().AddMessage("####### Scale to zero effect for " + positions[collum, row].name, "Effects");
        //Animations.Animation scaleToZero = Animations.AddAnimationScaleToZero(positions[collum, row].transform, swappingSpeed);
        playingAnimations.Add(new AnimationMessage(Animations.AnimationTypes.ScaleToZero, positions[collum, row].childIndex, swappingSpeed));

    }

    /// <summary>Check if with input it can create matches. Returns the amount matches found (for first correct input discovered) </summary>
    private int isPotentialInput(int collum, int row) {
        if (row - 1 > -1) {
            SwapBoardElementNeighbours(positions[collum, row], positions[collum, row - 1]);
            int matches = CheckForMatches(collum, row - 1, false);
            if (matches > 0) {
                SwapBoardElementNeighbours(positions[collum, row], positions[collum, row - 1]);
                return matches;
            }
            SwapBoardElementNeighbours(positions[collum, row], positions[collum, row - 1]);
        }
        if (row + 1 < rowsNumbers) {
            SwapBoardElementNeighbours(positions[collum, row], positions[collum, row + 1]);
            int matches = CheckForMatches(collum, row + 1, false);
            if (matches > 0) {
                SwapBoardElementNeighbours(positions[collum, row], positions[collum, row + 1]);
                return matches;
            }
            SwapBoardElementNeighbours(positions[collum, row], positions[collum, row + 1]);
        }
        if (collum - 1 > -1) {
            SwapBoardElementNeighbours(positions[collum, row], positions[collum - 1, row]);
            int matches = CheckForMatches(collum - 1, row, false);
            if (matches > 0) {
                SwapBoardElementNeighbours(positions[collum, row], positions[collum - 1, row]);
                return matches;
            }
            SwapBoardElementNeighbours(positions[collum, row], positions[collum - 1, row]);
        }
        if (collum + 1 < collumsNumber) {
            SwapBoardElementNeighbours(positions[collum, row], positions[collum + 1, row]);
            int matches = CheckForMatches(collum + 1, row, false);
            if (matches > 0) {
                SwapBoardElementNeighbours(positions[collum, row], positions[collum + 1, row]);
                return matches;
            }
            SwapBoardElementNeighbours(positions[collum, row], positions[collum + 1, row]);
        }
        return 0;
    }

    /// <summary>Is boardManager available</summary>
    public bool IsAvailable() {
        if (areCellsSwapping /*|| playingAnimations.Count > 0*/ ) {
            return false;
        }
        else {
            return true;
        }
    }

    public bool GetIfNeighbours(BoardElement elem1, BoardElement elem2) {
        KeyValuePair<int, int> elem1Pos = GetPositionOfElement(elem1);
        KeyValuePair<int, int> elem2Pos = GetPositionOfElement(elem2);
        // Case same collumn, one row         above                                 or below
        if (elem1Pos.Key == elem2Pos.Key && (elem2Pos.Value == elem1Pos.Value - 1 || elem2Pos.Value == elem1Pos.Value + 1)) {
            return true;
        }
        // Case same row, one collumn                left                                 or right
        else if (elem1Pos.Value == elem2Pos.Value && (elem2Pos.Key == elem1Pos.Key - 1 || elem2Pos.Key == elem1Pos.Key + 1)) {
            return true;
        }
        else {
            return false;
        }
    }

    public static Color[] GetAvailableColors() {
        return availColors;
    }


    /// Debug

    private void DebugElementsMatches(List<BoardElement> elements) {
        if (elements.Count > 0) {
            string d = "";
            foreach (BoardElement e in elements) {
                d += e.gameObject.name + ", ";
            }
            AlexDebugger.GetInstance().AddMessage(d, "Matches");
            Debug.Log(d + ", Matches");
        }
    }
    private void CheckPanels() {
#if UNITY_EDITOR
        if (detailsPanel.gameObject.name != "DetailsPanel") {
            Debug.LogError(boardPanel.gameObject.name + ".GetChild(0).name != DetailsPanel");
        }
        if (gamePanel.gameObject.name != "GamePanel") {
            Debug.LogError(boardPanel.gameObject.name + ".GetChild(1).name != GamePanel");
        }
        else {
            for (int i = 0; i < gamePanel.childCount; i++) {
                if (gamePanel.GetChild(i).GetComponent<UnityEngine.UI.Image>() == null) {
                    Debug.LogError(gamePanel.GetChild(i).name + " has no Image component");
                    gamePanel.GetChild(i).gameObject.AddComponent<UnityEngine.UI.Image>();
                }

            }
        }

#endif
    }

#region  Depricated
    // Derpicated

    //Transform[] imagesTransforms = null;
    //BoardElement[] elements = null;
    //UnityEngine.UI.Image[] images = null;

    // List<BoardElement> matchedElements = new List<BoardElement>();
    // List<BoardElement> destroyedElements = new List<BoardElement>();
    // public static List<BoardElement> changedElemtns = new List<BoardElement>();
    /// <summary>/// Holds values for swapping cells/// </summary>
    //BoardElement valueHolderElement;

    ///
    /// Old
    // voidAwake() {
    //    inst = this;
    //     boardPanel = this.transform;
    //     detailsPanel = boardPanel.GetChild(0);
    //     gamePanel = boardPanel.GetChild(1);
    //imagesTransforms = new Transform[gamePanel.childCount];
    // valueHolderElement = this.gameObject.AddComponent<BoardElement>();
    // images = new UnityEngine.UI.Image[imagesTransforms.Length];
    // elements = new BoardElement[imagesTransforms.Length];
    // #if UNITY_EDITOR
    //     if (detailsPanel.gameObject.name != "DetailsPanel") {
    //         Debug.LogError(boardPanel.gameObject.name + ".GetChild(0).name != DetailsPanel");
    //     }
    //     if (gamePanel.gameObject.name != "GamePanel") {
    //         Debug.LogError(boardPanel.gameObject.name + ".GetChild(1).name != GamePanel");
    //     }
    //     // if (imagesTransforms.Length == 0) {
    //     //     Debug.LogError("GamePanel: " + gamePanel.name + " has no child transforms");
    //     // }
    //     else {
    //         for (int i = 0; i < gamePanel.childCount; i++) {
    //             if (gamePanel.GetChild(i).GetComponent<UnityEngine.UI.Image>() == null) {
    //                 Debug.LogError(gamePanel.GetChild(i).name + " has no Image component");
    //                 gamePanel.GetChild(i).gameObject.AddComponent<UnityEngine.UI.Image>();
    //             }

    //         }
    //     }

    // #endif


    //int rowIndex = 0;
    //int collumIndex = 0;

    //for (int i = 0; i < gamePanel.childCount; i++) {
    // imagesTransforms[i] = gamePanel.GetChild(i);
    // images[i] = imagesTransforms[i].GetComponent<UnityEngine.UI.Image>();
    // elements[i] = imagesTransforms[i].gameObject.AddComponent<BoardElement>();

    // try {
    //     if (GetIfOnTopBoarder(i) == false) {
    //         elements[i].upperNeighbour = elements[i - collumsNumber];
    //         elements[i - collumsNumber].bottomNeighbour = elements[i];
    //     }
    //     if (GetIfOnLeftBoarder(i) == false) {
    //         elements[i].leftNeighbour = elements[i - 1];
    //         elements[i - 1].rightNeighbour = elements[i];
    //     }
    // }
    // catch {
    //     Debug.LogError("There was a problem with element: " + i);
    // }
    //}
    //}

    ///
    ///  Old
    //  void Start() {
    // int index = 0;
    // foreach (Image img in images) {
    //     img.color = availColors[index];
    //     index++;
    //     if (index > availColors.Length - 1) {
    //         index = 0;
    //     }
    // }
    //}


    /// Old
    // void Update() {
    //     if (playingAnimations.Count > 0) {
    //         for (int i = 0; i < playingAnimations.Count; i++) {

    //             if (playingAnimations[i].hasFinished) {
    //                 playingAnimations.Remove(playingAnimations[i]);
    //             }
    //         }
    //         return;
    //     }
    //     // else if (delay > 0 && timerDelay < delay) {
    //     //     timerDelay += Time.deltaTime;
    //     //     if (timerDelay >= delay) {
    //     //         delay = 0;
    //     //         timerDelay = 0;
    //     //     }
    //     // }
    //     else if (playingAnimations.Count < 1 && shouldCheckForMatches) {
    //         List<BoardElement> matchedElementsFounded = new List<BoardElement>();

    //         // About first input
    //         List<BoardElement> firstElementMatches = CheckForMatches(firstElement);


    //         if (firstElementMatches.Count > 0) {
    //             foreach (BoardElement element in firstElementMatches) {
    //                 if (!matchedElementsFounded.Contains(element)) {
    //                     matchedElementsFounded.Add(element);
    //                 }
    //             }
    //         }
    //         AlexDebugger.GetInstance().AddMessage("Total matches found for first element: " + firstElement.name + ", " + firstElementMatches.Count, "Matches");
    //         DebugElementsMatches(firstElementMatches);
    //         // About second input
    //         List<BoardElement> secondElementMatches = CheckForMatches(secondElement);


    //         if (secondElementMatches.Count > 0) {
    //             foreach (BoardElement element in secondElementMatches) {
    //                 if (!matchedElementsFounded.Contains(element)) {
    //                     matchedElementsFounded.Add(element);
    //                 }
    //             }
    //         }
    //         AlexDebugger.GetInstance().AddMessage("Total matches found for second element: " + secondElement.name + ", " + secondElementMatches.Count, "Matches");
    //         DebugElementsMatches(secondElementMatches);

    //         if (matchedElementsFounded.Count > 0) {
    //             AlexDebugger.GetInstance().AddMessage("Total matches found: " + matchedElementsFounded.Count + " for input: " + firstElement.name + " and " + secondElement.name, "Matches");
    //             DebugElementsMatches(matchedElementsFounded);
    //             matchedElements = matchedElementsFounded;
    //         }
    //         firstElement = null;
    //         secondElement = null;
    //         shouldCheckForMatches = false;
    //         return;
    //     }
    //     else if (playingAnimations.Count < 1 && (matchedElements.Count > 0 || isGameMatching)) {
    //         PlayMatchEffect(matchedElements);
    //         matchedElements = MoveMatchedElementsUpwards(matchedElements);
    //         if (matchedElements.Count < 1) {
    //             if (isGameMatching == false) {
    //                 ReplaceElements(destroyedElements);
    //                 destroyedElements.Clear();
    //                 matchedElements.Clear();
    //                 BoardManager.changedElemtns.Clear();
    //                 isGameMatching = true;
    //                 return;
    //             }
    //             else {

    //                 matchedElements.Clear();
    //                 CheckBoardForMatches();
    //                 isGameMatching = false;
    //                 return;
    //             }
    //         }


    //     }
    //     else if (playingAnimations.Count < 1) {
    //         OnDragEvent.hasDragBegin = false;
    //         areCellsSwapping = false;
    //     }
    // }

    /// <summary> Depricated, based on graph structure </summary>
    // private void ChangeElementsNeighbours(BoardElement oldElement, BoardElement newElement) {

    //     AlexDebugger.GetInstance().AddMessage("###### Swapping elements: " + oldElement.gameObject.name + " with " + newElement.gameObject.name + "##########", "Swap");
    //     valueHolderElement.upperNeighbour = newElement.upperNeighbour;
    //     valueHolderElement.leftNeighbour = newElement.leftNeighbour;
    //     valueHolderElement.rightNeighbour = newElement.rightNeighbour;
    //     valueHolderElement.bottomNeighbour = newElement.bottomNeighbour;

    //     //Change Top Neighbours
    //     newElement.upperNeighbour = oldElement.upperNeighbour;
    //     if (oldElement.upperNeighbour == newElement) {
    //         newElement.upperNeighbour = oldElement;
    //     }
    //     else {
    //         if (newElement.upperNeighbour != null) {
    //             newElement.upperNeighbour.bottomNeighbour = newElement;
    //         }
    //     }
    //     oldElement.upperNeighbour = valueHolderElement.upperNeighbour;
    //     if (valueHolderElement.upperNeighbour == oldElement) {
    //         oldElement.upperNeighbour = newElement;
    //     }
    //     else {
    //         if (oldElement.upperNeighbour != null) {
    //             oldElement.upperNeighbour.bottomNeighbour = oldElement;
    //         }
    //     }

    //     // Right
    //     newElement.rightNeighbour = oldElement.rightNeighbour;
    //     if (oldElement.rightNeighbour == newElement) {
    //         newElement.rightNeighbour = oldElement;
    //     }
    //     else {
    //         if (newElement.rightNeighbour != null) {
    //             newElement.rightNeighbour.leftNeighbour = newElement;
    //         }
    //     }
    //     oldElement.rightNeighbour = valueHolderElement.rightNeighbour;
    //     if (valueHolderElement.rightNeighbour == oldElement) {
    //         oldElement.rightNeighbour = newElement;
    //     }
    //     else {
    //         if (oldElement.rightNeighbour != null) {
    //             oldElement.rightNeighbour.leftNeighbour = oldElement;
    //         }
    //     }

    //     // Left
    //     newElement.leftNeighbour = oldElement.leftNeighbour;
    //     if (oldElement.leftNeighbour == newElement) {
    //         newElement.leftNeighbour = oldElement;
    //     }
    //     else {
    //         if (newElement.leftNeighbour != null) {
    //             newElement.leftNeighbour.rightNeighbour = newElement;
    //         }
    //     }
    //     oldElement.leftNeighbour = valueHolderElement.leftNeighbour;
    //     if (valueHolderElement.leftNeighbour == oldElement) {
    //         oldElement.leftNeighbour = newElement;
    //     }
    //     else {
    //         if (oldElement.leftNeighbour != null) {
    //             oldElement.leftNeighbour.rightNeighbour = oldElement;
    //         }
    //     }
    //     // Bottom
    //     newElement.bottomNeighbour = oldElement.bottomNeighbour;
    //     if (oldElement.bottomNeighbour == newElement) {
    //         newElement.bottomNeighbour = oldElement;
    //     }
    //     else {
    //         if (newElement.bottomNeighbour != null) {
    //             newElement.bottomNeighbour.upperNeighbour = newElement;
    //         }
    //     }
    //     oldElement.bottomNeighbour = valueHolderElement.bottomNeighbour;
    //     if (valueHolderElement.bottomNeighbour == oldElement) {
    //         oldElement.bottomNeighbour = newElement;

    //     }
    //     else {
    //         if (oldElement.bottomNeighbour != null) {
    //             oldElement.bottomNeighbour.upperNeighbour = oldElement;
    //         }
    //     }


    // }

    /// <summary> Depricated, based on graph system</summary>
    /// <returns>Returns a list will all elements that create matches</returns>
    //private List<BoardElement> CheckForMatches(BoardElement startingElement) {
    //     List<BoardElement> elemetns = new List<BoardElement>();
    //     KeyValuePair<int, int> startPosition = GetPositionOfElement(startingElement);
    //     List<BoardElement> upperMatches = CheckUpperNeighboursForMatches(startingElement);
    //     List<BoardElement> bottomMatches = CheckBottomNeighboursForMatches(startingElement);
    //     List<BoardElement> leftMatches = CheckLeftNeighboursForMatches(startingElement);
    //     List<BoardElement> rightMatches = CheckRightNeighboursForMatches(startingElement);
    //if (upperMatches.Count + bottomMatches.Count >= 2) {
    //string d = "";
    //foreach (BoardElement element in upperMatches) {
    //  if (element == null) {
    //     Debug.LogError("Null element after " + d);
    //  }
    //  else {
    //     d += element.name + ", ";
    //     if (element.upperNeighbour != null) {
    //          Debug.DrawLine(element.transform.position, element.upperNeighbour.transform.position, Color.red, 2.5f);
    //     }
    //  elemetns.Add(element);
    //
    //}


    //d = "";
    //foreach (BoardElement element in bottomMatches) {
    // if (element == null) {
    //     Debug.LogError("Null element after " + d);
    // }
    // else {
    //     d += element.name + ", ";
    //     if (element.bottomNeighbour != null) {
    //         Debug.DrawLine(element.transform.position, element.bottomNeighbour.transform.position, Color.red, 2.5f);
    //     }
    //elemetns.Add(element);
    //}
    //}

    //  if (upperMatches > 0) {
    //      AlexDebugger.GetInstance().AddMessage("Upper matches for element: " + startingElement.name + " are: " + upperMatches, "Matches");
    //     }
    //     if (bottomMatches > 0) {
    //         AlexDebugger.GetInstance().AddMessage("Bottom matches for element: " + startingElement.name + " are: " + bottomMatches, "Matches");
    //     }
    //     if (!elemetns.Contains(startingElement)) {
    //         //startingElement.GetComponent<UnityEngine.UI.Image>().color = startingElement.GetComponent<UnityEngine.UI.Image>().color + new Color(0, 0, 0, -0.5f);
    //         elemetns.Insert(0, startingElement);
    //     }
    //  }

    // if (leftMatches.Count + rightMatches.Count >= 2) {


    // d = "";
    // foreach (BoardElement element in leftMatches) {
    // if (element == null) {
    //     Debug.LogError("Null element after " + d);
    // }
    // else {
    //     d += element.name + ", ";
    //     if (element.leftNeighbour != null)
    //         Debug.DrawLine(element.transform.position, element.leftNeighbour.transform.position, Color.black, 2.5f);
    //element.GetComponent<UnityEngine.UI.Image>().color = element.GetComponent<UnityEngine.UI.Image>().color + new Color(0, 0, 0, -0.5f);
    //elemetns.Add(element);
    //}
    //}
    // if (leftMatches.Count > 0) {
    //     AlexDebugger.GetInstance().AddMessage("Left matches for element: " + startingElement.name + " are: " + leftMatches.Count, "Matches");
    // }
    // d = "";
    // foreach (BoardElement element in rightMatches) {
    // if (element == null) {
    //     Debug.LogError("Null element after " + d);
    // }
    // else {
    //     d += element.name + ", ";
    //     if (element.rightNeighbour != null)
    //         Debug.DrawLine(element.transform.position, element.rightNeighbour.transform.position, Color.black, 2.5f);
    //element.GetComponent<UnityEngine.UI.Image>().color = element.GetComponent<UnityEngine.UI.Image>().color + new Color(0, 0, 0, -0.5f);
    //elemetns.Add(element);
    // }

    //}
    // if (rightMatches.Count > 0) {
    //     AlexDebugger.GetInstance().AddMessage("Right matches for element: " + startingElement.name + " are: " + rightMatches.Count, "Matches");
    // }

    //     if (leftMatches > 0) {
    //         AlexDebugger.GetInstance().AddMessage("Left matches for element: " + startingElement.name + " are: " + leftMatches, "Matches");
    //     }
    //     if (rightMatches > 0) {
    //         AlexDebugger.GetInstance().AddMessage("Bottom matches for element: " + startingElement.name + " are: " + rightMatches, "Matches");
    //     }
    //     if (!elemetns.Contains(startingElement)) {
    //         elemetns.Insert(0, startingElement);
    //         //startingElement.GetComponent<UnityEngine.UI.Image>().color = startingElement.GetComponent<UnityEngine.UI.Image>().color + new Color(0, 0, 0, -0.5f);
    //     }
    // }
    // return elemetns;
    //}

    /// <summary> Depricated, based on graph structure </summary>
    // private List<BoardElement> CheckUpperNeighboursForMatches(BoardElement start) {
    // if (start.upperNeighbour != null && start.upperNeighbour.color != Color.white) {
    //     if (start.color == start.upperNeighbour.color) {
    //         elemetns.Add(start.upperNeighbour);
    //         foreach (BoardElement elem in CheckUpperNeighboursForMatches(start.upperNeighbour)) {
    //             elemetns.Add(elem);
    //         }
    //     }
    // }
    // Debug.Log("Total upperMatches for " + start.name + ", " + elemetns.Count);
    // return elemetns;
    //}

    /// <summary> Depricated, based on graph structure </summary>
    //  private List<BoardElement> CheckBottomNeighboursForMatches(BoardElement start) {
    // if (start.bottomNeighbour != null && start.bottomNeighbour.color != Color.white) {
    //     if (start.color == start.bottomNeighbour.color) {
    //         elemetns.Add(start.bottomNeighbour);
    //         foreach (BoardElement elem in CheckBottomNeighboursForMatches(start.bottomNeighbour)) {
    //             elemetns.Add(elem);
    //         }
    //     }
    // }
    // Debug.Log("Total bottomMatches for " + start.name + ", " + elemetns.Count);
    // return elemetns;
    // }

    /// <summary> Depricated, based on graph structure </summary>
    //private List<BoardElement> CheckLeftNeighboursForMatches(BoardElement start) {
    // if (start.leftNeighbour != null && start.leftNeighbour.color != Color.white) {
    //     if (start.color == start.leftNeighbour.color) {
    //         elemetns.Add(start.leftNeighbour);
    //         foreach (BoardElement elem in CheckLeftNeighboursForMatches(start.leftNeighbour)) {
    //             elemetns.Add(elem);
    //         }
    //     }
    // }
    //  Debug.Log("Total leftMatches for " + start.name + ", " + elemetns.Count);
    //         return elemetns;
    // }

    /// <summary> Depricated, based on graph structure </summary>
    // private List<BoardElement> CheckRightNeighboursForMatches(BoardElement start) {
    // if (start.rightNeighbour != null && start.rightNeighbour.color != Color.white) {
    //     if (start.color == start.rightNeighbour.color) {
    //         elemetns.Add(start.rightNeighbour);
    //         foreach (BoardElement elem in CheckRightNeighboursForMatches(start.rightNeighbour)) {
    //             elemetns.Add(elem);
    //         }
    //     }
    // }
    // Debug.Log("Total rightMatches for " + start.name + ", " + elemetns.Count);
    // return elemetns;
    // }

    /// <summary> Depricated, based on graph structure </summary>
    // private void PlayMatchEffect(List<BoardElement> elements) {
    //     Debug.Log("####### Scale to zero Effects played for " + elements.Count + ", elements...");
    //     AlexDebugger.GetInstance().AddMessage("####### Effects played for " + elements.Count + ", elements...", "Effects");
    //     for (int elemIndex = 0; elemIndex < elements.Count; elemIndex++) {
    //         BoardElement element = elements[elemIndex];
    //         Animations.Animation scaleToZero = Animations.AddAnimationScaleToZero(element.transform, swappingSpeed);
    //         playingAnimations.Add(scaleToZero);
    //         AlexDebugger.GetInstance().AddMessage("Scale to zero played for " + element.gameObject.name, "Effects");
    //     }
    // }

    /// <summary> Depricated, based on graph structure </summary>
    //   private List<BoardElement> MoveMatchedElementsUpwards(List<BoardElement> elements) {
    //     List<BoardElement> matchedElements = new List<BoardElement>();
    //     if (elements.Count > 0) {

    //         foreach (BoardElement e in elements) {
    //             matchedElements.Add(e);
    //         }
    //         // for (int i = 0; i < elements.Count; i++) {
    //         BoardElement currentElement = elements[0];
    //         BoardElement elementAbove = GetElementAbove(currentElement, elements);

    //         AlexDebugger.GetInstance().AddMessage("####### Moving upwards " + currentElement.name + ", elements", "UpawardMovement");
    //         currentElement.color = Color.white;
    //         //element.GetComponent<UnityEngine.UI.Image>().color = element.color;

    //         if (destroyedElements.Contains(currentElement) == false) {
    //             //BoardManager.changedElemtns.Add(currentElement);
    //             destroyedElements.Add(currentElement);
    //         }
    //         if (elementAbove != null) {
    //             // if (BoardManager.changedElemtns.Contains(currentElement) == false) {
    //             //     BoardManager.changedElemtns.Add(currentElement.upperNeighbour);
    //             // }
    //             SwapCells(currentElement, elementAbove, false, false);
    //         }
    //         else {
    //             matchedElements.Remove(currentElement);
    //         }

    //         //}
    //     }
    //     return matchedElements;

    /// <summary> Depricated, based on graph structure </summary>
    //  private void ReplaceElements(List<BoardElement> elements) {
    //     Debug.Log("####### Replacing " + elements.Count + ", new elements...");
    //     foreach (BoardElement element in elements) {
    //         element.color = BoardManager.GetAvailableColors()[Random.Range(0, GetAvailableColors().Length)];
    //         AlexDebugger.GetInstance().AddMessage("Replacing " + element.name, "Effects");
    //         element.GetComponent<UnityEngine.UI.Image>().color = element.color;
    //         playingAnimations.Add(Animations.AddAnimationScaleToOne(element.transform, swappingSpeed));

    //     }

    // }

    /// <summary> Depricated, based on graph structure </summary>
    // private void CheckBoardForMatches() {
    //     AlexDebugger.GetInstance().AddMessage("####### Checking after-match...", "AfterMatch");
    //     Debug.Log("####### Checking after-match...");
    //     string d = "New combos: ";
    //     foreach (BoardElement elem in GetCopyOfAllElements()) {
    //         foreach (BoardElement elem1 in CheckForMatches(elem)) {
    //             if (matchedElements.Contains(elem1) == false) {
    //                 d += elem1.name + ",";
    //                 matchedElements.Add(elem1);
    //             }
    //         }
    //     }
    //     if (matchedElements.Count > 0) {
    //         AlexDebugger.GetInstance().AddMessage(d, "AfterMatch");
    //         Debug.Log(d);
    //     }
    //     else {
    //         AlexDebugger.GetInstance().AddMessage("No new matches found", "AfterMatch");
    //         Debug.Log("No new matches found");
    //     }


    // }

    // private BoardElement[] GetCopyOfAllElements() {
    //     BoardElement[] array = new BoardElement[elements.Length];
    //     elements.CopyTo(array, 0);
    //     return array;
    // }


    // private bool GetIfOnLeftBoarder(int i) {
    //     for (int row = 0; row < rowsNumbers; row++) {
    //         if (i == row * collumsNumber) {
    //             return true;
    //         }
    //     }
    //     return false;

    // }
    // private bool GetIfOnRightBoarder(int i) {
    //     for (int row = 0; row < rowsNumbers - 1; row++) {
    //         if (i == row * collumsNumber + 3) {
    //             return true;
    //         }
    //     }
    //     return false;

    // }
    // private bool GetIfOnTopBoarder(int i) {
    //     if (i < collumsNumber) {
    //         return true;
    //     }
    //     return false;
    // }
    // private bool GetIfOnBottomBoarder(int i) {
    //     if (i > positions.GetLength(1) - 1 - collumsNumber) {
    //         return true;
    //     }
    //     return false;
    // }

    // private BoardElement GetElementAbove(BoardElement start, List<BoardElement> matches) {
    //     KeyValuePair<int, int> startPosition = GetPositionOfElement(start);
    //     if (startPosition.Value - 1 > -1) {
    //         if (matches.Contains(positions[startPosition.Key, startPosition.Value - 1]) || positions[startPosition.Key, startPosition.Value - 1].color == Color.white) {
    //             return GetElementAbove(positions[startPosition.Key, startPosition.Value - 1], matches);
    //         }
    //         else {
    //             return positions[startPosition.Key, startPosition.Value - 1];
    //         }
    //     }
    //     else {
    //         return null;
    //     }
    // }

#endregion
}