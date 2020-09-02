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
        CheckPanels();

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
            //Debug.Log("Checking for matches based on input");
            // About first input
            KeyValuePair<int, int> firstPosition = BoardFunctions.GetPositionOfElement(firstElement, positions);
            int numberOfMatchesForFirst = BoardFunctions.CheckForMatches(firstPosition.Key, firstPosition.Value, positions, ref matchedElemPositions, totalCollums, totalRows, true);
            // About second input
            KeyValuePair<int, int> secondPosition = BoardFunctions.GetPositionOfElement(secondElement, positions);
            int numberOfMatchesForSecond = BoardFunctions.CheckForMatches(secondPosition.Key, secondPosition.Value, positions, ref matchedElemPositions, totalCollums, totalRows, true);

            // if there are matches
            if (numberOfMatchesForFirst > 0 || numberOfMatchesForSecond > 0) {
                // allow Update() to play effects
                shouldPlayEffects = true;
                AlexDebugger.GetInstance().AddMessage("Total matches found: for input: " + firstElement.name + " and " + secondElement.name + " are: " + numberOfMatchesForFirst + numberOfMatchesForSecond, "Matches");
            }
            firstElement = null;
            secondElement = null;
            shouldCheckIfInputCreatesMatches = false;
            return;
        }
        // Step2: play effects for matched elements and move to step3
        else if (shouldPlayEffects) {
            Debug.Log("### Effects ### Playing effects for matches");
            for (int row = 0; row < positions.GetLength(0); row++) {
                for (int collum = 0; collum < positions.GetLength(1); collum++) {
                    if (matchedElemPositions[collum, row] == true) {
                        BoardFunctions.PlayMatchEffect(collum, row, positions, ref playingAnimations, swappingSpeed);
                    }
                    BoardFunctions.ToggleHighlightCell(collum, row, positions, false, highlightColor);
                }
            }
            SendAnimationMessagesToClient(ref playingAnimations);
            shouldPlayEffects = false;
            areElemetsReorienting = true;
            return;
        }
        // Step3: repeat until non-destroyed elements have droped to lower position and then move to step4
        else if (areElemetsReorienting) {
            //Debug.Log("Re-orienting board");
            bool hasChanged = false;
            for (int row = totalRows - 1; row > -1; row--) {
                for (int collum = 0; collum < totalCollums; collum++) {
                    if (matchedElemPositions[collum, row] == true) {
                        BoardFunctions.MoveMatchedElementUpwards(collum, row, ref positions, ref matchedElemPositions, ref positionsDestroyed, ref playingAnimations, swappingSpeed);
                        SendAnimationMessagesToClient(ref playingAnimations);
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
            Debug.Log("### Gen ### Generating new elements");
            int maxPossibleScore = -1;
            int bestInputCollum = -1;
            int bestInputRow = -1;
#if UNITY_EDITOR
            for (int row = 0; row < positions.GetLength(0); row++) {
                for (int collum = 0; collum < positions.GetLength(1); collum++) {
                    if (positionsDestroyed[collum, row] == false) {
                        int score = BoardFunctions.isPotentialInput(collum, row, positions, matchedElemPositions, totalCollums, totalRows);
                        if (score >= 3) {
                            Debug.Log("### SCORE ### " + positions[collum, row] + " exists with a possible score of: " + score);
                        }
                        if (score > maxPossibleScore) {
                            maxPossibleScore = score;
                            bestInputCollum = collum;
                            bestInputRow = row;
                        }
                    }
                }
            }
#endif
            for (int row = 0; row < positions.GetLength(0); row++) {
                for (int collum = 0; collum < positions.GetLength(1); collum++) {
                    if (positionsDestroyed[collum, row] == true) {
                        int score = BoardFunctions.ReplaceElement(collum, row, ref positions, ref matchedElemPositions, holders, maxScoreAllowed, maxPossibleScore, totalCollums, totalRows, ref playingAnimations, swappingSpeed);
                        if (score >= 3) {
                            Debug.Log("### SCORE ### " + positions[collum, row] + " was created with a possible score of: " + score);
                        }
                        if (score > maxPossibleScore) {
                            maxPossibleScore = score;
                            bestInputCollum = collum;
                            bestInputRow = row;
                        }
                        positionsDestroyed[collum, row] = false;
                    }
                }
            }
            Debug.Log("### SCORE ### Element: " + positions[bestInputCollum, bestInputRow].name + " has the best score possible: " + maxPossibleScore);
            SendAnimationMessagesToClient(ref playingAnimations);
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
                AlexDebugger.GetInstance().AddMessage("Total new matches found: " + newTotalMatches, "AfterMatch");
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
                AlexDebugger.GetInstance().AddMessage("No new matches found", "AfterMatch");
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
            AlexDebugger.GetInstance().AddMessage("Correct input: " + firstElement.transform.name + ", with " + secondElement.transform.name, "Input");
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
        if (areCellsSwapping /*|| playingAnimations.Count > 0*/ ) {
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


}