using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoardManager : MonoBehaviour {
    [SerializeField]
    private float swappingSpeed = 3.0f;
    [SerializeField]
    int totalCollums = 4;
    [SerializeField]
    int totalRows = 4;
    [SerializeField]
    int maxScoreAllowed = 25;
    [SerializeField]
    public Color highlightColor;


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

    /// <summary> Value holder for step1 to step2</summary>
    BoardElement firstElement = null;
    /// <summary> Value holder for step1 to step2</summary>
    BoardElement secondElement = null;


    // TO-DO: Create a proper singleton

    /// <summary> The instance of the manager</summary>
    public static BoardManager inst;

    /// <summary>Animations currently playing / qued to play. At server-client relationship this will be send to client to be played</summary>
    private List<AnimationMessage> playingAnimations = new List<AnimationMessage>();

    /// <summary> should be set false by the client</summary>
    public static bool areAnimationsPlaying = false;


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

    void Awake() {
        inst = this;
        boardPanel = this.transform;
        detailsPanel = boardPanel.GetChild(0);
        gamePanel = boardPanel.GetChild(1);

        positions = new BoardElement[totalCollums, totalRows];
        matchedElemPositions = new bool[totalCollums, totalRows];
        positionsDestroyed = new bool[totalCollums, totalRows];
        holders = new Transform[transform.parent.GetChild(1).childCount];

        for (int i = 0; i < holders.Length; i++) {
            holders[i] = transform.parent.GetChild(1).GetChild(i);
        }
        // Debug
        DebugCheckPanels();

        int rowIndex = 0;
        int collumIndex = 0;

        // Assign elements to the board
        for (int i = 0; i < gamePanel.childCount; i++) {
            if (collumIndex < positions.GetLength(0) && rowIndex < positions.GetLength(1)) {
                positions[collumIndex, rowIndex] = gamePanel.GetChild(i).gameObject.AddComponent<BoardElement>();
                positions[collumIndex, rowIndex].childIndex = i;
                //Debug.Log(positions[collumIndex, rowIndex].name + " is at position: c" + collumIndex + ", r" + rowIndex);
                collumIndex++;
                // fix collum index
                if (collumIndex == totalCollums) {
                    rowIndex++;
                    collumIndex = 0;
                }
            }

        }
    }

    // Start is called before the first frame update
    void Start() {
        // Assign color values to elemets

        int currentScore = 0;
        bool hasPotentialInputs = false;
        for (int row = 0; row < positions.GetLength(1); row++) {
            for (int collum = 0; collum < positions.GetLength(0); collum++) {
                positions[collum, row].color = BoardFunctions.GetColorInScore(collum, row, ref positions, matchedElemPositions, totalCollums, totalRows, maxScoreAllowed, ref currentScore, (currentScore >= maxScoreAllowed - 2) ? true : false);
                positions[collum, row].GetComponent<Image>().color = positions[collum, row].color;
                if (BoardFunctions.isPotentialInput(collum, row, positions, matchedElemPositions, totalCollums, totalRows) > 0 && !hasPotentialInputs) {
                    hasPotentialInputs = true;
                }

            }
        }
        shouldCheckBoard = true;
    }

    // Update is called once per frame
    void Update() {
        // Do nothing.
        if (areAnimationsPlaying) {
            return;
        }
        // Step1: check if input create matches and if true move to step2
        else if (shouldCheckIfInputCreatesMatches) {
            AlexDebugger.GetInstance().AddMessage("Entering Step1: -check if input between-" + firstElement.transform.name + " and " + secondElement.transform.name + " create matches", AlexDebugger.tags.Step1);
            //Debug.Log("Checking for matches based on input");
            // About first input
            KeyValuePair<int, int> firstPosition = BoardFunctions.GetPositionOfElement(firstElement, positions);
            int numberOfMatchesForFirst = BoardFunctions.CheckForMatches(firstPosition.Key, firstPosition.Value, positions, ref matchedElemPositions, totalCollums, totalRows, true);
            AlexDebugger.GetInstance().AddMessage("Number of matches found for input element: " + firstElement.transform.name + " are " + numberOfMatchesForFirst, AlexDebugger.tags.Step1);
            // About second input
            KeyValuePair<int, int> secondPosition = BoardFunctions.GetPositionOfElement(secondElement, positions);
            int numberOfMatchesForSecond = BoardFunctions.CheckForMatches(secondPosition.Key, secondPosition.Value, positions, ref matchedElemPositions, totalCollums, totalRows, true);
            AlexDebugger.GetInstance().AddMessage("Number of matches found for input element: " + secondElement.transform.name + " are " + numberOfMatchesForSecond, AlexDebugger.tags.Step1);
            // if there are matches
            if (numberOfMatchesForFirst > 0 || numberOfMatchesForSecond > 0) {
                AlexDebugger.GetInstance().AddMessage("Matches found, when finish, go to step2: -play effects for matched elements-", AlexDebugger.tags.Step1);
                // allow Update() to play effects
                shouldPlayEffects = true;
            }
            else {
                AlexDebugger.GetInstance().AddMessage("No matches found, when finish, go back to step0: -waiting for input-", AlexDebugger.tags.Step1);
            }
            firstElement = null;
            secondElement = null;
            shouldCheckIfInputCreatesMatches = false;
            AlexDebugger.GetInstance().AddMessage("Step1 Finished: -check if input between-" + firstElement.transform.name + " and " + secondElement.transform.name + " with total matches: " + numberOfMatchesForSecond, AlexDebugger.tags.Step1);

            return;
        }
        // Step2: play effects for matched elements and move to step3
        else if (shouldPlayEffects) {
            AlexDebugger.GetInstance().AddMessage("Entering Step2: -play effects for matched elements-", AlexDebugger.tags.Step2);
            for (int row = 0; row < positions.GetLength(0); row++) {
                for (int collum = 0; collum < positions.GetLength(1); collum++) {
                    if (matchedElemPositions[collum, row] == true) {
                        AlexDebugger.GetInstance().AddMessage("Element: " + positions[collum, row].transform.name + "  was a match of color: " + positions[collum, row].color.ToString() + ", de-activating highlight.", AlexDebugger.tags.Step2);
                        BoardFunctions.PlayMatchEffect(collum, row, positions, ref playingAnimations, swappingSpeed);
                    }
                    BoardFunctions.ToggleHighlightCell(collum, row, positions, false, highlightColor);
                }
            }
            AlexDebugger.GetInstance().AddMessage("Sending total animations to client: " + playingAnimations.Count, AlexDebugger.tags.Step2);
            SendAnimationMessagesToClient(ref playingAnimations);
            shouldPlayEffects = false;
            AlexDebugger.GetInstance().AddMessage("Step2 finished: -play effects for matched elements-, go to Step3: -reorient elements- " + playingAnimations.Count, AlexDebugger.tags.Step2);
            areElemetsReorienting = true;
            return;
        }
        // Step3: repeat until non-destroyed elements have droped to lower position and then move to step4
        else if (areElemetsReorienting) {
            AlexDebugger.GetInstance().AddMessage("Entering Step3: -reorienting board-", AlexDebugger.tags.Step3);
            bool hasChanged = false;
            for (int row = totalRows - 1; row > -1; row--) {
                for (int collum = 0; collum < totalCollums; collum++) {
                    if (matchedElemPositions[collum, row] == true) {
                        AlexDebugger.GetInstance().AddMessage("Moving element: " + positions[collum, row].transform.name + " upwards", AlexDebugger.tags.Step3);
                        BoardFunctions.MoveMatchedElementUpwards(collum, row, ref positions, ref matchedElemPositions, ref positionsDestroyed, ref playingAnimations, swappingSpeed);
                        AlexDebugger.GetInstance().AddMessage("Sending total animations to client: " + playingAnimations.Count, AlexDebugger.tags.Step3);
                        SendAnimationMessagesToClient(ref playingAnimations);
                        hasChanged = true;
                    }
                }
                // placed here so the entire row's animation play simutaniously
                if (hasChanged) {
                    AlexDebugger.GetInstance().AddMessage("Re-orienting has not finished, wait a frame and repeat Step3: -reorienting board-", AlexDebugger.tags.Step3);
                    return;
                }
            }
            if (!hasChanged) {
                AlexDebugger.GetInstance().AddMessage("Step3: -reorienting board- has finished, moving to Step4: -replace destroyed elements", AlexDebugger.tags.Step3);
                areElemetsReorienting = false;
                shouldGenerateElem = true;
            }
            return;
        }
        // Step4: replace destroyed elements by assigning new color,  move to step5
        else if (shouldGenerateElem) {
            AlexDebugger.GetInstance().AddMessage("Entering Step4: -replace destroyed elements-", AlexDebugger.tags.Step4);
            // the minimum maxOutput user's input can reach
            int maxOutput = -1;

            int bestInputCollum = -1;
            int bestInputRow = -1;

            // Check maxOutput for elements already on the board
            for (int row = 0; row < positions.GetLength(0); row++) {
                for (int collum = 0; collum < positions.GetLength(1); collum++) {
                    if (positionsDestroyed[collum, row] == false) {
                        // Get the max output of this element
                        int maxOutputForElement = BoardFunctions.isPotentialInput(collum, row, positions, matchedElemPositions, totalCollums, totalRows);
                        if (maxOutputForElement >= 1) {
                            AlexDebugger.GetInstance().AddMessage("Element: " + positions[collum, row] + " exists with a potential output of " + maxOutputForElement, AlexDebugger.tags.Step4);
                        }
                        if (maxOutputForElement > maxOutput) {
                            AlexDebugger.GetInstance().AddMessage("Element: " + positions[collum, row] + " exists with a potential output of " + maxOutputForElement + " is the new possible maxOut, previous: " + maxOutput, AlexDebugger.tags.Step4);
                            maxOutput = maxOutputForElement;
                            bestInputCollum = collum;
                            bestInputRow = row;
                        }
                    }
                }
            }

            for (int row = 0; row < positions.GetLength(0); row++) {
                for (int collum = 0; collum < positions.GetLength(1); collum++) {
                    if (positionsDestroyed[collum, row] == true) {
                        int maxOutputForElement = BoardFunctions.ReplaceElement(collum, row, ref positions, ref matchedElemPositions, holders, maxScoreAllowed, maxOutput, totalCollums, totalRows, ref playingAnimations, swappingSpeed);
                        if (maxOutputForElement >= 1) {
                            AlexDebugger.GetInstance().AddMessage("Element: " + positions[collum, row] + " was created wihh a potential output of " + maxOutputForElement, AlexDebugger.tags.Step4);
                        }
                        if (maxOutputForElement > maxOutput) {
                            AlexDebugger.GetInstance().AddMessage("Element: " + positions[collum, row] + " exists with a potential output of " + maxOutputForElement + " is the new possible maxOut, previous: " + maxOutput, AlexDebugger.tags.Step4);
                            maxOutput = maxOutputForElement;
                            bestInputCollum = collum;
                            bestInputRow = row;
                        }
                        positionsDestroyed[collum, row] = false;
                    }
                }
            }
            AlexDebugger.GetInstance().AddMessage("Element: " + positions[bestInputCollum, bestInputRow].name + " has the best score possible: " + maxOutput, AlexDebugger.tags.Step4);
            AlexDebugger.GetInstance().AddMessage("Sending total animations to client: " + playingAnimations.Count, AlexDebugger.tags.Step4);
            SendAnimationMessagesToClient(ref playingAnimations);
            AlexDebugger.GetInstance().AddMessage("Step4 -replace destroyed elements- has finished, moving to Step5 -Aftermatch-", AlexDebugger.tags.Step4);
            shouldGenerateElem = false;
            shouldCheckBoard = true;
            return;
        }
        // Step5: check for new matches
        else if (shouldCheckBoard) {
            Debug.Log("### AfterMatch ### Checking board for combos");
            int newTotalMatches = BoardFunctions.CheckBoardForMatches(positions, ref matchedElemPositions, totalCollums, totalRows);
            // Go back to step2
            if (newTotalMatches > 0) {
                Debug.Log("### AfterMatch ### Total new matches found: " + newTotalMatches);
                AlexDebugger.GetInstance().AddMessage("Total new matches found: " + newTotalMatches, AlexDebugger.tags.Aftermatch);
                shouldPlayEffects = true;
            }
            // Check for possible inputs
            else {
                bool hasPossibleInput = false;
                for (int row = 0; row < positions.GetLength(0); row++) {
                    for (int collum = 0; collum < positions.GetLength(1); collum++) {
                        if (BoardFunctions.isPotentialInput(collum, row, positions, matchedElemPositions, totalCollums, totalRows) > 0) {
                            BoardFunctions.ToggleHighlightCell(collum, row, positions, true, highlightColor);
                            hasPossibleInput = true;
                        }
                        else {
                            BoardFunctions.ToggleHighlightCell(collum, row, positions, false, highlightColor);
                        }
                    }
                }
                AlexDebugger.GetInstance().AddMessage("No new matches found", AlexDebugger.tags.Aftermatch);
                Debug.Log("### AfterMatch ### No new matches found");

                if (!hasPossibleInput) {
                    Debug.Log("### AfterMatch ### No possible inputs found, re-generating board");
                    for (int row = 0; row < positions.GetLength(0); row++) {
                        for (int collum = 0; collum < positions.GetLength(1); collum++) {
                            positionsDestroyed[collum, row] = true;
                        }
                    }
                    shouldGenerateElem = true;
                }
                SendAnimationMessagesToClient(ref playingAnimations);
                shouldCheckBoard = false;
                return;
            }
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

    public static BoardManager GetNewBoardManager() {
        return GetInstance().transform.gameObject.AddComponent<BoardManager>();
    }

    /// <summary>Receives input from client and handles it</summary>
    public void HandleInput(int firstElementIndexAtParent, int secondElementIndexAtParent) {
        // find first element based on gamePanel's child index
        firstElement = gamePanel.GetChild(firstElementIndexAtParent).GetComponent<BoardElement>();
        // find second element based on gamePanel's child index
        secondElement = gamePanel.GetChild(secondElementIndexAtParent).GetComponent<BoardElement>();

        // Check if elements are neighbours thus input correct
        // TO-DO: make it to check if it is a possible mathch
        if (BoardFunctions.GetIfNeighbours(firstElement, secondElement, positions)) {
            AlexDebugger.GetInstance().AddMessage("Correct input: " + firstElement.transform.name + ", with " + secondElement.transform.name, AlexDebugger.tags.Input);
            // Swap the elements on the board
            BoardFunctions.SwapElements(firstElement, secondElement, ref positions, ref playingAnimations, rewire : false, swappingSpeed);
            // Allow Update() to check if matches are created
            shouldCheckIfInputCreatesMatches = true;
        }
        else {
            // Swap elements, on rewire mode
            BoardFunctions.SwapElements(firstElement, secondElement, ref positions, ref playingAnimations, rewire : true, swappingSpeed);
        }
        // Send animations to client and empty list
        SendAnimationMessagesToClient(ref playingAnimations);
        areCellsSwapping = true;

    }

    public void SendAnimationMessagesToClient(ref List<AnimationMessage> animMsg) {
        if (animMsg.Count > 0) {
            Animations.SetAnimationMessages(animMsg.ToArray());
            animMsg.Clear();
            areAnimationsPlaying = true;
        }
    }

    /// <summary>Is boardManager available</summary>
    public bool IsAvailable() {
        if (areCellsSwapping || areAnimationsPlaying) {
            return false;
        }
        else {
            return true;
        }
    }


    /// Debug
    private void DebugElementsMatches(List<BoardElement> elements) {
        if (elements.Count > 0) {
            string d = "";
            foreach (BoardElement e in elements) {
                d += e.gameObject.name + ", ";
            }
            AlexDebugger.GetInstance().AddMessage(d, AlexDebugger.tags.Matches);
            Debug.Log(d + ", Matches");
        }
    }
    private void DebugCheckPanels() {
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


}